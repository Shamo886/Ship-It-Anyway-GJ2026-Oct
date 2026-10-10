
using System.Collections.Generic;
using UnityEngine;

public class ProgrammingManager : MonoBehaviour
{
    [Header("Package Settings")]
    [SerializeField] private int bigCapacity = 8;
    [SerializeField] private int smallCapacity = 6;

    // Commands currently stored in the two packages.
    private readonly List<GameCommand> bigPackage =
        new List<GameCommand>();

    private readonly List<GameCommand> smallPackage =
        new List<GameCommand>();

    // One record for each command added to Big.
    private readonly Stack<GameCommand> history =
        new Stack<GameCommand>();

    private bool hasWon = false;

    // Other scripts can read these lists, but not modify them.
    public IReadOnlyList<GameCommand> BigPackage => bigPackage;
    public IReadOnlyList<GameCommand> SmallPackage => smallPackage;

    public bool HasWon => hasWon;
    public bool CanUndo => history.Count > 0;

    private void OnEnable()
    {
        GameEvents.LevelWon += HandleWin;
    }

    private void OnDisable()
    {
        GameEvents.LevelWon -= HandleWin;
    }

    // Add a command to the small package.
    // This does NOT move the player.
    public void AddSmallCommand(GameCommand command)
    {
        if (hasWon) return;

        if (command == GameCommand.CallSmall)
        {
            Debug.LogWarning("Small cannot call itself.");
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

    // Add a command to the big package.
    // This immediately requests execution.
    public void AddBigCommand(GameCommand command)
    {
        if (hasWon) return;

        if (bigPackage.Count >= bigCapacity)
        {
            Debug.LogWarning("Big Package is full.");
            return;
        }

        if (command == GameCommand.CallSmall &&
            smallPackage.Count == 0)
        {
            Debug.LogWarning("Small Package is empty.");
            return;
        }

        List<GameCommand> sequence =
            new List<GameCommand>();

        if (command == GameCommand.CallSmall)
        {
            // Expand Small into movement commands.
            sequence.AddRange(smallPackage);
        }
        else
        {
            sequence.Add(command);
        }

        // Save the Big command before execution.
        bigPackage.Add(command);
        history.Push(command);

        GameEvents.ProgramsChanged?.Invoke();

        // A handles movement, collisions and tile changes.
        GameEvents.MoveSequenceRequested?.Invoke(sequence);
    }

    // Undo the most recent Big Package command.
    public void Undo()
    {
        if (history.Count == 0)
        {
            Debug.Log("Nothing to undo.");
            return;
        }

        history.Pop();
        bigPackage.RemoveAt(bigPackage.Count - 1);

        // A restores its previous world snapshot.
        GameEvents.UndoRequested?.Invoke();

        // Undo may also be used after reaching the goal.
        hasWon = false;

        GameEvents.ProgramsChanged?.Invoke();
    }

    // Reset B's stored program state.
    public void ResetProgramming()
    {
        bigPackage.Clear();
        smallPackage.Clear();
        history.Clear();
        hasWon = false;

        GameEvents.ProgramsChanged?.Invoke();
    }

    private void HandleWin()
    {
        hasWon = true;
        Debug.Log("Level completed!");
    }
}
