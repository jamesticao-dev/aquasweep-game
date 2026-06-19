using UnityEngine;

/// <summary>
/// Base class for anything the laser can hit (Trash, Fish).
/// Handles the "carried by player" state: when hit, it doesn't vanish immediately —
/// it gets collected onto the submarine (visually could float behind it, but for
/// the prototype we just track counts) until dropped off.
/// Inherit from this for specific collectible types.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public abstract class Collectible : MonoBehaviour
{
    [Header("Magnet")]
    public bool magnetable = true; // can this be pulled in by the magnet upgrade?

    protected Transform playerTransform;
    private bool isBeingMagneted = false;

    protected virtual void Start()
    {
        // Cache player reference for magnet pull behavior
        SubmarineController player = FindFirstObjectByType<SubmarineController>();
        if (player != null) playerTransform = player.transform;
    }

    protected virtual void Update()
    {
        // Magnet pull: if within magnet radius, drift toward the player
        if (magnetable && playerTransform != null && PlayerStats.Instance != null)
        {
            float magnetRadius = PlayerStats.Instance.CurrentMagnetRadius;
            if (magnetRadius > 0f)
            {
                float dist = Vector2.Distance(transform.position, playerTransform.position);
                if (dist <= magnetRadius)
                {
                    isBeingMagneted = true;
                }

                if (isBeingMagneted)
                {
                    float pullSpeed = 8f; // tune as desired, or expose in PlayerStats
                    transform.position = Vector2.MoveTowards(transform.position, playerTransform.position, pullSpeed * Time.deltaTime);

                    if (dist < 0.3f)
                    {
                        OnAutoCollected();
                    }
                }
            }
        }
    }

    /// <summary>
    /// Called when a laser hits this collectible. Implemented per-type.
    /// </summary>
    public abstract void OnHitByLaser();

    /// <summary>
    /// Called when the magnet pulls this all the way to the player without needing a laser hit.
    /// Default behavior: only trash should auto-collect this way; fish should NOT (they should
    /// still require a deliberate shot, otherwise magnet would farm time penalties accidentally).
    /// Override in subclasses as needed.
    /// </summary>
    protected virtual void OnAutoCollected()
    {
        // Default: do nothing. Trash overrides this to collect itself.
    }
}
