using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityEngine;

public class JsonResponseManager : MonoBehaviour
{
    public static JsonResponseManager Instance { get; private set; }

    private void Awake() {
        if (Instance == null) {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        } else {
            Destroy(gameObject);
        }
    }

    private void OnEnable() {
        GameEventsManager.instance.jsonResponseEvents.onJsonResponse += JsonResponse;
    }

    private void OnDisable() {
        GameEventsManager.instance.jsonResponseEvents.onJsonResponse -= JsonResponse;
    }

    public void JsonResponse(string responseText) {
        try {
            JObject jsonResponse = JObject.Parse(responseText);

            JObject success = jsonResponse["success"] as JObject;
            if (success != null) {
                Debug.Log("Success response:");
                foreach (var data in success) {
                    string field = data.Key;
                    Debug.Log($"{field}: {data.Value}");
                }
            } else {
                string message = jsonResponse["message"]?.ToString();
                JObject errors = jsonResponse["errors"] as JObject;

                Debug.LogError($"Message: {message}");

                if (errors != null) {
                    foreach (var error in errors) {
                        string field = error.Key;
                        JArray fieldErrors = error.Value as JArray;
                        foreach (var fieldError in fieldErrors) {
                            Debug.LogError($"{field}: {fieldError}");
                        }
                    }
                } else {
                    Debug.LogError("Unknown error response format.");
                }
            }
        } catch (System.Exception ex) {
            Debug.LogError($"Failed to parse response: {ex.Message}");
        }
    }

    public void JsonValidationResponse(string responseText) {
        try {
            JObject jsonResponse = JObject.Parse(responseText);
            string message = jsonResponse["message"]?.ToString();
            JObject errors = jsonResponse["errors"] as JObject;

            if (errors != null) {
                foreach (var error in errors) {
                    string field = error.Key;
                    JArray fieldErrors = error.Value as JArray;
                    foreach (var fieldError in fieldErrors) {
                        Debug.LogError($"{field}: {fieldError}");
                    }
                }
            } else {
                Debug.LogError("Unknown error response format.");
            }
        } catch (System.Exception ex) {
            Debug.LogError($"Failed to parse response: {ex.Message}");
        }
    }
    
}
