using System.Collections;
using System.Collections.Generic;
using Fusion;
using UnityEngine;


public enum InputButton {
    Jump,
    Interact,
}
public struct NetworkInputData : INetworkInput
{
    public NetworkButtons Buttons;
    public Vector2 Direction;
    public Vector2 LookDelta;
}
