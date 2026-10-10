using UnityEngine;

[CreateAssetMenu(fileName = "NewLevel", menuName = "Pattern Game/Level Data")]
public class LevelData : ScriptableObject
{
    public string levelName = "Level 1";

    [TextArea(5, 15)]
    public string map =
@"S######
T#..T##
...#.#.
####..G";

    [Header("Optional full-board artwork")]
    public Sprite boardSprite;

    [Min(0.01f)]
    public float cellSize = 1f;
}
