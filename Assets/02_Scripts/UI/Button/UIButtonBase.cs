using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;

public abstract class UIButtonBase : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    protected Vector3 originalScale;

    [SerializeField] protected float enterDuration = 0.2f;
    [SerializeField] protected float exitDuration = 0.1f;

    protected virtual void Awake()
    {
        originalScale = transform.localScale;
    }

    public abstract void OnPointerEnter(PointerEventData eventData);
    public abstract void OnPointerExit(PointerEventData eventData);
}