using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FusionEvents
{
    public event Action onPlayerJoined;

    public void PlayerJoined()
    {
        onPlayerJoined?.Invoke();
    }

    public event Action<bool> onLightToggle;

    public void LightToggle(bool value)
    {
        onLightToggle?.Invoke(value);
    }
}
