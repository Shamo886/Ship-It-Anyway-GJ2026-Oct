
using System.Collections.Generic;
using UnityEngine;

public class ProgrammingManager : MonoBehaviour
{
    [Header("Package Settings")]
    [SerializeField] private int bigCapacity = 8;
    [SerializeField] private int smallCapacity = 6;

    private readonly List<GameCommand> bigPackage =
        new List<GameCommand>();

    private readonly List<GameCommand> smallPackage =
        new List<GameCommand>();

    // History of executed Big commands.
    private readonly Stack<GameCommand> history =
        new Stack<GameCommand>();

    private bool hasWon = false;

    // True after Small has been called at least once.
    private bool smallHasBeenUsed = false;

    public IReadOnlyList<GameCommand> BigPackage => bigPackage;
    public IReadOnlyList<GameCommand> SmallPackage => smallPackage;

    public bool HasWon => hasWon;
    public bool SmallHasBeenUsed => smallHasBeenUsed;

    public bool CanUndoBig => history.Count > 0;

    public bool CanUndoSmall =>
        smallPackage.Count > 0 && !smallHasBeenUsed;

    private void OnEnable()
    {
        GameEvents.LevelWon += HandleWin;
    }

    private void OnDisable()
    {
        GameEvents.LevelWon -= HandleWin;
    }

    // Add a command to Small without executing it.
    public void AddSmallCommand(GameCommand command)
    {
        if (hasWon) return;

        if (command == GameCommand.CallSmall)
        {
            Debug.LogWarning("Small cannot call itself.");
            return;
        }

        // Small is locked after being used.
        if (smallHasBeenUsed)
        {
            Debug.LogWarning(
                "Small Package has already been used. Editing is locked."
            );
            return;
        }

        if (smallPackage.Count >= smallCapacity)
        {
            Debug.LogWarning("Small Package is full.");
            return;
        }

        smallPackage.Add(command);
        GameEvents.ProgramsChanged?.Invoke();
    }

    // Add a command to Big and execute immediately.
    public void AddBigCommand(GameCommand command)
    {
        if (hasWon) return;

        if (bigPackage.Count >= bigCapacity)
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

            // Copy the current Small commands.
            sequence.AddRange(smallPackage);

            // Lock Small after its first use.
            smallHasBeenUsed = true;
        }
        else
        {
            sequence.Add(command);
        }

        bigPackage.Add(command);
        history.Push(command);

        GameEvents.ProgramsChanged?.Invoke();

        // A / MockWorld executes the movement sequence.
        GameEvents.MoveSequenceRequested?.Invoke(sequence);
    }

    // Undo the most recent Big command.

    public void Undo()
    {
        if (history.Count == 0)
        {
            Debug.Log("Nothing to undo in Big Package.");
            return;
        }

        // Remove the latest Big command,
        // including CallSmall.
        GameCommand lastCommand = history.Pop();

        bigPackage.RemoveAt(bigPackage.Count - 1);

        // Restore the world state before this command.
        // If it was CallSmall, restore the whole sequence.
        GameEvents.UndoRequested?.Invoke();

        hasWon = false;

        // Do NOT reset smallHasBeenUsed.
        // Small stays locked after its first use.

        GameEvents.ProgramsChanged?.Invoke();

        Debug.Log("Big Package undone: " + lastCommand);
    }


    // Undo Small only if it has never been used.
    public void UndoSmall()
    {
        if (hasWon) return;

        if (smallHasBeenUsed)
        {
            Debug.LogWarning(
                "Small Package has been used and cannot be undone."
            );
            return;
        }

        if (smallPackage.Count == 0)
        {
            Debug.Log("Nothing to undo in Small Package.");
            return;
        }

        smallPackage.RemoveAt(smallPackage.Count - 1);

        GameEvents.ProgramsChanged?.Invoke();

        Debug.Log("Small Package: Last command removed.");
    }

    // Reset B's stored state.
    // The world must also be reset separately.
    public void ResetProgramming()
    {
        bigPackage.Clear();
        smallPackage.Clear();
        history.Clear();

        hasWon = false;
        smallHasBeenUsed = false;

        GameEvents.ProgramsChanged?.Invoke();

        Debug.Log("Programming reset.");
    }

    private void HandleWin()
    {
        hasWon = true;
        Debug.Log("Level completed!");
    }
}
