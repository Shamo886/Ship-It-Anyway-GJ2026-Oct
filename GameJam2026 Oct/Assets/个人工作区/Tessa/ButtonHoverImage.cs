
using UnityEngine;
using UnityEngine.EventSystems;

public class ButtonHoverImage : MonoBehaviour,
    IPointerEnterHandler,
    IPointerExitHandler
{
    [Header("Image To Show")]
    [SerializeField] private GameObject hoverImage;

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (hoverImage != null)
        {
            hoverImage.SetActive(true);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (hoverImage != null)
        {
            hoverImage.SetActive(false);
        }
    }

    private void OnDisable()
    {
        if (hoverImage != null)
        {
            hoverImage.SetActive(false);
        }
    }
}
