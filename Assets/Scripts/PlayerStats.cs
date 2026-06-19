using UnityEngine;

/// <summary>
/// Central holder for all upgradeable player values.
/// Persists across rounds (put on a GameObject that doesn't get destroyed,
/// or make it a singleton — see GameManager for DontDestroyOnLoad pattern).
/// Other scripts (SubmarineController, LaserShooter, GameManager) read from this
/// instead of having their own hardcoded values, so upgrades apply everywhere automatically.
/// </summary>
public class PlayerStats : MonoBehaviour
{
    public static PlayerStats Instance { get; private set; }

    [Header("Movement")]
    public float baseMoveSpeed = 5f;
    public float moveSpeedUpgradeStep = 1.5f;
    public int moveSpeedLevel = 0;
    public int maxMoveSpeedLevel = 5;

    [Header("Shooting")]
    public float baseFireRate = 2f; // shots per second
    public float fireRateUpgradeStep = 0.5f;
    public int fireRateLevel = 0;
    public int maxFireRateLevel = 5;

    public float baseLaserSpeed = 12f;
    public float laserSpeedUpgradeStep = 3f;
    public int laserSpeedLevel = 0;
    public int maxLaserSpeedLevel = 5;

    [Header("Multi-shot")]
    public int baseLaserCount = 1; // lasers fired per shot
    public int laserCountLevel = 0;
    public int maxLaserCountLevel = 2; // e.g. 1 -> 2 -> 3 lasers
    public float multiShotSpreadAngle = 12f; // degrees between extra lasers

    [Header("Economy / Round")]
    public int coins = 0;
    public float baseRoundTime = 60f;
    public float bonusTimeUpgradeStep = 5f;
    public int bonusTimeLevel = 0;
    public int maxBonusTimeLevel = 4;

    public float baseTrashCoinValue = 1f;
    public float trashValueUpgradeStep = 0.5f;
    public int trashValueLevel = 0;
    public int maxTrashValueLevel = 5;

    [Header("Fish Penalty")]
    public float baseFishPenalty = 5f; // seconds lost per fish hit
    public float fishPenaltyReductionStep = 1f;
    public int fishPenaltyLevel = 0;
    public int maxFishPenaltyLevel = 4; // can't reduce below ~1s

    [Header("Magnet")]
    public float baseMagnetRadius = 0f; // 0 = no magnet effect
    public float magnetRadiusUpgradeStep = 1.5f;
    public int magnetLevel = 0;
    public int maxMagnetLevel = 4;

    [Header("Dash")]
    public bool dashUnlocked = false;
    public float dashSpeedMultiplier = 2.5f;
    public float dashDuration = 0.25f;
    public float baseDashCooldown = 4f;

    // --- Upgrade costs (simple linear scaling, tune freely) ---
    [Header("Upgrade Costs")]
    public int baseUpgradeCost = 10;
    public int costIncreasePerLevel = 5;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    // ---- Computed live values ----
    public float CurrentMoveSpeed => baseMoveSpeed + (moveSpeedLevel * moveSpeedUpgradeStep);
    public float CurrentFireRate => baseFireRate + (fireRateLevel * fireRateUpgradeStep);
    public float CurrentLaserSpeed => baseLaserSpeed + (laserSpeedLevel * laserSpeedUpgradeStep);
    public int CurrentLaserCount => baseLaserCount + laserCountLevel;
    public float CurrentRoundTime => baseRoundTime + (bonusTimeLevel * bonusTimeUpgradeStep);
    public float CurrentTrashCoinValue => baseTrashCoinValue + (trashValueLevel * trashValueUpgradeStep);
    public float CurrentFishPenalty => Mathf.Max(1f, baseFishPenalty - (fishPenaltyLevel * fishPenaltyReductionStep));
    public float CurrentMagnetRadius => baseMagnetRadius + (magnetLevel * magnetRadiusUpgradeStep);

    public int GetUpgradeCost(int currentLevel)
    {
        return baseUpgradeCost + (currentLevel * costIncreasePerLevel);
    }

    public bool TrySpendCoins(int amount)
    {
        if (coins >= amount)
        {
            coins -= amount;
            return true;
        }
        return false;
    }

    public void AddCoins(int amount)
    {
        coins += amount;
    }
}
