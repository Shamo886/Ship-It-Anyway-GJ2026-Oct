
using System.Collections.Generic;
using UnityEngine;

public class ProgrammingManager : MonoBehaviour
{
    [Header("Big Package Settings")]
    [Min(1)]
    [SerializeField] private int bigTotalSlots = 8;
    [Min(0)]
    [SerializeField] private int bigUnlockedSlots = 8;

    [Header("Small Package Settings")]
    [Min(1)]
    [SerializeField] private int smallTotalSlots = 6;
    [Min(0)]
    [SerializeField] private int smallUnlockedSlots = 6;

    private readonly List<GameCommand> bigPackage =
        new List<GameCommand>();

    private readonly List<GameCommand> smallPackage =
        new List<GameCommand>();

    private readonly Stack<GameCommand> history =
        new Stack<GameCommand>();

    private bool hasWon = false;
    private bool smallHasBeenUsed = false;

    public IReadOnlyList<GameCommand> BigPackage => bigPackage;
    public IReadOnlyList<GameCommand> SmallPackage => smallPackage;

    public int BigTotalSlots => bigTotalSlots;
    public int SmallTotalSlots => smallTotalSlots;

    public int BigCapacity =>
        Mathf.Clamp(bigUnlockedSlots, 0, bigTotalSlots);

    public int SmallCapacity =>
        Mathf.Clamp(smallUnlockedSlots, 0, smallTotalSlots);

    public bool HasWon => hasWon;
    public bool SmallHasBeenUsed => smallHasBeenUsed;
    public bool CanUndoBig => history.Count > 0;

    public bool CanUndoSmall =>
        smallPackage.Count > 0 &&
        !smallHasBeenUsed &&
        !hasWon;

    private void OnValidate()
    {
        bigTotalSlots = Mathf.Max(1, bigTotalSlots);
        smallTotalSlots = Mathf.Max(1, smallTotalSlots);

        bigUnlockedSlots =
            Mathf.Clamp(bigUnlockedSlots, 0, bigTotalSlots);

        smallUnlockedSlots =
            Mathf.Clamp(smallUnlockedSlots, 0, smallTotalSlots);
    }

    private void OnEnable()
    {
        GameEvents.LevelWon += HandleWin;
    }

    private void OnDisable()
    {
        GameEvents.LevelWon -= HandleWin;
    }

    public void AddSmallCommand(GameCommand command)
    {
        if (hasWon || smallHasBeenUsed) return;
        if (command == GameCommand.CallSmall) return;

        if (smallPackage.Count >= SmallCapacity)
        {
            Debug.LogWarning("Small Package is full.");
            return;
        }

        smallPackage.Add(command);
        GameEvents.ProgramsChanged?.Invoke();
    }

    public void AddBigCommand(GameCommand command)
    {
        if (hasWon) return;

        if (bigPackage.Count >= BigCapacity)
        {
            Debug.LogWarning("Big Package is full.");
            return;
        }

        List<GameCommand> sequence =
            new List<GameCommand>();

        if (command == GameCommand.CallSmall)
        {
            if (smallPackage.Count == 0)
            {
                Debug.LogWarning("Small Package is empty.");
                return;
            }

            sequence.AddRange(smallPackage);
            smallHasBeenUsed = true;
        }
        else
        {
            sequence.Add(command);
        }

        bigPackage.Add(command);
        history.Push(command);

        GameEvents.ProgramsChanged?.Invoke();
        GameEvents.MoveSequenceRequested?.Invoke(sequence);
    }

    // Big Undo always works, including CallSmall.
    public void Undo()
    {
        if (history.Count == 0) return;

        GameCommand last = history.Pop();
        bigPackage.RemoveAt(bigPackage.Count - 1);

        GameEvents.UndoRequested?.Invoke();

        hasWon = false;
        GameEvents.ProgramsChanged?.Invoke();

        Debug.Log("Big Undo: " + last);
    }

    // Small editing is locked after first use.
    public void UndoSmall()
    {
        if (!CanUndoSmall) return;

        smallPackage.RemoveAt(smallPackage.Count - 1);
        GameEvents.ProgramsChanged?.Invoke();
    }

    public void ResetProgramming()
    {
        bigPackage.Clear();
        smallPackage.Clear();
        history.Clear();

        hasWon = false;
        smallHasBeenUsed = false;

        GameEvents.ProgramsChanged?.Invoke();
    }

    private void HandleWin()
    {
        hasWon = true;
        Debug.Log("Level completed!");
    }
}
