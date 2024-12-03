using System;
using System.Collections;
using System.Collections.Generic;
using Fusion;
using Fusion.Sockets;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameLogic : MonoBehaviour, INetworkRunnerCallbacks
{

    private NetworkRunner _runner;
    [SerializeField] private NetworkPrefabRef playerPrefab;
    public Dictionary<PlayerRef, NetworkObject> spawnedPlayers = new Dictionary<PlayerRef, NetworkObject>();
    private HashSet<int> processedPlayerIds = new HashSet<int>();
    
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
        if (runner.IsServer)
        {
            if(SystemInfo.graphicsDeviceType != UnityEngine.Rendering.GraphicsDeviceType.Null || player != runner.LocalPlayer){
                Vector3 playerPos = new Vector3(player.RawEncoded % runner.Config.Simulation.PlayerCount * 1.5f, 1f, 0f);
                NetworkObject networkObject = runner.Spawn(playerPrefab, playerPos, Quaternion.identity, player);
                spawnedPlayers.Add(player, networkObject);

                SyncPlayerData();
            }
            
        }
    }

    public void SyncPlayerData()
    {
        foreach (var player in spawnedPlayers)
        {
            RPC_SyncPlayerData(player.Key, player.Value);
        }
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_SyncPlayerData(PlayerRef player, NetworkObject networkObject)
    {
        if (!spawnedPlayers.ContainsKey(player))
        {
            spawnedPlayers.Add(player, networkObject);
            Debug.Log($"RPC_SyncPlayerData: {player}");
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