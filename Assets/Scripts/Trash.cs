using UnityEngine;

/// <summary>
/// Trash: static (doesn't move on its own). When hit by a laser, it's added to the
/// player's "carried trash" count via GameManager, then destroyed.
/// The actual coin conversion happens at the DropOffZone, not here —
/// this just tracks "collected" trash waiting to be turned in.
/// </summary>
public class Trash : Collectible
{
    [Header("Visual (optional)")]
    public GameObject collectEffectPrefab; // particle burst on hit, optional

    public override void OnHitByLaser()
    {
        Collect();
    }

    protected override void OnAutoCollected()
    {
        // Magnet pulled it all the way in without needing a laser shot
        Collect();
    }

    private void Collect()
    {
        SoundEffectManager.Play("Collect");

        if (GameManager.Instance != null)
        {
            GameManager.Instance.AddCarriedTrash(1);
        }

        if (collectEffectPrefab != null)
        {
            Instantiate(collectEffectPrefab, transform.position, Quaternion.identity);
        }

        Destroy(gameObject);
    }
}
