using Fusion;
using UnityEngine;

public class Chair : NetworkBehaviour
{
    [Networked] public bool IsOccupied { get; private set; } = false;
    [Networked] public NetworkObject OccupyingPlayer { get; private set; }

    public Transform sitPosition;

    private void OnTriggerEnter(Collider other)
    {
        if (IsOccupied || !other.CompareTag("Player")) return;

        var player = other.GetComponent<NetworkObject>();
        if (player != null && Runner.IsServer)
        {
            SitPlayer(player);
        }
    }

    [Rpc]
    private void SitPlayer(NetworkObject player)
    {
        if (player == null || IsOccupied) return;

        IsOccupied = true;
        OccupyingPlayer = player;

        var playerTransform = player.transform;
        playerTransform.position = sitPosition.position;
        playerTransform.rotation = sitPosition.rotation;
    }

    [Rpc]
    public void StandUp()
    {
        if (!IsOccupied || OccupyingPlayer == null) return;

        IsOccupied = false;
        OccupyingPlayer = null;
    }
}
