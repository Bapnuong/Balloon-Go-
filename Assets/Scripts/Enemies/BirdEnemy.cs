using UnityEngine;

public class BirdEnemy : MonoBehaviour
{
    public float speed = 6f;
    private Vector3 targetPosition;
    PlayerController balloon;
    void Start()
    {
        targetPosition = PlayerController.lastPosition;
    }
    void Update()
    {
        if (targetPosition != null)
        {
            transform.position = Vector3.MoveTowards(
                transform.position,
                targetPosition,
                speed * Time.deltaTime
            );

            // Sau khi tới target → tiếp tục bay ngang
            if (Vector3.Distance(transform.position, targetPosition) < 0.1f)
            {
                transform.Translate(Vector2.left * speed * Time.deltaTime);
            }

            if (transform.position.x < -15)
                Destroy(gameObject);
        }
    }
}