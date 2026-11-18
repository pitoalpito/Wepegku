
using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;

public class ButtonHoverZoom : MonoBehaviour,
    IPointerEnterHandler, IPointerExitHandler
{
    [Header("Hover Settings")]
    public float zoomScale = 1.15f;
    public float duration = 0.2f;

    private Vector3 originalScale;
    private Tween scaleTween;

    private void Awake()
    {
        originalScale = transform.localScale;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        scaleTween?.Kill();

        scaleTween = transform.DOScale(
            originalScale * zoomScale,
            duration
        ).SetEase(Ease.OutBack);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        scaleTween?.Kill();

        scaleTween = transform.DOScale(
            originalScale,
            duration
        ).SetEase(Ease.OutQuad);
    }

    private void OnDisable()
    {
        scaleTween?.Kill();
        transform.localScale = originalScale;
    }
}
