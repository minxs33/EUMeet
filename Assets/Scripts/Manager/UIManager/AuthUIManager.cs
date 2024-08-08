using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class AuthUIManager : MonoBehaviour
{
    [Header("Menu UI")]
    [SerializeField] private GameObject menuPanel;
    [SerializeField] private Button registerPageButton;
    [SerializeField] private GameObject registerPanel;
    [SerializeField] private Button loginPageButton;
    [SerializeField] private GameObject loginPanel;
    [SerializeField] private Button exit;
    
    [Header("Back to menu UI")]
    [SerializeField] private Button backToMenu;
    
    [Header("Register Form UI")]
    public TMP_InputField nameInputField;
    public TMP_InputField emailInputField;
    public TMP_InputField passwordInputField;
    [SerializeField] private Button registerButton;
    [SerializeField] private GameObject registerErrorPanel;


    public static AuthUIManager Instance { get; private set; }

    private void OnEnable() {
        // Menu UI
        registerPageButton.onClick.AddListener(OpenRegisterPage);
        loginPageButton.onClick.AddListener(OpenLoginPage);
        backToMenu.onClick.AddListener(OpenMenuPage);

        // Register UI
        GameEventsManager.instance.UIEvents.onRegisterError += RegisterError;
        registerButton.onClick.AddListener(()=> GameEventsManager.instance.authEvents.Register(nameInputField.text, emailInputField.text, passwordInputField.text));
    }

    private void OnDisable() {
        // Menu UI
        registerPageButton.onClick.RemoveListener(OpenRegisterPage);
        loginPageButton.onClick.RemoveListener(OpenLoginPage);
        backToMenu.onClick.RemoveListener(OpenMenuPage);

        // Register UI
        GameEventsManager.instance.UIEvents.onRegisterError -= RegisterError;
        registerButton.onClick.RemoveListener(()=> GameEventsManager.instance.authEvents.Register(nameInputField.text, emailInputField.text, passwordInputField.text));
    }
    private void OpenLoginPage(){
        StartCoroutine(FadeCanvasGroups(menuPanel.GetComponent<CanvasGroup>(), loginPanel.GetComponent<CanvasGroup>()));
    }
    private void OpenRegisterPage(){
        StartCoroutine(FadeCanvasGroups(menuPanel.GetComponent<CanvasGroup>(), registerPanel.GetComponent<CanvasGroup>()));
    }
    private void OpenMenuPage(){
        StartCoroutine(BackToMenu());
    }

    private IEnumerator FadeCanvasGroups(CanvasGroup fadeOutCanvas, CanvasGroup fadeInCanvas)
    {
        float duration = 0.1f;
        float time = 0;

        fadeInCanvas.gameObject.SetActive(true);
        fadeInCanvas.alpha = 0;

        backToMenu.gameObject.SetActive(true);
        CanvasGroup backToMenuCanvasGroup = backToMenu.GetComponent<CanvasGroup>();
        backToMenuCanvasGroup.alpha = 0;

        while (time < duration)
        {
            time += Time.deltaTime;
            float alpha = Mathf.Lerp(1, 0, time / duration);
            fadeOutCanvas.alpha = alpha;
            fadeInCanvas.alpha = 1 - alpha;
            backToMenuCanvasGroup.alpha = fadeInCanvas.alpha;
            yield return null;
        }

        fadeOutCanvas.alpha = 0;
        fadeInCanvas.alpha = 1;
        backToMenuCanvasGroup.alpha = fadeInCanvas.alpha;

        fadeOutCanvas.gameObject.SetActive(false);
    }

    private IEnumerator BackToMenu()
    {
        CanvasGroup regCanvas = registerPanel.GetComponent<CanvasGroup>();
        CanvasGroup logCanvas = loginPanel.GetComponent<CanvasGroup>();
        CanvasGroup menuCanvas = menuPanel.GetComponent<CanvasGroup>();
        CanvasGroup backToMenuCanvasGroup = backToMenu.GetComponent<CanvasGroup>();

        float duration = 0.1f;
        float time = 0;

        menuCanvas.gameObject.SetActive(true);
        menuCanvas.alpha = 0;

        while (time < duration)
        {
            time += Time.deltaTime;
            float alpha = Mathf.Lerp(1, 0, time / duration);
            regCanvas.alpha = alpha;
            logCanvas.alpha = alpha;
            backToMenuCanvasGroup.alpha = alpha;
            menuCanvas.alpha = 1 - alpha;
            yield return null;
        }

        regCanvas.alpha = 0;
        logCanvas.alpha = 0;
        backToMenuCanvasGroup.alpha = 0;
        menuCanvas.alpha = 1;

        backToMenu.gameObject.SetActive(false);
        registerPanel.SetActive(false);
        loginPanel.SetActive(false);
    }

    private void RegisterError(JObject error) {
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
