using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class DropOffZone : MonoBehaviour
{
    [Header("Visual Feedback (optional)")]
    public GameObject convertEffectPrefab;

    private void OnTriggerEnter2D(Collider2D other)
    {
        SubmarineController player = other.GetComponent<SubmarineController>();
        if (player == null) return;

        if (GameManager.Instance == null) return;

        // Optional safety: don't allow deposit after game over
        if (GameManager.Instance.CurrentState == GameManager.GameState.GameOver)
            return;

        GameManager.Instance.ConvertCarriedTrashToCoins();

        if (convertEffectPrefab != null)
        {
            Instantiate(convertEffectPrefab, transform.position, Quaternion.identity);
        }
    }
}