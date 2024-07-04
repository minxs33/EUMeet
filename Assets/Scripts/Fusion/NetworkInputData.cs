using System.Collections;
using System.Collections.Generic;
using Fusion;
using UnityEngine;

public struct NetworkInputData : INetworkInput
{
    // movement
    public Vector3 direction;

    // bullet function
    public const byte MOUSEBUTTON0 = 1;
    public NetworkButtons buttons;
}
