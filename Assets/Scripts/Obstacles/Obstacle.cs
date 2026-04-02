
using UnityEngine;

public class Obstacle : MonoBehaviour
{
    public float speed = 4f;

    void Update()
    {
        transform.Translate(Vector2.left * speed * Time.deltaTime);

        if(transform.position.x < -15)
            Destroy(gameObject);
    }
}
