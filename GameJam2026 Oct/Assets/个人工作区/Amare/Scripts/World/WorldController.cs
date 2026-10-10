using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldController : MonoBehaviour
{
    [Header("World Objects")]
    [SerializeField] private GridManager gridManager;
    [SerializeField] private PlayerMover playerMover;

    [Header("Levels")]
    [SerializeField] private LevelData[] levels;
    [SerializeField] private int initialLevelIndex = 0;

    [Header("Timing")]
    [SerializeField] private float blockedStepDelay = 0.22f;

    [Header("A-only temporary testing")]
    [SerializeField] private bool showTestButtons = true;

    private class HistoryEntry
    {
        public int id;
        public WorldSnapshot snapshot;
    }

    private readonly Stack<HistoryEntry> history = new Stack<HistoryEntry>();
    private readonly Stack<int> debugIds = new Stack<int>();
    private WorldSimulator simulator;
    private bool busy;
    private bool paused;
    private int activeActionId = -1;
    private int currentLevelIndex;
    private int nextDebugId = 10000;

    private void OnEnable()
    {
        WorldBus.BatchRequested += OnBatchRequested;
        WorldBus.UndoRequested += OnUndoRequested;
        WorldBus.ResetRequested += OnResetRequested;
        WorldBus.LoadLevelRequested += OnLoadLevelRequested;
        WorldBus.PauseRequested += OnPauseRequested;
        WorldBus.UndoFinished += OnDebugUndoFinished;
        WorldBus.BatchRejected += OnDebugBatchRejected;
    }

    private void OnDisable()
    {
        WorldBus.BatchRequested -= OnBatchRequested;
        WorldBus.UndoRequested -= OnUndoRequested;
        WorldBus.ResetRequested -= OnResetRequested;
        WorldBus.LoadLevelRequested -= OnLoadLevelRequested;
        WorldBus.PauseRequested -= OnPauseRequested;
        WorldBus.UndoFinished -= OnDebugUndoFinished;
        WorldBus.BatchRejected -= OnDebugBatchRejected;
        if (paused) Time.timeScale = 1f;
    }

    private void Start()
    {
        LoadLevel(initialLevelIndex);
    }

    private WorldStateInfo CurrentState() => new WorldStateInfo
    {
        levelIndex = currentLevelIndex,
        playerCell = simulator.PlayerCell,
        hasWon = simulator.HasWon
    };

    private void LoadLevel(int index)
    {
        if (levels == null || index < 0 || index >= levels.Length || levels[index] == null)
        {
            Debug.LogError("WorldController: invalid level index / missing LevelData: " + index);
            return;
        }

        if (busy)
        {
            StopAllCoroutines();
            WorldBus.RejectBatch(activeActionId, "Batch canceled by level change/reset.");
        }
        busy = false;
        activeActionId = -1;
        paused = false;
        Time.timeScale = 1f;
        history.Clear();
        debugIds.Clear();

        try
        {
            gridManager.Build(levels[index]);
            simulator = new WorldSimulator(gridManager);
        }
        catch (System.Exception e)
        {
            Debug.LogException(e);
            simulator = null;
            return;
        }

        currentLevelIndex = index;
        playerMover.SnapTo(gridManager.GridToWorld(simulator.PlayerCell));
        gridManager.SyncSpecialTiles(simulator.RaisedStates, false);
        WorldBus.ReportLevel(index);
        WorldBus.ReportWorld(CurrentState());
    }

    private static bool ValidDirection(Vector2Int step) =>
        (Mathf.Abs(step.x) == 1 && step.y == 0) ||
        (Mathf.Abs(step.y) == 1 && step.x == 0);

    private void OnBatchRequested(int id, Vector2Int[] moves)
    {
        if (simulator == null || busy || paused || simulator.HasWon)
        {
            WorldBus.RejectBatch(id, "World unavailable, busy, paused, or already won.");
            return;
        }
        if (moves == null || moves.Length > 64)
        {
            WorldBus.RejectBatch(id, "Null or oversized batch (max 64 moves).");
            return;
        }
        foreach (Vector2Int step in moves)
        {
            if (!ValidDirection(step))
            {
                WorldBus.RejectBatch(id, "Direction must be one cardinal grid step.");
                return;
            }
        }
        foreach (HistoryEntry item in history)
        {
            if (item.id == id)
            {
                WorldBus.RejectBatch(id, "Duplicate actionId.");
                return;
            }
        }

        // ONE snapshot per batch (including an empty batch).
        history.Push(new HistoryEntry { id = id, snapshot = simulator.Capture() });
        busy = true;
        activeActionId = id;
        StartCoroutine(ExecuteBatch(id, (Vector2Int[])moves.Clone()));
    }

    private IEnumerator ExecuteBatch(int id, Vector2Int[] moves)
    {
        int processed = 0;
        for (int i = 0; i < moves.Length; i++)
        {
            WorldStepResult step = simulator.Step(moves[i]);
            step.actionId = id;
            step.stepIndex = i;
            gridManager.SyncSpecialTiles(simulator.RaisedStates, true);

            if (step.moved)
                yield return playerMover.AnimateMove(gridManager.GridToWorld(step.to));
            else
                yield return new WaitForSeconds(blockedStepDelay);

            processed++;
            WorldBus.ReportStep(step);
            WorldBus.ReportWorld(CurrentState());
            if (step.reachedGoal) break; // win IMMEDIATELY; skip remaining moves
        }

        busy = false;
        activeActionId = -1;
        WorldBus.ReportBatch(new WorldBatchResult
        {
            actionId = id,
            stepsProcessed = processed,
            reachedGoal = simulator.HasWon
        });
        if (simulator.HasWon) WorldBus.ReportVictory(CurrentState());
    }

    private void OnUndoRequested(int id)
    {
        if (simulator == null || busy || history.Count == 0 || history.Peek().id != id)
        {
            WorldBus.RejectUndo(id, "Undo requires the most recent completed world batch.");
            return;
        }
        HistoryEntry entry = history.Pop();
        simulator.Restore(entry.snapshot);
        playerMover.SnapTo(gridManager.GridToWorld(simulator.PlayerCell));
        gridManager.SyncSpecialTiles(simulator.RaisedStates, false);
        WorldBus.ReportWorld(CurrentState());
        WorldBus.ReportUndo(id);
    }

    private void OnResetRequested()
    {
        LoadLevel(currentLevelIndex);
        if (simulator != null) WorldBus.ReportReset();
    }

    private void OnLoadLevelRequested(int index) => LoadLevel(index);

    private void OnPauseRequested(bool value)
    {
        paused = value;
        Time.timeScale = value ? 0f : 1f;
    }

    // Temporary IMGUI debug controls: remove/disable when C's UI is ready.
    private void DebugBatch(params Vector2Int[] steps)
    {
        int id = ++nextDebugId;
        debugIds.Push(id);
        WorldBus.SendBatch(id, steps);
    }

    private void OnDebugUndoFinished(int id)
    {
        if (debugIds.Count > 0 && debugIds.Peek() == id) debugIds.Pop();
    }

    private void OnDebugBatchRejected(int id, string reason)
    {
        if (debugIds.Count > 0 && debugIds.Peek() == id) debugIds.Pop();
        Debug.LogWarning("World batch rejected: " + reason);
    }

    private void OnGUI()
    {
        if (!showTestButtons || simulator == null) return;
        GUILayout.BeginArea(new Rect(16, 16, 290, 340), "A: World Test", GUI.skin.window);
        GUILayout.Label("Level " + (currentLevelIndex + 1) +
            " | Cell " + simulator.PlayerCell +
            " | Won: " + simulator.HasWon);
        GUI.enabled = !busy && !paused && !simulator.HasWon;
        if (GUILayout.Button("UP")) DebugBatch(Vector2Int.up);
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("LEFT")) DebugBatch(Vector2Int.left);
        if (GUILayout.Button("DOWN")) DebugBatch(Vector2Int.down);
        if (GUILayout.Button("RIGHT")) DebugBatch(Vector2Int.right);
        GUILayout.EndHorizontal();
        if (GUILayout.Button("Test 3-step batch: DOWN, DOWN, DOWN"))
            DebugBatch(Vector2Int.down, Vector2Int.down, Vector2Int.down);
        GUI.enabled = !busy && debugIds.Count > 0;
        if (GUILayout.Button("UNDO last batch"))
        {
            int id = debugIds.Peek();
            WorldBus.SendUndo(id);
        }
        GUI.enabled = true;
        if (GUILayout.Button("RESET current level")) WorldBus.SendReset();
        if (GUILayout.Button("NEXT level"))
            WorldBus.SendLoadLevel((currentLevelIndex + 1) % levels.Length);
        if (GUILayout.Button(paused ? "RESUME" : "PAUSE"))
            WorldBus.SendPause(!paused);
        GUILayout.EndArea();
    }
}
