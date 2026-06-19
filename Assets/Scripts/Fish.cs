using UnityEngine;

/// <summary>
/// Fish: swims around randomly (wander/patrol), and applies a TIME PENALTY when hit
/// instead of being collected. Fish should generally NOT be magnetable, so the magnet
/// upgrade can't accidentally drag fish into the player and read as a hit — magnet
/// only pulls magnetable collectibles, and OnAutoCollected does nothing for fish anyway.
/// </summary>
public class Fish : Collectible
{
    [Header("Swim Behavior")]
    public float swimSpeed = 1.5f;
    public float directionChangeIntervalMin = 1.5f;
    public float directionChangeIntervalMax = 3.5f;
    public float levelBoundsRadius = 8f; // how far from spawn point this fish wanders before turning back

    [Header("Visual (optional)")]
    public GameObject penaltyEffectPrefab; // e.g. a "-5s" floating text or splash effect

    private Vector2 swimDirection;
    private float directionTimer;
    private Vector2 spawnPoint;

    protected override void Start()
    {
        base.Start();
        magnetable = false; // fish should swim away from / ignore the magnet, not get reeled in
        spawnPoint = transform.position;
        PickNewDirection();
    }

    protected override void Update()
    {
        base.Update(); // keeps magnet-check logic harmless since magnetable = false

        directionTimer -= Time.deltaTime;
        if (directionTimer <= 0f)
        {
            PickNewDirection();
        }

        // If wandered too far from spawn point, steer back toward it
        if (Vector2.Distance(transform.position, spawnPoint) > levelBoundsRadius)
        {
            swimDirection = (spawnPoint - (Vector2)transform.position).normalized;
        }

        transform.position += (Vector3)(swimDirection * swimSpeed * Time.deltaTime);

        // Optional: rotate to face swim direction
        if (swimDirection.sqrMagnitude > 0.01f)
        {
            float angle = Mathf.Atan2(swimDirection.y, swimDirection.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0f, 0f, angle);
        }
    }

    private void PickNewDirection()
    {
        float randomAngle = Random.Range(0f, 360f);
        swimDirection = new Vector2(Mathf.Cos(randomAngle * Mathf.Deg2Rad), Mathf.Sin(randomAngle * Mathf.Deg2Rad));
        directionTimer = Random.Range(directionChangeIntervalMin, directionChangeIntervalMax);
    }

    public override void OnHitByLaser()
    {
        float penalty = PlayerStats.Instance != null ? PlayerStats.Instance.CurrentFishPenalty : 5f;

        if (GameManager.Instance != null)
        {
            GameManager.Instance.ApplyTimePenalty(penalty);
        }

        if (penaltyEffectPrefab != null)
        {
            Instantiate(penaltyEffectPrefab, transform.position, Quaternion.identity);
        }

        Destroy(gameObject);
    }
}
