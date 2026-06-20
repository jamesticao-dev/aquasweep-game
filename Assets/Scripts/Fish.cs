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

    [Header("Movement Bounds")]
    public Vector2 boundsCenter = Vector2.zero;
    public Vector2 boundsSize = new Vector2(16f, 9f);

    [Header("Death")]
    public float deathDelay = 1f;

    private bool isDead = false;
    private Animator animator;
    private Collider2D col;

    protected override void Start()
    {

        if (isDead)
        return;

        base.Update();
        base.Start();

        magnetable = false;
        spawnPoint = transform.position;
        PickNewDirection();

        animator = GetComponent<Animator>();
        col = GetComponent<Collider2D>();
    }

    protected override void Update()
    {
        base.Update(); // keeps magnet-check logic harmless since magnetable = false

        directionTimer -= Time.deltaTime;
        if (directionTimer <= 0f)
        {
            PickNewDirection();
        }

        float minX = boundsCenter.x - boundsSize.x / 2f;
        float maxX = boundsCenter.x + boundsSize.x / 2f;
        float minY = boundsCenter.y - boundsSize.y / 2f;
        float maxY = boundsCenter.y + boundsSize.y / 2f;

        Vector2 pos = transform.position;

        // Turn around if reaching edges
        if (pos.x <= minX || pos.x >= maxX)
        {
            swimDirection.x *= -1;
        }

        if (pos.y <= minY || pos.y >= maxY)
        {
            swimDirection.y *= -1;
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
        if (isDead)
            return;

        isDead = true;

        SoundEffectManager.Play("Vaporized");

        if (GameManager.Instance != null)
        {
            GameManager.Instance.AddFishKill();
        }

        float penalty = PlayerStats.Instance != null ?
                        PlayerStats.Instance.CurrentFishPenalty : 5f;

        if (GameManager.Instance != null)
        {
            GameManager.Instance.ApplyTimePenalty(penalty);
        }

        if (penaltyEffectPrefab != null)
        {
            Instantiate(penaltyEffectPrefab, transform.position, Quaternion.identity);
        }

        if (col != null)
            col.enabled = false;

        if (animator != null)
            animator.SetTrigger("Die");

        Destroy(gameObject, deathDelay);
    }
}
