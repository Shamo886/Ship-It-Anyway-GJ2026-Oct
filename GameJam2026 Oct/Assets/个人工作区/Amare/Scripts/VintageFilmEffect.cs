
using UnityEngine;
using UnityEngine.UI;

public class VintageFilmEffect : MonoBehaviour
{
    [Header("Target Canvas")]
    [SerializeField] private Canvas targetCanvas;

    [Header("Vignette")]
    [Range(0f, 1f)]
    [SerializeField] private float vignetteStrength = 0.35f;

    [Range(0.1f, 1f)]
    [SerializeField] private float vignetteSize = 0.7f;

    [Header("Film Grain")]
    [Range(0f, 1f)]
    [SerializeField] private float grainStrength = 0.12f;

    [Range(0f, 30f)]
    [SerializeField] private float grainFPS = 12f;

    [SerializeField] private int grainTextureSize = 256;

    private RawImage vignetteImage;
    private RawImage grainImage;

    private Texture2D vignetteTexture;
    private Texture2D grainTexture;

    private float grainTimer;

    private void Start()
    {
        if (targetCanvas == null)
            targetCanvas = FindAnyObjectByType<Canvas>();

        if (targetCanvas == null)
        {
            Debug.LogError("VintageFilmEffect: Canvas not found.");
            enabled = false;
            return;
        }

        CreateLayers();
        GenerateVignette();
        GenerateGrain();
    }

    private void Update()
    {
        if (grainImage == null)
            return;

        grainTimer += Time.unscaledDeltaTime;

        if (grainFPS > 0f && grainTimer >= 1f / grainFPS)
        {
            grainTimer = 0f;
            GenerateGrain();
        }
    }

    private RawImage CreateOverlay(string objectName)
    {
        GameObject obj = new GameObject(
            objectName,
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(RawImage)
        );

        obj.transform.SetParent(targetCanvas.transform, false);

        RectTransform rect = obj.GetComponent<RectTransform>();

        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        RawImage image = obj.GetComponent<RawImage>();

        // Important: overlays must not block UI buttons.
        image.raycastTarget = false;
        image.maskable = false;

        obj.transform.SetAsLastSibling();

        return image;
    }

    private void CreateLayers()
    {
        grainImage = CreateOverlay("FilmGrainOverlay");
        vignetteImage = CreateOverlay("VignetteOverlay");

        grainImage.color = Color.white;
        vignetteImage.color = Color.white;
    }

    private void GenerateVignette()
    {
        const int size = 256;

        vignetteTexture = new Texture2D(
            size, size, TextureFormat.RGBA32, false
        );

        vignetteTexture.wrapMode = TextureWrapMode.Clamp;
        vignetteTexture.filterMode = FilterMode.Bilinear;

        Color[] pixels = new Color[size * size];

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float u = (x + 0.5f) / size * 2f - 1f;
                float v = (y + 0.5f) / size * 2f - 1f;

                float distance = Mathf.Sqrt(u * u + v * v);

                float start = vignetteSize * 0.55f;
                float end = Mathf.Max(start + 0.01f, vignetteSize * 1.6f);

                float t = Mathf.InverseLerp(start, end, distance);
                t = t * t * (3f - 2f * t);

                pixels[y * size + x] =
                    new Color(0f, 0f, 0f, t * vignetteStrength);
            }
        }

        vignetteTexture.SetPixels(pixels);
        vignetteTexture.Apply(false, false);

        vignetteImage.texture = vignetteTexture;
    }

    private void GenerateGrain()
    {
        int size = Mathf.Clamp(grainTextureSize, 64, 512);

        if (grainTexture == null ||
            grainTexture.width != size)
        {
            if (grainTexture != null)
                Destroy(grainTexture);

            grainTexture = new Texture2D(
                size, size, TextureFormat.RGBA32, false
            );

            grainTexture.wrapMode = TextureWrapMode.Repeat;
            grainTexture.filterMode = FilterMode.Point;
        }

        Color32[] pixels = new Color32[size * size];

        float alpha = Mathf.Clamp01(grainStrength);

        for (int i = 0; i < pixels.Length; i++)
        {
            byte shade = (byte)Random.Range(0, 256);
            pixels[i] = new Color32(
                shade, shade, shade, (byte)(alpha * 255)
            );
        }

        grainTexture.SetPixels32(pixels);
        grainTexture.Apply(false, false);

        grainImage.texture = grainTexture;
    }

    private void OnDestroy()
    {
        if (vignetteTexture != null)
            Destroy(vignetteTexture);

        if (grainTexture != null)
            Destroy(grainTexture);
    }
}
