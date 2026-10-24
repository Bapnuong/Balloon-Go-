using UnityEngine;

public class Coin : MonoBehaviour
{
    public int value = 1;
    bool collected;

    void Awake()
    {
        // The imported animation replaces the sprite with sliced frames that
        // can be invisible after runtime prefab overrides. Keep the visible
        // prefab frame and let CoinMove handle the simple spin effect.
        var animator = GetComponent<Animator>();
        if (animator != null) animator.enabled = false;

        // Coins must render in front of branches and background layers.
        var sr = GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            sr.enabled = true;
            sr.sortingLayerID = SortingLayer.NameToID("Player");
            sr.sortingOrder = 20;
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
