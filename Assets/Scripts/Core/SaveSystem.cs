using UnityEngine;

public static class SaveSystem
{
    const string Prefix = "BalloonGo_";

    const string KeyHighScore = Prefix + "HighScore";
    const string KeyCoins = Prefix + "Coins";
    const string KeyOwned = Prefix + "Owned";
    const string KeySelected = Prefix + "Selected";
    const string KeySound = Prefix + "Sound";
    const string KeyStorySeen = Prefix + "StorySeen";

    public static int LoadHighScore() => PlayerPrefs.GetInt(KeyHighScore, 0);

    public static void SaveHighScore(int score)
    {
        PlayerPrefs.SetInt(KeyHighScore, Mathf.Max(0, score));
        PlayerPrefs.Save();
    }

    public static int LoadCoins() => PlayerPrefs.GetInt(KeyCoins, 0);

    public static void SaveCoins(int coins)
    {
        PlayerPrefs.SetInt(KeyCoins, Mathf.Max(0, coins));
        PlayerPrefs.Save();
    }

    public static string LoadOwned() => PlayerPrefs.GetString(KeyOwned, "");

    public static void SaveOwned(string owned)
    {
        PlayerPrefs.SetString(KeyOwned, owned ?? "");
        PlayerPrefs.Save();
    }

    public static string LoadSelected() => PlayerPrefs.GetString(KeySelected, "");

    public static void SaveSelected(string id)
    {
        PlayerPrefs.SetString(KeySelected, id ?? "");
        PlayerPrefs.Save();
    }

    public static bool LoadSound() => PlayerPrefs.GetInt(KeySound, 1) == 1;

    public static void SaveSound(bool enabled)
    {
        PlayerPrefs.SetInt(KeySound, enabled ? 1 : 0);
        PlayerPrefs.Save();
    }

    public static bool LoadStorySeen() => PlayerPrefs.GetInt(KeyStorySeen, 0) == 1;

    public static void SaveStorySeen()
    {
        PlayerPrefs.SetInt(KeyStorySeen, 1);
        PlayerPrefs.Save();
    }
}
