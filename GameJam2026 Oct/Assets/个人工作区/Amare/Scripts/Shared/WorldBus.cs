using System;
using UnityEngine;

// Shared contracts: B and C can use these without referencing any A script.
public struct WorldStateInfo
{
    public int levelIndex;
    public Vector2Int playerCell;
    public bool hasWon;
}

public struct WorldStepResult
{
    public int actionId;
    public int stepIndex;
    public Vector2Int from;
    public Vector2Int to;
    public bool moved;
    public bool reachedGoal;
}

public struct WorldBatchResult
{
    public int actionId;
    public int stepsProcessed;
    public bool reachedGoal;
}

public static class WorldBus
{
    // Requests (normally from Programming/UI; handled by A)
    public static event Action<int, Vector2Int[]> BatchRequested;
    public static event Action<int> UndoRequested;
    public static event Action ResetRequested;
    public static event Action<int> LoadLevelRequested;
    public static event Action<bool> PauseRequested;

    public static void SendBatch(int id, Vector2Int[] moves) =>
        BatchRequested?.Invoke(id, moves);
    public static void SendUndo(int id) => UndoRequested?.Invoke(id);
    public static void SendReset() => ResetRequested?.Invoke();
    public static void SendLoadLevel(int index) => LoadLevelRequested?.Invoke(index);
    public static void SendPause(bool paused) => PauseRequested?.Invoke(paused);

    // Results (sent by A; observed by B/C)
    public static event Action<WorldStepResult> StepFinished;
    public static event Action<WorldBatchResult> BatchFinished;
    public static event Action<int, string> BatchRejected;
    public static event Action<int> UndoFinished;
    public static event Action<int, string> UndoRejected;
    public static event Action<WorldStateInfo> WorldChanged;
    public static event Action<int> LevelLoaded;
    public static event Action ResetFinished;
    public static event Action<WorldStateInfo> Victory;

    public static void ReportStep(WorldStepResult value) => StepFinished?.Invoke(value);
    public static void ReportBatch(WorldBatchResult value) => BatchFinished?.Invoke(value);
    public static void RejectBatch(int id, string reason) => BatchRejected?.Invoke(id, reason);
    public static void ReportUndo(int id) => UndoFinished?.Invoke(id);
    public static void RejectUndo(int id, string reason) => UndoRejected?.Invoke(id, reason);
    public static void ReportWorld(WorldStateInfo value) => WorldChanged?.Invoke(value);
    public static void ReportLevel(int index) => LevelLoaded?.Invoke(index);
    public static void ReportReset() => ResetFinished?.Invoke();
    public static void ReportVictory(WorldStateInfo value) => Victory?.Invoke(value);
}
