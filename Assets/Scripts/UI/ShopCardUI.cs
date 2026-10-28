using UnityEngine;
using UnityEngine.UI;

public class ShopCardUI : MonoBehaviour
{
    public ShopItem Item;
    public Image background;
    public Image icon;
    public Text nameText;
    public Text statusText;
    public Button button;

    public void Refresh(ShopItem item, bool owned, bool selected, bool affordable)
    {
        if (item == null) return;

        icon.sprite = item.Sprite;
        icon.preserveAspect = true;
        nameText.text = item.Name;
        background.sprite = UITheme.WoodCard;
        background.type = Image.Type.Sliced;

        if (selected)
        {
            background.color = UITheme.WoodHoney;
            nameText.color = UITheme.WoodDeepTone;
            statusText.text = "ĐANG DÙNG";
            statusText.color = UITheme.WoodDeepTone;
            icon.color = Color.white;
        }
        else if (owned)
        {
            background.color = UITheme.WoodOak;
            nameText.color = UITheme.Cream;
            statusText.text = "ĐÃ CÓ";
            statusText.color = UITheme.Mint;
            icon.color = Color.white;
        }
        else
        {
            background.color = UITheme.WoodDarkTone;
            nameText.color = affordable ? UITheme.Cream : UITheme.Muted;
            statusText.text = item.Price + " xu";
            statusText.color = affordable ? UITheme.Gold : UITheme.Coral;
            icon.color = affordable ? Color.white : new Color(1f, 1f, 1f, 0.62f);
        }
    }
}
