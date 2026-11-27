using UnityEngine;

namespace LumiLantern
{
    public static class LumiBootstrap
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void AutoStartGame()
        {
            if (GameManager.Instance == null)
            {
                Debug.Log("[LumiBootstrap] Initializing Lumi Lantern Game...");
                GameObject root = new GameObject("--- LUMI LANTERN GAME ---");
                root.AddComponent<GameManager>();
            }
        }
    }
}
