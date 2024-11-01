using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

public class PlayerUID : MonoBehaviour
{
    public static PlayerUID Instance { get; private set; }
    public uint uid;

    public void Initialize(uint agoraUID){
        uid = agoraUID;
        Debug.Log("Initialize PlayerUID: " + uid);
    }
}
