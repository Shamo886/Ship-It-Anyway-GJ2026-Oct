using System;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Opening crow dialogue (1-2 pictures per level) + timed hints.
/// Keeps the existing clickable crow Button, its Sprite Swap and its audio OnClick.
/// WorldBus.LevelLoaded is Action<int> in this project.
/// </summary>
public class CrowHintController : MonoBehaviour
{
    [Serializable]
    public class LevelDialogue
    {
        public Sprite[] pictures;
    }

    [Header("Existing Crow Button")]
    [SerializeField] private Button crowButton;

    [Header("Crow and Hint Images")]
    [SerializeField] private Image autoOpenCrowOverlay;
    [SerializeField] private Image hintImage;
    [SerializeField] private Sprite openCrowSprite;

    [Header("Opening Dialogue UI")]
    [SerializeField] private Image dialogueImage;
    [SerializeField] private Button dialogueClickArea;

    [Header("Opening Dialogues (Element 0 = Level 0)")]
    [SerializeField] private LevelDialogue[] levelDialogues;

    [Header("30-Second Hint Images (Element 0 = Level 0)")]
    [SerializeField] private Sprite[] levelHintSprites;

    [Header("Hint Timing (starts AFTER dialogue ends)")]
    [Min(0f)]
    [SerializeField] private float hintDelaySeconds = 30f;

    private Image originalCrowImage;
    private float originalCrowAlpha = 1f;

    private int currentLevelIndex = -1;
    private int dialogueIndex;
    private float elapsedSeconds;
    private bool dialogueActive;
    private bool timerRunning;
    private bool hintShown;
    private bool hasWon;

    private void Awake()
    {
        if (crowButton != null)
        {
            originalCrowImage = crowButton.GetComponent<Image>();
            if (originalCrowImage != null)
            {
                originalCrowAlpha = originalCrowImage.color.a;
                originalCrowImage.raycastTarget = true; // Keep the old Button clickable.
            }

            if (openCrowSprite == null)
                openCrowSprite = crowButton.spriteState.pressedSprite;
        }

        if (autoOpenCrowOverlay != null)
        {
            autoOpenCrowOverlay.raycastTarget = false;
            if (openCrowSprite != null)
                autoOpenCrowOverlay.sprite = openCrowSprite;
        }
        if (hintImage != null)
            hintImage.raycastTarget = false;
        if (dialogueImage != null)
            dialogueImage.raycastTarget = false;
        if (dialogueClickArea != null && dialogueClickArea.image != null)
            dialogueClickArea.image.raycastTarget = true;

        if (autoOpenCrowOverlay != null && autoOpenCrowOverlay == originalCrowImage)
            Debug.LogError("CrowHint: AutoOpenCrowOverlay must not be the original crow Image.");
        if (dialogueClickArea != null && dialogueClickArea.gameObject == crowButton.gameObject)
            Debug.LogError("CrowHint: DialogueClickArea must be a separate full-screen Button.");

        HideAllVisuals();
    }

    private void OnEnable()
    {
        WorldBus.LevelLoaded += OnLevelLoaded;
        WorldBus.WorldChanged += OnWorldChanged;

        if (dialogueClickArea != null)
            dialogueClickArea.onClick.AddListener(NextDialogue);
    }

    private void OnDisable()
    {
        WorldBus.LevelLoaded -= OnLevelLoaded;
        WorldBus.WorldChanged -= OnWorldChanged;

        if (dialogueClickArea != null)
            dialogueClickArea.onClick.RemoveListener(NextDialogue);

        bool wasInDialogue = dialogueActive;
        dialogueActive = false;
        timerRunning = false;
        HideAllVisuals();
        if (wasInDialogue)
            WorldBus.SendPause(false);
    }

    private void Update()
    {
        if (!timerRunning || hasWon || hintShown || dialogueActive)
            return;

        elapsedSeconds += Time.deltaTime;
        if (elapsedSeconds >= hintDelaySeconds)
            ShowTimedHint();
    }

    private void OnLevelLoaded(int levelIndex)
    {
        currentLevelIndex = levelIndex;
        dialogueIndex = 0;
        elapsedSeconds = 0f;
        dialogueActive = false;
        timerRunning = false;
        hasWon = false;
        hintShown = false;
        HideAllVisuals();

        Sprite[] pictures = GetDialoguePictures(levelIndex);
        if (pictures != null && pictures.Length > 0)
        {
            dialogueActive = true;
            ShowDialogueFrame();
            WorldBus.SendPause(true); // Block game commands while reading.
        }
        else
        {
            timerRunning = true; // Empty dialogue array: start the 30-second timer.
        }
    }

