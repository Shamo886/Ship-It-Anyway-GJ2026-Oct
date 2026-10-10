using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Bridges Programmer B's GameEvents/GameCommand to Programmer A's WorldBus.
/// Attach once to an empty GameObject in the same scene as WorldController.
/// Requires the B project's GameCommand + GameEvents scripts.
/// </summary>
public class ProgrammingWorldBridge : MonoBehaviour
{
    [Header("B - Optional, for resetting both systems")]
    [SerializeField] private ProgrammingManager programmingManager;

    [Header("Disable these B buttons while a batch is executing")]
    [SerializeField] private Button[] editingButtons;
    [SerializeField] private Button undoButton;

    private readonly Stack<int> completedActionIds = new Stack<int>();
    private int nextActionId = 1;
    private int activeActionId = -1;
    private bool busy;
    private bool undoInProgress;
    private bool paused;
    private bool hasWon;

    private void OnEnable()
    {
        GameEvents.MoveSequenceRequested += HandleMovementFromB;
        GameEvents.UndoRequested += HandleUndoFromB;

        WorldBus.BatchFinished += HandleBatchFinished;
        WorldBus.BatchRejected += HandleBatchRejected;
        WorldBus.UndoFinished += HandleUndoFinished;
        WorldBus.UndoRejected += HandleUndoRejected;
        WorldBus.WorldChanged += HandleWorldChanged;
        WorldBus.LevelLoaded += HandleLevelLoaded;
        WorldBus.PauseRequested += HandlePause;
    }

    private void OnDisable()
    {
        GameEvents.MoveSequenceRequested -= HandleMovementFromB;
        GameEvents.UndoRequested -= HandleUndoFromB;

        WorldBus.BatchFinished -= HandleBatchFinished;
        WorldBus.BatchRejected -= HandleBatchRejected;
        WorldBus.UndoFinished -= HandleUndoFinished;
        WorldBus.UndoRejected -= HandleUndoRejected;
        WorldBus.WorldChanged -= HandleWorldChanged;
        WorldBus.LevelLoaded -= HandleLevelLoaded;
        WorldBus.PauseRequested -= HandlePause;
    }

    private void Start()
    {
        RefreshButtons();
    }

    // B sends an already-expanded list of directions. Small-package calls
    // must be expanded by B before reaching this method.
    private void HandleMovementFromB(List<GameCommand> commands)
    {
        if (busy || paused || hasWon)
        {
            Debug.LogError("Bridge rejected B movement: world is busy, paused or won. B already edited its slots; prevent inputs while busy.");
            return;
        }

        if (commands == null)
        {
            Debug.LogError("Bridge: B sent a null command list.");
            return;
        }

        Vector2Int[] steps = new Vector2Int[commands.Count];

        for (int i = 0; i < commands.Count; i++)
        {
            switch (commands[i])
            {
                case GameCommand.Up:
                    steps[i] = Vector2Int.up;
                    break;
                case GameCommand.Down:
                    steps[i] = Vector2Int.down;
                    break;
                case GameCommand.Left:
                    steps[i] = Vector2Int.left;
                    break;
                case GameCommand.Right:
                    steps[i] = Vector2Int.right;
                    break;
                default:
                    Debug.LogError("Bridge: B must expand CallSmall to directions before sending the batch.");
                    return;
            }
        }

        busy = true;
        undoInProgress = false;
        activeActionId = nextActionId++;
        RefreshButtons();
        WorldBus.SendBatch(activeActionId, steps);
    }

    // B's BIG-package undo emits GameEvents.UndoRequested.
    // B's SMALL-package undo must NOT emit it, because nothing moved.
    private void HandleUndoFromB()
    {
        if (busy || paused)
        {
            Debug.LogError("Bridge rejected B undo during animation/pause.");
            return;
        }

        if (completedActionIds.Count == 0)
        {
            Debug.LogWarning("Bridge: no completed BIG-package world action to undo.");
            return;
        }

        busy = true;
        undoInProgress = true;
        activeActionId = completedActionIds.Peek();
        RefreshButtons();
        WorldBus.SendUndo(activeActionId);
    }

    private void HandleBatchFinished(WorldBatchResult result)
    {
        if (!busy || undoInProgress || result.actionId != activeActionId)
            return;

        completedActionIds.Push(result.actionId);
        hasWon = result.reachedGoal;
        EndOperation();
    }

    private void HandleBatchRejected(int id, string reason)
    {
        if (!busy || undoInProgress || id != activeActionId)
            return;

        Debug.LogError("World rejected the batch: " + reason +
                       ". B may already show the command; reset both systems if they differ.");
        EndOperation();
    }

    private void HandleUndoFinished(int id)
    {
        if (!busy || !undoInProgress || id != activeActionId)
            return;

        if (completedActionIds.Count > 0 && completedActionIds.Peek() == id)
            completedActionIds.Pop();
        EndOperation();
    }

    private void HandleUndoRejected(int id, string reason)
    {
        if (!busy || !undoInProgress || id != activeActionId)
            return;

        Debug.LogError("World rejected undo: " + reason +
                       ". B already removed its command; reset both systems if they differ.");
        EndOperation();
    }

    private void HandleWorldChanged(WorldStateInfo state)
    {
        hasWon = state.hasWon;
        RefreshButtons();
    }

    private void HandleLevelLoaded(int levelIndex)
    {
        completedActionIds.Clear();
        hasWon = false;
        paused = false;
        busy = false;
        undoInProgress = false;
        activeActionId = -1;
        RefreshButtons();
    }

    private void HandlePause(bool isPaused)
    {
        paused = isPaused;
        RefreshButtons();
    }

    private void EndOperation()
    {
        busy = false;
        undoInProgress = false;
        activeActionId = -1;
        RefreshButtons();
    }

    private void RefreshButtons()
    {
        bool canEdit = !busy && !paused && !hasWon;
        if (editingButtons != null)
        {
            foreach (Button button in editingButtons)
            {
                if (button != null) button.interactable = canEdit;
            }
        }
        // Undo is allowed even after victory, to restore the previous world.
        if (undoButton != null)
            undoButton.interactable = !busy && !paused;
    }

    /// <summary>Connect a Restart UI Button to this method in OnClick.</summary>
    public void ResetBoth()
    {
        if (busy)
        {
            Debug.LogWarning("Cannot reset while an action is animating.");
            return;
        }

        if (programmingManager != null)
            programmingManager.ResetProgramming();
        else
            Debug.LogWarning("Bridge: ProgrammingManager not assigned. B's command slots will not reset.");

        WorldBus.SendReset();
    }
}
