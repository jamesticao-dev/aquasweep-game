using UnityEngine;

/// <summary>
/// The laser projectile. Travels in a straight line at a fixed speed,
/// destroys itself on hitting a collectible (or after a lifetime/leaving bounds).
/// Attach to the Laser prefab (needs Rigidbody2D set to Kinematic or Gravity Scale 0,
/// and a Collider2D set to "Is Trigger").
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class Laser : MonoBehaviour
{
    [Header("Lifetime")]
    public float maxLifetime = 3f; // auto-destroy after this many seconds, in case it never hits anything

    private Rigidbody2D rb;
    private Vector2 direction;
    private float speed;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
    }

    public void Launch(Vector2 dir, float laserSpeed)
    {
        direction = dir.normalized;
        speed = laserSpeed;
        Destroy(gameObject, maxLifetime);
    }

    private void FixedUpdate()
    {
        rb.linearVelocity = direction * speed;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        Collectible collectible = other.GetComponent<Collectible>();
        if (collectible != null)
        {
            collectible.OnHitByLaser();
            Destroy(gameObject);
            return;
        }

        // Optional: destroy laser if it hits level bounds / obstacles tagged "Wall"
        if (other.CompareTag("Wall"))
        {
            Destroy(gameObject);
        }
    }
}
