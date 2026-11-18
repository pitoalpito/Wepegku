
using UnityEngine;
using UnityEngine.EventSystems;

public class ButtonHoverZoom : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public float hoverScale = 1.1f;
    public float smoothSpeed = 10f;

    private Vector3 originalScale;
    private Vector3 targetScale;

    private void Start()
    {
        originalScale = transform.localScale;
        targetScale = originalScale;

        
    }

    private void Update()
    {
        transform.localScale = Vector3.Lerp(
            transform.localScale,
            targetScale,
            Time.unscaledDeltaTime * smoothSpeed
        );
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        targetScale = originalScale * hoverScale;
        AudioManager.Instance.PlaySFX("Hover");
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        targetScale = originalScale;
    }

}
