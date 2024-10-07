using System;
using System.Collections;
using System.Collections.Generic;
using Fusion;
using Photon.Voice.Unity;
using UnityEngine;

public class VoiceManager : MonoBehaviour
{
    public static VoiceManager instance { get; private set; }
    
    [SerializeField] GameObject _runnerGameObject;
    
    private void OnEnable() {
        GameEventsManager.instance.RTCEvents.OnUnMuteVoice += UnMute;
        GameEventsManager.instance.RTCEvents.OnMuteVoice += Mute;
    }

    private void OnDisable() {
        GameEventsManager.instance.RTCEvents.OnUnMuteVoice -= UnMute;
        GameEventsManager.instance.RTCEvents.OnMuteVoice -= Mute;
    }

    private void Mute()
    {
        Recorder _recorder = _runnerGameObject.GetComponentInChildren<Recorder>();
        _recorder.TransmitEnabled = false;
        Debug.Log("Mute");
    }

    private void UnMute()
    {
        Recorder _recorder = _runnerGameObject.GetComponentInChildren<Recorder>();
        _recorder.TransmitEnabled = true;
        Debug.Log("UnMute");
    }


    
}
