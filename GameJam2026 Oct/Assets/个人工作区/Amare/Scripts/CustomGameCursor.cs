
using UnityEngine;

public class CustomGameCursor : MonoBehaviour
{
    [Header("Custom Cursor")]
    [SerializeField] private Texture2D cursorImage;

    [Header("Click Position")]
    [SerializeField] private Vector2 hotSpot = Vector2.zero;

    private void Start()
    {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        Cursor.SetCursor(
            cursorImage,
            hotSpot,
            CursorMode.Auto
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
}
