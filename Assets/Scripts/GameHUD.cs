using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Minimal HUD: shows round timer, carried trash count, and total coins.
/// Subscribes to GameManager events so it updates automatically.
/// Attach to a Canvas object and assign the Text fields.
/// Swap Text -> TMP_Text if using TextMeshPro.
/// </summary>
public class GameHUD : MonoBehaviour
{
    [Header("UI References")]
    public Text timerText;
    public Text carriedTrashText;
    public Text coinsText;
    public Text statusText; // shows prompts like "Return to drop-off!"

    private void OnEnable()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnTimerUpdated += HandleTimerUpdated;
            GameManager.Instance.OnCarriedTrashChanged += HandleCarriedTrashChanged;
            GameManager.Instance.OnCoinsChanged += HandleCoinsChanged;
            GameManager.Instance.OnStateChanged += HandleStateChanged;
            GameManager.Instance.OnTimePenaltyApplied += HandleTimePenalty;
        }
    }

    private void OnDisable()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnTimerUpdated -= HandleTimerUpdated;
            GameManager.Instance.OnCarriedTrashChanged -= HandleCarriedTrashChanged;
            GameManager.Instance.OnCoinsChanged -= HandleCoinsChanged;
            GameManager.Instance.OnStateChanged -= HandleStateChanged;
            GameManager.Instance.OnTimePenaltyApplied -= HandleTimePenalty;
        }
    }

    private void Start()
    {
        if (PlayerStats.Instance != null && coinsText != null)
        {
            coinsText.text = $"Coins: {PlayerStats.Instance.coins}";
        }
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
            case GameManager.GameState.Playing:
                statusText.text = "Shoot trash! Avoid fish!";
                break;
            case GameManager.GameState.DropOff:
                statusText.text = "Time's up! Return to the drop-off point!";
                break;
            case GameManager.GameState.Shop:
                statusText.text = "Round complete! Spend your coins.";
                break;
            case GameManager.GameState.GameOver:
                statusText.text = "Game Over!";
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
