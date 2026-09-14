using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
using TMPro;

public class PlayerScoreAndMovement : NetworkBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5.0f;

    [Header("UI References (Local Player)")]
    [SerializeField] private TMP_Text scoreText;

    // NetworkVariable synchronizes score across all clients automatically when modified on the server.
    public NetworkVariable<int> PlayerScore = new NetworkVariable<int>(
        0,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    // Synchronize player name/ID string across clients
    public NetworkVariable<FixedString32Bytes> PlayerName = new NetworkVariable<FixedString32Bytes>(
        "",
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    public override void OnNetworkSpawn()
    {
        // Bind score change listener on all clients
        PlayerScore.OnValueChanged += OnScoreChanged;

        if (IsServer)
        {
            // Assign a readable name based on NetworkClientId
            PlayerName.Value = $"Player {OwnerClientId + 1}";
        }

        // Initialize UI display for local player
        if (IsOwner && scoreText != null)
        {
            UpdateScoreUI(PlayerScore.Value);
        }
    }

    public override void OnNetworkDespawn()
    {
        PlayerScore.OnValueChanged -= OnScoreChanged;
    }

    private void Update()
    {
        // Enforce Local Authority: Only control local player character
        if (!IsOwner) return;

        HandleMovement();
    }

    private void HandleMovement()
    {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        Vector3 moveDirection = new Vector3(horizontal, 0f, vertical).normalized;
        transform.Translate(moveDirection * moveSpeed * Time.deltaTime, Space.World);
    }

    private void OnScoreChanged(int previousValue, int newValue)
    {
        if (IsOwner && scoreText != null)
        {
            UpdateScoreUI(newValue);
        }
    }

    private void UpdateScoreUI(int currentScore)
    {
        scoreText.text = $"Score: {currentScore}";
    }

    // Server RPC: Triggered by collectible trigger on client, executed ONLY on server.
    [ServerRpc]
    public void RequestCollectServerRpc(int points, FixedString32Bytes itemTypeName)
    {
        // 1. Server updates the score NetworkVariable
        PlayerScore.Value += points;

        // 2. Server triggers ClientRPC announcement to all connected clients
        AnnouncementManager.Instance.AnnounceCollectionClientRpc(PlayerName.Value, itemTypeName, points);
    }
}