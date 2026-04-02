
using UnityEngine;

public class SaveSystem
{
    public static void SaveHighScore(int score)
    {
        int high = PlayerPrefs.GetInt("HighScore",0);
        if(score > high)
        {
            PlayerPrefs.SetInt("HighScore",score);
        }
    }

    public static int LoadHighScore()
    {
        return PlayerPrefs.GetInt("HighScore",0);
    }
}
