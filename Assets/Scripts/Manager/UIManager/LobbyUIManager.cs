using System;
using Fusion;
using Photon.Voice.Unity;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class LobbyUIManager : MonoBehaviour
{
    public static LobbyUIManager instance { get; private set; }

    [SerializeField] public Button _toggleMuteButton;
    bool _isMuted = true;

    private void OnEnable() {
        _toggleMuteButton.onClick.AddListener(ToggleMute);
    }

    private void OnDisable() {
        _toggleMuteButton.onClick.RemoveListener(ToggleMute);
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
            GameEventsManager.instance.voiceEvents?.UnMute();
            _isMuted = false;

            foreach(var s in _sprites){
                if(s.name == "mic_0"){
                    _buttonImage.sprite = s;
                }
            }
        } else {
            GameEventsManager.instance.voiceEvents?.Mute();
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
