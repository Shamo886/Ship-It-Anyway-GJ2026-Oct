
using UnityEngine;

public class CustomGameCursor : MonoBehaviour
{
    [Header("Cursor Image")]
    [SerializeField] private Texture2D cursorImage;

    [Header("Cursor Settings")]
    [Range(0.5f, 4f)]
    [SerializeField] private float cursorScale = 2f;

    [SerializeField] private Vector2 hotSpot = Vector2.zero;

    private Texture2D scaledCursor;

    private void Start()
    {
        if (cursorImage == null)
        {
            Debug.LogWarning("Cursor image is missing!");
            return;
        }

        int width = Mathf.RoundToInt(
            cursorImage.width * cursorScale
        );

        int height = Mathf.RoundToInt(
            cursorImage.height * cursorScale
        );

        scaledCursor = new Texture2D(
            width,
            height,
            TextureFormat.RGBA32,
            false
        );

        scaledCursor.filterMode = FilterMode.Point;

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                float u = (x + 0.5f) / width;
                float v = (y + 0.5f) / height;

                Color color = cursorImage.GetPixelBilinear(u, v);

                scaledCursor.SetPixel(x, y, color);
            }
        }

        scaledCursor.Apply();

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        Cursor.SetCursor(
        scaledCursor,
        hotSpot * cursorScale,
        CursorMode.ForceSoftware
);
    }

    private void OnDisable()
    {
        Cursor.SetCursor(
            null,
            Vector2.zero,
            CursorMode.Auto
        );
    }

    private void OnDestroy()
    {
        if (scaledCursor != null)
        {
            Destroy(scaledCursor);
        }
    }
}
