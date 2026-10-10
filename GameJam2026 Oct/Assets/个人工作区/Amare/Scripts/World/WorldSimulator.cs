using System.Collections.Generic;
using UnityEngine;

// Snapshot is a deep copy. Undo must restore both player position and every tile.
public class WorldSnapshot
{
    public Vector2Int playerCell;
    public bool hasWon;
    public Dictionary<Vector2Int, bool> raisedStates;
}

// Pure grid rules; never reads UI or Big/Small Program code.
public class WorldSimulator
{
    private readonly GridManager grid;
    private readonly Dictionary<Vector2Int, bool> raisedStates =
        new Dictionary<Vector2Int, bool>();

    public Vector2Int PlayerCell { get; private set; }
    public bool HasWon { get; private set; }
    public Dictionary<Vector2Int, bool> RaisedStates => raisedStates;

    public WorldSimulator(GridManager gridManager)
    {
        grid = gridManager;
        PlayerCell = grid.StartCell;
        HasWon = false;
        foreach (Vector2Int position in grid.SpecialCells)
            raisedStates[position] = grid.InitiallyRaised(position);
    }

    public WorldSnapshot Capture()
    {
        return new WorldSnapshot
        {
            playerCell = PlayerCell,
            hasWon = HasWon,
            raisedStates = new Dictionary<Vector2Int, bool>(raisedStates)
        };
    }

    public void Restore(WorldSnapshot snapshot)
    {
        PlayerCell = snapshot.playerCell;
        HasWon = snapshot.hasWon;
        raisedStates.Clear();
        foreach (var pair in snapshot.raisedStates)
            raisedStates.Add(pair.Key, pair.Value);
    }

    // Directions are always cardinal steps: (0,1), (0,-1), (-1,0), (1,0).
    public WorldStepResult Step(Vector2Int direction)
    {
        Vector2Int before = PlayerCell;
        Vector2Int target = before + direction;
        bool canEnter = grid.IsBaseWalkable(target);

        // IMPORTANT: check the target tile BEFORE changing any states.
        if (canEnter && raisedStates.TryGetValue(target, out bool raised) && raised)
            canEnter = false;

        if (canEnter) PlayerCell = target;

        // EVERY attempted step toggles tiles, including failed wall/boundary hits.
        // The tile under the player is always down. A tile just vacated rises.
        var positions = new List<Vector2Int>(raisedStates.Keys);
        foreach (Vector2Int p in positions)
        {
            if (p == PlayerCell)
                raisedStates[p] = false;
            else if (canEnter && p == before)
                raisedStates[p] = true;
            else
                raisedStates[p] = !raisedStates[p];
        }

        if (canEnter && PlayerCell == grid.GoalCell) HasWon = true;

        return new WorldStepResult
        {
            from = before,
            to = PlayerCell,
            moved = canEnter,
            reachedGoal = HasWon
        };
    }
}
