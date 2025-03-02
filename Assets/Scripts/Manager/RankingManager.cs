using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using static QuizManager;

public class RankingManager : MonoBehaviour
{
    [SerializeField] private GameObject parentRankingGo;
    [SerializeField] private GameObject rankingPrefab;
    [SerializeField] private TMP_Dropdown subjectDropdown;
    private int currentSubjectId;
    private Dictionary<int, int> subjectIdMapping = new Dictionary<int, int>();

    private void OnEnable() {
        GameEventsManager.instance.QuizEvents.OnRankingModalOpen += GetSubjects;
        GameEventsManager.instance.QuizEvents.OnResetRanking += ResetRanking;
    }

    private void OnDisable() {
        GameEventsManager.instance.QuizEvents.OnRankingModalOpen -= GetSubjects;
        GameEventsManager.instance.QuizEvents.OnResetRanking -= ResetRanking;
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

                subjectDropdown.AddOptions(options);
                subjectDropdown.onValueChanged.AddListener((index)=>{
                    if (subjectIdMapping.TryGetValue(index, out int subjectId))
                    {
                        if (index == 0)
                        {
                            Debug.LogWarning("Please select a valid option.");
                            return;
                        }
                        GetRanking(subjectId);
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

    private void GetRanking(int subjectId) {
        List<IMultipartFormSection> formData = new List<IMultipartFormSection>
        {
            new MultipartFormDataSection("subject_id", subjectId.ToString()),
        };

        StartCoroutine(GetRankingCoroutine(formData));
    }

    private IEnumerator GetRankingCoroutine(List<IMultipartFormSection> formData) {
        
        UnityWebRequest www = UnityWebRequest.Post("http://172.29.174.196/get-ranking", formData);
        yield return www.SendWebRequest();

        try{
            
            if(www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError){
                string responseText = www.downloadHandler.text;
                Debug.Log($"Response text: {responseText}");
            }else if(www.result == UnityWebRequest.Result.Success){
                string responseText = www.downloadHandler.text;
                // Debug.Log($"Ranking fetched successfully on subject ID:" + subjectIdMapping[subjectDropdown.value]+ " - "+ responseText);

                RankingResponseWrapper rankingData = JsonUtility.FromJson<RankingResponseWrapper>(responseText);

                foreach (Transform child in parentRankingGo.transform)
                {
                    Destroy(child.gameObject);
                }

                Debug.Log(rankingData.data.Count);

                var i = 0;
                foreach(RankingItem ranking in rankingData.data)
                {
                    GameObject go = Instantiate(rankingPrefab, parentRankingGo.transform);
                    TMP_Text rankingText = go.transform.Find("rankingText").GetComponentInChildren<TMP_Text>();
                    TMP_Text usernameText = go.transform.Find("usernameText").GetComponentInChildren<TMP_Text>();
                    TMP_Text scoreText = go.transform.Find("scoreText").GetComponentInChildren<TMP_Text>();

                    Outline outline = go.transform.Find("Background").GetComponent<Outline>();
                    outline.enabled = true;
                    switch (i){
                        case 0:
                            outline.effectColor = new Color(0.1798683f,0.8867924f,0.8373993f);
                            break;
                        case 1:
                            outline.effectColor = new Color(0.8773585f,0.8728968f,0.004138493f);
                            break;
                        case 2:
                            outline.effectColor = new Color(0.8392157f,0.5568628f,0.4117647f);
                            break;
                        default:
                            outline.enabled = false;
                            break;
                    }
                    rankingText.text = (i + 1).ToString();
                    usernameText.text = ranking.name;
                    scoreText.text = ranking.points.ToString();
                    i++;
                }
                
            }
        }catch(Exception e){
            Debug.Log(e);
        }
    }

    private void ResetRanking() {
        List<IMultipartFormSection> formData = new List<IMultipartFormSection>(){
            new MultipartFormDataSection("subject_id", currentSubjectId.ToString()),
        };
        StartCoroutine(ResetRankingCoroutine(formData));
    }

    private IEnumerator ResetRankingCoroutine(List<IMultipartFormSection> formData) {
        UnityWebRequest www = UnityWebRequest.Post("http://172.29.174.196/reset-ranking", formData);
        yield return www.SendWebRequest();

        try{
            if(www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError){
                string responseText = www.downloadHandler.text;
                Debug.Log($"Response text: {responseText}");
            }else if(www.result == UnityWebRequest.Result.Success){
                string responseText = www.downloadHandler.text;
                Debug.Log($"Ranking reset successfully");
                GetRanking(currentSubjectId);
            }
        }catch(Exception e){
            Debug.Log(e);
        }
    }
    
    [System.Serializable]
    public class RankingResponseWrapper
    {
        public List<RankingItem> data;
        public string status;
    }

    [System.Serializable]
    public class RankingItem
    {
        public string name;
        public int points;
    }
}
