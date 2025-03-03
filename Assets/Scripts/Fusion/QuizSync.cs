using System.Collections;
using System.Collections.Generic;
using Fusion;
using Photon.Pun;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using ExitGames.Client.Photon.StructWrapping;
using UnityEngine.Networking;
using System.Net.Http;
using System;
using static QuizManager;

public class QuizSync : NetworkBehaviour
{
    public static QuizSync Instance { get; private set; }
    private List<QuestionItem> questions;
    GameLogic gameLogic;
    Leaderboard leaderboard;
    [Networked] private int currentSubjectID { get; set; }
    [Networked] private int currentQuestionIndex { get; set; } = 0;
    [SerializeField] private GameObject[] questionGo;

    private List<Player> playerList;
    private float questionTimer = 10f;
    private bool isTimerRunning = false;
    private float timeLeft;
    private Coroutine questionTimerCoroutine;
    private char selectedAnswer;
    private int selectedIndex = -1;

    private void Start()
    {
        if(HasStateAuthority)
        {
            GameLogic gameLogic = FindObjectOfType<GameLogic>();
            if(!gameLogic){
                Debug.LogError("GameLogic not found in the scene!");
            }
        }else{
            Debug.LogWarning("QuizSync does not have state authority. Ensure it's managed by the host.");
        }

        gameLogic = FindObjectOfType<GameLogic>();
        if (gameLogic == null) {
            Debug.LogError("GameLogic not found in the scene!");
        }

        leaderboard = FindObjectOfType<Leaderboard>();
        if (leaderboard == null) {
            Debug.LogError("Leaderboard not found in the scene!");
        }
    }

