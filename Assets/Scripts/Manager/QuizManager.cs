using System;
using System.Collections;
using System.Collections.Generic;
using ExitGames.Client.Photon.StructWrapping;
using Newtonsoft.Json.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class QuizManager : MonoBehaviour
{
    public static QuizManager Instance { get; private set; }
    [SerializeField] private GameObject quizCardPrefab;
    [SerializeField] private Transform parentTransform;

    private void OnEnable(){
        GameEventsManager.instance.RTCEvents.OnPlayerJoined += StartQuiz;
        GameEventsManager.instance.QuizEvents.OnAddQuiz += AddQuiz;
        GameEventsManager.instance.QuizEvents.OnDeleteQuiz += DeleteQuiz;
        GameEventsManager.instance.QuizEvents.OnUpdateQuiz += UpdateQuiz;
    }

    private void OnDisable(){
        GameEventsManager.instance.RTCEvents.OnPlayerJoined -= StartQuiz;
        GameEventsManager.instance.QuizEvents.OnAddQuiz -= AddQuiz;
        GameEventsManager.instance.QuizEvents.OnDeleteQuiz -= DeleteQuiz;
        GameEventsManager.instance.QuizEvents.OnUpdateQuiz -= UpdateQuiz;
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
                            GameEventsManager.instance.QuizEvents.OpenQuestion(quiz.id);
                        });
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
                Debug.Log($"Success: {www.downloadHandler.text}");
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
                Debug.Log($"Success: {www.downloadHandler.text}");
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
                Debug.Log($"Success: {www.downloadHandler.text}");
                GetQuiz();
            }
        }catch(Exception e){
            
            Debug.Log(e);
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

}
