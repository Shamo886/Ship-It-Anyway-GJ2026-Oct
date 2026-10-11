using UnityEngine;
using UnityEngine.UI;

public class AutoCreditsScroll : MonoBehaviour
{
    [SerializeField] private ScrollRect scrollRect;
    [SerializeField] private float scrollSpeed = 0.05f;

    private bool isScrolling = false;

    private void OnEnable()
    {
        if (scrollRect == null)
            scrollRect = GetComponent<ScrollRect>();

        scrollRect.verticalNormalizedPosition = 1f;
        isScrolling = true;
    }

    private void Update()
    {
        if (!isScrolling || scrollRect == null)
            return;

        scrollRect.verticalNormalizedPosition -=
            scrollSpeed * Time.unscaledDeltaTime;

        if (scrollRect.verticalNormalizedPosition <= 0f)
        {
            scrollRect.verticalNormalizedPosition = 0f;
            isScrolling = false;
        }
    }
}