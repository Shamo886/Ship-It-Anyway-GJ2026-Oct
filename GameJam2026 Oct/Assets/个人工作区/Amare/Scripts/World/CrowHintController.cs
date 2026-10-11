using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Shows a crow hint after the player has spent the specified amount of time
/// in a level without winning.
///
/// IMPORTANT: Keep the original crow Button's Image, Sprite Swap and OnClick
/// unchanged. This controller displays a separate, non-interactive overlay
/// with the open-mouth sprite, so it cannot disrupt the Button's pressed state.
/// </summary>
public class CrowHintController : MonoBehaviour
{
    [Header("Existing Crow Button")]
    [SerializeField] private Button crowButton;

    [Header("Images (must NOT be the Button's original Image)")]
    [SerializeField] private Image autoOpenCrowOverlay;
    [SerializeField] private Image hintImage;

    [Header("Open Crow Sprite (optional if Button has Pressed Sprite)")]
    [SerializeField] private Sprite openCrowSprite;

    [Header("Hint Sprite Per Level (Element 0 = Level 0)")]
    [SerializeField] private Sprite[] levelHintSprites;

    [Header("Timing")]
    [Min(0f)]
    [SerializeField] private float hintDelaySeconds = 30f;

    // The Image on the ORIGINAL clickable crow Button.
    // Hide only its pixels during the automatic hint; do not disable the GameObject/Image.
    private Image originalCrowImage;
    private float originalCrowAlpha = 1f;

    private int currentLevelIndex = -1;
    private float elapsedSeconds;
    private bool timerRunning;
    private bool hasWon;
    private bool hintShown;

    private void Awake()
    {
        if (crowButton != null)
        {
            originalCrowImage = crowButton.GetComponent<Image>();
            if (originalCrowImage != null)
                originalCrowAlpha = originalCrowImage.color.a;
        }

        if (originalCrowImage != null && originalCrowImage == autoOpenCrowOverlay)
            Debug.LogError("CrowHint: The overlay must be a separate child Image, not the Button's own Image.");

        // Reuse B's existing Pressed Sprite if a separate sprite isn't assigned.
        if (openCrowSprite == null && crowButton != null)
            openCrowSprite = crowButton.spriteState.pressedSprite;

        if (autoOpenCrowOverlay != null)
        {
            autoOpenCrowOverlay.raycastTarget = false;
            if (openCrowSprite != null)
                autoOpenCrowOverlay.sprite = openCrowSprite;
        }

        if (hintImage != null)
            hintImage.raycastTarget = false;

        HideHintVisuals();
    }

    private void OnEnable()
    {
        WorldBus.LevelLoaded += OnLevelLoaded;
        WorldBus.WorldChanged += OnWorldChanged;
    }

    private void OnDisable()
    {
        // Also restore the original crow if the hint controller is disabled.
        HideHintVisuals();
        WorldBus.LevelLoaded -= OnLevelLoaded;
        WorldBus.WorldChanged -= OnWorldChanged;
    }

    private void Update()
    {
        if (!timerRunning || hasWon || hintShown)
            return;

        // A pause using Time.timeScale = 0 also pauses this timer.
        elapsedSeconds += Time.deltaTime;
        if (elapsedSeconds >= hintDelaySeconds)
            ShowHint();
    }

    private void OnLevelLoaded(int levelIndex)
    {
        currentLevelIndex = levelIndex;
        elapsedSeconds = 0f;
        timerRunning = true;
        hasWon = false;
        hintShown = false;
        HideHintVisuals();
    }

    private void OnWorldChanged(WorldStateInfo state)
    {
        if (currentLevelIndex != state.levelIndex)
            return;

        if (state.hasWon)
        {
            hasWon = true;
            timerRunning = false;
            HideHintVisuals();
        }
        else if (hasWon)
        {
            // Covers undoing a winning action in the final level.
            hasWon = false;
            if (elapsedSeconds >= hintDelaySeconds)
                ShowHint();
            else
                timerRunning = true;
        }
    }

    private void ShowHint()
    {
        hintShown = true;
        timerRunning = false;

        if (autoOpenCrowOverlay != null)
        {
            if (openCrowSprite != null)
                autoOpenCrowOverlay.sprite = openCrowSprite;

            if (autoOpenCrowOverlay.sprite != null && originalCrowImage != autoOpenCrowOverlay)
            {
                autoOpenCrowOverlay.gameObject.SetActive(true);
                SetOriginalCrowVisible(false); // hide only the original graphic, keep Button clickable
            }
            else
                Debug.LogWarning("CrowHint: Open crow sprite is missing.");
        }
        else
        {
            Debug.LogWarning("CrowHint: Auto Open Crow Overlay is not assigned.");
        }

        if (hintImage == null)
        {
            Debug.LogWarning("CrowHint: Hint Image is not assigned.");
            return;
        }

        if (levelHintSprites == null ||
            currentLevelIndex < 0 ||
            currentLevelIndex >= levelHintSprites.Length ||
            levelHintSprites[currentLevelIndex] == null)
        {
            Debug.LogWarning("CrowHint: Hint sprite missing for level index " + currentLevelIndex);
            hintImage.gameObject.SetActive(false);
            return;
        }

        hintImage.sprite = levelHintSprites[currentLevelIndex];
        hintImage.preserveAspect = true;
        hintImage.gameObject.SetActive(true);
    }

    private void SetOriginalCrowVisible(bool visible)
    {
        if (originalCrowImage == null)
            return;

        Color color = originalCrowImage.color;
        color.a = visible ? originalCrowAlpha : 0f;
        originalCrowImage.color = color;
        // Do not disable originalCrowImage or its GameObject: the Button needs it for clicks.
    }

    private void HideHintVisuals()
    {
        SetOriginalCrowVisible(true);
        if (autoOpenCrowOverlay != null)
            autoOpenCrowOverlay.gameObject.SetActive(false);
        if (hintImage != null)
            hintImage.gameObject.SetActive(false);
    }
}
