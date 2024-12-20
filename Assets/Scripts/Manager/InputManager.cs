using System;
using System.Collections;
using System.Collections.Generic;
using Fusion;
using Fusion.Addons.SimpleKCC;
using Fusion.Sockets;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : SimulationBehaviour, IBeforeUpdate, INetworkRunnerCallbacks
{

    public Player LocalPlayer;
    public Vector2 AccumulatedMouseDelta => mouseDeltaAccumulator.AccumulatedValue;
    private NetworkInputData accumulatedInput;
    private bool resetInput;
    private bool isTyping = false;
    private bool isOverlayActive = false;
    private Vector2Accumulator mouseDeltaAccumulator = new() { SmoothingWindow = 0.025f };
    public void BeforeUpdate()
    {
        if(resetInput){
            resetInput = false;
            accumulatedInput = default;
        }

        Keyboard keyboard = Keyboard.current;
        if (keyboard != null && isTyping == false && (keyboard.tabKey.wasPressedThisFrame || keyboard.escapeKey.wasPressedThisFrame))
        {
           if(Cursor.lockState == CursorLockMode.Locked){
               Cursor.lockState = CursorLockMode.None;
               Cursor.visible = true;
               GameEventsManager.instance.UIEvents.ToggleOverlay(true);
               isOverlayActive = true;
           } else {
               Cursor.lockState = CursorLockMode.Locked;
               Cursor.visible = false;
               GameEventsManager.instance.UIEvents.ToggleOverlay(false);
               isOverlayActive = false;
           }
        }

        if(keyboard != null &&  isOverlayActive == false && (keyboard.enterKey.wasPressedThisFrame || keyboard.numpadEnterKey.wasPressedThisFrame)){
            if(Cursor.lockState == CursorLockMode.Locked && !isTyping){
                GameEventsManager.instance.RTCEvents.ChatInputPressed(true);
                Cursor.lockState = CursorLockMode.None;
                isTyping = true;
            }else{
                GameEventsManager.instance.RTCEvents.ChatInputPressed(false);
                Cursor.lockState = CursorLockMode.Locked;
                isTyping = false;
            }
        }


        // accumulate input only if the cursor is locked
        if (Cursor.lockState != CursorLockMode.Locked){
            return;
        }

        NetworkButtons buttons = default;

        Mouse mouse = Mouse.current;
        if(mouse != null)
        {
            Vector2 mouseDelta = mouse.delta.ReadValue();
            Vector2 lookRotationDelta = new(-mouseDelta.y, mouseDelta.x);
            mouseDeltaAccumulator.Accumulate(lookRotationDelta);
        }

        if(keyboard != null){
            
            Vector2 moveDirection = Vector2.zero;

            if(keyboard.wKey.isPressed)
                moveDirection += Vector2.up;
            if(keyboard.sKey.isPressed)
                moveDirection += Vector2.down;
            if(keyboard.aKey.isPressed)
                moveDirection += Vector2.left;
            if(keyboard.dKey.isPressed)
                moveDirection += Vector2.right;

            accumulatedInput.Direction += moveDirection;
            buttons.Set(InputButton.Jump, keyboard.spaceKey.isPressed);
            buttons.Set(InputButton.Interact, keyboard.eKey.isPressed);
        }

        accumulatedInput.Buttons = new NetworkButtons(accumulatedInput.Buttons.Bits | buttons.Bits);
    }

    public void OnConnectedToServer(NetworkRunner runner)
    {

    }

    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason)
    {
        
    }

    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token)
    {
        
    }

    public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data)
    {
        
    }

    public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason)
    {
        
    }

    public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken)
    {
        
    }

    public void OnInput(NetworkRunner runner, NetworkInput input)
    {
        if (!Application.isBatchMode)
        {
            accumulatedInput.Direction.Normalize();
            accumulatedInput.LookDelta = mouseDeltaAccumulator.ConsumeTickAligned(runner);
            input.Set<NetworkInputData>(accumulatedInput);
        }

        resetInput = true;
    }

    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input)
    {
        
    }

    public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player)
    {
        
    }

    public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player)
    {
        
    }

    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {
        
    }

    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
    {
        
    }

    public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress)
    {
        
    }

    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ArraySegment<byte> data)
    {
        
    }

    public void OnSceneLoadDone(NetworkRunner runner)
    {
        
    }

    public void OnSceneLoadStart(NetworkRunner runner)
    {
        
    }

    public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList)
    {
        
    }

    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason)
    {
        
    }

    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message)
    {
        
    }

}
