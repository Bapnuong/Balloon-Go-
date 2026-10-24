using System;
using System.Collections.Generic;
using UnityEngine;

public enum ShopCategory
{
    Color,
    Shape
}

public enum ShopActionResult
{
    Selected,
    Purchased,
    NotEnoughCoins,
    Invalid
}

[Serializable]
public class ShopItem
{
    public string Id;
    public string Name;
    public string SpritePath;
    public ShopCategory Category;
    public int Price;
    public string Description;

    Sprite cached;

    public Sprite Sprite
    {
        get
        {
            if (cached == null)
                cached = Resources.Load<Sprite>("Balloons/" + SpritePath);
            return cached;
        }
    }
}

public class ShopSystem : MonoBehaviour
{
    public static ShopSystem Instance;

    public const string DefaultId = "classic_pink";

    public readonly List<ShopItem> Items = new List<ShopItem>();
    readonly HashSet<string> owned = new HashSet<string>();

    public string SelectedId { get; private set; }
    public ShopItem SelectedItem { get; private set; }

    public event Action OnSelectionChanged;
    public event Action OnShopChanged;

    void Awake()
    {
        Instance = this;
        BuildCatalog();
        LoadProgress();
    }

    void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    void BuildCatalog()
    {
        Items.Clear();
        Add("classic_pink", "Hồng đào", "balloon_1", ShopCategory.Color, 0, "Chiếc bóng bay đầu tiên của bạn.");
        Add("classic_purple", "Tím mộng", "balloon_2", ShopCategory.Color, 15, "Một chút mộng mơ giữa rừng sương.");
        Add("classic_cyan", "Xanh băng", "balloon_3", ShopCategory.Color, 15, "Mát lạnh như làn sương sớm.");
        Add("classic_green", "Xanh lá", "balloon_4", ShopCategory.Color, 20, "Hòa mình vào tán rừng.");
        Add("classic_yellow", "Vàng nắng", "balloon_5", ShopCategory.Color, 20, "Mang theo một tia nắng.");
        Add("classic_orange", "Cam cháy", "balloon_6", ShopCategory.Color, 25, "Ấm áp như hoàng hôn.");
        Add("classic_red", "Đỏ rực", "balloon_7", ShopCategory.Color, 25, "Nổi bật giữa màn sương.");
        Add("classic_lightpink", "Hồng phấn", "balloon_8", ShopCategory.Color, 30, "Dịu dàng và tinh khôi.");
        Add("classic_lavender", "Oải hương", "balloon_9", ShopCategory.Color, 35, "Mùi hương của rừng sâu.");
        Add("classic_gold", "Hoàng kim", "balloon_10", ShopCategory.Color, 50, "Chiếc bóng của những nhà phiêu lưu.");

        Add("shape_heart_pink", "Tim hồng", "balloon_heart1", ShopCategory.Shape, 40, "Bay bằng cả trái tim.");
        Add("shape_heart_red", "Tim đỏ", "balloon_heart2", ShopCategory.Shape, 45, "Yêu thương đong đầy.");
        Add("shape_star_yellow", "Sao vàng", "balloon_star1", ShopCategory.Shape, 50, "Lấp lánh giữa rừng đêm.");
        Add("shape_star_green", "Sao xanh", "balloon_star2", ShopCategory.Shape, 55, "Một vì sao của riêng bạn.");
        Add("shape_pixel_green", "Pixel xanh", "PixelBalloon2", ShopCategory.Shape, 30, "Hoài niệm 8-bit.");
        Add("shape_pixel_pink", "Pixel hồng", "PixelBalloon1", ShopCategory.Shape, 30, "Một chút retro dễ thương.");
        Add("shape_pixel_long_blue", "Pixel dài", "PixelBalloonLong1", ShopCategory.Shape, 35, "Cao hơn, dài hơn, ngầu hơn.");
        Add("shape_pixel_long_orange", "Pixel cam", "PixelBalloonLong2", ShopCategory.Shape, 35, "Phong cách pixel cổ điển.");
    }

    void Add(string id, string name, string sprite, ShopCategory category, int price, string description)
    {
        Items.Add(new ShopItem
        {
            Id = id,
            Name = name,
            SpritePath = sprite,
            Category = category,
            Price = price,
            Description = description
        });
    }

    void LoadProgress()
    {
        owned.Clear();
        string saved = SaveSystem.LoadOwned();
        if (!string.IsNullOrEmpty(saved))
        {
            string[] parts = saved.Split(',');
            for (int i = 0; i < parts.Length; i++)
            {
                if (!string.IsNullOrEmpty(parts[i]))
                    owned.Add(parts[i]);
            }
        }

        if (!owned.Contains(DefaultId))
            owned.Add(DefaultId);

        string selected = SaveSystem.LoadSelected();
        ShopItem item = GetItem(selected);
        if (item == null || !owned.Contains(item.Id))
            item = GetItem(DefaultId);

        SelectedItem = item;
        SelectedId = item.Id;
        SaveOwned();
        SaveSelected();
    }

    void SaveOwned()
    {
        SaveSystem.SaveOwned(string.Join(",", owned));
    }

    void SaveSelected()
    {
        SaveSystem.SaveSelected(SelectedId);
    }

    public ShopItem GetItem(string id)
    {
        if (string.IsNullOrEmpty(id)) return null;
        for (int i = 0; i < Items.Count; i++)
        {
            if (Items[i].Id == id) return Items[i];
        }
        return null;
    }

    public List<ShopItem> GetItems(ShopCategory category)
    {
        var list = new List<ShopItem>();
        for (int i = 0; i < Items.Count; i++)
        {
            if (Items[i].Category == category)
                list.Add(Items[i]);
        }
        return list;
    }

    public bool IsOwned(string id) => owned.Contains(id);

    public bool IsSelected(string id) => SelectedId == id;

    public bool CanAfford(ShopItem item)
    {
        if (item == null) return false;
        if (owned.Contains(item.Id)) return true;
        return ScoreManager.Instance != null && ScoreManager.Instance.TotalCoins >= item.Price;
    }

    public ShopActionResult BuyOrSelect(ShopItem item)
    {
        if (item == null) return ShopActionResult.Invalid;

        if (owned.Contains(item.Id))
        {
            if (SelectedId == item.Id)
                return ShopActionResult.Selected;

            SelectedId = item.Id;
            SelectedItem = item;
            SaveSelected();
            OnSelectionChanged?.Invoke();
            OnShopChanged?.Invoke();
            return ShopActionResult.Selected;
        }

        if (ScoreManager.Instance == null || !ScoreManager.Instance.SpendCoins(item.Price))
            return ShopActionResult.NotEnoughCoins;

        owned.Add(item.Id);
        SaveOwned();

        SelectedId = item.Id;
        SelectedItem = item;
        SaveSelected();

        OnSelectionChanged?.Invoke();
        OnShopChanged?.Invoke();
        return ShopActionResult.Purchased;
    }

    public Sprite GetSelectedSprite()
    {
        return SelectedItem != null ? SelectedItem.Sprite : null;
    }
}
