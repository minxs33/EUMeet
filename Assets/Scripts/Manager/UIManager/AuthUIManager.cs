using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using POpusCodec.Enums;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class AuthUIManager : MonoBehaviour
{
    [Header("Menu UI")]
    [SerializeField] private GameObject menuPanel;
    [SerializeField] private GameObject authPanel;
    [SerializeField] private Button registerPageButton;
    [SerializeField] private GameObject registerPanel;
    [SerializeField] private Button startButton;
    [SerializeField] private GameObject loginPanel;
    [SerializeField] private Button exit;
    
    [Header("Back to menu UI")]
    [SerializeField] private Button[] backToMenuButton;
    
    [Header("Register Form UI")]
    public TMP_InputField nameInputField;
    public TMP_InputField emailInputField;
    public TMP_InputField passwordInputField;
    [SerializeField] private Button registerButton;
    [SerializeField] private GameObject registerErrorPanel;
    [SerializeField] private Button[] backToLogin;

    [Header("Login Form UI")]
    public TMP_InputField loginEmailInputField;
    public TMP_InputField loginPasswordInputField;
    [SerializeField] private Button loginButton;
    [SerializeField] private GameObject loginErrorPanel;

    [Header("Gender UI")]
    [SerializeField] private Button maleButton;
    [SerializeField] private Button femaleButton;
    [SerializeField] private Button playButton;
    [SerializeField] private GameObject genderPanel;

    [Header("Auth Debug")]
    [SerializeField] private Button Player1;
    [SerializeField] private Button Player2;



    public static AuthUIManager Instance { get; private set; }

    private void OnEnable() {
        // Menu UI
        registerPageButton.onClick.AddListener(OpenRegisterPage);
        foreach (Button button in backToMenuButton) button.onClick.AddListener(OpenMenuPage);
        startButton.onClick.AddListener(OpenAuthPanel);

        // Register UI
        GameEventsManager.instance.UIEvents.onRegisterError += RegisterError;
        registerButton.onClick.AddListener(()=> GameEventsManager.instance.authEvents.Register(nameInputField.text, emailInputField.text, passwordInputField.text));
        foreach (Button button in backToLogin) button.onClick.AddListener(OpenLoginPanel);

        // Login UI
        GameEventsManager.instance.UIEvents.onLoginError += LoginError;
        loginButton.onClick.AddListener(()=> GameEventsManager.instance.authEvents.Login(loginEmailInputField.text, loginPasswordInputField.text));

        // Auth Debug
        Player1.onClick.AddListener(()=> GameEventsManager.instance.authEvents.Login("testing1@esaunggul.ac.id", "testing123"));
        Player2.onClick.AddListener(()=> GameEventsManager.instance.authEvents.Login("testing2@esaunggul.ac.id", "testing123"));

        // Gender UI
        maleButton.onClick.AddListener(()=>TogglePlayButton("male"));
        femaleButton.onClick.AddListener(()=>TogglePlayButton("female"));
        playButton.onClick.AddListener(StartGame);
        GameEventsManager.instance.UIEvents.onShowGenderPanel += ShowGenderPanel;

    }

    private void OnDisable() {
        // Menu UI
        registerPageButton.onClick.RemoveListener(OpenRegisterPage);
        foreach (Button button in backToMenuButton) button.onClick.RemoveListener(OpenMenuPage);
        startButton.onClick.RemoveListener(OpenAuthPanel);

        // Register UI
        GameEventsManager.instance.UIEvents.onRegisterError -= RegisterError;
        registerButton.onClick.RemoveListener(()=> GameEventsManager.instance.authEvents.Register(nameInputField.text, emailInputField.text, passwordInputField.text));

        // Login UI
        GameEventsManager.instance.UIEvents.onLoginError -= LoginError;
        loginButton.onClick.RemoveListener(()=> GameEventsManager.instance.authEvents.Login(loginEmailInputField.text, loginPasswordInputField.text));

        // Auth Debug
        Player1.onClick.RemoveListener(()=> {
            GameEventsManager.instance.authEvents.Login("testing1@esaunggul.ac.id", "testing123");
            GameEventsManager.instance.authEvents.SaveGender("male");
        });
        Player2.onClick.RemoveListener(()=> {
            GameEventsManager.instance.authEvents.Login("testing2@esaunggul.ac.id", "testing123");
            GameEventsManager.instance.authEvents.SaveGender("female");
        });

        // Gender UI
        maleButton.onClick.RemoveListener(()=>TogglePlayButton(""));
        femaleButton.onClick.RemoveListener(()=>TogglePlayButton(""));
        playButton.onClick.RemoveListener(StartGame);
        GameEventsManager.instance.UIEvents.onShowGenderPanel -= ShowGenderPanel;

    }
    private void OpenAuthPanel(){
        StartCoroutine(FadeCanvasGroups(menuPanel.GetComponent<CanvasGroup>(), authPanel.GetComponent<CanvasGroup>()));
    }
    public void OpenLoginPanel(){
        StartCoroutine(FadeCanvasGroups(registerPanel.GetComponent<CanvasGroup>(), loginPanel.GetComponent<CanvasGroup>()));
    }
    private void OpenRegisterPage(){
        StartCoroutine(FadeCanvasGroups(loginPanel.GetComponent<CanvasGroup>(), registerPanel.GetComponent<CanvasGroup>()));
    }
    private void OpenMenuPage(){
        StartCoroutine(BackToMenu());
    }

    private IEnumerator FadeCanvasGroups(CanvasGroup fadeOutCanvas, CanvasGroup fadeInCanvas)
    {
        float duration = 0.3f;
        float time = 0;

        fadeInCanvas.gameObject.SetActive(true);
        fadeInCanvas.alpha = 0;


        while (time < duration)
        {
            time += Time.deltaTime;
            float alpha = Mathf.Lerp(1, 0, time / duration);
            fadeOutCanvas.alpha = alpha;
            fadeInCanvas.alpha = 1 - alpha;
            yield return null;
        }

        fadeOutCanvas.alpha = 0;
        fadeInCanvas.alpha = 1;

        fadeOutCanvas.gameObject.SetActive(false);
    }

    private IEnumerator BackToMenu()
    {
        CanvasGroup authCanvas = authPanel.GetComponent<CanvasGroup>();
        CanvasGroup loginCanvas = loginPanel.GetComponent<CanvasGroup>();
        CanvasGroup menuCanvas = menuPanel.GetComponent<CanvasGroup>();

        float duration = 0.1f;
        float time = 0;

        menuCanvas.gameObject.SetActive(true);
        menuCanvas.alpha = 0;

        while (time < duration)
        {
            time += Time.deltaTime;
            float alpha = Mathf.Lerp(1, 0, time / duration);
            authCanvas.alpha = alpha;
            menuCanvas.alpha = 1 - alpha;
            yield return null;
        }

        menuCanvas.alpha = 1;

        registerPanel.SetActive(false);
        loginPanel.SetActive(true);
        loginCanvas.alpha = 1;
        authPanel.SetActive(false);

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

    private void LoginError(JObject error) {
        loginErrorPanel.SetActive(false);
        var errorText = loginErrorPanel.transform.GetComponentInChildren<TextMeshProUGUI>();
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

        loginErrorPanel.SetActive(true);
    }

    private void ShowGenderPanel(){
        StartCoroutine(FadeCanvasGroups(authPanel.GetComponent<CanvasGroup>(), genderPanel.GetComponent<CanvasGroup>()));
    }

    private void TogglePlayButton(string state) {
        GameEventsManager.instance.authEvents.SaveGender(state);
        playButton.interactable = true;
    }

    private void StartGame(){
        GameEventsManager.instance.levelEvents.LevelLoad("Lobby");
    }

}
