using UnityEngine;
using TMPro;

/// <summary>
/// Upgrade shop UI logic. Shows/hides itself based on GameManager's state,
/// and exposes button-callable methods for each upgrade type.
/// Wire each method to a Button's OnClick() in the Inspector, and assign
/// Text/TMP references to display current level + cost (basic Text used here;
/// swap to TMP_Text if your project uses TextMeshPro).
///
/// This is intentionally simple/flat for prototyping — one method per upgrade.
/// </summary>
public class UpgradeShop : MonoBehaviour
{
    [Header("UI Root")]
    public GameObject shopPanel; // the whole shop UI, enabled/disabled based on game state

    [Header("Display (optional, hook up as desired)")]
    public TMP_Text coinsText;
    public TMP_Text roundText;

    private bool isSubscribed = false;

    private void OnEnable()
    {
        TrySubscribe();
    }

    private void Start()
    {
        // Fallback: if OnEnable ran before GameManager.Awake() set its Instance
        // (Unity doesn't guarantee execution order by default), try again here.
        if (!isSubscribed)
        {
            TrySubscribe();
        }
    }

    private void TrySubscribe()
    {
        if (isSubscribed) return;

        if (GameManager.Instance == null)
        {
            Debug.LogWarning("[UpgradeShop] GameManager.Instance was null when trying to subscribe. " +
                "If this warning appears in OnEnable but not Start, it's an execution-order issue " +
                "(fixed automatically by the Start() retry). If it persists, GameManager is missing from the scene.");
            return;
        }

        GameManager.Instance.OnStateChanged += HandleStateChanged;
        GameManager.Instance.OnCoinsChanged += HandleCoinsChanged;
        isSubscribed = true;

        // Push current state immediately in case GameManager already entered Shop
        // state before we subscribed (covers edge cases beyond the normal flow).
        HandleStateChanged(GameManager.Instance.CurrentState);
    }

    private void OnDisable()
    {
        if (!isSubscribed || GameManager.Instance == null) return;

        GameManager.Instance.OnStateChanged -= HandleStateChanged;
        GameManager.Instance.OnCoinsChanged -= HandleCoinsChanged;
        isSubscribed = false;
    }

    private void HandleStateChanged(GameManager.GameState newState)
    {
        bool isShop = newState == GameManager.GameState.Shop;
        if (shopPanel != null) shopPanel.SetActive(isShop);

        if (isShop)
        {
            RefreshDisplay();
        }
    }

    private void HandleCoinsChanged(int newCoinTotal)
    {
        RefreshDisplay();
    }

    private void RefreshDisplay()
    {
        if (PlayerStats.Instance == null) return;
        if (coinsText != null) coinsText.text = $"Coins: {PlayerStats.Instance.coins}";
        if (roundText != null && GameManager.Instance != null) roundText.text = $"Round {GameManager.Instance.currentRound} Complete!";
    }

    // ---------- Button-callable upgrade purchases ----------
    // Each follows the same pattern: check max level, check cost, spend, increment, refresh.

    public void BuyMoveSpeed()
    {
        var s = PlayerStats.Instance;
        if (s == null || s.moveSpeedLevel >= s.maxMoveSpeedLevel) return;
        int cost = s.GetUpgradeCost(s.moveSpeedLevel);
        if (s.TrySpendCoins(cost))
        {
            s.moveSpeedLevel++;
            RefreshDisplay();
        }
    }

    public void BuyFireRate()
    {
        var s = PlayerStats.Instance;
        if (s == null || s.fireRateLevel >= s.maxFireRateLevel) return;
        int cost = s.GetUpgradeCost(s.fireRateLevel);
        if (s.TrySpendCoins(cost))
        {
            s.fireRateLevel++;
            RefreshDisplay();
        }
    }

    public void BuyLaserSpeed()
    {
        var s = PlayerStats.Instance;
        if (s == null || s.laserSpeedLevel >= s.maxLaserSpeedLevel) return;
        int cost = s.GetUpgradeCost(s.laserSpeedLevel);
        if (s.TrySpendCoins(cost))
        {
            s.laserSpeedLevel++;
            RefreshDisplay();
        }
    }

    public void BuyMultiShot()
    {
        var s = PlayerStats.Instance;
        if (s == null || s.laserCountLevel >= s.maxLaserCountLevel) return;
        int cost = s.GetUpgradeCost(s.laserCountLevel * 3); // pricier scaling since it's strong
        if (s.TrySpendCoins(cost))
        {
            s.laserCountLevel++;
            RefreshDisplay();
        }
    }

    public void BuyBonusTime()
    {
        var s = PlayerStats.Instance;
        if (s == null || s.bonusTimeLevel >= s.maxBonusTimeLevel) return;
        int cost = s.GetUpgradeCost(s.bonusTimeLevel * 2);
        if (s.TrySpendCoins(cost))
        {
            s.bonusTimeLevel++;
            RefreshDisplay();
        }
    }

    public void BuyTrashValue()
    {
        var s = PlayerStats.Instance;
        if (s == null || s.trashValueLevel >= s.maxTrashValueLevel) return;
        int cost = s.GetUpgradeCost(s.trashValueLevel);
        if (s.TrySpendCoins(cost))
        {
            s.trashValueLevel++;
            RefreshDisplay();
        }
    }

    public void BuyFishPenaltyReduction()
    {
        var s = PlayerStats.Instance;
        if (s == null || s.fishPenaltyLevel >= s.maxFishPenaltyLevel) return;
        int cost = s.GetUpgradeCost(s.fishPenaltyLevel);
        if (s.TrySpendCoins(cost))
        {
            s.fishPenaltyLevel++;
            RefreshDisplay();
        }
    }

    public void BuyMagnet()
    {
        var s = PlayerStats.Instance;
        if (s == null || s.magnetLevel >= s.maxMagnetLevel) return;
        int cost = s.GetUpgradeCost(s.magnetLevel * 2);
        if (s.TrySpendCoins(cost))
        {
            s.magnetLevel++;
            RefreshDisplay();
        }
    }

    public void BuyDashUnlock()
    {
        var s = PlayerStats.Instance;
        if (s == null || s.dashUnlocked) return;
        int cost = 30; // flat one-time cost
        if (s.TrySpendCoins(cost))
        {
            s.dashUnlocked = true;
            RefreshDisplay();
        }
    }

    /// <summary>
    /// Wire this to the "Continue" / "Next Round" button.
    /// </summary>
    public void OnContinuePressed()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.StartNextRound();
        }
    }
}