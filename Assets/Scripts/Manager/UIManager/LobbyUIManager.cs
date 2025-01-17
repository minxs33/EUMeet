using System;
using System.Collections;
using System.Threading;
using Agora.Rtc;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using ScrollView = UnityEngine.UIElements.ScrollView;

public class LobbyUIManager : MonoBehaviour
{
    public static LobbyUIManager Instance { get; private set; }
    [SerializeField] private GameObject _overlay;
    [SerializeField] private GameObject _loadingScreen;
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
    [SerializeField] private Button _addSubjectButton;
    [SerializeField] private TMP_InputField _addSubjectInputField;
    [SerializeField] private Button _addQuizButton;
    [SerializeField] private TMP_InputField _addQuizInputField;
    [SerializeField] private TMP_Text _CountDownText;

    [Header("Question UI")]
    [SerializeField] private TMP_Text _quizTitleText;
    [SerializeField] private GameObject _questionModalContent;
    [SerializeField] private Button _startQuizButton;
    [SerializeField] private Button _doneQuestionModalButton;
    [SerializeField] private Button _addQuestionButton;
    [SerializeField] private GameObject _quizPanel;
    [SerializeField] private GameObject _leaderboardPanel;
    
    [Header("Ranking UI")]
    [SerializeField] private TMP_Dropdown _selectSubjectDropdown;
    [SerializeField] private ScrollView _rankingScrollView;
    [SerializeField] private Button _toggleRankingButton;
    [SerializeField] private GameObject _rankingModal;
    [SerializeField] private Button _closeRankingModalButton;
    [SerializeField] private Button _resetRankingButton;
    bool _isVoiceMuted = true;
    bool _isVideoMuted = true;
    bool _isShareScreenModalOpen = false;
    bool _isQuizModalOpen = false;
    bool _isQuizModalContentOpen = true;
    bool _isQuizOverlayOpen = false;
    bool _isLeaderboardOpen = false;
    bool _isRankingModalOpen = false;
    bool _isQuizStarted = false;
    private Coroutine fadeChatCoroutine;
    private FadeAnimation _fadeAnimation;
    private SlideAnimation _slideAnimation;

    // Camera Source
    [SerializeField] private TMP_Dropdown _webCamDropdown;
    private WebCamDevice[] _webCamDevices;

    private void OnEnable() {
        GameEventsManager.instance.UIEvents.onLocalPlayerJoined += DisableLoading;
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
        _addSubjectButton.onClick.AddListener(AddSubject);
        _doneQuestionModalButton.onClick.AddListener(ToggleModalContent);
        _addQuestionButton.onClick.AddListener(AddQuestion);
        _startQuizButton.onClick.AddListener(GameEventsManager.instance.QuizEvents.StartQuizClicked);
        GameEventsManager.instance.QuizEvents.OnOpenQuizQuestion += ToggleModalContent;
        GameEventsManager.instance.QuizEvents.OnSetTitleText += SetTitleText;
        GameEventsManager.instance.QuizEvents.OnToggleQuizSelected += ToggleQuizSelected;
        GameEventsManager.instance.QuizEvents.OnStartQuizSetup += StartQuizSetup;
        GameEventsManager.instance.QuizEvents.OnStartQuiz += StartQuiz;
        GameEventsManager.instance.QuizEvents.OnEndQuiz += EndQuiz;
        GameEventsManager.instance.QuizEvents.OnToggleLeaderboard += ToggleLeaderboard;
        GameEventsManager.instance.QuizEvents.OnCountDownStart += CountDownStart;
        // ranking
        _toggleRankingButton.onClick.AddListener(ToggleRanking);
        _closeRankingModalButton.onClick.AddListener(ToggleRanking);
        _resetRankingButton.onClick.AddListener(GameEventsManager.instance.QuizEvents.ResetRanking);
        
    }

    private void OnDisable() {
        GameEventsManager.instance.UIEvents.onLocalPlayerJoined -= DisableLoading;
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
        _addSubjectButton.onClick.RemoveListener(AddSubject);
        _doneQuestionModalButton.onClick.RemoveListener(ToggleModalContent);
        _addQuestionButton.onClick.RemoveListener(AddQuestion);
        _startQuizButton.onClick.RemoveListener(GameEventsManager.instance.QuizEvents.StartQuizClicked);
        GameEventsManager.instance.QuizEvents.OnOpenQuizQuestion -= ToggleModalContent;
        GameEventsManager.instance.QuizEvents.OnSetTitleText -= SetTitleText;
        GameEventsManager.instance.QuizEvents.OnToggleQuizSelected -= ToggleQuizSelected;
        GameEventsManager.instance.QuizEvents.OnStartQuizSetup -= StartQuizSetup;
        GameEventsManager.instance.QuizEvents.OnStartQuiz -= StartQuiz;
        GameEventsManager.instance.QuizEvents.OnEndQuiz -= EndQuiz;
        GameEventsManager.instance.QuizEvents.OnToggleLeaderboard -= ToggleLeaderboard;
        GameEventsManager.instance.QuizEvents.OnCountDownStart -= CountDownStart;
        _toggleRankingButton.onClick.RemoveListener(ToggleRanking);
        _closeRankingModalButton.onClick.RemoveListener(ToggleRanking);
        _resetRankingButton.onClick.RemoveListener(GameEventsManager.instance.QuizEvents.ResetRanking);
    }

