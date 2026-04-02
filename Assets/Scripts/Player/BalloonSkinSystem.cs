
using UnityEngine;

public class BalloonSkinSystem : MonoBehaviour
{
    public SpriteRenderer balloonRenderer;

    public Color[] skins;

    int current;

    public void NextSkin()
    {
        current++;

        if(current >= skins.Length)
            current = 0;

        balloonRenderer.color = skins[current];
    }
}
