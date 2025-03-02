using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using Newtonsoft.Json;
using static QuizManager;
using System.Globalization;

public class ForumManager : MonoBehaviour
{
    public static ForumManager Instance { get; private set; }
    private Dictionary<int, int> subjectIdMapping = new Dictionary<int, int>();
    private int currentSubjectId;
    private int currentForumPostId;
    [SerializeField] private TMP_Dropdown subjectDropdown;
    [SerializeField] private GameObject parentForumPostGo;
    [SerializeField] private GameObject forumPostGo;
    [SerializeField] private TMP_Dropdown subjectAddDropdown;
    [SerializeField] private GameObject parentForumGo;
    [SerializeField] private GameObject forumGo;
    [SerializeField] private GameObject forumReplyGo;

    private void OnEnable()
    {
        GameEventsManager.instance.forumEvents.onAddForumPost += AddForumPost;
        GameEventsManager.instance.forumEvents.onGetSubject += GetSubjects;
    }

    private void OnDisable()
    {
        GameEventsManager.instance.forumEvents.onAddForumPost -= AddForumPost;
        GameEventsManager.instance.forumEvents.onGetSubject -= GetSubjects;
    }

    private void GetSubjects() {
        StartCoroutine(GetSubjectsCoroutine());
    }

    private IEnumerator GetSubjectsCoroutine() {
        UnityWebRequest www = UnityWebRequest.Get("http://172.29.174.196/get-subjects");
        yield return www.SendWebRequest();

        try{
            if(www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError){
                string responseText = www.downloadHandler.text;
                Debug.Log($"Response text: {responseText}");
            }else if(www.result == UnityWebRequest.Result.Success){
                // SubjectResponse dan SubjectItem ada di QuizManager.cs
                String responseText = www.downloadHandler.text;
                SubjectResponse subjectResponse = JsonUtility.FromJson<SubjectResponse>(responseText);

                subjectDropdown.ClearOptions();
                subjectAddDropdown.ClearOptions();

                List<string> options = new List<string>();
                subjectIdMapping.Clear();
                int index = 1;
                options.Add("Pilih Mata Kuliah...");
                foreach (SubjectItem subject in subjectResponse.data)
                {
                    options.Add(subject.subject_name);
                    subjectIdMapping[index] = subject.id;
                    index++;
                }

                subjectAddDropdown.AddOptions(options);
                subjectDropdown.AddOptions(options);
                subjectDropdown.onValueChanged.AddListener((index)=>{
                    if (subjectIdMapping.TryGetValue(index, out int subjectId))
                    {
                        if (index == 0)
                        {
                            Debug.LogWarning("Please select a valid option.");
                            return;
                        }
                        GetForumPostList(subjectId);
                        currentSubjectId = subjectId;
                    }else{
                        Debug.LogWarning("No ID found for the selected option.");
                    }
                });
            }
        }catch(Exception e){
            Debug.Log(e);
        }
    }

    private void GetForumPostList(int subjectId) {
        List<IMultipartFormSection> formData = new List<IMultipartFormSection>
        {
            new MultipartFormDataSection("subject_id", subjectId.ToString())
        };
        StartCoroutine(GetForumPostListCoroutine(formData));
    }

    private IEnumerator GetForumPostListCoroutine(List<IMultipartFormSection> formData){
        UnityWebRequest www = UnityWebRequest.Post("http://172.29.174.196/get-forum-post-list", formData);
        yield return www.SendWebRequest();

        try{
            if(www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError){
                string responseText = www.downloadHandler.text;
                Debug.Log($"Response text: {responseText}");
            }else if(www.result == UnityWebRequest.Result.Success){
                string responseText = www.downloadHandler.text;
                ForumPostResponseWrapper forumPostResponseWrapper = JsonUtility.FromJson<ForumPostResponseWrapper>(responseText);
                foreach (Transform child in parentForumPostGo.transform)
                {
                    Destroy(child.gameObject);
                }

                foreach (ForumPostListItem forumPostListItem in forumPostResponseWrapper.data)
                {
                    GameObject forumPostCard = Instantiate(forumPostGo, parentForumPostGo.transform);
                    Button forumPostListButton = forumPostCard.transform.Find("ForumPostListButton").GetComponent<Button>();
                    forumPostListButton.onClick.AddListener(() => {
                        currentForumPostId = forumPostListItem.id;
                        Debug.Log($"Forum post ID: {currentForumPostId}");
                        GetForum(currentForumPostId);
                    });
                    forumPostCard.transform.Find("ForumPostListButton/TitleText").GetComponent<TMP_Text>().text = forumPostListItem.title;
                }
                Debug.Log($"Success");
            }
        }catch(Exception e){
            Debug.Log(e);
        }
    }