    private Sprite[] GetDialoguePictures(int levelIndex)
    {
        if (levelDialogues == null || levelIndex < 0 || levelIndex >= levelDialogues.Length)
            return null;
        LevelDialogue level = levelDialogues[levelIndex];
        return level == null ? null : level.pictures;
    }

    private void ShowDialogueFrame()
    {
        Sprite[] pictures = GetDialoguePictures(currentLevelIndex);
        if (pictures == null || dialogueIndex >= pictures.Length)
        {
            FinishDialogue();
            return;
        }

        ShowOpenCrow();

        if (dialogueImage != null)
        {
            dialogueImage.sprite = pictures[dialogueIndex];
            dialogueImage.preserveAspect = true;
            dialogueImage.gameObject.SetActive(pictures[dialogueIndex] != null);
        }

        if (dialogueClickArea != null)
        {
            dialogueClickArea.gameObject.SetActive(true);
            // The transparent full-screen Button goes above all other UI,
            // preventing a dialogue click from also pressing game buttons.
            dialogueClickArea.transform.SetAsLastSibling();
        }
        else
        {
            Debug.LogWarning("CrowHint: DialogueClickArea is not assigned.");
        }
    }

    public void NextDialogue()
    {
        if (!dialogueActive)
            return;

        dialogueIndex++;
        Sprite[] pictures = GetDialoguePictures(currentLevelIndex);
        if (pictures == null || dialogueIndex >= pictures.Length)
            FinishDialogue();
        else
            ShowDialogueFrame();
    }

    private void FinishDialogue()
    {
        if (!dialogueActive)
            return;

        dialogueActive = false;
        if (dialogueClickArea != null)
            dialogueClickArea.gameObject.SetActive(false);
        if (dialogueImage != null)
            dialogueImage.gameObject.SetActive(false);
        HideOpenCrow();

        if (!hasWon)
        {
            elapsedSeconds = 0f;
            timerRunning = true;
        }

        WorldBus.SendPause(false);
    }

    private void OnWorldChanged(WorldStateInfo state)
    {
        if (state.levelIndex != currentLevelIndex)
            return;

        if (state.hasWon)
        {
            hasWon = true;
            timerRunning = false;
            hintShown = false;
            bool wasInDialogue = dialogueActive;
            dialogueActive = false;
            HideAllVisuals();
            if (wasInDialogue)
                WorldBus.SendPause(false);
        }
    }

    private void ShowTimedHint()
    {
        timerRunning = false;
        hintShown = true;
        ShowOpenCrow();

        if (hintImage == null)
        {
            Debug.LogWarning("CrowHint: Hint Image has not been assigned.");
            return;
        }

        if (levelHintSprites == null || currentLevelIndex < 0 ||
            currentLevelIndex >= levelHintSprites.Length ||
            levelHintSprites[currentLevelIndex] == null)
        {
            Debug.LogWarning("CrowHint: No timed hint sprite for level " + currentLevelIndex);
            hintImage.gameObject.SetActive(false);
            return;
        }

        hintImage.sprite = levelHintSprites[currentLevelIndex];
        hintImage.preserveAspect = true;
        hintImage.gameObject.SetActive(true);
    }

    private void ShowOpenCrow()
    {
        if (autoOpenCrowOverlay == null)
        {
            Debug.LogWarning("CrowHint: AutoOpenCrowOverlay is not assigned.");
            return;
        }
        if (openCrowSprite != null)
            autoOpenCrowOverlay.sprite = openCrowSprite;
        if (autoOpenCrowOverlay.sprite == null || originalCrowImage == autoOpenCrowOverlay)
        {
            Debug.LogWarning("CrowHint: Open crow sprite is missing or overlay is not separate.");
            return;
        }
        autoOpenCrowOverlay.gameObject.SetActive(true);
        SetOriginalCrowVisible(false);
    }

    private void HideOpenCrow()
    {
        SetOriginalCrowVisible(true);
        if (autoOpenCrowOverlay != null)
            autoOpenCrowOverlay.gameObject.SetActive(false);
    }

    private void SetOriginalCrowVisible(bool visible)
    {
        if (originalCrowImage == null)
            return;
        Color color = originalCrowImage.color;
        color.a = visible ? originalCrowAlpha : 0f;
        originalCrowImage.color = color;
    }

    private void HideAllVisuals()
    {
        HideOpenCrow();
        if (hintImage != null)
            hintImage.gameObject.SetActive(false);
        if (dialogueImage != null)
            dialogueImage.gameObject.SetActive(false);
        if (dialogueClickArea != null)
            dialogueClickArea.gameObject.SetActive(false);
    }
}
