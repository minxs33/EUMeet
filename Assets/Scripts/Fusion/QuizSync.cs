using System.Collections;
using System.Collections.Generic;
using Fusion;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class QuizSync : NetworkBehaviour
{
    QuizManager quizManager;
    private List<QuizManager.QuestionItem> questions;
    [Networked] private int currentQuestionIndex { get; set; } = 0;
    [SerializeField] private GameObject[] questionGo;


    private void Start() {
        if (!HasStateAuthority)
        {
            Debug.LogWarning("QuizSync does not have state authority. Ensure it's managed by the host.");
        }
    }

    // public void StartQuiz(){
    //     quizManager = GetComponent<QuizManager>();
    //     questions = quizManager.questions;

    //     string serializedQuestions = JsonUtility.ToJson(new QuestionResponseWrapper
    //     {
    //         questions = questions
    //     });

    //     RPC_RequestStartQuiz(serializedQuestions);  

    // }

    // [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    // public void RPC_RequestStartQuiz(string serializedQuestions){
    //     RPC_BeginQuiz(serializedQuestions);
    // }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void RPC_BeginQuiz(string serializedQuestions){
        Debug.Log("Begin Quiz");
        questions = JsonUtility.FromJson<QuestionResponseWrapper>(serializedQuestions).questions;
        DisplayQuestion(0);
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
    }

    private void DisplayQuestion(int questionIndex)
    {
        if (questionIndex < 0 || questionIndex >= questions.Count)
        {
            Debug.LogError("Invalid question index!");
            return;
        }

        var question = questions[questionIndex];
        
        questionGo[0].GetComponentInChildren<TMP_Text>().text = question.question;
        questionGo[1].GetComponentInChildren<TMP_Text>().text = question.a;
        questionGo[2].GetComponentInChildren<TMP_Text>().text = question.b;
        questionGo[3].GetComponentInChildren<TMP_Text>().text = question.c;
        questionGo[4].GetComponentInChildren<TMP_Text>().text = question.d;

        foreach (var go in questionGo)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(go.GetComponent<RectTransform>());
        }
        
        Debug.Log($"Question: {question.question}");
        Debug.Log($"A: {question.a}, B: {question.b}, C: {question.c}, D: {question.d}");
    }

    [System.Serializable]
    public class QuestionResponseWrapper
    {
        public List<QuizManager.QuestionItem> questions;
    }
}