    private void AddForumPost(int subject, string title, string content){
        if (!subjectIdMapping.TryGetValue(subject, out int subjectId))
        {
            Debug.LogError($"Subject index {subject} not found in mapping.");
            return;
        }
        List<IMultipartFormSection> formData = new List<IMultipartFormSection>
        {
            new MultipartFormDataSection("uniqueID", PlayerPrefs.GetInt("uid").ToString()),
            new MultipartFormDataSection("subject_id", subjectId.ToString()),
            new MultipartFormDataSection("title", title),
            new MultipartFormDataSection("content", content)
        };

        StartCoroutine(AddForumPostCoroutine(formData));
        Debug.Log("Sending forum post...");
    }

    private IEnumerator AddForumPostCoroutine(List<IMultipartFormSection> formData){
        UnityWebRequest www = UnityWebRequest.Post("http://172.29.174.196/add-forum-post", formData);

        yield return www.SendWebRequest();

        try{
            
            if(www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError){
                string responseText = www.downloadHandler.text;
                Debug.Log($"Response text: {responseText}");
            }else if(www.result == UnityWebRequest.Result.Success){
                string responseText = www.downloadHandler.text;
                Debug.Log($"Success");
                GetForumPostList(currentSubjectId);
                GameEventsManager.instance.forumEvents.AddForumPostSuccess();
            }
        }catch(Exception e){            
            Debug.Log(e);
        }
    }

    private void GetForum(int forumPostId) {
        List<IMultipartFormSection> formData = new List<IMultipartFormSection>
        {
            new MultipartFormDataSection("post_id", forumPostId.ToString())
        };
        StartCoroutine(GetForumCoroutine(formData));
    }

