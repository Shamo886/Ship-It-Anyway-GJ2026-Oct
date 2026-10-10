using UnityEngine;

/// <summary>
/// Loads the next LevelData after the World system reports victory.
/// Keeps Program B's instructions and Program A's world state in sync.
/// Attach exactly once in the GameScene.
/// </summary>
public class LevelAdvanceController : MonoBehaviour
{
    [Header("Program B")]
    [SerializeField] private ProgrammingManager programmingManager;

    [Header("Level progression")]
    [Min(1)]
    [SerializeField] private int levelCount = 2;

    private void OnEnable()
    {
        WorldBus.Victory += HandleVictory;
    }

    private void OnDisable()
    {
        WorldBus.Victory -= HandleVictory;
    }

    private void HandleVictory(WorldStateInfo state)
    {
        int nextLevelIndex = state.levelIndex + 1;

        if (nextLevelIndex >= levelCount)
        {
            Debug.Log("All levels completed!");
            return;
        }

        if (programmingManager == null)
        {
            Debug.LogError("LevelAdvanceController: Assign ProgrammingManager before enabling automatic level changes.");
            return;
        }

        // Clear Big/Small programs first, so commands from the previous
        // level do not linger when the new level becomes interactive.
        programmingManager.ResetProgramming();

        // WorldController handles rebuilding the map, respawning the player,
        // resetting special tiles, clearing undo history, and broadcasting LevelLoaded.
        WorldBus.SendLoadLevel(nextLevelIndex);
    }
}
