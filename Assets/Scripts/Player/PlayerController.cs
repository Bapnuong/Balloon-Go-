
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float flyForce = 7f;
    Rigidbody2D rb;
    public static Vector3 lastPosition;
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        lastPosition = transform.position;
        if (GameManager.Instance.gameOver) return;

        if(Input.GetKeyDown(KeyCode.Space))
        {
            rb.linearVelocity = Vector2.up * flyForce;
        }
    }

    void OnCollisionEnter2D(Collision2D col)
    {
        if(col.gameObject.CompareTag("Obstacle") || col.gameObject.CompareTag("Enemy"))
        {
            GameManager.Instance.EndGame();
        }
    }
}
