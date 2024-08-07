using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class AuthUIManager : MonoBehaviour
{
    public TMP_InputField nameInputField;
    public TMP_InputField emailInputField;
    public TMP_InputField passwordInputField;
    public Button registerButton;

    [SerializeField] private GameObject registerErrorPanel;
    public static AuthUIManager Instance { get; private set; }

    private void OnEnable() {
        GameEventsManager.instance.UIEvents.onRegisterError += RegisterError;
        registerButton.onClick.AddListener(()=> GameEventsManager.instance.authEvents.Register(nameInputField.text, emailInputField.text, passwordInputField.text));
    }

    private void OnDisable() {
        GameEventsManager.instance.UIEvents.onRegisterError -= RegisterError;
        registerButton.onClick.RemoveListener(()=> GameEventsManager.instance.authEvents.Register(nameInputField.text, emailInputField.text, passwordInputField.text));
    }

    public void RegisterError(JObject error) {
        registerErrorPanel.SetActive(false);
        var errorText = registerErrorPanel.transform.GetComponentInChildren<TextMeshProUGUI>();
        errorText.text = "";

        if (error.TryGetValue("errors", out JToken errorsToken)) {
            JObject errors = (JObject)errorsToken;
            foreach (var errorPair in errors) {
                JArray errorMessages = (JArray)errorPair.Value;
                for (int i = 0; i < errorMessages.Count; i++) {
                    errorText.text += $"{errorMessages[i]}";
                    
                }
            }
        }

        registerErrorPanel.SetActive(true);
    }

}
