using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

public class AuthManager : MonoBehaviour
{
    public static AuthManager Instance { get; private set; }

    private void OnEnable() {
        GameEventsManager.instance.authEvents.onRegister += Register;
        GameEventsManager.instance.authEvents.onLogin += Login;
    }

    private void OnDisable() {
        GameEventsManager.instance.authEvents.onRegister -= Register;
        GameEventsManager.instance.authEvents.onLogin -= Login;
    }
    
    public void Register(string name, string email, string password) {
        List<IMultipartFormSection> formData = new List<IMultipartFormSection>
        {
            new MultipartFormDataSection("name", name),
            new MultipartFormDataSection("email", email),
            new MultipartFormDataSection("password", password)
        };
        
        StartCoroutine(RegisterRoutine(formData));
    }

    public IEnumerator RegisterRoutine(List<IMultipartFormSection> formData)
    {
        UnityWebRequest www = UnityWebRequest.Post("http://172.29.174.196/register", formData);

        yield return www.SendWebRequest();

        try
        {
            if (www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError)
            {
                // Debug.Log($"Network or protocol error: {www.error}");
                string responseText = www.downloadHandler.text;
                Debug.Log($"Response text: {responseText}");
                
                try
                {
                    JObject error = JObject.Parse(responseText);
                    Debug.Log($"{error}");
                    GameEventsManager.instance.UIEvents.RegisterError(error);
                }
                catch (JsonReaderException ex)
                {
                    Debug.LogError($"Failed to parse response text as JSON: {ex.Message}\nResponse text: {responseText}");
                }
                
            }
            else if (www.result == UnityWebRequest.Result.Success)
            {
                Debug.Log($"Success: {www.downloadHandler.text}");
                var response = JObject.Parse(www.downloadHandler.text);
                GameEventsManager.instance.authEvents.Authenticate(response);
                GameEventsManager.instance.UIEvents.ShowGenderPanel();
            }
            else
            {
                Debug.LogError($"Unexpected result: {www.result}");
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Exception caught: {ex.Message}\n{ex.StackTrace}");
        }
        finally
        {
            www.Dispose();
        }
    }
    
    public void Login(string email, string password)
    {
        List<IMultipartFormSection> formData = new List<IMultipartFormSection>
        {
            new MultipartFormDataSection("email", email),
            new MultipartFormDataSection("password", password)
        };
        
        StartCoroutine(LoginRoutine(formData));
    }

    public IEnumerator LoginRoutine(List<IMultipartFormSection> formData)
    {
        UnityWebRequest www = UnityWebRequest.Post("http://172.29.174.196/login", formData);

        yield return www.SendWebRequest();

        try
        {
            if (www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError)
            {
                string responseText = www.downloadHandler.text;
                Debug.Log($"Response text: {responseText}");

                try
                {
                    JObject error = JObject.Parse(responseText);
                    Debug.Log($"{error}");
                    GameEventsManager.instance.UIEvents.LoginError(error);
                }
                catch (JsonReaderException ex)
                {
                    Debug.LogError($"Failed to parse response text as JSON: {ex.Message}\nResponse text: {responseText}");
                }
            }
            else if (www.result == UnityWebRequest.Result.Success)
            {
                Debug.Log($"Success: {www.downloadHandler.text}");
                var response = JObject.Parse(www.downloadHandler.text);
                GameEventsManager.instance.authEvents.Authenticate(response);
                GameEventsManager.instance.UIEvents.ShowGenderPanel();
            }
            else
            {
                Debug.LogError($"Unexpected result: {www.result}");
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Exception caught: {ex.Message}\n{ex.StackTrace}");
        }
        finally
        {
            www.Dispose();
        }
    }

}
