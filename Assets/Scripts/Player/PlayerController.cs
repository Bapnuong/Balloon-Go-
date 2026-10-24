using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(CircleCollider2D))]
[RequireComponent(typeof(SpriteRenderer))]
public class PlayerController : MonoBehaviour
{
    public float flyForce = 6.6f;
    public float gravityScale = 2.25f;
    public float maxFallSpeed = -9.5f;
    public float tiltAmount = 24f;

    public bool IsAlive { get; private set; }

    Rigidbody2D rb;
    CircleCollider2D col;
    SpriteRenderer sr;
    Camera cam;
    float puffTimer;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<CircleCollider2D>();
        sr = GetComponent<SpriteRenderer>();
        cam = Camera.main;

        rb.gravityScale = 0f;
        rb.constraints = RigidbodyConstraints2D.FreezePositionX;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        rb.simulated = false;
    }

    void OnDestroy()
    {
        if (GameManager.Instance != null && GameManager.Instance.Player == this)
            GameManager.Instance.Player = null;
    }

    public void PrepareForRun()
    {
        gameObject.SetActive(true);
        IsAlive = true;
        if (cam == null) cam = Camera.main;

        rb.simulated = false;
        rb.gravityScale = 0f;
        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;
        rb.rotation = 0f;
        col.enabled = true;
        sr.enabled = true;

        float halfWidth = cam != null ? cam.orthographicSize * cam.aspect : 8.9f;
        float x = (cam != null ? cam.transform.position.x : 0f) - halfWidth + 2.25f;
        float y = cam != null ? cam.transform.position.y : 0f;
        transform.position = new Vector3(x, y, 0f);
        transform.rotation = Quaternion.identity;
    }

    public void BeginRun()
    {
        IsAlive = true;
        rb.simulated = true;
        rb.gravityScale = gravityScale;
        rb.linearVelocity = Vector2.zero;
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }

    public void Die()
    {
        IsAlive = false;
        rb.simulated = false;
        col.enabled = false;
        Color tint = sr.color;
        sr.enabled = false;
        if (EffectsManager.Instance != null)
            EffectsManager.Instance.SpawnPop(transform.position, tint);
    }

    void Update()
    {
        if (GameManager.Instance == null || GameManager.Instance.State != GameState.Playing) return;
        if (!IsAlive) return;

        HandleInput();

        if (rb.linearVelocity.y < maxFallSpeed)
            rb.linearVelocity = new Vector2(0f, maxFallSpeed);

        float target = Mathf.Clamp(rb.linearVelocity.y * 2.6f, -tiltAmount, tiltAmount);
        rb.rotation = Mathf.LerpAngle(rb.rotation, target, Time.deltaTime * 9f);

        if (rb.linearVelocity.y < -4f)
        {
            puffTimer -= Time.deltaTime;
            if (puffTimer <= 0f)
            {
                puffTimer = 0.12f;
                if (EffectsManager.Instance != null)
                    EffectsManager.Instance.SpawnTrail(transform.position);
            }
        }

        CheckBounds();
    }

    void HandleInput()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Flap();
            return;
        }

        if (Input.GetMouseButtonDown(0) && !PointerOverUi())
        {
            Flap();
            return;
        }

        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            if (touch.phase == TouchPhase.Began && !PointerOverUi(touch.fingerId))
                Flap();
        }
    }

    static bool PointerOverUi()
    {
        return EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();
    }

    static bool PointerOverUi(int fingerId)
    {
        return EventSystem.current != null && EventSystem.current.IsPointerOverGameObject(fingerId);
    }

    public void Flap()
    {
        rb.linearVelocity = new Vector2(0f, flyForce);
        if (AudioManager.Instance != null) AudioManager.Instance.Play(Sfx.Flap, 0.85f);
        if (EffectsManager.Instance != null) EffectsManager.Instance.SpawnFlapPuff(transform.position);
    }

    void CheckBounds()
    {
        if (cam == null) cam = Camera.main;
        if (cam == null) return;

        float top = cam.transform.position.y + cam.orthographicSize;
        float bottom = cam.transform.position.y - cam.orthographicSize;
        float halfHeight = 0.58f;

        if (transform.position.y + halfHeight >= top - 0.02f ||
            transform.position.y - halfHeight <= bottom + 0.02f)
        {
            GameManager.Instance.EndGame(DeathCause.Boundary);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (GameManager.Instance == null || GameManager.Instance.State != GameState.Playing) return;
        if (other.CompareTag("Enemy"))
            GameManager.Instance.EndGame(DeathCause.Obstacle);
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (GameManager.Instance == null || GameManager.Instance.State != GameState.Playing) return;
        if (collision.collider.CompareTag("Enemy"))
            GameManager.Instance.EndGame(DeathCause.Obstacle);
    }
}
