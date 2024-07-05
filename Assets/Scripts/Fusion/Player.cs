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

    private InputManager inputManager;
    private Vector2 baseLookRotation;
    public override void Spawned()
    {
        kcc.SetGravity(Physics.gravity.y * 2f);
            
            inputManager = Runner.GetComponent<InputManager>();
            inputManager.LocalPlayer = this;
            
            if(HasInputAuthority)
            CameraFollow.Singleton.SetTarget(camTarget);

            kcc.Settings.ForcePredictedLookRotation = true;
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
