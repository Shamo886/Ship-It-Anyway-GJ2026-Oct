
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ProgrammingTestUI : MonoBehaviour
{
    [Header("Programming")]
    [SerializeField] private ProgrammingManager manager;

    [Header("Clickable Package Panels")]
    [SerializeField] private Button bigPanel;
    [SerializeField] private Button smallPanel;

    [Header("Grid Containers")]
    [SerializeField] private Transform bigGrid;
    [SerializeField] private Transform smallGrid;

    [Header("Slot Prefab")]
    [SerializeField] private GameObject slotPrefab;

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
        if (manager == null || bigPanel == null ||
            smallPanel == null || bigGrid == null ||
            smallGrid == null || slotPrefab == null)
        {
            Debug.LogError("Missing UI references!");
            enabled = false;
            return;
        }

        CreateSlots(bigGrid, 8, bigTexts);
        CreateSlots(smallGrid, 6, smallTexts);

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
        int count,
        List<TMP_Text> texts)
    {
        for (int i = 0; i < count; i++)
        {
            GameObject slot = Instantiate(slotPrefab, parent);
            slot.name = "Slot_" + (i + 1);

            Image image = slot.GetComponent<Image>();

            if (image != null)
                image.raycastTarget = false;

            TMP_Text text =
                slot.GetComponentInChildren<TMP_Text>();

            if (text == null)
            {
                Debug.LogError("CommandSlot missing TMP_Text.");
                continue;
            }

            text.raycastTarget = false;
            text.text = "";

            texts.Add(text);
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

        Image bigImage = bigPanel != null
            ? bigPanel.GetComponent<Image>() : null;

        Image smallImage = smallPanel != null
            ? smallPanel.GetComponent<Image>() : null;

        Color selected =
            new Color(0.15f, 0.55f, 0.4f, 1f);

        Color normal =
            new Color(0.27f, 0.27f, 0.27f, 1f);

        if (bigImage != null)
            bigImage.color = editingBig ? selected : normal;

        if (smallImage != null)
            smallImage.color = editingBig ? normal : selected;
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

    // Select a package by clicking its panel.
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

    // Call Small always adds an operation to Big.
    public void CallSmall()
    {
        manager.AddBigCommand(GameCommand.CallSmall);
    }

    // Undo depends on the selected package.
    public void Undo()
    {
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
