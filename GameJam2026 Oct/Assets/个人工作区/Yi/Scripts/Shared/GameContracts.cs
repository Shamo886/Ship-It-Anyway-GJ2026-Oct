
using System;
using System.Collections.Generic;

// Types of commands the player can use.
public enum GameCommand
{
    Up,
    Down,
    Left,
    Right,
    CallSmall
}

// Shared events used by all three systems.
public static class GameEvents
{
    // B -> A: Execute these movement commands.
    public static Action<List<GameCommand>> MoveSequenceRequested;

    // B -> A: Restore the world before the last batch.
    public static Action UndoRequested;

    // B -> C: The command slots have changed.
    public static Action ProgramsChanged;

    // A -> B: The player has reached the goal.
    public static Action LevelWon;
}
