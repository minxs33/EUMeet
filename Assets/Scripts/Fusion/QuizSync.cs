using System.Collections;
using System.Collections.Generic;
using Fusion;
using Photon.Pun;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using ExitGames.Client.Photon.StructWrapping;

public class QuizSync : NetworkBehaviour
{
    private List<QuizManager.QuestionItem> questions;
    GameLogic gameLogic;
    Leaderboard leaderboard;
    [Networked] private int currentQuestionIndex { get; set; } = 0;
    [SerializeField] private GameObject[] questionGo;

    private float questionTimer = 10f;
    private Coroutine questionTimerCoroutine;
    private char selectedAnswer;
    

    private void Start()
    {
        if (!HasStateAuthority)
        {
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
    public void RPC_BeginQuiz(string serializedQuestions)
    {
        Debug.Log("Begin Quiz");
        questions = JsonUtility.FromJson<QuestionResponseWrapper>(serializedQuestions).questions;
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
    }

    private IEnumerator QuestionTimerCoroutine()
    {
        yield return new WaitForSeconds(questionTimer);

        if (currentQuestionIndex + 1 >= questions.Count)
        {
            Rpc_GetLeaderboard();
            Rpc_ToggleLeaderboard();
            yield return new WaitForSeconds(3f);
            Rpc_ToggleLeaderboard();
            EndQuiz();
        }
        else
        {
            Rpc_GetLeaderboard();
            Rpc_ToggleLeaderboard();
            yield return new WaitForSeconds(3f);
            Rpc_ToggleLeaderboard();
            NextQuestion();
        }
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
        if (questionIndex < 0 || questionIndex >= questions.Count)
        {
            Debug.LogError("Invalid question index!");
            return;
        }

        var question = questions[questionIndex];

        // Set pertanyaan dan pilihan jawaban
        questionGo[0].GetComponentInChildren<TMP_Text>().text = question.question;
        questionGo[1].GetComponentInChildren<TMP_Text>().text = question.a;
        questionGo[2].GetComponentInChildren<TMP_Text>().text = question.b;
        questionGo[3].GetComponentInChildren<TMP_Text>().text = question.c;
        questionGo[4].GetComponentInChildren<TMP_Text>().text = question.d;

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
            button.onClick.AddListener(() => OnAnswerSelected(question.correct_answer, buttonIndex));
        }

        Debug.Log($"Question: {question.question}");
        Debug.Log($"A: {question.a}, B: {question.b}, C: {question.c}, D: {question.d}");
        Debug.Log($"Answer: {question.correct_answer}");
    }

    private void OnAnswerSelected(string correctAnswer, int selectedIndex)
    {
        selectedAnswer = (char)('a' + (selectedIndex - 1));

        foreach (var go in questionGo)
        {
            var button = go.GetComponent<Button>();
            if (button != null)
            {
                button.interactable = false;
            }
        }

        for (int i = 1; i <= 4; i++)
        {
            var button = questionGo[i].GetComponent<Button>();
            var buttonColors = button.colors;

            if (i == selectedIndex)
            {
                if (correctAnswer.Equals(selectedAnswer.ToString(), System.StringComparison.OrdinalIgnoreCase))
                {
                    buttonColors.disabledColor = Color.green;
                    // var player = gameLogic.spawnedPlayers.Values.Select(networkObject => networkObject.GetComponent<Player>()).FirstOrDefault(p => p.PlayerName == PlayerPrefs.GetString("name"));

                    var player = FindObjectsOfType<Player>().FirstOrDefault(p => p.PlayerName == PlayerPrefs.GetString("name"));
                    if(player != null)
                    {
                        player.Rpc_AddScore(10);
                        Debug.Log("Player found:" +player.PlayerName);
                    }else{
                        Debug.Log("Player not found");
                    }
                    // AddPlayerScore(Object.InputAuthority.ToString(), 10); // Example: 10 points for a correct answer
                }
                else
                {
                    buttonColors.disabledColor = Color.red;
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

        foreach (var go in questionGo)
        {
            go.GetComponentInChildren<TMP_Text>().text = string.Empty;
        }
        
        ResetAllPlayerScores();

        GameEventsManager.instance.QuizEvents.EndQuiz();
    }

    [System.Serializable]
    public class QuestionResponseWrapper
    {
        public List<QuizManager.QuestionItem> questions;
    }
}