    private void ResetAllPlayerScores()
    {
        var players = FindObjectsOfType<Player>();
        foreach (var player in players)
        {
            if (player.HasStateAuthority)
            {
                player.Rpc_ResetScore();
            }
        }
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void Rpc_BeginQuiz(int quizId, int subjectId)
    {
        Debug.Log("Begin Quiz");
        currentSubjectID = subjectId;
        GetQuizLocal(quizId);
        if (questions == null || questions.Count == 0)
        {
            Debug.LogError("No questions received!");
            return;
        }
        GameEventsManager.instance.QuizEvents.StartQuizSetup();
        StartCoroutine(CountDown());
    }

    private void GetQuizLocal(int quizId){
        List<IMultipartFormSection> formData = new List<IMultipartFormSection>
        {
            new MultipartFormDataSection("quiz_id", quizId.ToString()),
        };

        StartCoroutine(GetQuizLocalCoroutine(formData));
    }

    private IEnumerator GetQuizLocalCoroutine(List<IMultipartFormSection> formData){
        UnityWebRequest www = UnityWebRequest.Post("http://172.29.174.196/get-quiz-question", formData);
        yield return www.SendWebRequest();

        try{
            if(www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError){
                string responseText = www.downloadHandler.text;
                Debug.Log($"Response text: {responseText}");
            }else if(www.result == UnityWebRequest.Result.Success){
                string responseText = www.downloadHandler.text;
                Debug.Log($"Success : {responseText}");
                QuestionResponse questionResponse = JsonUtility.FromJson<QuestionResponse>(responseText);
                questions = questionResponse.data.questions;
            }
        }catch(Exception e){
            Debug.Log(e);
        }
    }

    private IEnumerator CountDown(){
        for (int i = 3; i > 0; i--)
        {
            GameEventsManager.instance.QuizEvents.CountDownStart(i);
            SoundManager.PlaySound(SoundType.QUIZ_START_COUNTDOWN,null,0.5f);
            yield return new WaitForSeconds(1f);
        }
        // SoundManager.PlaySound(SoundType.QUIZ_START, null, 0.5f);
        GameEventsManager.instance.QuizEvents.CountDownStart(0);

        GameEventsManager.instance.QuizEvents.StartQuiz();
        DisplayQuestion(0);

        if (HasStateAuthority)
        {
            StartQuestionTimer();
        }
    }

    private void StartQuestionTimer()
    {
        if (questionTimerCoroutine != null){
            StopCoroutine(questionTimerCoroutine);
        }
        questionTimerCoroutine = StartCoroutine(QuestionTimerCoroutine());
        RPC_StartClientTimer();
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_StartClientTimer(){
        if (!isTimerRunning)
        {
            SoundManager.PlaySound(SoundType.QUIZ_COUNTDOWN,null,0.5f);
            isTimerRunning = true;
            StartCoroutine(ClientTimerCoroutine());
        }
    }

    private IEnumerator ClientTimerCoroutine(){
        timeLeft = questionTimer;
        while (timeLeft > 0)
        {
            timeLeft -= Time.deltaTime;
            yield return null;
        }

        yield return new WaitForSeconds(questionTimer);
    }

    private IEnumerator QuestionTimerCoroutine()
    {

        yield return new WaitForSeconds(questionTimer);

        RPC_ShowAnswer();

        yield return new WaitForSeconds(4f);
        Debug.Log($"CurrentQuestionIndex: {currentQuestionIndex}, Questions.Count: {questions.Count}");
        if (currentQuestionIndex >= questions.Count - 1)
        {
            RPC_PlayLeaderboardSound(SoundType.QUIZ_SHOW_LEADERBOARD);
            Rpc_GetLeaderboard();
            Rpc_ToggleLeaderboard();
            yield return new WaitForSeconds(3f);
            Rpc_ToggleLeaderboard();
            AddPoints();
            EndQuiz();
        }
        else
        {
            RPC_PlayLeaderboardSound(SoundType.QUIZ_SHOW_END_LEADERBOARD);
            Rpc_GetLeaderboard();
            Rpc_ToggleLeaderboard();
            yield return new WaitForSeconds(5f);
            Rpc_ToggleLeaderboard();
            NextQuestion();
        }
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_PlayLeaderboardSound(SoundType soundType){
        SoundManager.PlaySoundWithTransition(soundType,0.5f);
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void Rpc_ToggleLeaderboard()
    {
        GameEventsManager.instance.QuizEvents.ToggleLeaderboard();
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void Rpc_GetLeaderboard()
    {
        GameEventsManager.instance.QuizEvents.GetLeaderboard();
    }

     private void AddPoints(){
        playerList = gameLogic.spawnedPlayers.Values.Select(networkObject => networkObject.GetComponent<Player>()).Where(player => player != null).Where(player => player.IsDosen == 0).ToList();

        foreach(var player in playerList){
            List<IMultipartFormSection> formData = new List<IMultipartFormSection>(){
                    new MultipartFormDataSection("uniqueID", player.Uid.ToString()),
                    new MultipartFormDataSection("subject_id", currentSubjectID.ToString()),
                    new MultipartFormDataSection("points", player.LeaderboardScore.ToString()),
                };
            StartCoroutine(StorePointsCoroutine(formData));
        }
    }

    private IEnumerator StorePointsCoroutine(List<IMultipartFormSection> formData){
        UnityWebRequest www = UnityWebRequest.Post("http://172.29.174.196/add-point", formData);

        yield return www.SendWebRequest();

        try{
            if(www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError){
                string responseText = www.downloadHandler.text;
                Debug.Log($"Response text: {responseText}");
            }else if(www.result == UnityWebRequest.Result.Success){
                string responseText = www.downloadHandler.text;
                Debug.Log($"Point added successfully");
            }
        }catch(Exception e){
            Debug.Log(e);
        }
    }

    public void NextQuestion()
    {
        if (Object.HasStateAuthority)
        {
            currentQuestionIndex++;
            RPC_UpdateQuestion(currentQuestionIndex);
        }
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_UpdateQuestion(int questionIndex)
    {
        DisplayQuestion(questionIndex);
        if (Object.HasStateAuthority)
        {
            StartQuestionTimer();
        }
    }

    private void DisplayQuestion(int questionIndex)
    {
        Debug.Log("questions : "+questions);
        if (questionIndex < 0 || questionIndex >= questions.Count)
        {
            Debug.LogError("Invalid question index!");
            return;
        }


        selectedIndex = -1;
        selectedAnswer = '\0';

        var question = questions[questionIndex];

        questionGo[0].GetComponentInChildren<TMP_Text>().text = question.question.ToString();
        questionGo[1].GetComponentInChildren<TMP_Text>().text = question.a.ToString();
        questionGo[2].GetComponentInChildren<TMP_Text>().text = question.b.ToString();
        questionGo[3].GetComponentInChildren<TMP_Text>().text = question.c.ToString();
        questionGo[4].GetComponentInChildren<TMP_Text>().text = question.d.ToString();

        foreach (var go in questionGo)
        {
            var button = go.GetComponent<Button>();
            if (button != null)
            {
                var buttonColors = button.colors;
                buttonColors.normalColor = Color.white;
                buttonColors.disabledColor = Color.gray;
                button.colors = buttonColors;

                button.interactable = true;
            }

            LayoutRebuilder.ForceRebuildLayoutImmediate(go.GetComponent<RectTransform>());
        }

        for (int i = 1; i <= 4; i++)
        {
            int buttonIndex = i;
            var button = questionGo[i].GetComponent<Button>();
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => OnAnswerSelected(buttonIndex));
        }

        // Debug.Log($"Question: {question.question}");
        // Debug.Log($"A: {question.a}, B: {question.b}, C: {question.c}, D: {question.d}");
        // Debug.Log($"Answer: {question.correct_answer}");
    }

    private void OnAnswerSelected(int index)
    {
        SoundManager.PlaySound(SoundType.UI_CONFIRM, null, 0.5f);
        selectedAnswer = (char)('a' + (index - 1));
        selectedIndex = index;

        for (int i = 0; i < questionGo.Length; i++)
        {
            var button = questionGo[i].GetComponent<Button>();
            if (button != null)
            {
                var buttonColors = button.colors;

                if (i == index)
                {
                    buttonColors.disabledColor = new Color(0.8313726f, 0.6392157f, 0.4509804f);
                }
                else
                {
                    buttonColors.disabledColor = Color.gray;
                }

                button.colors = buttonColors;
                button.interactable = false;
            }
        }

        Debug.Log($"Player selected: {selectedAnswer}");

        var question = questions[currentQuestionIndex];
        string correctAnswer = question.correct_answer;
        if (correctAnswer.Equals(selectedAnswer.ToString(), System.StringComparison.OrdinalIgnoreCase))
        {
            if(PlayerPrefs.GetInt("isDosen") != 1){
                int bonusPoints = Mathf.CeilToInt(timeLeft);
                int totalScore = 10 + bonusPoints;
                AddScore(totalScore);
            }
        }
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_ShowAnswer()
    {
        isTimerRunning = false;
        var question = questions[currentQuestionIndex];
        string correctAnswer = question.correct_answer.ToString();

        for (int i = 1; i <= 4; i++)
        {
            var button = questionGo[i].GetComponent<Button>();
            button.interactable = false;
            var buttonColors = button.colors;

            if (i == selectedIndex)
            {
                if (correctAnswer.Equals(selectedAnswer.ToString(), System.StringComparison.OrdinalIgnoreCase))
                {
                    buttonColors.disabledColor = Color.green;
                    SoundManager.PlaySoundWithTransition(SoundType.QUIZ_CORRECT, 0.5f);
                }
                else
                {
                    buttonColors.disabledColor = Color.red;
                    SoundManager.PlaySoundWithTransition(SoundType.QUIZ_WRONG, 0.5f);
                }
            }

            char currentOption = (char)('a' + (i - 1));
            if (correctAnswer.Equals(currentOption.ToString(), System.StringComparison.OrdinalIgnoreCase))
            {
                buttonColors.normalColor = Color.green;
                buttonColors.disabledColor = Color.green;
            }

            button.colors = buttonColors;
        }

        selectedIndex = -1;
    }

    private void AddScore(int score){
        var player = FindObjectsOfType<Player>().FirstOrDefault(p => p.PlayerName == PlayerPrefs.GetString("name"));
        if(player != null)
        {
            player.Rpc_AddScore(score);
            Debug.Log("Player found:" +player.PlayerName);
        }else{
            Debug.Log("Player not found");
        }
    }

    public void EndQuiz()
    {
        if (questionTimerCoroutine != null)
        {
            StopCoroutine(questionTimerCoroutine);
            questionTimerCoroutine = null;
        }

        Debug.Log("Quiz has ended.");

        RPC_EndQuiz();
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_EndQuiz()
    {
        Debug.Log("All players notified: Quiz has ended.");
        currentQuestionIndex = 0;
        questions = null;

        GameEventsManager.instance.QuizEvents.EndQuiz();

        foreach (var go in questionGo)
        {
            go.GetComponentInChildren<TMP_Text>().text = string.Empty;
        }

        ResetAllPlayerScores();
    }
}
