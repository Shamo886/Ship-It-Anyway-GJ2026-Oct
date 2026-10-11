
using UnityEngine;
using UnityEngine.SceneManagement;

public class FinalGoalBranching : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GridManager gridManager;

    [Header("Final Level")]
    [SerializeField] private int finalLevelIndex = 5;

    [Header("Goal A")]
    [SerializeField] private Vector2Int goalACell;
    [SerializeField] private Sprite goalASprite;
    [SerializeField] private string goalASceneName = "Ending_A";

    [Header("Goal B")]
    [SerializeField] private Vector2Int goalBCell;
    [SerializeField] private Sprite goalBSprite;
    [SerializeField] private string goalBSceneName = "Ending_B";

    [Header("Visual Settings")]
    [SerializeField] private float tileSize = 1f;
    [SerializeField] private int sortingOrder = 5;

    private GameObject goalAVisual;
    private GameObject goalBVisual;

    private void OnEnable()
    {
        WorldBus.LevelLoaded += OnLevelLoaded;
    }

    private void OnDisable()
    {
        WorldBus.LevelLoaded -= OnLevelLoaded;
        ClearVisuals();
    }

    private void OnLevelLoaded(int levelIndex)
    {
        ClearVisuals();

        if (levelIndex != finalLevelIndex)
            return;

        if (gridManager == null)
        {
            Debug.LogError(
                "FinalGoalBranching: GridManager is missing."
            );
            return;
        }

        goalAVisual = CreateGoalVisual(
            "FinalGoal_A",
            goalACell,
            goalASprite
        );

        goalBVisual = CreateGoalVisual(
            "FinalGoal_B",
            goalBCell,
            goalBSprite
        );
    }

    private GameObject CreateGoalVisual(
        string objectName,
        Vector2Int cell,
        Sprite sprite)
    {
        if (sprite == null)
            return null;

        GameObject obj = new GameObject(objectName);
        obj.transform.SetParent(transform, true);

        // Use the same grid position as the real goal
        obj.transform.position = gridManager.GridToWorld(cell);

        SpriteRenderer sr =
            obj.AddComponent<SpriteRenderer>();

        sr.sprite = sprite;
        sr.color = Color.white;
        sr.sortingOrder = sortingOrder;

        // Fit the artwork inside one tile
        Vector3 size = sprite.bounds.size;

        if (size.x > 0f && size.y > 0f)
        {
            obj.transform.localScale = new Vector3(
                tileSize / size.x,
                tileSize / size.y,
                1f
            );
        }

        return obj;
    }

    private void ClearVisuals()
    {
        if (goalAVisual != null)
            Destroy(goalAVisual);

        if (goalBVisual != null)
            Destroy(goalBVisual);

        goalAVisual = null;
        goalBVisual = null;
    }

    // Returns true if this is the final level.
    public bool TryHandleWin(
        int levelIndex,
        Vector2Int playerCell)
    {
        if (levelIndex != finalLevelIndex)
            return false;

        Debug.Log(
            "Final level goal reached at cell: " +
            playerCell
        );

        string targetScene;

        if (playerCell == goalACell)
        {
            targetScene = goalASceneName;
        }
        else if (playerCell == goalBCell)
        {
            targetScene = goalBSceneName;
        }
        else
        {
            Debug.LogError(
                "FinalGoalBranching: Goal cell not matched! " +
                "Player cell = " + playerCell +
                ", Goal A = " + goalACell +
                ", Goal B = " + goalBCell
            );
            return true;
        }

        if (string.IsNullOrEmpty(targetScene) ||
            !Application.CanStreamedLevelBeLoaded(targetScene))
        {
            Debug.LogError(
                "FinalGoalBranching: Scene not found in " +
                "Build Profiles Scene List: " + targetScene
            );
            return true;
        }

        SceneManager.LoadScene(targetScene);
        return true;
    }
}
