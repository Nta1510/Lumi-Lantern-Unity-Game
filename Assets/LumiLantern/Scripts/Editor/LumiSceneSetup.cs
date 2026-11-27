#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace LumiLantern.Editor
{
    public static class LumiSceneSetup
    {
        [MenuItem("Lumi Lantern/Setup Main Scene", false, 1)]
        public static void SetupMainScene()
        {
            Scene scene = EditorSceneManager.GetActiveScene();

            // Setup or find Camera
            Camera cam = Camera.main;
            if (cam == null)
            {
                GameObject camObj = new GameObject("Main Camera");
                cam = camObj.AddComponent<Camera>();
                camObj.tag = "MainCamera";
            }

            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.082f, 0.067f, 0.141f, 1f);
            cam.orthographic = true;
            cam.orthographicSize = 4.5f;
            cam.transform.position = new Vector3(0, 0.4f, -10f);

            // Add or ensure Game Manager
            GameManager gm = Object.FindObjectOfType<GameManager>();
            if (gm == null)
            {
                GameObject gmObj = new GameObject("--- LUMI LANTERN GAME ---");
                gm = gmObj.AddComponent<GameManager>();
            }

            // Mark dirty & save
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Debug.Log("[Lumi Lantern] Scene setup successfully for Lumi Lantern!");
        }

        [MenuItem("Lumi Lantern/Reset All Save Data", false, 20)]
        public static void ResetData()
        {
            SaveSystem.ResetAllProgress();
            Debug.Log("[Lumi Lantern] All player progress and stars have been reset!");
        }

        [MenuItem("Lumi Lantern/Verify All 15 Levels In Editor", false, 30)]
        public static void VerifyLevels()
        {
            var levels = LevelDatabase.GetAllLevels();
            int passed = 0;
            for (int i = 0; i < levels.Count; i++)
            {
                var lvl = levels[i];
                var placed = new System.Collections.Generic.Dictionary<GridCoord, MirrorType>();
                foreach (var sol in lvl.solution)
                {
                    placed[sol.coord] = sol.type;
                }

                var res = LumiSimulation.Simulate(lvl, placed);
                if (res.isWin && res.stars == 3)
                {
                    passed++;
                    Debug.Log($"<color=green>[PASS]</color> Màn {lvl.levelNumber}: {lvl.levelName} - 3 Sao hoàn hảo!");
                }
                else
                {
                    Debug.LogError($"<color=red>[FAIL]</color> Màn {lvl.levelNumber}: {lvl.levelName} - Lit: {res.litLanterns.Count}/{res.totalLanterns.Count}, Stars: {res.stars}");
                }
            }

            Debug.Log($"<b>[Lumi Lantern] Kết quả kiểm thử: {passed}/{levels.Count} màn chơi chính xác 100%!</b>");
        }
    }
}
#endif
