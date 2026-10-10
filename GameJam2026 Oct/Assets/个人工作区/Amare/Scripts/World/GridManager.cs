using System;
using System.Collections.Generic;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    [Header("Grid: origin is the center of bottom-left cell")]
    [SerializeField] private Transform gridOrigin;

    [Header("Art and optional debug grid")]
    [SerializeField] private SpriteRenderer boardArt;
    [SerializeField] private Transform boardVisuals;
    [SerializeField] private GameObject cellPrefab;
    [SerializeField] private bool showDebugCells = true;

    [Header("Special tiles")]
    [SerializeField] private Transform specialTilesRoot;
    [SerializeField] private GameObject specialTilePrefab;

    private string[] rows;
    private LevelData currentLevel;
    private readonly Dictionary<Vector2Int, ToggleTile> tileViews =
        new Dictionary<Vector2Int, ToggleTile>();

    public int Width { get; private set; }
    public int Height { get; private set; }
    public float CellSize => currentLevel == null ? 1f : currentLevel.cellSize;
    public Vector2Int StartCell { get; private set; }
    public Vector2Int GoalCell { get; private set; }

    public void Build(LevelData level)
    {
        if (level == null || gridOrigin == null || boardVisuals == null ||
            specialTilesRoot == null || specialTilePrefab == null)
            throw new Exception("GridManager: assign level, GridOrigin, BoardVisuals, SpecialTiles, and SpecialTilePrefab.");

        currentLevel = level;
        ParseMap(level.map);

        ClearChildren(boardVisuals);
        ClearChildren(specialTilesRoot);
        tileViews.Clear();

        if (boardArt != null)
        {
            boardArt.sprite = level.boardSprite;
            boardArt.gameObject.SetActive(level.boardSprite != null);
            boardArt.transform.position = GridToWorld(Vector2Int.zero) +
                new Vector3((Width - 1) * CellSize * 0.5f,
                            (Height - 1) * CellSize * 0.5f, 0f);
            boardArt.transform.localScale = Vector3.one;
            boardArt.sortingOrder = 0;
        }

        for (int y = 0; y < Height; y++)
        for (int x = 0; x < Width; x++)
        {
            Vector2Int cell = new Vector2Int(x, y);
            char type = GetCellType(cell);
            if (type == '#') continue; // absent cell: no artwork, no movement

            if (showDebugCells && cellPrefab != null)
            {
                GameObject obj = Instantiate(cellPrefab, GridToWorld(cell),
                    Quaternion.identity, boardVisuals);
                obj.name = "Cell_" + x + "_" + y;
                obj.transform.localScale = new Vector3(CellSize * 0.96f, CellSize * 0.96f, 1f);
                SpriteRenderer sr = obj.GetComponent<SpriteRenderer>();
                if (sr != null)
                {
                    sr.sortingOrder = 1;

                    // Adjacent cells (including special-tile floors) alternate A / B.
                    // This changes artwork only. Collision is handled elsewhere.
                    bool useRoadArt = type == '.' || type == 'T' || type == 't';
                    Sprite roadArt = ((x + y) % 2 == 0)
                        ? level.roadSpriteA : level.roadSpriteB;

                    // If one image is missing, use the other instead of leaving a gap.
                    if (roadArt == null)
                        roadArt = level.roadSpriteA != null
                            ? level.roadSpriteA : level.roadSpriteB;

                    if (useRoadArt && roadArt != null)
                    {
                        sr.sprite = roadArt;
                        sr.color = Color.white; // no gray/red tint on imported art
                    }
                    else
                    {
                        // Preserve original test colors for start, goal, and walls.
                        sr.color = type == 'S' ? new Color(0.45f, 0.85f, 0.55f) :
                                   type == 'G' ? new Color(1f, 0.78f, 0.30f) :
                                   type == 'W' ? new Color(0.3f, 0.3f, 0.35f) :
                                   new Color(0.78f, 0.80f, 0.86f);
                    }
                }
            }

            if (type == 'T' || type == 't')
            {
                GameObject obj = Instantiate(specialTilePrefab, GridToWorld(cell),
                    Quaternion.identity, specialTilesRoot);
                obj.name = "ToggleTile_" + x + "_" + y;
                obj.transform.localScale = new Vector3(CellSize * 0.84f, CellSize * 0.84f, 1f);
                ToggleTile view = obj.GetComponent<ToggleTile>();
                if (view == null)
                    throw new Exception("SpecialTilePrefab needs ToggleTile component.");
                view.Initialize(CellSize, type == 'T');
                tileViews.Add(cell, view);
            }
        }
    }

    private void ParseMap(string text)
    {
        if (string.IsNullOrWhiteSpace(text)) throw new Exception("Level map is empty.");
        rows = text.Replace("\r", "").Trim('\n').Split('\n');
        Height = rows.Length;
        Width = rows[0].Length;
        int starts = 0, goals = 0;

        for (int r = 0; r < Height; r++)
        {
            if (rows[r].Length != Width)
                throw new Exception("Every map row must have the same number of characters. Row: " + r);
            for (int x = 0; x < Width; x++)
            {
                char c = rows[r][x];
                if ("#.SGWTt".IndexOf(c) < 0)
                    throw new Exception("Unknown map character: " + c);
                Vector2Int cell = new Vector2Int(x, Height - 1 - r);
                if (c == 'S') { StartCell = cell; starts++; }
                if (c == 'G') { GoalCell = cell; goals++; }
            }
        }
        if (starts != 1 || goals != 1)
            throw new Exception("Map must contain exactly one S and one G.");
    }

    private static void ClearChildren(Transform parent)
    {
        for (int i = parent.childCount - 1; i >= 0; i--)
        {
            GameObject obj = parent.GetChild(i).gameObject;
            obj.SetActive(false);
            Destroy(obj);
        }
    }

    public bool IsInsideGrid(Vector2Int cell) =>
        cell.x >= 0 && cell.x < Width && cell.y >= 0 && cell.y < Height;

    public char GetCellType(Vector2Int cell) => IsInsideGrid(cell)
        ? rows[Height - 1 - cell.y][cell.x] : '#';

    // This only handles *static* geometry. WorldSimulator checks special tile state.
    public bool IsBaseWalkable(Vector2Int cell)
    {
        char c = GetCellType(cell);
        return c == '.' || c == 'S' || c == 'G' || c == 'T' || c == 't';
    }

    public Vector3 GridToWorld(Vector2Int cell) =>
        gridOrigin.position + new Vector3(cell.x * CellSize, cell.y * CellSize, 0f);

    public IEnumerable<Vector2Int> SpecialCells => tileViews.Keys;

    public bool InitiallyRaised(Vector2Int cell) => GetCellType(cell) == 'T';

    public void SyncSpecialTiles(Dictionary<Vector2Int, bool> states, bool animate)
    {
        foreach (var pair in states)
        {
            if (tileViews.TryGetValue(pair.Key, out ToggleTile tile))
                tile.SetRaised(pair.Value, animate);
        }
    }
}
