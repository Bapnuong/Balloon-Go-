using UnityEngine;

public class BirdEnemy : MonoBehaviour
{
    public float speed = 6f;
    private Vector3 targetPosition;
    private bool reachedTarget = false;

    void Start()
    {
        targetPosition = PlayerController.lastPosition;
    }

    void Update()
    {
        if (!reachedTarget)
        {
            transform.position = Vector3.MoveTowards(
                transform.position,
                targetPosition,
                speed * Time.deltaTime
            );

            if (Vector3.Distance(transform.position, targetPosition) < 0.1f)
            {
                reachedTarget = true;
            }
        }
        else
        {
            transform.Translate(Vector2.left * speed * Time.deltaTime);
        }

        if (transform.position.x < -15)
            Destroy(gameObject);
    }
}