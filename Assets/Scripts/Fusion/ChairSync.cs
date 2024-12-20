using Fusion;
using UnityEngine;

public class ChairSync : NetworkBehaviour
{
    [Networked] public bool IsOccupied { get; private set; } = false;
    [Networked] public Player OccupyingPlayer { get; private set; }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void RPC_ToggleOccupancy(Player player)
    {
        Debug.Log($"RPC_ToggleOccupancy called by: {player} | StateAuthority: {Object.HasStateAuthority}");
        if (IsOccupied && OccupyingPlayer == player)
        {
            SetChairState(false, null);
        }
        else if (!IsOccupied)
        {
            SetChairState(true, player);
        }

        Debug.Log($"Chair state toggled");
    }

    private void SetChairState(bool occupied, Player player){
        if (IsOccupied == occupied && OccupyingPlayer == player) return;
        Rpc_UpdateChairState(occupied, player);
        Debug.Log($"Chair state updated: {(IsOccupied ? "Occupied" : "Unoccupied")}");
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void Rpc_UpdateChairState(bool occupied, Player player){
        IsOccupied = occupied;
        OccupyingPlayer = player;
    }

}
