using UnityEngine;

public class Obstacle : MonoBehaviour
{
    public float speed = 4f;

    void Update()
    {
        if (GameManager.Instance != null && GameManager.Instance.State != GameState.Playing) return;

        transform.position += Vector3.left * (speed * Time.deltaTime);
        if (transform.position.x < -24f)
            Destroy(gameObject);
    }
}
