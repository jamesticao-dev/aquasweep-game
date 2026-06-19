using UnityEngine;
using System;

/// <summary>
/// Central state machine for a round:
/// Playing (60s timer, shoot trash/fish) -> DropOff (timer hit 0, swim to drop-off zone)
/// -> Shop (after drop-off, spend coins) -> back to Playing (next round).
///
/// Other scripts call into this:
/// - Trash.cs calls AddCarriedTrash()
/// - Fish.cs calls ApplyTimePenalty()
/// - DropOffZone.cs calls ConvertCarriedTrashToCoins()
/// - UpgradeShop.cs calls StartNextRound() when player closes the shop
///
/// Subscribe to OnStateChanged / OnTimerUpdated from your UI scripts to update HUD text.
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public enum GameState { Playing, DropOff, Shop, GameOver }

    [Header("References")]
    public SpawnManager spawnManager;
    public Transform playerStartPoint;
    public Transform playerTransform; // assign the submarine

    [Header("Round Settings")]
    public int currentRound = 1;
    public int maxRounds = 0; // 0 = infinite rounds

    public GameState CurrentState { get; private set; } = GameState.Playing;

    private float timeRemaining;
    private int carriedTrash = 0;

    // --- Events for UI to subscribe to ---
    public event Action<float> OnTimerUpdated;          // passes timeRemaining
    public event Action<int> OnCarriedTrashChanged;      // passes carriedTrash count
    public event Action<float> OnTimePenaltyApplied;     // passes penalty amount (for floating "-5s" feedback)
    public event Action<GameState> OnStateChanged;
    public event Action<int> OnCoinsChanged;             // passes new total coins

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        StartRound();
    }

    private void Update()
    {
        if (CurrentState != GameState.Playing) return;

        timeRemaining -= Time.deltaTime;
        if (timeRemaining < 0f) timeRemaining = 0f;

        OnTimerUpdated?.Invoke(timeRemaining);

        if (timeRemaining <= 0f)
        {
            EnterDropOffPhase();
        }
    }

    // ---------- Round Flow ----------

    public void StartRound()
    {
        carriedTrash = 0;
        OnCarriedTrashChanged?.Invoke(carriedTrash);

        float roundTime = PlayerStats.Instance != null ? PlayerStats.Instance.CurrentRoundTime : 60f;
        timeRemaining = roundTime;
        OnTimerUpdated?.Invoke(timeRemaining);

        if (playerTransform != null && playerStartPoint != null)
        {
            playerTransform.position = playerStartPoint.position;
            Rigidbody2D rb = playerTransform.GetComponent<Rigidbody2D>();
            if (rb != null) rb.linearVelocity = Vector2.zero;
        }

        SetState(GameState.Playing);

        if (spawnManager != null) spawnManager.StartSpawning();
    }

    private void EnterDropOffPhase()
    {
        SetState(GameState.DropOff);
        if (spawnManager != null) spawnManager.StopSpawning();
        // UI should now show "Return to drop-off point!" prompt.
        // If carriedTrash == 0, you could skip straight to shop:
        if (carriedTrash <= 0)
        {
            EnterShopPhase();
        }
    }

    public void ConvertCarriedTrashToCoins()
    {
        if (CurrentState != GameState.DropOff) return;
        if (carriedTrash <= 0) return;

        float coinValue = PlayerStats.Instance != null ? PlayerStats.Instance.CurrentTrashCoinValue : 1f;
        int coinsEarned = Mathf.RoundToInt(carriedTrash * coinValue);

        if (PlayerStats.Instance != null)
        {
            PlayerStats.Instance.AddCoins(coinsEarned);
            OnCoinsChanged?.Invoke(PlayerStats.Instance.coins);
        }

        carriedTrash = 0;
        OnCarriedTrashChanged?.Invoke(carriedTrash);

        EnterShopPhase();
    }

    private void EnterShopPhase()
    {
        SetState(GameState.Shop);
        // UpgradeShop UI script should listen for this state and open its menu.
    }

    /// <summary>
    /// Call this from your UpgradeShop "Continue" / "Next Round" button.
    /// </summary>
    public void StartNextRound()
    {
        currentRound++;

        if (maxRounds > 0 && currentRound > maxRounds)
        {
            SetState(GameState.GameOver);
            return;
        }

        StartRound();
    }

    private void SetState(GameState newState)
    {
        CurrentState = newState;
        OnStateChanged?.Invoke(newState);
    }

    // ---------- Called by gameplay objects ----------

    public void AddCarriedTrash(int amount)
    {
        carriedTrash += amount;
        OnCarriedTrashChanged?.Invoke(carriedTrash);
    }

    public void ApplyTimePenalty(float seconds)
    {
        if (CurrentState != GameState.Playing) return;

        timeRemaining -= seconds;
        if (timeRemaining < 0f) timeRemaining = 0f;

        OnTimePenaltyApplied?.Invoke(seconds);
        OnTimerUpdated?.Invoke(timeRemaining);

        if (timeRemaining <= 0f)
        {
            EnterDropOffPhase();
        }
    }
}
