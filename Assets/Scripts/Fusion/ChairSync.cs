using System.Collections;
using Fusion;
using UnityEngine;

public class ChairSync : NetworkBehaviour
{
    [Networked] public bool IsOccupied { get; private set; } = false;
    [Networked] public Player OccupyingPlayer { get; private set; }
    public bool IsDosen;

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void RPC_ToggleOccupancy(Player player)
    {
        if (player == null)
        {
            Debug.LogWarning("RPC_ToggleOccupancy: Player is null!");
            return;
        }

        if (IsOccupied && OccupyingPlayer == player)
        {
            SetChairState(false, null);
        }
        else if (!IsOccupied)
        {
            SetChairState(true, player);
        }
    }

    private void SetChairState(bool occupied, Player player)
    {
        if (IsOccupied == occupied && OccupyingPlayer == player)
        {
            return;
        }

        Rpc_UpdateChairState(occupied, player);
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void Rpc_UpdateChairState(bool occupied, Player player)
    {
        IsOccupied = occupied;
        OccupyingPlayer = player;

        Debug.Log($"Chair state updated: {(occupied ? "Occupied" : "Unoccupied")} by {player?.name ?? "None"}");
    }
}
