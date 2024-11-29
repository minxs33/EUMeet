using System;
using System.Collections;
using Agora.Rtc;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class LobbyUIManager : MonoBehaviour
{
    public static LobbyUIManager Instance { get; private set; }
    [SerializeField] private GameObject _overlay;
    [Header("Chat UI")]
    [SerializeField] private TMP_InputField _inputField;
    [SerializeField] private GameObject _chatPanel;
    [SerializeField] private TMP_Text _chatText;

    [Header("Video UI")]
    [SerializeField] private Button _toggleVideoMuteButton;
    [SerializeField] private Button _toggleVideoSourceButton;
    [SerializeField] private GameObject _localView;
    [Header("Audio UI")]
    [SerializeField] private Button _toggleAudioMuteButton;
    [Header("Share Screen UI")]
    [SerializeField] private Button _toggleShareScreenButton;
    [SerializeField] private GameObject _shareScreenModal;
    [SerializeField] private Button _windowButton;
    [SerializeField] private Button _screenButton;
    [SerializeField] private Button _publishButton;
    [SerializeField] private Button _cancelButton;
    [SerializeField] private Button _unpublishButton;
    [Header("Quiz UI")]
    [SerializeField] private GameObject _quizModalContent;
    [SerializeField] private Button _toggleQuizButton;
    [SerializeField] private Button _cancelQuizModalButton;
    [SerializeField] private GameObject _quizModal;
    [SerializeField] private Button _addQuizButton;
    [SerializeField] private TMP_InputField _addQuizInputField;

    [Header("Question UI")]
    [SerializeField] private TMP_Text _quizTitleText;
    [SerializeField] private GameObject _questionModalContent;
    [SerializeField] private Button _doneQuestionModalButton;
    [SerializeField] private Button _addQuestionButton;
    bool _isVoiceMuted = true;
    bool _isVideoMuted = true;
    bool _isShareScreenModalOpen = false;
    bool _isQuizModalOpen = false;
    bool _isQuizModalContentOpen = true;
    private Coroutine fadeChatCoroutine;

    // Camera Source
    [SerializeField] private TMP_Dropdown _webCamDropdown;
    private WebCamDevice[] _webCamDevices;

    private void OnEnable() {
        GameEventsManager.instance.UIEvents.onToggleOverlay += ToggleOverlay;
        GameEventsManager.instance.RTCEvents.onChatInputPressed += InputTextSelected;
        // Chat
        _inputField.onEndEdit.AddListener(ChatHandler);
        // Audio
        _toggleAudioMuteButton.onClick.AddListener(ToggleVoice);
        // Video
        _toggleVideoMuteButton.onClick.AddListener(ToggleVideo);
        _toggleVideoSourceButton.onClick.AddListener(ToggleVideoDevice);
        _webCamDropdown.onValueChanged.AddListener(OnWebcamSelected);
        GameEventsManager.instance.RTCEvents.OnCameraDeviceUpdated += UpdateWebCamDropdown;
        // Share Screen
        _toggleShareScreenButton.onClick.AddListener(ToggleShareScreen);
        _windowButton.onClick.AddListener(SelectWindowCapture);
        _screenButton.onClick.AddListener(SelectScreenCapture);
        _publishButton.onClick.AddListener(PublishScreenCapture);
        _unpublishButton.onClick.AddListener(UnPublishScreenCapture);
        _cancelButton.onClick.AddListener(ToggleShareScreen);
        GameEventsManager.instance.RTCEvents.OnToggleCaptureSelected += ToggleCaptureSelected;
        GameEventsManager.instance.RTCEvents.OnCaptureState += ShareScreenState;
        // Quiz
        _toggleQuizButton.onClick.AddListener(ToggleQuiz);
        _cancelQuizModalButton.onClick.AddListener(ToggleQuiz);
        _addQuizButton.onClick.AddListener(AddQuiz);
        _doneQuestionModalButton.onClick.AddListener(ToggleModalContent);
        _addQuestionButton.onClick.AddListener(AddQuestion);
        GameEventsManager.instance.QuizEvents.OnOpenQuizQuestion += ToggleModalContent;
        GameEventsManager.instance.QuizEvents.OnSetTitleText += SetTitleText;
        
    }

    private void OnDisable() {
        GameEventsManager.instance.UIEvents.onToggleOverlay -= ToggleOverlay;
        GameEventsManager.instance.RTCEvents.onChatInputPressed -= InputTextSelected;
        _inputField.onEndEdit.RemoveListener(ChatHandler); 
        _toggleAudioMuteButton.onClick.RemoveListener(ToggleVoice);
        _toggleVideoMuteButton.onClick.RemoveListener(ToggleVideo);
        _toggleVideoSourceButton.onClick.RemoveListener(ToggleVideoDevice);
        _webCamDropdown.onValueChanged.RemoveListener(OnWebcamSelected);
        _toggleShareScreenButton.onClick.RemoveListener(ToggleShareScreen);
        _windowButton.onClick.RemoveListener(SelectWindowCapture);
        _screenButton.onClick.RemoveListener(SelectScreenCapture);
        _publishButton.onClick.RemoveListener(PublishScreenCapture);
        _unpublishButton.onClick.RemoveListener(UnPublishScreenCapture);
        _cancelButton.onClick.RemoveListener(ToggleShareScreen);
        GameEventsManager.instance.RTCEvents.OnToggleCaptureSelected -= ToggleCaptureSelected;
        GameEventsManager.instance.RTCEvents.OnCaptureState -= ShareScreenState;
        GameEventsManager.instance.RTCEvents.OnCameraDeviceUpdated -= UpdateWebCamDropdown;
        _toggleQuizButton.onClick.RemoveListener(ToggleQuiz);
        _cancelQuizModalButton.onClick.RemoveListener(ToggleQuiz);
        _addQuizButton.onClick.RemoveListener(AddQuiz);
        _doneQuestionModalButton.onClick.RemoveListener(ToggleModalContent);
        _addQuestionButton.onClick.RemoveListener(AddQuestion);
        GameEventsManager.instance.QuizEvents.OnOpenQuizQuestion -= ToggleModalContent;
        GameEventsManager.instance.QuizEvents.OnSetTitleText -= SetTitleText;
    }

    private void Awake() {
        Instance = this;
    }
    private void Start(){
        // _webCamDropdown.ClearOptions();
        // _webCamDevices = WebCamTexture.devices;
        // foreach (var device in _webCamDevices)
        // {
        //     _webCamDropdown.options.Add(new TMP_Dropdown.OptionData(device.name));
        // }
    }

    private void UpdateWebCamDropdown(DeviceInfo[] deviceInfos){
        _webCamDropdown.ClearOptions();
        foreach (var device in deviceInfos)
        {
            _webCamDropdown.options.Add(new TMP_Dropdown.OptionData(device.deviceName));
        }
    }

    private void OnWebcamSelected(int index)
    {

        GameEventsManager.instance.RTCEvents.WebCamSelected(index);
    }

    private void ToggleOverlay(Boolean state)
    {
        if(state){
            _overlay.SetActive(true);
        } else {
            _overlay.SetActive(false);
        }
    }

    private void ToggleVoice(){
        Transform _button = _toggleAudioMuteButton.GetComponentInChildren<Transform>();
        GameObject _buttonChild = _button.Find("Icon").gameObject;
        Image _buttonImage = _buttonChild.GetComponentInChildren<Image>();
        
        if (_buttonImage == null) {
            Debug.LogError("Button Image component is missing.");
            return;
        }

        if (_isVoiceMuted) {
            GameEventsManager.instance.RTCEvents?.UnMuteVoice();
            _isVoiceMuted = false;
            _buttonImage.sprite = Resources.Load<Sprite>("Icons/Free Flat Mic Icon");

        } else {
            GameEventsManager.instance.RTCEvents?.MuteVoice();
            _isVoiceMuted = true;

            _buttonImage.sprite = Resources.Load<Sprite>("Icons/Free Flat Mic Off Icon");
        }

    }

    private void ToggleVideo(){
        Transform _button = _toggleVideoMuteButton.GetComponentInChildren<Transform>();
        GameObject _buttonChild = _button.Find("Icon").gameObject;
        Image _buttonImage = _buttonChild.GetComponentInChildren<Image>();

        if (_buttonImage == null) {
            Debug.LogError("Button Image component is missing.");
            return;
        }

        if (_isVideoMuted) {
            GameEventsManager.instance.RTCEvents?.UnMuteVideo();
            _isVideoMuted = false;
            _buttonImage.sprite = Resources.Load<Sprite>("Icons/Free Flat Video Icon");
            _localView.SetActive(true);
        } else {
            GameEventsManager.instance.RTCEvents?.MuteVideo();
            _isVideoMuted = true;
            _buttonImage.sprite = Resources.Load<Sprite>("Icons/Free Flat Video Off Icon");
            _localView.SetActive(false);
        }
    }

    public void ToggleVideoDevice(){
        _webCamDropdown.Show();
    }

    private void ShareScreenState(RTCEvents.CaptureStates state){
        var colors = _toggleShareScreenButton.colors;
        switch (state){
            case RTCEvents.CaptureStates.Started:
                _unpublishButton.interactable = true;
                _unpublishButton.gameObject.SetActive(true);
                break;
            case RTCEvents.CaptureStates.Stopped:
                _unpublishButton.interactable = false;
                _unpublishButton.gameObject.SetActive(false);
                break;
        }
    }

    private void ToggleShareScreen(){
        if(_isShareScreenModalOpen){
            _shareScreenModal.SetActive(false);
            _isShareScreenModalOpen = false;
        } else {
            _shareScreenModal.SetActive(true);
            _isShareScreenModalOpen = true;
        }
    }

    private void SelectWindowCapture(){
        GameEventsManager.instance.RTCEvents?.ToggleSelectCaptureType(true);
        _windowButton.interactable = false;
        _screenButton.interactable = true;
    }

    private void SelectScreenCapture(){
        GameEventsManager.instance.RTCEvents?.ToggleSelectCaptureType(false);
        _windowButton.interactable = true;
        _screenButton.interactable = false;
    }

    private void ToggleCaptureSelected(bool state){
        if(state){
            _publishButton.interactable = true;
        } else {
            _publishButton.interactable = false;
        }
    }

    private void PublishScreenCapture()
    {
        GameEventsManager.instance.RTCEvents?.PublishCapture();
        GameEventsManager.instance.RTCEvents?.CaptureState(RTCEvents.CaptureStates.Started);
        _publishButton.interactable = false;
        _shareScreenModal.SetActive(false);
        _isShareScreenModalOpen = false;
    }

    private void UnPublishScreenCapture()
    {
        GameEventsManager.instance.RTCEvents?.UnPublishCapture();
        GameEventsManager.instance.RTCEvents?.CaptureState(RTCEvents.CaptureStates.Stopped);
        _publishButton.interactable = false;
        _shareScreenModal.SetActive(false);
        _isShareScreenModalOpen = false;
    }

    // chat

    private void InputTextSelected(bool state){
        if (state) {
            StartFadeChatPanel(false);
            _chatPanel.GetComponent<CanvasGroup>().alpha = 1;

            _inputField.gameObject.SetActive(true);
            _inputField.Select();
            _inputField.ActivateInputField();
        } else {
            _inputField.gameObject.SetActive(false);
            EventSystem.current.SetSelectedGameObject(null);
            
            StartFadeChatPanel(true);
        }
    }

    public void AddMessage(string message) {
        if(message != null) {
            _chatText.text +="\n"+ message;
            _chatPanel.GetComponent<CanvasGroup>().alpha = 1;
            StartFadeChatPanel(true);
        }
    }

    private void ChatHandler(string message){
        if (_inputField.text.Length > 0) {
            GameEventsManager.instance.RTCEvents?.SendChat(_inputField.text);
            _inputField.text = "";
        }
    }

    public void StartFadeChatPanel(bool start) {
        if (fadeChatCoroutine != null) {
            StopCoroutine(fadeChatCoroutine);
            fadeChatCoroutine = null;
        }
    
        if (start) {
            fadeChatCoroutine = StartCoroutine(FadeChatPanel());
        }
    }

    IEnumerator FadeChatPanel() {
        yield return new WaitForSeconds(5f); // Wait before starting to fade
        CanvasGroup canvasGroup = _chatPanel.GetComponent<CanvasGroup>();

        while (canvasGroup.alpha > 0) {
            canvasGroup.alpha -= 0.05f;
            yield return new WaitForSeconds(0.05f);
        }

        canvasGroup.alpha = 0;
    }

    // Quiz
    public void ToggleQuiz() {
        if(_isQuizModalOpen){
            _quizModal.SetActive(false);
            _isQuizModalOpen = false;
        } else {
            _quizModal.SetActive(true);
            _isQuizModalOpen = true;
        }
    }

    public void AddQuiz(){
        if(_addQuizInputField.text.Length > 0){
            GameEventsManager.instance.QuizEvents?.AddQuiz(_addQuizInputField.text);
            _addQuizInputField.text = "";
        }
    }

    public void UpdateQuiz(string title, int id){
        if(!string.IsNullOrEmpty(title)){
            GameEventsManager.instance.QuizEvents?.UpdateQuiz(title, id);
        }
    }

    public void ToggleModalContent(){
        if(_isQuizModalContentOpen){
            _quizModalContent.SetActive(false);
            _questionModalContent.SetActive(true);
            _isQuizModalContentOpen = false;
        } else {
            _quizModalContent.SetActive(true);
            _questionModalContent.SetActive(false);
            _isQuizModalContentOpen = true;
        }
        Debug.Log("isQuizModalContentOpen: " + _isQuizModalContentOpen);
    }

    public void SetTitleText(string title) => _quizTitleText.text = title;

    public void AddQuestion() => GameEventsManager.instance.QuizEvents?.AddQuestion();
}
