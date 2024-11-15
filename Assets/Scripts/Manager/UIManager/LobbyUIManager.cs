using System;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class LobbyUIManager : MonoBehaviour
{
    public static LobbyUIManager Instance { get; private set; }

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
    bool _isVoiceMuted = true;
    bool _isVideoMuted = true;
    bool _isShareScreenModalOpen = false;
    bool _isVideoSource;
    // Overlay
    [SerializeField] private GameObject _overlay;
    // Camera Source
    [SerializeField] private TMP_Dropdown _webCamDropdown;
    private WebCamDevice[] _webCamDevices;

    private void OnEnable() {
        GameEventsManager.instance.UIEvents.onToggleOverlay += ToggleOverlay;
        // Audio
        _toggleAudioMuteButton.onClick.AddListener(ToggleVoice);
        // Video
        _toggleVideoMuteButton.onClick.AddListener(ToggleVideo);
        _toggleVideoSourceButton.onClick.AddListener(ToggleVideoDevice);
        _webCamDropdown.onValueChanged.AddListener(OnWebcamSelected);
        // Share Screen
        _toggleShareScreenButton.onClick.AddListener(ToggleShareScreen);
        _windowButton.onClick.AddListener(SelectWindowCapture);
        _screenButton.onClick.AddListener(SelectScreenCapture);
        _publishButton.onClick.AddListener(PublishScreenCapture);
        _unpublishButton.onClick.AddListener(UnPublishScreenCapture);
        _cancelButton.onClick.AddListener(ToggleShareScreen);
        GameEventsManager.instance.RTCEvents.OnToggleCaptureSelected += ToggleCaptureSelected;
        GameEventsManager.instance.RTCEvents.OnCaptureState += ShareScreenState;
    }

    private void OnDisable() {
        GameEventsManager.instance.UIEvents.onToggleOverlay -= ToggleOverlay;
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
    }

    private void Start(){
        _webCamDevices = WebCamTexture.devices;
        _webCamDropdown.ClearOptions();
        foreach (var device in _webCamDevices)
        {
            _webCamDropdown.options.Add(new TMP_Dropdown.OptionData(device.name));
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

    public void UnPublishScreenCapture()
    {
        GameEventsManager.instance.RTCEvents?.UnPublishCapture();
        GameEventsManager.instance.RTCEvents?.CaptureState(RTCEvents.CaptureStates.Stopped);
        _publishButton.interactable = false;
        _shareScreenModal.SetActive(false);
        _isShareScreenModalOpen = false;
    }

}
