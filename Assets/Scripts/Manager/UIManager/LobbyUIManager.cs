using System;
using Fusion;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LobbyUIManager : MonoBehaviour
{
    public static LobbyUIManager Instance { get; private set; }

    [SerializeField] private Button _toggleMuteButton;
    bool _isMuted = true;
    // Overlay
    [SerializeField] private GameObject _overlay;
    // Camera Source
    [SerializeField] private TMP_Dropdown _webCamDropdown;
    private WebCamDevice[] _webCamDevices;

    private void OnEnable() {
        _toggleMuteButton.onClick.AddListener(ToggleMute);
        GameEventsManager.instance.UIEvents.onToggleOverlay += ToggleOverlay;
    }

    private void OnDisable() {
        _toggleMuteButton.onClick.RemoveListener(ToggleMute);
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

    private void ToggleMute(){
        Transform _button = _toggleMuteButton.GetComponentInChildren<Transform>();
        GameObject _buttonChild = _button.Find("Icon").gameObject;
        Image _buttonImage = _buttonChild.GetComponentInChildren<Image>();
        Sprite[] _sprites = Resources.LoadAll<Sprite>("Icons/mic");
        
        if (_buttonImage == null) {
            Debug.LogError("Button Image component is missing.");
            return;
        }

        if (_isMuted) {
            GameEventsManager.instance.RTCEvents?.UnMute();
            _isMuted = false;

            foreach(var s in _sprites){
                if(s.name == "mic_0"){
                    _buttonImage.sprite = s;
                }
            }
        } else {
            GameEventsManager.instance.RTCEvents?.Mute();
            _isMuted = true;

            foreach(var s in _sprites){
                if(s.name == "mic_1"){
                    _buttonImage.sprite = s;
                }
            }
        }

        Debug.Log(_isMuted);
    }
}
