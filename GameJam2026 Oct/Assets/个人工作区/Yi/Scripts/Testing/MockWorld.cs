
using System.Collections.Generic;
using UnityEngine;

public class MockWorld : MonoBehaviour
{
    // Simulated player grid position.
    private Vector2Int playerPos = Vector2Int.zero;

    // Simulated moving special tile.
    private Vector2Int specialTilePos = new Vector2Int(2, 0);

    // Save one snapshot before each Big command.
    private struct WorldSnapshot
    {
        public Vector2Int player;
        public Vector2Int specialTile;
    }

    private readonly Stack<WorldSnapshot> snapshots =
        new Stack<WorldSnapshot>();

    private void OnEnable()
    {
        GameEvents.MoveSequenceRequested += Execute;
        GameEvents.UndoRequested += UndoWorld;
    }

    private void OnDisable()
    {
        GameEvents.MoveSequenceRequested -= Execute;
        GameEvents.UndoRequested -= UndoWorld;
    }

    private void Execute(List<GameCommand> commands)
    {
        // Save the complete state before this batch.
        snapshots.Push(new WorldSnapshot
        {
            player = playerPos,
            specialTile = specialTilePos
        });

        foreach (GameCommand command in commands)
        {
            Vector2Int direction = GetDirection(command);
            Vector2Int target = playerPos + direction;

            // Simulate world boundaries.
            if (target.x < 0 || target.x > 4 ||
                target.y < 0 || target.y > 4)
            {
                Debug.Log("Hit wall. Turn consumed.");
                continue;
            }

            // Simulate collision with a special tile.
            if (target == specialTilePos)
            {
                specialTilePos += Vector2Int.down;
                Debug.Log("Special tile moved downward!");
                continue;
            }

            playerPos = target;
            Debug.Log("Player: " + playerPos);
        }

        PrintState();
    }

    private Vector2Int GetDirection(GameCommand command)
    {
        switch (command)
        {
            case GameCommand.Up:
                return Vector2Int.up;

            case GameCommand.Down:
                return Vector2Int.down;

            case GameCommand.Left:
                return Vector2Int.left;

            case GameCommand.Right:
                return Vector2Int.right;

            default:
                return Vector2Int.zero;
        }
    }

    private void UndoWorld()
    {
        if (snapshots.Count == 0)
        {
            Debug.LogWarning("No world snapshot.");
            return;
        }

        WorldSnapshot previous = snapshots.Pop();

        playerPos = previous.player;
        specialTilePos = previous.specialTile;

        Debug.Log("World restored!");
        PrintState();
    }

    private void PrintState()
    {
        Debug.Log(
            "Player = " + playerPos +
            " | Special Tile = " + specialTilePos
        );
    }
}
