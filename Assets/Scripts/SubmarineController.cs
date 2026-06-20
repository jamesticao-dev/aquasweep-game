using UnityEngine;

/// <summary>
/// WASD movement for the submarine, plus rotation to face the mouse cursor.
/// Reads move speed from PlayerStats so upgrades apply instantly.
/// Attach to the submarine GameObject (needs a Rigidbody2D, set to Gravity Scale 0).
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class SubmarineController : MonoBehaviour
{
    [Header("References")]
    public Camera mainCamera;

    [Header("Rotation")]
    public bool rotateToFaceMouse = true;
    public float rotationOffsetDegrees = 0f; // adjust if your sprite doesn't face "right" by default

    [Header("Dash (optional, requires PlayerStats.dashUnlocked)")]
    public KeyCode dashKey = KeyCode.Space;
    private bool isDashing = false;
    private float dashTimer = 0f;
    private float dashCooldownTimer = 0f;
    private Vector2 dashDirection;

    private Rigidbody2D rb;
    private Vector2 moveInput;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        if (mainCamera == null) mainCamera = Camera.main;
    }

    private void Update()
    {
        // Freeze controls in menu/shop/game-over. Movement stays active during
        // Playing AND DropOff (player needs to swim to the drop-off zone after
        // the timer ends), so we explicitly check for the blocked states.
        if (GameManager.Instance != null)
        {
            var state = GameManager.Instance.CurrentState;
            bool controlsBlocked = state == GameManager.GameState.MainMenu
                || state == GameManager.GameState.Shop
                || state == GameManager.GameState.GameOver;

            if (controlsBlocked)
            {
                moveInput = Vector2.zero;
                rb.linearVelocity = Vector2.zero;
                return;
            }
        }

        // --- Read input ---
        float x = Input.GetAxisRaw("Horizontal"); // A/D
        float y = Input.GetAxisRaw("Vertical");   // W/S
        moveInput = new Vector2(x, y).normalized;

        // --- Rotate to face mouse ---
        if (rotateToFaceMouse && mainCamera != null)
        {
            Vector3 mouseWorldPos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
            mouseWorldPos.z = 0f;
            Vector2 direction = (mouseWorldPos - transform.position);
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0f, 0f, angle + rotationOffsetDegrees);
        }

        // --- Dash input ---
        if (PlayerStats.Instance != null && PlayerStats.Instance.dashUnlocked)
        {
            dashCooldownTimer -= Time.deltaTime;
            if (Input.GetKeyDown(dashKey) && dashCooldownTimer <= 0f && !isDashing && moveInput.sqrMagnitude > 0.01f)
            {
                isDashing = true;
                dashTimer = PlayerStats.Instance.dashDuration;
                dashCooldownTimer = PlayerStats.Instance.baseDashCooldown;
                dashDirection = moveInput;
            }
            if (isDashing)
            {
                dashTimer -= Time.deltaTime;
                if (dashTimer <= 0f) isDashing = false;
            }
        }
    }

    private void FixedUpdate()
    {
        float speed = PlayerStats.Instance != null ? PlayerStats.Instance.CurrentMoveSpeed : 5f;

        if (isDashing)
        {
            float dashMultiplier = PlayerStats.Instance != null ? PlayerStats.Instance.dashSpeedMultiplier : 2.5f;
            rb.linearVelocity = dashDirection * speed * dashMultiplier;
        }
        else
        {
            rb.linearVelocity = moveInput * speed;
        }
    }
}