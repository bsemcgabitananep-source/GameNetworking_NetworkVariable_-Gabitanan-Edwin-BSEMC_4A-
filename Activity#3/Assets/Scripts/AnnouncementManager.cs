using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
using TMPro;
using System.Collections;

public class AnnouncementManager : NetworkBehaviour
{
    public static AnnouncementManager Instance { get; private set; }

    [Header("UI Component")]
    [SerializeField] private TMP_Text announcementText;
    [SerializeField] private float displayDuration = 3.0f;

    private Coroutine clearMessageCoroutine;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    // ClientRPC: Called by Server, executed on ALL connected Clients and Host
    [ClientRpc]
    public void AnnounceCollectionClientRpc(FixedString32Bytes playerName, FixedString32Bytes itemTypeName, int pointsAwarded)
    {
        string message = $"{playerName} collected a {itemTypeName}! +{pointsAwarded} Points";

        if (announcementText != null)
        {
            announcementText.text = message;

            if (clearMessageCoroutine != null)
            {
                StopCoroutine(clearMessageCoroutine);
            }
            clearMessageCoroutine = StartCoroutine(ClearMessageAfterDelay());
        }

        Debug.Log($"[ANNOUNCEMENT] {message}");
    }

    private IEnumerator ClearMessageAfterDelay()
    {
        yield return new WaitForSeconds(displayDuration);
        if (announcementText != null)
        {
            announcementText.text = "";
        }
    }
}