    private void Awake() {
        Instance = this;
    }
    private void Start(){
        _loadingScreen.SetActive(true);
        if(PlayerPrefs.GetInt("isDosen") == 1)
        {
            _toggleQuizButton.gameObject.SetActive(true);
        }else{
            _toggleQuizButton.gameObject.SetActive(false);
        }
    }

    private void DisableLoading(){
        _fadeAnimation = _loadingScreen.GetComponent<FadeAnimation>();
        _fadeAnimation.FadeOut(()=>{
            _loadingScreen.SetActive(false);
        });
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
        SoundManager.PlaySound(SoundType.UI_PRESS, null, 0.5f);
    }

    private void ToggleOverlay(Boolean state)
    {
        _fadeAnimation = _overlay.GetComponent<FadeAnimation>();
        if(state){
            SoundManager.PlaySound(SoundType.UI_OVERLAY_OPEN, null, 0.5f);
            _overlay.SetActive(true);
            _fadeAnimation.FadeIn();
            
        } else {
            SoundManager.PlaySound(SoundType.UI_OVERLAY_CLOSE, null, 0.5f);
            _fadeAnimation.FadeOut(()=>{
                _overlay.SetActive(false);
            });
        }
    }

    private void ToggleVoice(){
        Button _button = _toggleAudioMuteButton.GetComponentInChildren<Button>();
        Image _buttonGoImage = _button.GetComponent<Image>();
        GameObject _buttonChild = _button.transform.Find("Icon").gameObject;
        Image _buttonImage = _buttonChild.GetComponent<Image>();
        
        if (_buttonImage == null) {
            Debug.LogError("Button Image component is missing.");
            return;
        }

        if (_isVoiceMuted) {
            GameEventsManager.instance.RTCEvents?.UnMuteVoice();
            _isVoiceMuted = false;
            
            _button.transition = Selectable.Transition.None;
            _buttonGoImage.color = new Color(0.172549f,0.172549f,0.172549f,1f);
            _buttonImage.color = new Color(0.9607843f, 0.9607843f, 0.9607843f, 1f);
            _buttonImage.sprite = Resources.Load<Sprite>("Icons/Free Flat Mic Icon");

        } else {
            GameEventsManager.instance.RTCEvents?.MuteVoice();
            _isVoiceMuted = true;

            _button.transition = Selectable.Transition.ColorTint;
            _buttonGoImage.color = new Color(1f, 1f, 1f, 1f);
            _buttonImage.color = new Color(0.9811321f, 0.1434674f, 0.1776674f, 1f);
            _buttonImage.sprite = Resources.Load<Sprite>("Icons/Free Flat Mic Off Icon");
        }

    }

    private void ToggleVideo(){
        Button _button = _toggleVideoMuteButton.GetComponentInChildren<Button>();
        Image _buttonGoImage = _button.GetComponent<Image>();
        GameObject _buttonChild = _button.transform.Find("Icon").gameObject;
        Image _buttonImage = _buttonChild.GetComponent<Image>();

        if (_buttonImage == null) {
            Debug.LogError("Button Image component is missing.");
            return;
        }

        if (_isVideoMuted) {
            GameEventsManager.instance.RTCEvents?.UnMuteVideo();
            _isVideoMuted = false;
            _button.transition = Selectable.Transition.None;
            _buttonGoImage.color = new Color(0.172549f,0.172549f,0.172549f,1f);
            _buttonImage.color = new Color(0.9607843f, 0.9607843f, 0.9607843f, 1f);
            _buttonImage.sprite = Resources.Load<Sprite>("Icons/Free Flat Video Icon");
            _localView.SetActive(true);
        } else {
            GameEventsManager.instance.RTCEvents?.MuteVideo();
            _isVideoMuted = true;
            _localView.SetActive(false);
            _button.transition = Selectable.Transition.ColorTint;
            _buttonGoImage.color = new Color(1f, 1f, 1f, 1f);
            _buttonImage.color = new Color(0.9811321f, 0.1434674f, 0.1776674f, 1f);
            _buttonImage.sprite = Resources.Load<Sprite>("Icons/Free Flat Video Off Icon");
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
            SoundManager.PlaySound(SoundType.UI_CLOSE_POP_UP, null, 0.5f);
        } else {
            _shareScreenModal.SetActive(true);
            _isShareScreenModalOpen = true;
            SoundManager.PlaySound(SoundType.UI_OPEN_POP_UP, null, 0.5f);
        }
    }

