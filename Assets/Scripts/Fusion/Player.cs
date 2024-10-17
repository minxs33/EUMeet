using System;
using Fusion;
using Fusion.Addons.SimpleKCC;
using UnityEngine;

public class Player : NetworkBehaviour
{
    [SerializeField] private SimpleKCC kcc;
    [SerializeField] private float speed = 5f;
    [SerializeField] private float jumpImpluse = 10f;

    [SerializeField] private Transform camTarget;
    [SerializeField] private float lookSensitivity = 0.15f;
    [Networked] private NetworkButtons PreviousButtons {get;set;}
    [Networked] public uint Uid {get; set;}

    private InputManager inputManager;
    private Vector2 baseLookRotation;

    private ChangeDetector changeDetector;

    public uint SetUID(){
        this.Uid = (uint)PlayerPrefs.GetInt("uid");
        return Uid;
    }
    public override void Spawned()
    {
        changeDetector = new ChangeDetector();
        kcc.SetGravity(Physics.gravity.y * 2f);

        inputManager = Runner.GetComponent<InputManager>();
        inputManager.LocalPlayer = this;

        if (HasInputAuthority)
        {
            CameraFollow.Singleton.SetTarget(camTarget);
            // Client requests the server to set the UID
            Rpc_RequestSetUID(SetUID());
        }

        kcc.Settings.ForcePredictedLookRotation = true;
    }

    // Client sends a request to the server to set its UID
    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    public void Rpc_RequestSetUID(uint Uid)
    {
        Rpc_SetUID(Uid); // Server then sets the UID for this player
    }

    // This RPC is executed by the server to update the UID
    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void Rpc_SetUID(uint uid)
    {
        this.Uid = uid;
        Debug.Log("UID set to: " + uid);
    }
    public override void FixedUpdateNetwork()
    {
        
        if(GetInput(out NetworkInputData input)){
            kcc.AddLookRotation(input.LookDelta * lookSensitivity);
            UpdateCamTarget();
            Vector3 worldDirection = kcc.TransformRotation * new Vector3(input.Direction.x, 0f, input.Direction.y);
            float jump = 0f;

            if(input.Buttons.WasPressed(PreviousButtons,InputButton.Jump) && kcc.IsGrounded){
                jump = jumpImpluse;
            }

            kcc.Move(worldDirection.normalized * speed, jump);
            PreviousButtons = input.Buttons;
            baseLookRotation = kcc.GetLookRotation();
        }
    }

    public override void Render(){
        if(kcc.Settings.ForcePredictedLookRotation)
        {
            Vector2 predictedLookRotation = baseLookRotation + inputManager.AccumulatedMouseDelta * lookSensitivity;
            kcc.SetLookRotation(predictedLookRotation);
        }
        UpdateCamTarget();
    }

    private void UpdateCamTarget(){
        camTarget.localRotation = Quaternion.Euler(kcc.GetLookRotation().x, 0f, 0f);
    }

}
