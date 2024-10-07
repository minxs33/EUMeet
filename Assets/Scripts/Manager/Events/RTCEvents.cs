using System;
using UnityEngine;

public class RTCEvents
{
    public event Action OnUnMuteVoice;

    public void UnMute()
    {
        OnUnMuteVoice?.Invoke();
    }

    public event Action OnMuteVoice;

    public void Mute()
    {
        OnMuteVoice?.Invoke();
    }

    public event Action<int> OnWebCamSelected;

    public void WebCamSelected(int webCamIndex)
    {
        OnWebCamSelected?.Invoke(webCamIndex);
    }
}