    private void SelectWindowCapture(){
        GameEventsManager.instance.RTCEvents?.ToggleSelectCaptureType(true);
        _windowButton.interactable = false;
        _screenButton.interactable = true;
        SoundManager.PlaySound(SoundType.UI_PRESS,null,0.5f);
    }

    private void SelectScreenCapture(){
        GameEventsManager.instance.RTCEvents?.ToggleSelectCaptureType(false);
        _windowButton.interactable = true;
        _screenButton.interactable = false;
        SoundManager.PlaySound(SoundType.UI_PRESS,null,0.5f);
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
        SoundManager.PlaySound(SoundType.UI_PRESS, null, 0.5f);
    }

    private void UnPublishScreenCapture()
    {
        GameEventsManager.instance.RTCEvents?.UnPublishCapture();
        GameEventsManager.instance.RTCEvents?.CaptureState(RTCEvents.CaptureStates.Stopped);
        _publishButton.interactable = false;
        _shareScreenModal.SetActive(false);
        _isShareScreenModalOpen = false;
        SoundManager.PlaySound(SoundType.UI_CANCEL, null ,0.5f);
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
            SoundManager.PlaySound(SoundType.UI_CHAT, null, 0.5f);
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
            SoundManager.PlaySound(SoundType.UI_CLOSE_POP_UP,null, 0.5f);
        } else {
            _quizModal.SetActive(true);
            _isQuizModalOpen = true;
            SoundManager.PlaySound(SoundType.UI_OPEN_POP_UP,null, 0.5f);
        }
    }

    public void ToggleQuizOverlay(){
        if(!_isQuizOverlayOpen){
            _quizPanel.SetActive(true);
            _isQuizOverlayOpen = true;
            // Sound start quiz
        } else {
            _quizPanel.SetActive(false);
            _isQuizOverlayOpen = false;
            // Sound end quiz
        }
    }

    public void ToggleQuizSelected(bool state){
        if(state){
            _startQuizButton.interactable = true;
        } else {
            _startQuizButton.interactable = false;
        }
    }

    public void AddSubject(){
        if(_addSubjectInputField.text.Length > 0){
            GameEventsManager.instance.QuizEvents?.AddSubject(_addSubjectInputField.text);
            _addSubjectInputField.text = "";
            SoundManager.PlaySound(SoundType.UI_PRESS, null, 0.5f);
        }
    }

    public void UpdateSubject(string subject, int id){
        if(!string.IsNullOrEmpty(subject)){
            GameEventsManager.instance.QuizEvents?.UpdateSubject(subject, id);
        }
    }

    public void AddQuiz(){
        if(_addQuizInputField.text.Length > 0){
            GameEventsManager.instance.QuizEvents?.AddQuiz(_addQuizInputField.text);
            _addQuizInputField.text = "";
            SoundManager.PlaySound(SoundType.UI_PRESS, null, 0.5f);
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
        SoundManager.PlaySound(SoundType.UI_PRESS, null, 0.5f);
        Debug.Log("isQuizModalContentOpen: " + _isQuizModalContentOpen);
    }

    public void StartQuizSetup(){
        _isShareScreenModalOpen = false;
        _shareScreenModal.SetActive(false);
        _isQuizModalOpen = false;
        _quizModal.SetActive(false);
        _isRankingModalOpen = false;
        _rankingModal.SetActive(false);
    }

    public void StartQuiz(){
        // ToggleQuiz();
        if(_isQuizStarted) return;
        _isQuizStarted = true;
        ToggleQuizOverlay();
        
    }

    public void EndQuiz(){
        _isQuizStarted = false;
        ToggleQuizOverlay();
    }

    public void ToggleLeaderboard(){
        if(_isLeaderboardOpen){
            _leaderboardPanel.SetActive(false);
            _isLeaderboardOpen = false;
        }else{
            _isLeaderboardOpen = true;
            _leaderboardPanel.SetActive(true);
            // Sound Leaderboard
            GameEventsManager.instance.QuizEvents?.GetLeaderboard();
        }
    }
    public void SetTitleText(string title) => _quizTitleText.text = title;

    public void AddQuestion(){
        GameEventsManager.instance.QuizEvents?.AddQuestion();
        SoundManager.PlaySound(SoundType.UI_OPEN_POP_UP, null, 0.5f);
    }

    public void CountDownStart(int num){
        if (num == 0){
            _CountDownText.gameObject.SetActive(false);
        }else{
            _CountDownText.gameObject.SetActive(true);
            _CountDownText.text = num.ToString();
        }
    }

    private void ToggleRanking() {
        if(_isRankingModalOpen){
            _rankingModal.SetActive(false);
            _isRankingModalOpen = false;
            SoundManager.PlaySound(SoundType.UI_CLOSE_POP_UP,null, 0.5f);
        }else{
            _rankingModal.SetActive(true);
            _isRankingModalOpen = true;
            SoundManager.PlaySound(SoundType.UI_OPEN_POP_UP,null, 0.5f);
            GameEventsManager.instance.QuizEvents?.RankingModalOpen();
        }
    }
}
