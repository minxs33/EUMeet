using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using Unity.VisualScripting;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;

public class AuthUIManager : MonoBehaviour
{
    [SerializeField] 
    private GameObject registerErrorPanel;
    public static AuthUIManager Instance { get; private set; }

    private void OnEnable() {
        GameEventsManager.instance.UIEvents.onRegisterError += RegisterError;
    }

    private void OnDisable() {
        GameEventsManager.instance.UIEvents.onRegisterError -= RegisterError;
    }

    public void RegisterError(JObject error) {
        registerErrorPanel.SetActive(false);
        var errorText = registerErrorPanel.transform.GetComponentInChildren<TMPro.TextMeshProUGUI>();
        errorText.text = "";

        if (error.TryGetValue("errors", out JToken errorsToken)) {
            JObject errors = (JObject)errorsToken;
            foreach (var errorPair in errors) {
                JArray errorMessages = (JArray)errorPair.Value;
                for (int i = 0; i < errorMessages.Count; i++) {
                    if (i > 0) {
                        errorText.text += $", {errorMessages[i]}";
                    } else {
                        errorText.text += $"{errorMessages[i]}";
                    }
                }
            }
        }

        registerErrorPanel.SetActive(true);
    }

}
