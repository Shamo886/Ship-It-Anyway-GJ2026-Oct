using System.Collections;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class ToggleTile : MonoBehaviour
{
    [Header("Special tile sprites")]
    [SerializeField] private Sprite openEyeSprite;   // UP: blocks movement
    [SerializeField] private Sprite closedEyeSprite; // DOWN: walkable
    [SerializeField] private bool useLiftAnimation = false;

    [Header("Fallback colors (if sprites not assigned)")]
    [SerializeField] private Color upColor = new Color(0.95f, 0.40f, 0.33f);
    [SerializeField] private Color downColor = new Color(0.34f, 0.72f, 0.82f);
    [SerializeField] private float animationDuration = 0.16f;

    private SpriteRenderer sprite;
    private Vector3 baseLocalPosition;
    private float liftHeight;
    private Coroutine animation;

    public void Initialize(float cellSize, bool initiallyRaised)
    {
        sprite = GetComponent<SpriteRenderer>();
        sprite.sortingOrder = 3;
        baseLocalPosition = transform.localPosition;
        liftHeight = useLiftAnimation ? 0.15f * cellSize : 0f;
        SetRaised(initiallyRaised, false);
    }

    public void SetRaised(bool raised, bool animate)
    {
        if (sprite == null) sprite = GetComponent<SpriteRenderer>();
        if (animation != null) StopCoroutine(animation);
        animation = null;

        Vector3 destination = baseLocalPosition +
            (raised ? Vector3.up * liftHeight : Vector3.zero);
        Sprite stateImage = raised ? openEyeSprite : closedEyeSprite;
        if (stateImage != null)
            sprite.sprite = stateImage;

        // Art sprites keep their original colors; test squares use fallback tints.
        Color color = stateImage != null
            ? Color.white
            : (raised ? upColor : downColor);

        if (!animate || animationDuration <= 0f)
        {
            transform.localPosition = destination;
            sprite.color = color;
        }
        else
        {
            animation = StartCoroutine(Animate(destination, color));
        }
    }

    private IEnumerator Animate(Vector3 destination, Color color)
    {
        Vector3 start = transform.localPosition;
        Color fromColor = sprite.color;
        float elapsed = 0f;
        while (elapsed < animationDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / animationDuration);
            transform.localPosition = Vector3.Lerp(start, destination, t);
            sprite.color = Color.Lerp(fromColor, color, t);
            yield return null;
        }
        transform.localPosition = destination;
        sprite.color = color;
        animation = null;
    }
}
