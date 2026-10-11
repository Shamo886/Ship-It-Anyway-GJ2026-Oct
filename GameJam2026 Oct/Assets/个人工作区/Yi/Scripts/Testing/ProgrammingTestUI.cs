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

    [Header("Big Package Prefabs")]
    [SerializeField] private GameObject bigCommandSlotPrefab;
    [SerializeField] private GameObject bigLockedSlotPrefab;

    [Header("Small Package Prefabs")]
    [SerializeField] private GameObject smallCommandSlotPrefab;
    [SerializeField] private GameObject smallLockedSlotPrefab;

    [Header("Empty Slot Sprites")]
    [SerializeField] private Sprite bigEmptySprite;
    [SerializeField] private Sprite smallEmptySprite;

    [Header("Big Command Sprites")]
    [SerializeField] private Sprite bigUpSprite;
    [SerializeField] private Sprite bigDownSprite;
    [SerializeField] private Sprite bigLeftSprite;
    [SerializeField] private Sprite bigRightSprite;
    [SerializeField] private Sprite bigCallSmallSprite;

    [Header("Small Command Sprites")]
    [SerializeField] private Sprite smallUpSprite;
    [SerializeField] private Sprite smallDownSprite;
    [SerializeField] private Sprite smallLeftSprite;
    [SerializeField] private Sprite smallRightSprite;

    [Header("Big Panel Sprites")]
    [SerializeField] private Sprite bigNormalSprite;
    [SerializeField] private Sprite bigSelectedSprite;

    [Header("Small Panel Sprites")]
    [SerializeField] private Sprite smallNormalSprite;
    [SerializeField] private Sprite smallSelectedSprite;

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
            bigCommandSlotPrefab == null ||
            bigLockedSlotPrefab == null ||
            smallCommandSlotPrefab == null ||
            smallLockedSlotPrefab == null)
        {
            Debug.LogError(
                "ProgrammingTestUI: Missing UI references!"
            );

            enabled = false;
            return;
        }

        CreateSlots(
            bigPanel.transform,
            manager.BigTotalSlots,
            manager.BigCapacity,
            bigCommandSlotPrefab,
            bigLockedSlotPrefab,
            bigEmptySprite,
            bigImages
        );

        CreateSlots(
            smallPanel.transform,
            manager.SmallTotalSlots,
            manager.SmallCapacity,
            smallCommandSlotPrefab,
            smallLockedSlotPrefab,
            smallEmptySprite,
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
        int totalSlots,
        int unlockedSlots,
        GameObject normalPrefab,
        GameObject lockedPrefab,
        Sprite emptySprite,
        List<Image> images)
    {
        images.Clear();

        for (int i = 0; i < totalSlots; i++)
        {
            bool locked = i >= unlockedSlots;

            GameObject prefab = locked
                ? lockedPrefab
                : normalPrefab;

            GameObject slot =
                Instantiate(prefab, parent, false);

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
            {
                layout.ignoreLayout = false;
            }

            foreach (Graphic graphic in
                slot.GetComponentsInChildren<Graphic>(true))
            {
                graphic.raycastTarget = false;
            }

            if (locked)
                continue;

            Image image = slot.GetComponent<Image>();

            if (image == null)
            {
                Debug.LogError(
                    slot.name + ": Missing Image component!"
                );
                continue;
            }

            image.sprite = emptySprite;
            image.color = Color.white;

            images.Add(image);
        }
    }

    private Sprite GetBigCommandSprite(GameCommand command)
    {
        switch (command)
        {
            case GameCommand.Up:
                return bigUpSprite;

            case GameCommand.Down:
                return bigDownSprite;

            case GameCommand.Left:
                return bigLeftSprite;

            case GameCommand.Right:
                return bigRightSprite;

            case GameCommand.CallSmall:
                return bigCallSmallSprite;

            default:
                return null;
        }
    }

    private Sprite GetSmallCommandSprite(GameCommand command)
    {
        switch (command)
        {
            case GameCommand.Up:
                return smallUpSprite;

            case GameCommand.Down:
                return smallDownSprite;

            case GameCommand.Left:
                return smallLeftSprite;

            case GameCommand.Right:
                return smallRightSprite;

            default:
                return null;
        }
    }

    public void RefreshUI()
    {
        if (manager == null)
            return;

        UpdateBigSlots();
        UpdateSmallSlots();
        UpdatePanelSprites();
    }

    private void UpdateBigSlots()
    {
        for (int i = 0; i < bigImages.Count; i++)
        {
            if (i < manager.BigPackage.Count)
            {
                bigImages[i].sprite =
                    GetBigCommandSprite(manager.BigPackage[i]);
            }
            else
            {
                bigImages[i].sprite = bigEmptySprite;
            }

            bigImages[i].color = Color.white;
        }
    }

    private void UpdateSmallSlots()
    {
        for (int i = 0; i < smallImages.Count; i++)
        {
            if (i < manager.SmallPackage.Count)
            {
                smallImages[i].sprite =
                    GetSmallCommandSprite(manager.SmallPackage[i]);
            }
            else
            {
                smallImages[i].sprite = smallEmptySprite;
            }

            smallImages[i].color = Color.white;
        }
    }

    private void UpdatePanelSprites()
    {
        if (bigPanel != null && bigPanel.image != null)
        {
            bigPanel.image.sprite = editingBig
                ? bigSelectedSprite
                : bigNormalSprite;

            bigPanel.image.color = Color.white;
        }

        if (smallPanel != null && smallPanel.image != null)
        {
            smallPanel.image.sprite = editingBig
                ? smallNormalSprite
                : smallSelectedSprite;

            smallPanel.image.color = Color.white;
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
        if (manager == null)
            return;

        manager.AddBigCommand(GameCommand.CallSmall);
    }

    public void Undo()
    {
        if (manager == null)
            return;

        if (editingBig)
        {
            manager.Undo();
        }
        else
        {
            manager.UndoSmall();
        }
    }
}