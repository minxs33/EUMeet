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
    public int _quizId;
    public int _subjectId;
    private Button _selectedSubject;
    private Button _selectedQuiz;
    public List<QuestionItem> questions;
    [SerializeField] private GameObject subjectCardPrefab;
    [SerializeField] private Transform subjectParentTransform;
    [SerializeField] private GameObject quizCardPrefab;
    [SerializeField] private Transform quizParentTransform;

    [SerializeField] private GameObject questionCardPrefab;
    [SerializeField] private Transform questionParentTransform;

    private void OnEnable(){
        GameEventsManager.instance.RTCEvents.OnPlayerJoined += InitSubject;
        GameEventsManager.instance.QuizEvents.OnAddSubject += AddSubject;
        GameEventsManager.instance.QuizEvents.OnUpdateSubject += UpdateSubject;
        GameEventsManager.instance.QuizEvents.OnDeleteSubject += DeleteSubject;
        GameEventsManager.instance.QuizEvents.OnAddQuiz += AddQuiz;
        GameEventsManager.instance.QuizEvents.OnUpdateQuiz += UpdateQuiz;
        GameEventsManager.instance.QuizEvents.OnAddQuestion += AddQuestion;
        GameEventsManager.instance.QuizEvents.OnUpdateQuestion += UpdateQuestion;
    }

    private void OnDisable(){
        GameEventsManager.instance.RTCEvents.OnPlayerJoined -= InitSubject;
        GameEventsManager.instance.QuizEvents.OnAddSubject -= AddSubject;
        GameEventsManager.instance.QuizEvents.OnUpdateSubject -= UpdateSubject;
        GameEventsManager.instance.QuizEvents.OnDeleteSubject -= DeleteSubject;
        GameEventsManager.instance.QuizEvents.OnAddQuiz -= AddQuiz;
        GameEventsManager.instance.QuizEvents.OnUpdateQuiz -= UpdateQuiz;
        GameEventsManager.instance.QuizEvents.OnAddQuestion -= AddQuestion;
        GameEventsManager.instance.QuizEvents.OnUpdateQuestion -= UpdateQuestion;
    }

    // subject
    private void InitSubject()
    {
        // GetQuiz();
        StartCoroutine(GetSubjectCoroutine());
    }

    private IEnumerator GetSubjectCoroutine(){
        UnityWebRequest www = UnityWebRequest.Get("http://172.29.174.196/get-subjects");

        yield return www.SendWebRequest();

        try{
            
            if(www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError){
                string responseText = www.downloadHandler.text;
                Debug.Log($"Response text: {responseText}");
            }else if(www.result == UnityWebRequest.Result.Success){
                string responseText = www.downloadHandler.text;
                Debug.Log($"Subject recieved successfully");

                SubjectResponse subjectResponse = JsonUtility.FromJson<SubjectResponse>(responseText);

                foreach (Transform child in subjectParentTransform)
                {
                    Destroy(child.gameObject);
                }
                
                foreach (SubjectItem subject in subjectResponse.data)
                {
                    int currentSubjectID = subject.id;
                    GameObject subjectCard = Instantiate(subjectCardPrefab, subjectParentTransform);

                    Button selectSubjectButton = subjectCard.transform.Find("SubjectSelect")?.GetComponent<Button>();
                    selectSubjectButton.GetComponent<SubjectCardID>().Setup(subject.subject_name, subject.id);
                    Button deleteSubjectButton = subjectCard.transform.Find("DeleteSubjectButton")?.GetComponent<Button>();

                    if(selectSubjectButton != null){
                        selectSubjectButton.onClick.AddListener(() => {
                            _subjectId = currentSubjectID;
                            GetQuiz(_subjectId);
                            HighlightSelectedSubject(selectSubjectButton);
                            GameEventsManager.instance.QuizEvents.ToggleQuizSelected(false);
                        });
                    }

                    if(deleteSubjectButton != null){
                        deleteSubjectButton.onClick.AddListener(() => {
                            DeleteSubject(currentSubjectID);
                            
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

    private void HighlightSelectedSubject(Button button)
    {
        if (_selectedSubject != null)
        {
            Transform previousBackground = _selectedSubject.transform.Find("Background");
            if (previousBackground != null)
            {
                Image previousImage = previousBackground.GetComponent<Image>();
                if (previousImage != null)
                {
                    previousImage.color = Color.white;
                }
            }
        }

        _selectedSubject = button;

        Transform currentBackground = _selectedSubject.transform.Find("Background");
        if (currentBackground != null)
        {
            Image currentImage = currentBackground.GetComponent<Image>();
            if (currentImage != null)
            {
                currentImage.color = new Color(0.8313726f, 0.6392157f, 0.4509804f);
            }
        }
    }


    private void AddSubject(string subjectName){
        List<IMultipartFormSection> formData = new List<IMultipartFormSection>
        {
            new MultipartFormDataSection("uniqueID", PlayerPrefs.GetInt("uid").ToString()),
            new MultipartFormDataSection("subject_name", subjectName),
        };

        StartCoroutine(AddSubjectCoroutine(formData));
    }

    private IEnumerator AddSubjectCoroutine(List<IMultipartFormSection> formData){
        UnityWebRequest www = UnityWebRequest.Post("http://172.29.174.196/add-subject", formData);

        yield return www.SendWebRequest();

        try{
            
            if(www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError){
                string responseText = www.downloadHandler.text;
                Debug.Log($"Response text: {responseText}");
            }else if(www.result == UnityWebRequest.Result.Success){
                string responseText = www.downloadHandler.text;
                Debug.Log($"Subject added successfully");
                StartCoroutine(GetSubjectCoroutine());
            }
        }catch(Exception e){
            Debug.Log(e);
        }
    }

    private void UpdateSubject(string subjectName, int subjectID){
        List<IMultipartFormSection> formData = new List<IMultipartFormSection>
        {
            new MultipartFormDataSection("id", subjectID.ToString()),
            new MultipartFormDataSection("subject_name", subjectName),
        };

        StartCoroutine(UpdateSubjectCoroutine(formData));
    }

    private IEnumerator UpdateSubjectCoroutine(List<IMultipartFormSection> formData){
        UnityWebRequest www = UnityWebRequest.Post("http://172.29.174.196/update-subject", formData);

        yield return www.SendWebRequest();

        try{
            
            if(www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError){
                string responseText = www.downloadHandler.text;
                Debug.Log($"Response text: {responseText}");
            }else if(www.result == UnityWebRequest.Result.Success){
                string responseText = www.downloadHandler.text;
                Debug.Log($"Subject updated successfully");
                StartCoroutine(GetSubjectCoroutine());
            }
        }catch(Exception e){
            Debug.Log(e);
        }
    }

    private void DeleteSubject(int subjectID){
        List<IMultipartFormSection> formData = new List<IMultipartFormSection>
        {
            new MultipartFormDataSection("id", subjectID.ToString()),
        };

        StartCoroutine(DeleteSubjectCoroutine(formData));
    }

    private IEnumerator DeleteSubjectCoroutine(List<IMultipartFormSection> formData){
        UnityWebRequest www = UnityWebRequest.Post("http://172.29.174.196/delete-subject", formData);

        yield return www.SendWebRequest();

        try{
            
            if(www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError){
                string responseText = www.downloadHandler.text;
                Debug.Log($"Response text: {responseText}");
            }else if(www.result == UnityWebRequest.Result.Success){
                string responseText = www.downloadHandler.text;
                Debug.Log($"Subject deleted successfully");
                StartCoroutine(GetSubjectCoroutine());
                GetQuiz(_subjectId);
            }
        }catch(Exception e){
            Debug.Log(e);
        }
    }

    // Quiz

    private void GetQuiz(int subjectID){
        List<IMultipartFormSection> formData = new List<IMultipartFormSection>
        {
            new MultipartFormDataSection("uniqueID", PlayerPrefs.GetInt("uid").ToString()),
            new MultipartFormDataSection("subject_id", subjectID.ToString()),
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
                foreach (Transform child in quizParentTransform)
                {
                    Destroy(child.gameObject);
                }
            }else if(www.result == UnityWebRequest.Result.Success){
                string responseText = www.downloadHandler.text;
                Debug.Log($"Quiz recieved successfully");

                QuizResponse quizResponse = JsonUtility.FromJson<QuizResponse>(responseText);

                foreach (Transform child in quizParentTransform)
                {
                    Destroy(child.gameObject);
                }
                
                foreach (QuizItem quiz in quizResponse.data)
                {
                    int currentQuizID = quiz.id;
                    GameObject quizCard = Instantiate(quizCardPrefab, quizParentTransform);

                    Button selectQuizButton = quizCard.transform.Find("QuizSelect")?.GetComponent<Button>();
                    selectQuizButton.GetComponent<QuizCardID>().Setup(quiz.title, quiz.id);
                    Button openQuestionButton = quizCard.transform.Find("QuestionButton")?.GetComponent<Button>();
                    Button deleteQuizButton = quizCard.transform.Find("DeleteQuizButton")?.GetComponent<Button>();

                    if(selectQuizButton != null){
                        selectQuizButton.onClick.AddListener(() => {
                            _quizId = currentQuizID;
                            GameEventsManager.instance.QuizEvents.ToggleQuizSelected(true);
                            OpenQuizQuestion(currentQuizID);
                            HighlightSelectedQuiz(selectQuizButton);
                        });  
                    }

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

    private void HighlightSelectedQuiz(Button button)
    {
        if (_selectedQuiz != null)
        {
            Transform previousBackground = _selectedQuiz.transform.Find("Background");
            if (previousBackground != null)
            {
                Image previousImage = previousBackground.GetComponent<Image>();
                if (previousImage != null)
                {
                    previousImage.color = Color.white;
                }
            }
        }

        _selectedQuiz = button;

        Transform currentBackground = _selectedQuiz.transform.Find("Background");
        if (currentBackground != null)
        {
            Image currentImage = currentBackground.GetComponent<Image>();
            if (currentImage != null)
            {
                currentImage.color = new Color(0.8313726f, 0.6392157f, 0.4509804f);
            }
        }
    }

    private void AddQuiz(string title){
        List<IMultipartFormSection> formData = new List<IMultipartFormSection>
        {
            new MultipartFormDataSection("uniqueID", PlayerPrefs.GetInt("uid").ToString()),
            new MultipartFormDataSection("subject_id", _subjectId.ToString()),
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
                GetQuiz(_subjectId);
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
                GetQuiz(_subjectId);
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
                GetQuiz(_subjectId);
            }
        }catch(Exception e){
            
            Debug.Log(e);
        }

    }

    public void OpenQuizQuestion(int id){
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

                questions = questionResponse.data.questions;

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
    public class SubjectItem
    {
        public int id;
        public string subject_name;
    }

    [System.Serializable]
    public class SubjectResponse{
        public List<SubjectItem> data;
        public string status;
    }

    [System.Serializable]
    public class QuizItem
    {
        public int id;
        public int user_id;
        public string title;
    }

    [System.Serializable]
    public class QuizResponse
    {
        public List<QuizItem> data;
        public string status;
    }

    [System.Serializable]

    public class QuizQuestionItem{
        public string title;
        public List<QuestionItem> questions;
    }
    
    [System.Serializable]
    public class QuestionItem
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
    public class QuestionResponse
    {
        public QuizQuestionItem data;
        public string status;
    }

}
