using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Linq;
using Fusion;
using UnityEngine.UI;

public class Leaderboard : NetworkBehaviour
{
    [SerializeField] private GameObject parentGo;
    [SerializeField] private GameObject leaderboardPrefab;


    private List<Player> sortedPlayerList;
    GameLogic gameLogic;

    private void OnEnable() {
        GameEventsManager.instance.QuizEvents.OnGetLeaderboard += StartLeaderboard;
    } 

    private void OnDisable() {
        GameEventsManager.instance.QuizEvents.OnGetLeaderboard -= StartLeaderboard;
    }

    private void Start() {
        gameLogic = FindObjectOfType<GameLogic>();
        if (gameLogic == null) {
            Debug.LogError("GameLogic not found in the scene!");
        }
    }

    private void StartLeaderboard()
    {
        if (gameLogic == null)
        {
            Debug.LogError("GameLogic is null! Ensure it is assigned correctly.");
            return;
        }

        if (gameLogic.spawnedPlayers == null)
        {
            Debug.LogError("spawnedPlayers dictionary is null! Ensure it is initialized.");
            return;
        }

        sortedPlayerList = gameLogic.spawnedPlayers.Values
            .Select(networkObject => networkObject.GetComponent<Player>())
            .Where(player => player != null)
            .Where(player => player.IsDosen == 0)
            .OrderByDescending(player => player.LeaderboardScore)
            .ToList();

        foreach(var player in sortedPlayerList){
            Debug.Log($"Player: {player.PlayerName}, IsDosen: {player.IsDosen}");
        }

        var playerUid = sortedPlayerList.Select(player => player.Uid).ToList();
        var playerNames = sortedPlayerList.Select(player => player.PlayerName).ToList();
        var playerScores = sortedPlayerList.Select(player => player.LeaderboardScore).ToList();

        var leaderboardData = new LeaderboardResponseWrapper
        {
            playerUids = playerUid,
            playerNames = playerNames,
            playerScores = playerScores
        };

        string serializedLeaderboard = JsonUtility.ToJson(leaderboardData);

        if (HasStateAuthority)
        {
            Rpc_ShowLeaderboard(serializedLeaderboard);
        }
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void Rpc_ShowLeaderboard(string serializedLeaderboard)
    {
        LeaderboardResponseWrapper leaderboardData = JsonUtility.FromJson<LeaderboardResponseWrapper>(serializedLeaderboard);
        ShowLeaderBoard(leaderboardData.playerNames, leaderboardData.playerScores);
    }


    public void ShowLeaderBoard(List<string> playerNames, List<int> playerScores)
    {
        foreach (Transform child in parentGo.transform)
        {
            Destroy(child.gameObject);
        }

        for (int i = 0; i < playerNames.Count; i++)
        {
            GameObject go = Instantiate(leaderboardPrefab, parentGo.transform);
            TMP_Text usernameText = go.transform.Find("usernameText").GetComponentInChildren<TMP_Text>();
            TMP_Text scoreText = go.transform.Find("scoreText").GetComponentInChildren<TMP_Text>();

            usernameText.text = playerNames[i];
            scoreText.text = playerScores[i].ToString();

            Debug.Log(playerNames[i] + " " + playerScores[i]);
        }
        LayoutRebuilder.ForceRebuildLayoutImmediate(parentGo.GetComponent<RectTransform>());
    }


    [System.Serializable]
    public class LeaderboardResponseWrapper
    {
        public List<uint> playerUids;
        public List<string> playerNames;
        public List<int> playerScores;
    }

}