    public IEnumerator GetForumCoroutine(List<IMultipartFormSection> formData)
    {
        UnityWebRequest www = UnityWebRequest.Post("http://172.29.174.196/get-forum", formData);
        yield return www.SendWebRequest();

        try
        {
            if (www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.Log($"Error: {www.downloadHandler.text}");
            }
            else if (www.result == UnityWebRequest.Result.Success)
            {
                string responseText = www.downloadHandler.text;
                Debug.Log(responseText);

                ForumResponse forumResponse = JsonConvert.DeserializeObject<ForumResponse>(responseText);

                if (forumResponse != null && forumResponse.status == "success" && forumResponse.data != null)
                {
                    foreach (Transform child in parentForumGo.transform)
                    {
                        Destroy(child.gameObject);
                    }

                    GameObject forumPostGo = Instantiate(forumGo, parentForumGo.transform);
                    string formattedDate = FormatDate(forumResponse.data.created_at);

                    forumPostGo.transform.Find("TopWrapper/ForumPostAuthor").GetComponent<TMP_Text>().text = $"{forumResponse.data.user.name} - {formattedDate}";
                    forumPostGo.transform.Find("ForumPostTitle").GetComponent<TMP_Text>().text = forumResponse.data.title;
                    forumPostGo.transform.Find("ForumPostContent").GetComponent<TMP_Text>().text = forumResponse.data.content;

                    Button toggleReplyButton = forumPostGo.transform.Find("Reply/ToggleReplyButton").GetComponent<Button>();
                    toggleReplyButton.onClick.AddListener(() => {
                        GameObject reply = forumPostGo.transform.Find("Reply").gameObject;
                        GameObject replyInput = forumPostGo.transform.Find("ReplyInput").gameObject;
                        reply.SetActive(false);
                        replyInput.SetActive(true);
                        LayoutRebuilder.ForceRebuildLayoutImmediate(parentForumGo.transform as RectTransform);
                    });

                    Button submitReplyButton = forumPostGo.transform.Find("ReplyInput/ReplyButtonWrapper/SubmitReplyButton").GetComponent<Button>();
                    submitReplyButton.onClick.AddListener(() => {
                        GameObject replyInputField = forumPostGo.transform.Find("ReplyInput/ReplyInputField").gameObject;
                        AddForumPostReply(PlayerPrefs.GetInt("uid"),forumResponse.data.id, replyInputField.GetComponent<TMP_InputField>().text);
                    });

                    if(forumResponse.data.user.uniqueID == PlayerPrefs.GetInt("uid")){    
                        Button deleteButton = forumPostGo.transform.Find("TopWrapper/DeleteWrapper/DeleteButton").GetComponent<Button>();
                        deleteButton.gameObject.SetActive(true);
                        deleteButton.onClick.AddListener(() => {
                            DeleteForumPost(forumResponse.data.id);
                        });
                    }
                    
                    LayoutRebuilder.ForceRebuildLayoutImmediate(parentForumGo.transform as RectTransform);

                    foreach (var reply in forumResponse.data.replies)
                    {
                        InstantiateReply(reply, forumPostGo.transform, 1);
                    }
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Exception: {e.Message}");
        }
    }

    private void InstantiateReply(ForumReplyItem reply, Transform parent, int depth)
    {
        GameObject replyGo = Instantiate(forumReplyGo, parent);
        string formattedDate = FormatDate(reply.created_at);
        replyGo.transform.Find("TopWrapper/ForumPostAuthor").GetComponent<TMP_Text>().text = $"{reply.user.name} - {formattedDate}";
        replyGo.transform.Find("ForumPostContent").GetComponent<TMP_Text>().text = reply.content;

        Button toggleReplyButton = replyGo.transform.Find("Reply/ToggleReplyButton").GetComponent<Button>();
        toggleReplyButton.onClick.AddListener(() => {
            GameObject reply = replyGo.transform.Find("Reply").gameObject;
            GameObject replyInput = replyGo.transform.Find("ReplyInput").gameObject;
            reply.SetActive(false);
            replyInput.SetActive(true);
            LayoutRebuilder.ForceRebuildLayoutImmediate(parentForumGo.transform as RectTransform);
        });

        Button submitReplyButton = replyGo.transform.Find("ReplyInput/ReplyButtonWrapper/SubmitReplyButton").GetComponent<Button>();
        submitReplyButton.onClick.AddListener(() => {
            GameObject replyInputField = replyGo.transform.Find("ReplyInput/ReplyInputField").gameObject;
            AddForumReply(PlayerPrefs.GetInt("uid"), reply.id, replyInputField.GetComponent<TMP_InputField>().text);
        });

        if(reply.user.uniqueID == PlayerPrefs.GetInt("uid")){    
            Button deleteButton = replyGo.transform.Find("TopWrapper/DeleteWrapper/DeleteButton").GetComponent<Button>();
            deleteButton.gameObject.SetActive(true);
            deleteButton.onClick.AddListener(() => {
                DeleteForumReply(reply.id);
            });
        }

        LayoutRebuilder.ForceRebuildLayoutImmediate(parent as RectTransform);
        foreach (var child in reply.child_replies)
        {
            InstantiateReply(child, replyGo.transform, depth + 1);
        }
    }

    private void AddForumPostReply(int uniqueID, int post_id, string content){
        List<IMultipartFormSection> formData = new List<IMultipartFormSection>
        {
            new MultipartFormDataSection("uniqueID", uniqueID.ToString()),
            new MultipartFormDataSection("post_id", post_id.ToString()),
            new MultipartFormDataSection("content", content)
        };

        StartCoroutine(AddPostReplyCoroutine(formData));
    }

    private IEnumerator AddPostReplyCoroutine(List<IMultipartFormSection> formData){
        UnityWebRequest www = UnityWebRequest.Post("http://172.29.174.196/add-forum-post-reply", formData);

        yield return www.SendWebRequest();

        try{
            if(www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError){
                string responseText = www.downloadHandler.text;
                Debug.Log($"Response text: {responseText}");
            }else if(www.result == UnityWebRequest.Result.Success){
                string responseText = www.downloadHandler.text;
                Debug.Log($"Success");
                GetForum(currentForumPostId);
            }
        }catch(Exception e){
            Debug.Log(e);
        }
    }

    private void AddForumReply(int uniqueID, int reply_id, string content){
        List<IMultipartFormSection> formData = new List<IMultipartFormSection>
        {
            new MultipartFormDataSection("uniqueID", uniqueID.ToString()),
            new MultipartFormDataSection("reply_id", reply_id.ToString()),
            new MultipartFormDataSection("content", content)
        };

        StartCoroutine(AddReplyCoroutine(formData));
    }

    private IEnumerator AddReplyCoroutine(List<IMultipartFormSection> formData){
        UnityWebRequest www = UnityWebRequest.Post("http://172.29.174.196/add-forum-reply", formData);

        yield return www.SendWebRequest();

        try{
            if(www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError){
                string responseText = www.downloadHandler.text;
                Debug.Log($"Response text: {responseText}");
            }else if(www.result == UnityWebRequest.Result.Success){
                string responseText = www.downloadHandler.text;
                Debug.Log($"Success");
                GetForum(currentForumPostId);
            }
        }catch(Exception e){
            Debug.Log(e);
        }
    }

    private void DeleteForumPost(int id){
        List<IMultipartFormSection> formData = new List<IMultipartFormSection>
        {
            new MultipartFormDataSection("id", id.ToString())
        };

        StartCoroutine(DeleteForumPostCoroutine(formData));
    }

    private IEnumerator DeleteForumPostCoroutine(List<IMultipartFormSection> formData){
        UnityWebRequest www = UnityWebRequest.Post("http://172.29.174.196/delete-forum-post", formData);

        yield return www.SendWebRequest();

        try{
            if(www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError){
                string responseText = www.downloadHandler.text;
                Debug.Log($"Response text: {responseText}");
            }else if(www.result == UnityWebRequest.Result.Success){
                string responseText = www.downloadHandler.text;
                Debug.Log($"Success");
                GetForumPostList(currentSubjectId);
                foreach (Transform child in parentForumGo.transform)
                {
                    Destroy(child.gameObject);
                }
            }
        }catch(Exception e){
            Debug.Log(e);
        }
    }

    private void DeleteForumReply(int id){
        List<IMultipartFormSection> formData = new List<IMultipartFormSection>
        {
            new MultipartFormDataSection("id", id.ToString())
        };

        StartCoroutine(DeleteForumReplyCoroutine(formData));
    }

    private IEnumerator DeleteForumReplyCoroutine(List<IMultipartFormSection> formData){
        UnityWebRequest www = UnityWebRequest.Post("http://172.29.174.196/delete-forum-reply", formData);

        yield return www.SendWebRequest();

        try{
            if(www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError){
                string responseText = www.downloadHandler.text;
                Debug.Log($"Response text: {responseText}");
            }else if(www.result == UnityWebRequest.Result.Success){
                string responseText = www.downloadHandler.text;
                Debug.Log($"Success");
                GetForum(currentForumPostId);
            }
        }catch(Exception e){
            Debug.Log(e);
        }
    }

    private string FormatDate(string rawDate)
    {
        if (!string.IsNullOrEmpty(rawDate))
        {
            DateTime dateTime;
            if (DateTime.TryParse(rawDate, null, DateTimeStyles.RoundtripKind, out dateTime))
            {
                TimeSpan timeSince = DateTime.UtcNow - dateTime;

                if (timeSince.TotalSeconds < 60)
                    return $"{(int)timeSince.TotalSeconds} detik yang lalu";
                if (timeSince.TotalMinutes < 60)
                    return $"{(int)timeSince.TotalMinutes} menit yang lalu";
                if (timeSince.TotalHours < 24)
                    return $"{(int)timeSince.TotalHours} jam yang lalu";
                if (timeSince.TotalDays < 7)
                    return $"{(int)timeSince.TotalDays} hari yang lalu";
                if (timeSince.TotalDays < 30)
                    return $"{(int)(timeSince.TotalDays / 7)} minggu yang lalu";
                if (timeSince.TotalDays < 365)
                    return $"{(int)(timeSince.TotalDays / 30)} bulan yang lalu";
                
                return $"{(int)(timeSince.TotalDays / 365)} tahun yang lalu";
            }
        }
        return "";
    }


    [System.Serializable]
    public class ForumPostResponseWrapper
    {
        public List<ForumPostListItem> data;
        public string status;
    }

    [System.Serializable]
    public class ForumPostListItem
    {
        public int id;
        public string title;
    }

    [System.Serializable]
    public class ForumPostItem
    {
        public int id;
        public int user_id;
        public int subject_id;
        public string title;
        public string content;
        public string created_at;
        public string updated_at;
        public User user;
        public List<ForumReplyItem> replies;
    }

    [System.Serializable]
    public class ForumReplyItem
    {
        public int id;
        public int user_id;
        public int? post_id;
        public int? parent_reply_id;
        public string content;
        public string created_at;
        public string updated_at;
        public List<ForumReplyItem> child_replies;
        public User user;
    }

    [System.Serializable]
    public class User
    {
        public int id;
        public int uniqueID;
        public string name;
    }

    [System.Serializable]
    public class ForumResponse
    {
        public ForumPostItem data;
        public string status;
    }
}
