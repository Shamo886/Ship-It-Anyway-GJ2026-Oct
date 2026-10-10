
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ProgrammingTestUI : MonoBehaviour
{
    [Header("Programming")]
    [SerializeField] private ProgrammingManager manager;

    [Header("Clickable Panels")]
    [SerializeField] private Button bigPanel;
    [SerializeField] private Button smallPanel;

    [Header("Slot Prefabs")]
    [SerializeField] private GameObject commandSlotPrefab;
    [SerializeField] private GameObject lockedSlotPrefab;

    [Header("Command Sprites")]
    [SerializeField] private Sprite emptySlotSprite;
    [SerializeField] private Sprite upSprite;
    [SerializeField] private Sprite downSprite;
    [SerializeField] private Sprite leftSprite;
    [SerializeField] private Sprite rightSprite;
    [SerializeField] private Sprite callSmallSprite;

    [Header("Test Selection Colors")]
    [SerializeField]
    private Color selectedColor =
        new Color(0.15f, 0.55f, 0.4f, 1f);

    [SerializeField]
    private Color normalColor =
        new Color(0.27f, 0.27f, 0.27f, 1f);

    private readonly List<Image> bigImages =
        new List<Image>();

    private readonly List<Image> smallImages =
        new List<Image>();

    private bool editingBig = true;

    private void OnEnable()
    {
        GameEvents.ProgramsChanged += RefreshUI;
    }

    private void OnDisable()
    {
        GameEvents.ProgramsChanged -= RefreshUI;
    }

    private void Start()
    {
        if (manager == null ||
            bigPanel == null ||
            smallPanel == null ||
            commandSlotPrefab == null ||
            lockedSlotPrefab == null)
        {
            Debug.LogError("Missing Programming UI references!");
            enabled = false;
            return;
        }

        CreateSlots(
            bigPanel.transform,
            manager.BigTotalSlots,
            manager.BigCapacity,
            bigImages
        );

        CreateSlots(
            smallPanel.transform,
            manager.SmallTotalSlots,
            manager.SmallCapacity,
            smallImages
        );

        bigPanel.onClick.AddListener(SelectBig);
        smallPanel.onClick.AddListener(SelectSmall);

        RefreshUI();
    }

    private void OnDestroy()
    {
        if (bigPanel != null)
            bigPanel.onClick.RemoveListener(SelectBig);

        if (smallPanel != null)
            smallPanel.onClick.RemoveListener(SelectSmall);
    }

    private void CreateSlots(
        Transform parent,
        int total,
        int unlocked,
        List<Image> images)
    {
        images.Clear();

        for (int i = 0; i < total; i++)
        {
            bool locked = i >= unlocked;

            GameObject prefab = locked
                ? lockedSlotPrefab
                : commandSlotPrefab;

            GameObject slot = Instantiate(prefab, parent, false);

            slot.name = locked
                ? "LockedSlot_" + (i + 1)
                : "Slot_" + (i + 1);

            RectTransform rect =
                slot.GetComponent<RectTransform>();

            if (rect != null)
            {
                rect.localScale = Vector3.one;
                rect.localRotation = Quaternion.identity;
            }

            LayoutElement layout =
                slot.GetComponent<LayoutElement>();

            if (layout != null)
                layout.ignoreLayout = false;

            // Allow clicks to pass through to Panel.
            foreach (Graphic graphic in
                slot.GetComponentsInChildren<Graphic>())
            {
                graphic.raycastTarget = false;
            }

            // Locked slots keep their own prefab appearance.
            if (locked)
                continue;

            Image image = slot.GetComponent<Image>();

            if (image == null)
            {
                Debug.LogError(
                    "CommandSlot prefab needs an Image component!"
                );
                continue;
            }

            image.color = Color.white;
            image.sprite = emptySlotSprite;

            images.Add(image);
        }
    }

    // Choose the correct image for each command.
    private Sprite GetCommandSprite(GameCommand command)
    {
        switch (command)
        {
            case GameCommand.Up:
                return upSprite;

            case GameCommand.Down:
                return downSprite;

            case GameCommand.Left:
                return leftSprite;

            case GameCommand.Right:
                return rightSprite;

            case GameCommand.CallSmall:
                return callSmallSprite;

            default:
                return emptySlotSprite;
        }
    }

    public void RefreshUI()
    {
        if (manager == null)
            return;

        UpdateSlots(bigImages, manager.BigPackage);
        UpdateSlots(smallImages, manager.SmallPackage);

        if (bigPanel != null && bigPanel.image != null)
        {
            bigPanel.image.color =
                editingBig ? selectedColor : normalColor;
        }

        if (smallPanel != null && smallPanel.image != null)
        {
            smallPanel.image.color =
                editingBig ? normalColor : selectedColor;
        }
    }

    // Update the Image component instead of TMP text.
    private void UpdateSlots(
        List<Image> images,
        IReadOnlyList<GameCommand> commands)
    {
        for (int i = 0; i < images.Count; i++)
        {
            Sprite sprite = i < commands.Count
                ? GetCommandSprite(commands[i])
                : emptySlotSprite;

            images[i].sprite = sprite;
            images[i].color = Color.white;
        }
    }

    public void SelectBig()
    {
        editingBig = true;
        RefreshUI();
    }

    public void SelectSmall()
    {
        editingBig = false;
        RefreshUI();
    }

    private void AddDirection(GameCommand command)
    {
        if (manager == null)
            return;

        if (editingBig)
        {
            manager.AddBigCommand(command);
        }
        else
        {
            manager.AddSmallCommand(command);
        }
    }

    public void AddUp()
    {
        AddDirection(GameCommand.Up);
    }

    public void AddDown()
    {
        AddDirection(GameCommand.Down);
    }

    public void AddLeft()
    {
        AddDirection(GameCommand.Left);
    }

    public void AddRight()
    {
        AddDirection(GameCommand.Right);
    }

    public void CallSmall()
    {
        if (manager != null)
            manager.AddBigCommand(GameCommand.CallSmall);
    }

    public void Undo()
    {
        if (manager == null)
            return;

        if (editingBig)
            manager.Undo();
        else
            manager.UndoSmall();
    }
}
