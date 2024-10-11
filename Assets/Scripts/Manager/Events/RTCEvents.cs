using System;
using UnityEngine;

public class RTCEvents
{
    public event Action OnUnMuteVoice;

    public void UnMuteVoice()
    {
        OnUnMuteVoice?.Invoke();
    }

    public event Action OnMuteVoice;

    public void MuteVoice()
    {
        OnMuteVoice?.Invoke();
    }

    public event Action<int> OnWebCamSelected;

    public void WebCamSelected(int webCamIndex)
    {
        OnWebCamSelected?.Invoke(webCamIndex);
    }

    public event Action OnUnMuteVideo;

    public void UnMuteVideo(){
        OnUnMuteVideo?.Invoke();
    }

    public event Action OnMuteVideo;

    public void MuteVideo(){
        OnMuteVideo?.Invoke();
    }
}
