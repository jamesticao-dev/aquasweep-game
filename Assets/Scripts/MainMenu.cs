using UnityEngine;
using TMPro;

/// <summary>
/// Main menu overlay shown at game start. Shows itself during GameManager.GameState.MainMenu
/// and hides on any other state. The Play button is wired to OnPlayButtonPressed(),
/// which tells GameManager to start round 1.
///
/// Setup:
/// 1. Create a Canvas (or use your existing one).
/// 2. Create a Panel as a child, name it "MainMenuPanel" — add a title Text and a "Play" Button.
/// 3. Attach this script to the Canvas (or the panel itself), assign `menuPanel`.
/// 4. Wire the Play Button's OnClick() -> this GameObject -> OnPlayButtonPressed().
/// </summary>
public class MainMenu : MonoBehaviour
{
    [Header("UI Root")]
    public GameObject menuPanel; // the panel containing title + Play button

    [Header("Optional Display")]
    public TMP_Text bestCoinsText; // e.g. show coins from a previous session, if you add persistence later

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
            Debug.LogWarning("[MainMenu] GameManager.Instance was null when trying to subscribe. " +
                "If this warning appears in OnEnable but not Start, it's an execution-order issue " +
                "(fixed automatically by the Start() retry). If it persists, GameManager is missing from the scene.");
            return;
        }

        GameManager.Instance.OnStateChanged += HandleStateChanged;
        isSubscribed = true;
        HandleStateChanged(GameManager.Instance.CurrentState);
    }

    private void OnDisable()
    {
        if (!isSubscribed || GameManager.Instance == null) return;

        GameManager.Instance.OnStateChanged -= HandleStateChanged;
        isSubscribed = false;
    }

    private void HandleStateChanged(GameManager.GameState newState)
    {
        bool isMenu = newState == GameManager.GameState.MainMenu;
        if (menuPanel != null) menuPanel.SetActive(isMenu);

        if (isMenu && PlayerStats.Instance != null && bestCoinsText != null)
        {
            bestCoinsText.text = $"Coins: {PlayerStats.Instance.coins}";
        }
    }

    /// <summary>
    /// Wire this to the Play Button's OnClick().
    /// </summary>
    public void OnPlayButtonPressed()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.PlayButtonPressed();
        }
    }
}