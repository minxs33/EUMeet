using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LobbyUIManager : MonoBehaviour
{
    public static LobbyUIManager Instance { get; private set; }

    [SerializeField] private Button _toggleAudioMuteButton;
    [SerializeField] private Button _toggleVideoMuteButton;
    bool _isVoiceMuted = true;
    bool _isVideoMuted = true;
    // Overlay
    [SerializeField] private GameObject _overlay;
    // Camera Source
    [SerializeField] private TMP_Dropdown _webCamDropdown;
    private WebCamDevice[] _webCamDevices;

    private void OnEnable() {
        _toggleAudioMuteButton.onClick.AddListener(ToggleVoice);
        _toggleVideoMuteButton.onClick.AddListener(ToggleVideo);
        GameEventsManager.instance.UIEvents.onToggleOverlay += ToggleOverlay;
    }

    private void OnDisable() {
        _toggleAudioMuteButton.onClick.RemoveListener(ToggleVoice);
        _toggleVideoMuteButton.onClick.RemoveListener(ToggleVideo);
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
            _buttonImage.color = new Color(0, 0, 0, 255);

        } else {
            GameEventsManager.instance.RTCEvents?.MuteVoice();
            _isVoiceMuted = true;

            _buttonImage.sprite = Resources.Load<Sprite>("Icons/Free Flat Mic Off Icon");
            _buttonImage.color = new Color(225, 22, 22, 255);
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
            _buttonImage.color = new Color(0, 0, 0, 255);
        } else {
            GameEventsManager.instance.RTCEvents?.MuteVideo();
            _isVideoMuted = true;
            _buttonImage.sprite = Resources.Load<Sprite>("Icons/Free Flat Video Off Icon");
            _buttonImage.color = new Color(225, 22, 22, 255);
        }
    }
}
