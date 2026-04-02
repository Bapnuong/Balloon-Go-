
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

    void OnTriggerEnter2D(Collider2D col)
    {
        if (col.CompareTag("Enemy"))
        {
            GameManager.Instance.EndGame();
            Debug.Log("Game Over!");
        }

        if (col.CompareTag("Item"))
        {
            Destroy(col.gameObject);
        }
    }
}
