using UnityEngine;

/// <summary>
/// Trigger zone the player swims into to convert carried trash into coins.
/// Attach to a GameObject with a Collider2D set to "Is Trigger".
/// Only active/relevant once GameManager enters the "DropOff" phase (after timer ends),
/// but works any time the player enters it for flexibility (e.g. mid-round partial turn-ins
/// if you want that later).
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class DropOffZone : MonoBehaviour
{
    [Header("Settings")]
    public bool onlyActiveDuringDropOffPhase = true;

    [Header("Visual Feedback (optional)")]
    public GameObject convertEffectPrefab;

    private void OnTriggerEnter2D(Collider2D other)
    {
        SubmarineController player = other.GetComponent<SubmarineController>();
        if (player == null) return;

        if (onlyActiveDuringDropOffPhase && GameManager.Instance != null)
        {
            if (GameManager.Instance.CurrentState != GameManager.GameState.DropOff) return;
        }

        if (GameManager.Instance != null)
        {
            GameManager.Instance.ConvertCarriedTrashToCoins();
        }

        if (convertEffectPrefab != null)
        {
            Instantiate(convertEffectPrefab, transform.position, Quaternion.identity);
        }
    }
}
