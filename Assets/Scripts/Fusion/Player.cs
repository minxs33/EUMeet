using System;
using Fusion;
using UnityEngine;

public class Player : NetworkBehaviour
{
    private NetworkCharacterController characterController;
    [SerializeField] private Bullet bulletPrefab;
    private Vector3 _forward = Vector3.forward;


    [Networked] private TickTimer delay {get;set;}
    private void Awake(){
        characterController = GetComponent<NetworkCharacterController>();

    }

    public override void FixedUpdateNetwork()
    {
        
        // every tick
        if(GetInput(out NetworkInputData data)){ 
            // movement
            data.direction.Normalize();
            characterController.Move(10 * Runner.DeltaTime * data.direction);

            // instantiate bullet
            if(data.direction.sqrMagnitude > 0){
                _forward = data.direction; //contains direction of the movement
            }

            if(HasStateAuthority && delay.ExpiredOrNotRunning(Runner)){
                
                if(data.buttons.IsSet(NetworkInputData.MOUSEBUTTON0)){
                    Runner.Spawn(bulletPrefab, transform.position + _forward, Quaternion.LookRotation(_forward), Object.InputAuthority,
                        (Runner, O) =>
                        {
                            O.GetComponent<Bullet>().Init();
                        }
                    );
                }
            }
        }
    }
}
