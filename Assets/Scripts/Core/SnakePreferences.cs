using UnityEngine;

namespace SnakeTrio
{
    public static class SnakePreferences
    {
        public static bool Fullscreen
        {
            get => PlayerPrefs.GetInt("SnakeTrio_Fullscreen", 0) == 1;
            set { PlayerPrefs.SetInt("SnakeTrio_Fullscreen", value ? 1 : 0); PlayerPrefs.Save(); }
        }
        public static int Difficulty
        {
            get => Mathf.Clamp(PlayerPrefs.GetInt("SnakeTrio_Difficulty", 1), 0, 2);
            set { PlayerPrefs.SetInt("SnakeTrio_Difficulty", Mathf.Clamp(value, 0, 2)); PlayerPrefs.Save(); }
        }
        public static float MusicVolume
        {
            get => Mathf.Clamp01(PlayerPrefs.GetFloat("SnakeTrio_Music", .25f));
            set { PlayerPrefs.SetFloat("SnakeTrio_Music", Mathf.Clamp01(value)); PlayerPrefs.Save(); }
        }
        public static float EffectsVolume
        {
            get => Mathf.Clamp01(PlayerPrefs.GetFloat("SnakeTrio_Effects", .32f));
            set { PlayerPrefs.SetFloat("SnakeTrio_Effects", Mathf.Clamp01(value)); PlayerPrefs.Save(); }
        }
        public static bool Muted
        {
            get => PlayerPrefs.GetInt("SnakeTrio_Muted", 0) == 1;
            set { PlayerPrefs.SetInt("SnakeTrio_Muted", value ? 1 : 0); PlayerPrefs.Save(); }
        }
    }
}
