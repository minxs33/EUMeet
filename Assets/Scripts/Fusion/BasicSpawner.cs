using System;
using System.Collections.Generic;
using Fusion;
using Fusion.Sockets;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameLogic : MonoBehaviour, INetworkRunnerCallbacks
{

    private NetworkRunner _runner;
    [SerializeField] private NetworkPrefabRef playerPrefab;

    private Dictionary<PlayerRef, NetworkObject> spawnedPlayers = new Dictionary<PlayerRef, NetworkObject>();

    private void Start(){
        if(SystemInfo.graphicsDeviceType == UnityEngine.Rendering.GraphicsDeviceType.Null)
        {
            GameStart(GameMode.Host);
        }else{
            GameStart(GameMode.Client);
        }
    }
    async void GameStart(GameMode mode)
    {
        // Getting a network runner and user is giving the input
        _runner = gameObject.GetComponent<NetworkRunner>();
        _runner.ProvideInput = true;
        
        var scene = SceneRef.FromIndex(SceneManager.GetSceneByBuildIndex(1).buildIndex);
        var sceneInfo = new NetworkSceneInfo();

        Debug.Log(scene);
        if(scene.IsValid)
        {
            sceneInfo.AddSceneRef(scene, LoadSceneMode.Single);
        }

        // Create Lobby Session
        await _runner.StartGame( new StartGameArgs(){
            GameMode = mode,
            SessionName = "Lobby",
            Scene=scene,
            SceneManager = gameObject.AddComponent<NetworkSceneManagerDefault>(),
        });

    }

    public void OnConnectedToServer(NetworkRunner runner)
    {
        // masuk channel RTC lobby
        // if(Application.isBatchMode){
        //     Application.targetFrameRate = 30;
        //     GameStart(GameMode.Host);
        // }else{
        //     GameStart(GameMode.Client);
        // }
    }

    // private void OnGUI()
    // {
    //     if(_runner == null){
    //         if(GUI.Button(new Rect(10,10,100,30),"Host")){
    //             GameStart(GameMode.Host);
    //         }else if(GUI.Button(new Rect(10,50,100,30),"Client")){
    //             GameStart(GameMode.Client);
    //         }
    //     }
    // }
    

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
        if(runner.IsServer){
            if (SystemInfo.graphicsDeviceType != UnityEngine.Rendering.GraphicsDeviceType.Null || player != runner.LocalPlayer){
                Vector3 playerPos = new Vector3(player.RawEncoded % runner.Config.Simulation.PlayerCount * 3f,1f,0f);

                NetworkObject networkObject = runner.Spawn(playerPrefab, playerPos, Quaternion.identity, player);
                
                spawnedPlayers.Add(player, networkObject);
            }else{
                Debug.Log("This is the client, no player is instantiated");
                GameEventsManager.instance.RTCEvents.Mute();
            }
        }
        
    }

    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
    {
        if(spawnedPlayers.TryGetValue(player, out NetworkObject networkObject)){
            runner.Despawn(networkObject);
            spawnedPlayers.Remove(player);
        }
        
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
