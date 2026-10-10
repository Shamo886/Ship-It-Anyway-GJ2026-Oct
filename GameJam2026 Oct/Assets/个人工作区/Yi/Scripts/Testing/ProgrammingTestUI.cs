
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ProgrammingTestUI : MonoBehaviour
{
    [Header("Programming")]
    [SerializeField] private ProgrammingManager manager;

    [Header("Clickable Panels")]
    [SerializeField] private Button bigPanel;
    [SerializeField] private Button smallPanel;

    [Header("Prefabs")]
    [SerializeField] private GameObject commandSlotPrefab;
    [SerializeField] private GameObject lockedSlotPrefab;

    [Header("Test Selection Colors")]
    [SerializeField]
    private Color selectedColor =
        new Color(0.15f, 0.55f, 0.4f, 1f);

    [SerializeField]
    private Color normalColor =
        new Color(0.27f, 0.27f, 0.27f, 1f);

    private readonly List<TMP_Text> bigTexts =
        new List<TMP_Text>();

    private readonly List<TMP_Text> smallTexts =
        new List<TMP_Text>();

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
            bigTexts
        );

        CreateSlots(
            smallPanel.transform,
            manager.SmallTotalSlots,
            manager.SmallCapacity,
            smallTexts
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
        List<TMP_Text> texts)
    {
        texts.Clear();

        for (int i = 0; i < total; i++)
        {
            bool locked = i >= unlocked;

            GameObject prefab =
                locked ? lockedSlotPrefab : commandSlotPrefab;

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
                layout.ignoreLayout = false;

            foreach (Graphic graphic
                in slot.GetComponentsInChildren<Graphic>())
            {
                graphic.raycastTarget = false;
            }

            if (!locked)
            {
                TMP_Text text =
                    slot.GetComponentInChildren<TMP_Text>();

                if (text == null)
                {
                    Debug.LogError(
                        "CommandSlot prefab needs TMP_Text!"
                    );
                    continue;
                }

                text.text = "";
                texts.Add(text);
            }
        }
    }

    private string GetSymbol(GameCommand command)
    {
        switch (command)
        {
            case GameCommand.Up: return "↑";
            case GameCommand.Down: return "↓";
            case GameCommand.Left: return "←";
            case GameCommand.Right: return "→";
            case GameCommand.CallSmall: return "S";
            default: return "?";
        }
    }

    public void RefreshUI()
    {
        if (manager == null) return;

        UpdateSlots(bigTexts, manager.BigPackage);
        UpdateSlots(smallTexts, manager.SmallPackage);

        if (bigPanel != null && bigPanel.image != null)
            bigPanel.image.color =
                editingBig ? selectedColor : normalColor;

        if (smallPanel != null && smallPanel.image != null)
            smallPanel.image.color =
                editingBig ? normalColor : selectedColor;
    }

    private void UpdateSlots(
        List<TMP_Text> texts,
        IReadOnlyList<GameCommand> commands)
    {
        for (int i = 0; i < texts.Count; i++)
        {
            texts[i].text = i < commands.Count
                ? GetSymbol(commands[i])
                : "";
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
        if (manager == null) return;

        if (editingBig)
            manager.AddBigCommand(command);
        else
            manager.AddSmallCommand(command);
    }

    public void AddUp() => AddDirection(GameCommand.Up);
    public void AddDown() => AddDirection(GameCommand.Down);
    public void AddLeft() => AddDirection(GameCommand.Left);
    public void AddRight() => AddDirection(GameCommand.Right);

    public void CallSmall()
    {
        if (manager != null)
            manager.AddBigCommand(GameCommand.CallSmall);
    }

    public void Undo()
    {
        if (manager == null) return;

        if (editingBig)
            manager.Undo();
        else
            manager.UndoSmall();
    }
}
