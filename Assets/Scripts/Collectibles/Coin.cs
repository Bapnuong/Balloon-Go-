using UnityEngine;

public class Coin : MonoBehaviour
{
    public int value = 1;
    bool collected;

    void Awake()
    {
        // Coins must render in front of branches; the old prefab sat on the
        // Item sorting layer, behind every obstacle, so they were invisible.
        var sr = GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            sr.sortingLayerName = "Player";
            sr.sortingOrder = 5;
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (collected) return;
        if (!other.CompareTag("Player")) return;

        collected = true;
        if (ScoreManager.Instance != null) ScoreManager.Instance.AddCoin(value);
        if (AudioManager.Instance != null) AudioManager.Instance.Play(Sfx.Coin, 0.8f);
        if (EffectsManager.Instance != null) EffectsManager.Instance.SpawnCoinBurst(transform.position);
        if (UIManager.Instance != null) UIManager.Instance.PulseCoins();
        Destroy(gameObject);
    }
}
