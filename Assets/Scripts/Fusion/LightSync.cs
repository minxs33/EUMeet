using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using System.Linq;
public class LightSync : NetworkBehaviour
{
    [Networked] public NetworkBool IsOn { get; set; }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void RPC_RequestLightState()
    {
        if (Object.HasStateAuthority)
        {
            RPC_UpdateLight(IsOn);
        }
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void RPC_ToggleLight(){
        if (Object.HasStateAuthority)
        {
            IsOn = !IsOn;
            RPC_UpdateLight(IsOn);
        }
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void RPC_UpdateLight(bool isOn){
        IsOn = isOn;
        GameEventsManager.instance.fusionEvents.LightToggle(IsOn);
        Debug.Log("Light toggled: " + IsOn);
    }

}
