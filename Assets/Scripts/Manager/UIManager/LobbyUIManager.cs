using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LobbyUIManager : MonoBehaviour
{
    public static LobbyUIManager Instance { get; private set; }

    [SerializeField] private Button _toggleAudioMuteButton;
    [SerializeField] private Button _toggleVideoMuteButton;
    [SerializeField] private Button _toggleVideoSourceButton;
    bool _isVoiceMuted = true;
    bool _isVideoMuted = true;
    bool _isVideoSource;
    // Overlay
    [SerializeField] private GameObject _overlay;
    // Camera Source
    [SerializeField] private TMP_Dropdown _webCamDropdown;
    private WebCamDevice[] _webCamDevices;

    private void OnEnable() {
        _toggleAudioMuteButton.onClick.AddListener(ToggleVoice);
        _toggleVideoMuteButton.onClick.AddListener(ToggleVideo);
        _toggleVideoSourceButton.onClick.AddListener(ToggleVideoDevice);
        GameEventsManager.instance.UIEvents.onToggleOverlay += ToggleOverlay;
    }

    private void OnDisable() {
        _toggleAudioMuteButton.onClick.RemoveListener(ToggleVoice);
        _toggleVideoMuteButton.onClick.RemoveListener(ToggleVideo);
        _toggleVideoSourceButton.onClick.RemoveListener(ToggleVideoDevice);
        GameEventsManager.instance.UIEvents.onToggleOverlay -= ToggleOverlay;
    }

    private void Start(){
        _webCamDevices = WebCamTexture.devices;
        _webCamDropdown.ClearOptions();
        foreach (var device in _webCamDevices)
        {
            _webCamDropdown.options.Add(new TMP_Dropdown.OptionData(device.name));
        }

        _webCamDropdown.onValueChanged.AddListener(OnWebcamSelected);
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
        } else {
            GameEventsManager.instance.RTCEvents?.MuteVideo();
            _isVideoMuted = true;
            _buttonImage.sprite = Resources.Load<Sprite>("Icons/Free Flat Video Off Icon");
        }
    }

    public void ToggleVideoDevice(){
        _webCamDropdown.Show();
    }
}
