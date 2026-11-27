using UnityEngine;

namespace LumiLantern
{
    public static class SaveSystem
    {
        private const string KEY_UNLOCKED_LEVEL = "Lumi_UnlockedLevel";
        private const string KEY_LEVEL_STARS = "Lumi_Stars_Level_";
        private const string KEY_HINTS_COUNT = "Lumi_HintsCount";

        public static int GetUnlockedLevel()
        {
            return PlayerPrefs.GetInt(KEY_UNLOCKED_LEVEL, 1);
        }

        public static void UnlockLevel(int level)
        {
            int current = GetUnlockedLevel();
            if (level > current)
            {
                PlayerPrefs.SetInt(KEY_UNLOCKED_LEVEL, level);
                PlayerPrefs.Save();
            }
        }

        public static int GetLevelStars(int level)
        {
            return PlayerPrefs.GetInt(KEY_LEVEL_STARS + level, 0);
        }

        public static void SetLevelStars(int level, int stars)
        {
            int current = GetLevelStars(level);
            if (stars > current)
            {
                PlayerPrefs.SetInt(KEY_LEVEL_STARS + level, stars);
                PlayerPrefs.Save();
            }
        }

        public static int GetTotalStars()
        {
            int total = 0;
            for (int i = 1; i <= 15; i++)
            {
                total += GetLevelStars(i);
            }
            return total;
        }

        public static int GetHintsRemaining()
        {
            return PlayerPrefs.GetInt(KEY_HINTS_COUNT, 3);
        }

        public static void UseHint()
        {
            int current = GetHintsRemaining();
            if (current > 0)
            {
                PlayerPrefs.SetInt(KEY_HINTS_COUNT, current - 1);
                PlayerPrefs.Save();
            }
        }

        public static void ResetAllProgress()
        {
            PlayerPrefs.DeleteAll();
            PlayerPrefs.Save();
        }
    }
}
