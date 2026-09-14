using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

public class NetworkCollectible : NetworkBehaviour
{
    [Header("Collectible Attributes")]
    [SerializeField] private string collectibleName = "Coin";
    [SerializeField] private int pointValue = 10;

    private bool isCollected = false;

    private void OnTriggerEnter(Collider other)
    {
        // Server handles collision validation and authoritative state updates
        if (!IsServer || isCollected) return;

        // Verify if colliding object is a networked player
        if (other.TryGetComponent<PlayerScoreAndMovement>(out var player))
        {
            isCollected = true;

            // Execute Server RPC call on player to award points and send announcement
            player.RequestCollectServerRpc(pointValue, new FixedString32Bytes(collectibleName));

            // Server despawns the networked object (removes it across all clients)
            GetComponent<NetworkObject>().Despawn();
        }
    }
}