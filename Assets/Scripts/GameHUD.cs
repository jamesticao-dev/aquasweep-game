using UnityEngine;
using TMPro;

/// <summary>
/// Minimal HUD: shows round timer, carried trash count, and total coins.
/// Subscribes to GameManager events so it updates automatically.
/// Attach to a Canvas object and assign the Text fields.
/// Swap Text -> TMP_Text if using TextMeshPro.
/// </summary>
public class GameHUD : MonoBehaviour
{
    [Header("UI References")]
    public TMP_Text timerText;
    public TMP_Text carriedTrashText;
    public TMP_Text coinsText;
    public TMP_Text statusText; // shows prompts like "Return to drop-off!"

    private bool isSubscribed = false;

    private void OnEnable()
    {
        TrySubscribe();
    }

    private void OnDisable()
    {
        Unsubscribe();
    }

    private void Start()
    {
        // Fallback: if OnEnable ran before GameManager.Awake() set its Instance
        // (Unity doesn't guarantee execution order by default), try again here.
        if (!isSubscribed)
        {
            TrySubscribe();
        }

        if (PlayerStats.Instance != null && coinsText != null)
        {
            coinsText.text = $"Highest coins earned: {PlayerStats.Instance.coins}";
        }
        else if (PlayerStats.Instance == null)
        {
            Debug.LogWarning("[GameHUD] PlayerStats.Instance is null in Start(). " +
                "Make sure a PlayerStats object exists in the scene and has run its Awake() before this.");
        }
    }

    private void TrySubscribe()
    {
        if (isSubscribed) return;

        if (GameManager.Instance == null)
        {
            Debug.LogWarning("[GameHUD] GameManager.Instance was null when trying to subscribe. " +
                "If this warning appears in OnEnable but not Start, it's an execution-order issue " +
                "(fixed automatically by the Start() retry). If it persists, GameManager is missing from the scene.");
            return;
        }

        GameManager.Instance.OnTimerUpdated += HandleTimerUpdated;
        GameManager.Instance.OnCarriedTrashChanged += HandleCarriedTrashChanged;
        GameManager.Instance.OnCoinsChanged += HandleCoinsChanged;
        GameManager.Instance.OnStateChanged += HandleStateChanged;
        GameManager.Instance.OnTimePenaltyApplied += HandleTimePenalty;
        isSubscribed = true;

        // Immediately push current values so the HUD isn't blank/stale until the next change
        // (in case GameManager.Start() already ran its StartRound() before we subscribed).
        HandleStateChanged(GameManager.Instance.CurrentState);
        if (PlayerStats.Instance != null)
        {
            HandleTimerUpdated(PlayerStats.Instance.CurrentRoundTime);
        }
        HandleCarriedTrashChanged(0);
    }

    private void Unsubscribe()
    {
        if (!isSubscribed || GameManager.Instance == null) return;

        GameManager.Instance.OnTimerUpdated -= HandleTimerUpdated;
        GameManager.Instance.OnCarriedTrashChanged -= HandleCarriedTrashChanged;
        GameManager.Instance.OnCoinsChanged -= HandleCoinsChanged;
        GameManager.Instance.OnStateChanged -= HandleStateChanged;
        GameManager.Instance.OnTimePenaltyApplied -= HandleTimePenalty;
        isSubscribed = false;
    }

    private void HandleTimerUpdated(float timeRemaining)
    {
        if (timerText == null) return;
        int seconds = Mathf.CeilToInt(timeRemaining);
        timerText.text = $"{seconds}s";
    }

    private void HandleCarriedTrashChanged(int count)
    {
        if (carriedTrashText != null) carriedTrashText.text = $"Trash: {count}";
    }

    private void HandleCoinsChanged(int coins)
    {
        if (coinsText != null) coinsText.text = $"Coins: {coins}";
    }

    private void HandleStateChanged(GameManager.GameState state)
    {
        if (statusText == null) return;

        switch (state)
        {
            case GameManager.GameState.MainMenu:
                statusText.text = "";
                if (timerText != null) timerText.text = "";
                if (carriedTrashText != null) carriedTrashText.text = "";
                break;
            case GameManager.GameState.Playing:
                statusText.text = "Shoot trash and deposit anytime!";
                break;
            /*case GameManager.GameState.DropOff:
                statusText.text = "Time's up! Return to the drop-off point!";
                break;*/
            case GameManager.GameState.Shop:
                statusText.text = "Round complete! Spend your coins.";
                break;
            case GameManager.GameState.GameOver:
                statusText.text = "Times up! Game Over!";
                break;
        }
    }

    private void HandleTimePenalty(float penaltyAmount)
    {
        // Hook for floating "-5s" feedback text, screen flash, sound, etc.
        // Simple version: briefly flash status text.
        if (statusText != null)
        {
            statusText.text = $"Hit a fish! -{penaltyAmount:0}s";
        }
    }
}