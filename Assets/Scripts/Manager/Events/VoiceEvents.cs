using System;
using UnityEngine;

public class VoiceEvents : MonoBehaviour
{
    public event Action OnPlayerJoined;

    public void PlayerJoined()
    {
        OnPlayerJoined?.Invoke();
    }
}
