using UnityEngine;

[RequireComponent(typeof(Camera))]
public class FixedAspectRatio : MonoBehaviour
{
    [SerializeField]
    private float targetAspect = 16f / 9f;

    private Camera cam;

    private int lastWidth;
    private int lastHeight;

    private void Awake()
    {
        cam = GetComponent<Camera>();
        ApplyAspectRatio();
    }

    private void Update()
    {
        if (Screen.width != lastWidth ||
            Screen.height != lastHeight)
        {
            ApplyAspectRatio();
        }
    }

    private void ApplyAspectRatio()
    {
        lastWidth = Screen.width;
        lastHeight = Screen.height;

        float windowAspect =
            (float)Screen.width / Screen.height;

        float scaleHeight =
            windowAspect / targetAspect;

        if (scaleHeight < 1f)
        {
            // 屏幕比较窄：上下加黑边
            Rect rect = cam.rect;

            rect.width = 1f;
            rect.height = scaleHeight;
            rect.x = 0f;
            rect.y = (1f - scaleHeight) / 2f;

            cam.rect = rect;
        }
        else
        {
            // 屏幕比较宽：左右加黑边
            float scaleWidth =
                1f / scaleHeight;

            Rect rect = cam.rect;

            rect.width = scaleWidth;
            rect.height = 1f;
            rect.x = (1f - scaleWidth) / 2f;
            rect.y = 0f;

            cam.rect = rect;
        }
    }
}