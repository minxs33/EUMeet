using System;

public class VoiceEvents
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
}
