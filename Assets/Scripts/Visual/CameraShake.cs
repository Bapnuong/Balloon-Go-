using UnityEngine;

public class CameraShake : MonoBehaviour
{
    Vector3 basePosition;
    float timer;
    float duration;
    float magnitude;

    void Awake()
    {
        basePosition = transform.localPosition;
    }

    public void Shake(float seconds, float strength)
    {
        duration = Mathf.Max(0.01f, seconds);
        magnitude = strength;
        timer = duration;
    }

    void LateUpdate()
    {
        if (timer > 0f)
        {
            timer -= Time.unscaledDeltaTime;
            float damper = Mathf.Clamp01(timer / duration);
            Vector2 offset = Random.insideUnitCircle * magnitude * damper;
            transform.localPosition = basePosition + new Vector3(offset.x, offset.y, 0f);
        }
        else
        {
            transform.localPosition = basePosition;
        }
    }
}
