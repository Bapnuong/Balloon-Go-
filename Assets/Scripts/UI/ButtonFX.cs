using UnityEngine;
using UnityEngine.EventSystems;

public class ButtonFX : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    public float hoverScale = 1.035f;
    public float pressScale = 0.965f;

    Vector3 baseScale;
    bool hovering;
    bool pressed;

    void Awake()
    {
        baseScale = transform.localScale;
    }

    void OnEnable()
    {
        baseScale = transform.localScale;
        hovering = false;
        pressed = false;
    }

    void OnDisable()
    {
        transform.localScale = baseScale;
    }

    void Update()
    {
        float target = pressed ? pressScale : (hovering ? hoverScale : 1f);
        transform.localScale = Vector3.Lerp(transform.localScale, baseScale * target, Time.unscaledDeltaTime * 18f);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        hovering = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        hovering = false;
        pressed = false;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        pressed = true;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        pressed = false;
    }
}
