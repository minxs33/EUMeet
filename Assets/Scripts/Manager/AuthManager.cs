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
    public TMP_InputField nameInputField;
    public TMP_InputField emailInputField;
    public TMP_InputField passwordInputField;
    public Button registerButton;
    public static AuthManager Instance { get; private set; }

    private void OnEnable() {
        GameEventsManager.instance.authEvents.onRegister += Register;
        registerButton.onClick.AddListener(() => GameEventsManager.instance.authEvents.Register(nameInputField.text, emailInputField.text, passwordInputField.text));
    }

    private void OnDisable() {
        GameEventsManager.instance.authEvents.onRegister -= Register;
        registerButton.onClick.RemoveListener(() => GameEventsManager.instance.authEvents.Register(nameInputField.text, emailInputField.text, passwordInputField.text));
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
                GameEventsManager.instance.authEvents.RegisterSuccess();
            }
            else
            {
                Debug.LogError($"Unexpected result: {www.result}");
                Application.Quit();
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
