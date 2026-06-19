using UnityEngine;

/// <summary>
/// Fires projectile lasers from the submarine toward the mouse world position.
/// Reads fire rate, laser speed, and laser count from PlayerStats.
/// Attach to the submarine (or a child "FirePoint" object).
/// </summary>
public class LaserShooter : MonoBehaviour
{
    [Header("References")]
    public Camera mainCamera;
    public Transform firePoint;       // where lasers spawn from (defaults to this transform)
    public GameObject laserPrefab;    // assign the Laser prefab in inspector

    [Header("Input")]
    public bool holdToFire = false;   // false = click each shot, true = hold mouse to auto-fire at fire rate

    private float fireCooldownTimer = 0f;

    private void Awake()
    {
        if (mainCamera == null) mainCamera = Camera.main;
        if (firePoint == null) firePoint = transform;
    }

    private void Update()
    {
        fireCooldownTimer -= Time.deltaTime;

        bool wantsToFire = holdToFire ? Input.GetMouseButton(0) : Input.GetMouseButtonDown(0);

        if (wantsToFire && fireCooldownTimer <= 0f)
        {
            Fire();
            float fireRate = PlayerStats.Instance != null ? PlayerStats.Instance.CurrentFireRate : 2f;
            fireCooldownTimer = 1f / Mathf.Max(0.01f, fireRate);
        }
    }

    private void Fire()
    {
        if (laserPrefab == null || mainCamera == null) return;

        Vector3 mouseWorldPos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        mouseWorldPos.z = 0f;
        Vector2 baseDirection = (mouseWorldPos - firePoint.position).normalized;

        int laserCount = PlayerStats.Instance != null ? PlayerStats.Instance.CurrentLaserCount : 1;
        float spread = PlayerStats.Instance != null ? PlayerStats.Instance.multiShotSpreadAngle : 12f;
        float laserSpeed = PlayerStats.Instance != null ? PlayerStats.Instance.CurrentLaserSpeed : 12f;

        if (laserCount <= 1)
        {
            SpawnLaser(baseDirection, laserSpeed);
        }
        else
        {
            // Spread shots evenly around the base direction
            float totalSpread = spread * (laserCount - 1);
            float startAngle = -totalSpread / 2f;

            for (int i = 0; i < laserCount; i++)
            {
                float angleOffset = startAngle + (spread * i);
                Vector2 dir = Quaternion.Euler(0, 0, angleOffset) * baseDirection;
                SpawnLaser(dir, laserSpeed);
            }
        }
    }

    private void SpawnLaser(Vector2 direction, float speed)
    {
        GameObject laserObj = Instantiate(laserPrefab, firePoint.position, Quaternion.identity);

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        laserObj.transform.rotation = Quaternion.Euler(0f, 0f, angle);

        Laser laser = laserObj.GetComponent<Laser>();
        if (laser != null)
        {
            laser.Launch(direction, speed);
        }
    }
}
