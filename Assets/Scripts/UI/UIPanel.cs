using System.Collections;
using UnityEngine;

[RequireComponent(typeof(CanvasGroup))]
public class UIPanel : MonoBehaviour
{
    public RectTransform content;
    public float duration = 0.18f;

    CanvasGroup group;
    Coroutine anim;
    bool visible;

    public bool IsVisible => visible;

    void Awake()
    {
        group = GetComponent<CanvasGroup>();
        if (content == null) content = transform as RectTransform;
    }

    public void Show(bool instant = false)
    {
        gameObject.SetActive(true);
        visible = true;
        group.interactable = true;
        group.blocksRaycasts = true;
        if (anim != null) StopCoroutine(anim);
        if (instant)
        {
            group.alpha = 1f;
            if (content != null) content.localScale = Vector3.one;
        }
        else
        {
            anim = StartCoroutine(Animate(1f, 1f));
        }
    }

    public void Hide(bool instant = false)
    {
        visible = false;
        group.interactable = false;
        group.blocksRaycasts = false;
        if (anim != null) StopCoroutine(anim);
        if (instant)
        {
            group.alpha = 0f;
            gameObject.SetActive(false);
        }
        else
        {
            anim = StartCoroutine(Animate(0f, 0.96f, true));
        }
    }

    IEnumerator Animate(float targetAlpha, float targetScale, bool disableAtEnd = false)
    {
        float startAlpha = group.alpha;
        Vector3 startScale = content != null ? content.localScale : Vector3.one;
        float t = 0f;
        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            float k = Mathf.Clamp01(t / duration);
            k = 1f - Mathf.Pow(1f - k, 3f);
            group.alpha = Mathf.Lerp(startAlpha, targetAlpha, k);
            if (content != null)
                content.localScale = Vector3.Lerp(startScale, Vector3.one * targetScale, k);
            yield return null;
        }
        group.alpha = targetAlpha;
        if (content != null) content.localScale = Vector3.one * targetScale;
        if (disableAtEnd) gameObject.SetActive(false);
    }
}
