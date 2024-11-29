using System;
using System.Collections;
using System.Collections.Generic;
using Agora.Rtc.LitJson;
using ExitGames.Client.Photon.StructWrapping;
using Newtonsoft.Json.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class QuizManager : MonoBehaviour
{
    public static QuizManager Instance { get; private set; }
    private int _quizId;
    [SerializeField] private GameObject quizCardPrefab;
    [SerializeField] private Transform parentTransform;

    [SerializeField] private GameObject questionCardPrefab;
    [SerializeField] private Transform questionParentTransform;

    private void OnEnable(){
        GameEventsManager.instance.RTCEvents.OnPlayerJoined += StartQuiz;
        GameEventsManager.instance.QuizEvents.OnAddQuiz += AddQuiz;
        GameEventsManager.instance.QuizEvents.OnUpdateQuiz += UpdateQuiz;
        GameEventsManager.instance.QuizEvents.OnAddQuestion += AddQuestion;
        GameEventsManager.instance.QuizEvents.OnUpdateQuestion += UpdateQuestion;
    }

    private void OnDisable(){
        GameEventsManager.instance.RTCEvents.OnPlayerJoined -= StartQuiz;
        GameEventsManager.instance.QuizEvents.OnAddQuiz -= AddQuiz;
        GameEventsManager.instance.QuizEvents.OnUpdateQuiz -= UpdateQuiz;
        GameEventsManager.instance.QuizEvents.OnAddQuestion -= AddQuestion;
        GameEventsManager.instance.QuizEvents.OnUpdateQuestion -= UpdateQuestion;
    }
    private void StartQuiz()
    {
        GetQuiz();
    }

    private void GetQuiz(){
        List<IMultipartFormSection> formData = new List<IMultipartFormSection>
        {
            new MultipartFormDataSection("uniqueID", PlayerPrefs.GetInt("uid").ToString()),
        };

        StartCoroutine(GetQuizList(formData));
    }
    private IEnumerator GetQuizList(List<IMultipartFormSection> formData){
        UnityWebRequest www = UnityWebRequest.Post("http://172.29.174.196/get-quizzes", formData);

        yield return www.SendWebRequest();

        try{
            
            if(www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError){
                string responseText = www.downloadHandler.text;
                Debug.Log($"Response text: {responseText}");
            }else if(www.result == UnityWebRequest.Result.Success){
                string responseText = www.downloadHandler.text;
                Debug.Log($"Quiz recieved successfully");

                QuizResponse quizResponse = JsonUtility.FromJson<QuizResponse>(responseText);

                foreach (Transform child in parentTransform)
                {
                    Destroy(child.gameObject);
                }
                
                foreach (QuizItem quiz in quizResponse.data)
                {
                    int currentQuizID = quiz.id;
                    GameObject quizCard = Instantiate(quizCardPrefab, parentTransform);
                    quizCard.transform.Find("QuizSelect").GetComponent<QuizCardID>().Setup(quiz.title, quiz.id);

                    Button openQuestionButton = quizCard.transform.Find("QuestionButton")?.GetComponent<Button>();
                    Button deleteQuizButton = quizCard.transform.Find("DeleteQuizButton")?.GetComponent<Button>();

                    if(openQuestionButton != null){
                        openQuestionButton.onClick.AddListener(() => {
                            GameEventsManager.instance.QuizEvents.OpenQuizQuestion();
                            OpenQuizQuestion(quiz.id);
                        });
                    }else{
                        Debug.Log("Open question button not found");
                    }
                    
                    if(deleteQuizButton != null){
                        deleteQuizButton.onClick.AddListener(() => {
                            DeleteQuiz(currentQuizID);
                        });
                    }else{
                        Debug.Log("Delete button not found");
                    }
                }
            }

        }catch(Exception e){
            
            Debug.Log(e);
        
        }
    }

    private void AddQuiz(string title){
        List<IMultipartFormSection> formData = new List<IMultipartFormSection>
        {
            new MultipartFormDataSection("uniqueID", PlayerPrefs.GetInt("uid").ToString()),
            new MultipartFormDataSection("title", title),
        };

        StartCoroutine(AddQuiz(formData));
    }

    private IEnumerator AddQuiz(List<IMultipartFormSection> formData){
        UnityWebRequest www = UnityWebRequest.Post("http://172.29.174.196/add-quiz", formData);

        yield return www.SendWebRequest();

        try{
            
            if(www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError){
                string responseText = www.downloadHandler.text;
                Debug.Log($"Response text: {responseText}");
            }else if(www.result == UnityWebRequest.Result.Success){
                string responseText = www.downloadHandler.text;
                Debug.Log($"Success");
                GetQuiz();
            }
        }catch(Exception e){
            
            Debug.Log(e);
        }

    }

    private void DeleteQuiz(int id){
        List<IMultipartFormSection> formData = new List<IMultipartFormSection>
        {
            new MultipartFormDataSection("id", id.ToString()),
        };

        StartCoroutine(DeleteQuiz(formData));
    }

    private IEnumerator DeleteQuiz(List<IMultipartFormSection> formData){
        UnityWebRequest www = UnityWebRequest.Post("http://172.29.174.196/delete-quiz", formData);

        yield return www.SendWebRequest();

        try{
            
            if(www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError){
                string responseText = www.downloadHandler.text;
                Debug.Log($"Response text: {responseText}");
            }else if(www.result == UnityWebRequest.Result.Success){
                string responseText = www.downloadHandler.text;
                Debug.Log($"Success");
                GetQuiz();
            }
        }catch(Exception e){
            
            Debug.Log(e);
        }
    }

    private void UpdateQuiz(string title, int id){
        List<IMultipartFormSection> formData = new List<IMultipartFormSection>
        {
            new MultipartFormDataSection("id", id.ToString()),
            new MultipartFormDataSection("title", title),
        };

        StartCoroutine(UpdateQuiz(formData));
    }

    private IEnumerator UpdateQuiz(List<IMultipartFormSection> formData){
        UnityWebRequest www = UnityWebRequest.Post("http://172.29.174.196/update-quiz", formData);

        yield return www.SendWebRequest();

        try{
            
            if(www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError){
                string responseText = www.downloadHandler.text;
                Debug.Log($"Response text: {responseText}");
            }else if(www.result == UnityWebRequest.Result.Success){
                string responseText = www.downloadHandler.text;
                Debug.Log($"Success");
                GetQuiz();
            }
        }catch(Exception e){
            
            Debug.Log(e);
        }

    }

    private void OpenQuizQuestion(int id){
        _quizId = id;
        List<IMultipartFormSection> formData = new List<IMultipartFormSection>
        {
            new MultipartFormDataSection("quiz_id", id.ToString()),
        };

        StartCoroutine(OpenQuizQuestion(formData));
    }

    private IEnumerator OpenQuizQuestion(List<IMultipartFormSection> formData)
    {
        UnityWebRequest www = UnityWebRequest.Post("http://172.29.174.196/get-quiz-question", formData);

        yield return www.SendWebRequest();

        try
        {
            if (www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError($"Request failed: {www.downloadHandler.text}");
            }
            else if (www.result == UnityWebRequest.Result.Success)
            {
                string responseText = www.downloadHandler.text;
                Debug.Log($"Request succeeded. Response text: {responseText}");

                QuestionResponse questionResponse = JsonUtility.FromJson<QuestionResponse>(responseText);

                GameEventsManager.instance.QuizEvents.SetTitleText(questionResponse.data.title);

                foreach (Transform child in questionParentTransform)
                {
                    Destroy(child.gameObject);
                }

                foreach (var question in questionResponse.data.questions)
                {
                    GameObject questionCard = Instantiate(questionCardPrefab, questionParentTransform);

                    questionCard.GetComponent<QuestionCardID>().Setup(
                        question.id,
                        question.question,
                        question.a,
                        question.b,
                        question.c,
                        question.d,
                        question.correct_answer
                    );

                    questionCard.transform.Find("DeleteQuestion").GetComponent<Button>().onClick.AddListener(() => DeleteQuestion(question.id));
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Exception caught: {e.Message}");
        }
    }

    private void AddQuestion(){
        List<IMultipartFormSection> formData = new List<IMultipartFormSection>
        {
            new MultipartFormDataSection("quiz_id", _quizId.ToString()),
        };

        StartCoroutine(AddQuestion(formData));
    }

    private IEnumerator AddQuestion(List<IMultipartFormSection> formData){
        UnityWebRequest www = UnityWebRequest.Post("http://172.29.174.196/add-question", formData);

        yield return www.SendWebRequest();

        try{
            
            if(www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError){
                string responseText = www.downloadHandler.text;
                Debug.Log($"Response text: {responseText}");
            }else if(www.result == UnityWebRequest.Result.Success){
                string responseText = www.downloadHandler.text;
                Debug.Log($"Success");
                OpenQuizQuestion(_quizId);
            }
        }catch(Exception e){            
            Debug.Log(e);
        }
    }

    private void DeleteQuestion(int id){
        List<IMultipartFormSection> formData = new List<IMultipartFormSection>
        {
            new MultipartFormDataSection("id", id.ToString()),
        };

        StartCoroutine(DeleteQuestion(formData));
    }

    private IEnumerator DeleteQuestion(List<IMultipartFormSection> formData){
        UnityWebRequest www = UnityWebRequest.Post("http://172.29.174.196/delete-question", formData);

        yield return www.SendWebRequest();

        try{
            
            if(www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError){
                string responseText = www.downloadHandler.text;
                Debug.Log($"Response text: {responseText}");
            }else if(www.result == UnityWebRequest.Result.Success){
                string responseText = www.downloadHandler.text;
                Debug.Log($"Success");
                OpenQuizQuestion(_quizId);
            }
        }catch(Exception e){
            
            Debug.Log(e);
        }
    }

    public void UpdateQuestion(int id, string fieldName, string fieldValue)
    {
        List<IMultipartFormSection> formData = new List<IMultipartFormSection>
        {
            new MultipartFormDataSection("id", id.ToString()),
            new MultipartFormDataSection("field", fieldName),
            new MultipartFormDataSection("value", fieldValue),
        };

        StartCoroutine(UpdateQuestion(formData));
    }

    private IEnumerator UpdateQuestion(List<IMultipartFormSection> formData)
    {
        UnityWebRequest www = UnityWebRequest.Post("http://172.29.174.196/update-question", formData);

        yield return www.SendWebRequest();

        try
        {
            if (www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError($"Request failed: {www.downloadHandler.text}");
            }
            else if (www.result == UnityWebRequest.Result.Success)
            {
                Debug.Log($"Update succeeded");
         
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Exception caught: {e.Message}");
        }
    }


    [System.Serializable]
    internal class QuizItem
    {
        public int id;
        public int user_id;
        public string title;
    }

    [System.Serializable]
    internal class QuizResponse
    {
        public List<QuizItem> data;
        public string status;
    }

    [System.Serializable]

    internal class QuizQuestionItem{
        public string title;
        public List<QuestionItem> questions;
    }
    [System.Serializable]
    internal class QuestionItem
    {
        public int id;
        public int quiz_id;
        public string question;
        public string a;
        public string b;
        public string c;
        public string d;
        public string correct_answer;
    }

    [System.Serializable]
    internal class QuestionResponse
    {
        public QuizQuestionItem data;
        public string status;
    }

}
