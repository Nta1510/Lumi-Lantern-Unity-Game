using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LumiLantern
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [SerializeField] private int startLevel = 1;

        private LevelData currentLevel;
        private int currentLevelNumber = 1;
        private bool isLevelCompleted = false;

        private GridManager gridManager;
        private UIManager uiManager;
        private BeamRenderer beamRenderer;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
                return;
            }

            SetupCamera();
            EnsureComponents();
        }

        private void SetupCamera()
        {
            Camera cam = Camera.main;
            if (cam == null)
            {
                GameObject camObj = new GameObject("Main Camera");
                cam = camObj.AddComponent<Camera>();
                camObj.tag = "MainCamera";
            }
            cam.clearFlags = CameraClearFlags.SolidColor;
            // Warm dark purple background #151124 matching GDD mockups
            cam.backgroundColor = new Color(0.082f, 0.067f, 0.141f, 1f);
            cam.orthographic = true;
        }

        private void EnsureComponents()
        {
            try
            {
                // AudioManager
                if (AudioManager.Instance == null)
                {
                    GameObject audioObj = new GameObject("AudioManager");
                    audioObj.transform.SetParent(transform, false);
                    audioObj.AddComponent<AudioManager>();
                }

                // BeamRenderer
                GameObject beamObj = new GameObject("BeamRenderer");
                beamObj.transform.SetParent(transform, false);
                beamRenderer = beamObj.AddComponent<BeamRenderer>();
                beamRenderer.Initialize(LumiAssetLoader.GetSprite);

                // GridManager
                GameObject gridObj = new GameObject("GridManager");
                gridObj.transform.SetParent(transform, false);
                gridManager = gridObj.AddComponent<GridManager>();
                gridManager.Initialize(beamRenderer);
                gridManager.OnSimulationUpdated += HandleSimulationUpdated;

                // UIManager
                GameObject uiObj = new GameObject("UIManager");
                uiObj.transform.SetParent(transform, false);
                uiManager = uiObj.AddComponent<UIManager>();
                uiManager.Initialize();

                Debug.Log("[GameManager] All components initialized.");
            }
            catch (System.Exception e)
            {
                Debug.LogError("[GameManager] EnsureComponents failed: " + e.Message + "\n" + e.StackTrace);
            }
        }

        private void Start()
        {
            if (gridManager == null || uiManager == null)
            {
                Debug.LogError("[GameManager] Components not initialized! Cannot load level.");
                return;
            }
            // Start at Level 1 (per GDD: straight into Level 1 without splash/menu)
            currentLevelNumber = startLevel;
            LoadLevel(currentLevelNumber);
        }

        public void LoadLevel(int levelNumber)
        {
            if (gridManager == null || uiManager == null) return;
            currentLevelNumber = Mathf.Clamp(levelNumber, 1, 15);
            currentLevel = LevelDatabase.GetLevel(currentLevelNumber);
            isLevelCompleted = false;

            gridManager.SetInputEnabled(true);
            gridManager.LoadLevel(currentLevel, LumiAssetLoader.GetSprite);

            uiManager.HideWinModal();
            uiManager.HideChapterSelect();
            uiManager.UpdateHeader(currentLevel, 0);

            CheckOnboarding();
        }

        public void ReplayLevel()
        {
            LoadLevel(currentLevelNumber);
        }

        public void GoNextLevel()
        {
            int next = currentLevelNumber + 1;
            if (next > 15) next = 1; // Loop back or chapter select
            LoadLevel(next);
        }

        public void LoadDailyLevel()
        {
            // Daily puzzle: Level 15 (Màn tổng hợp)
            LoadLevel(15);
        }

        public void OpenChapterSelect()
        {
            uiManager.ShowChapterSelect();
        }

        public void CloseChapterSelect()
        {
            uiManager.HideChapterSelect();
        }

        public void RequestHint()
        {
            if (isLevelCompleted) return;
            if (SaveSystem.GetHintsRemaining() <= 0)
            {
                uiManager.UpdateHeader(currentLevel, gridManager.GetPlacedMirrorsCount());
                return;
            }

            // Find missing solution mirror
            if (currentLevel.solution != null && currentLevel.solution.Count > 0)
            {
                MirrorPlacement targetPlacement = null;
                foreach (var placement in currentLevel.solution)
                {
                    // Check if already placed correctly
                    // We can check through GridManager
                    targetPlacement = placement;
                    break;
                }

                if (targetPlacement != null)
                {
                    SaveSystem.UseHint();
                    gridManager.HighlightTile(targetPlacement.coord, true);
                    gridManager.ApplyHint(targetPlacement);
                    uiManager.UpdateHeader(currentLevel, gridManager.GetPlacedMirrorsCount());
                }
            }
        }

        private void CheckOnboarding()
        {
            gridManager.ClearAllHighlights();
            uiManager.HideTutorialHand();

            if (currentLevelNumber == 1)
            {
                // Level 1: Highlight cell (3, 4) and show animated pointing hand
                GridCoord targetCoord = new GridCoord(3, 4);
                gridManager.HighlightTile(targetCoord, true);

                StartCoroutine(DelayedShowHand(targetCoord));
            }
        }

        private IEnumerator DelayedShowHand(GridCoord coord)
        {
            yield return new WaitForSeconds(0.1f);
            Vector2 worldPos = LumiSimulation.CoordToWorld(coord, currentLevel) * gridManager.CellSize;
            Vector3 screenPos = Camera.main.WorldToScreenPoint(new Vector3(worldPos.x, worldPos.y, 0));
            uiManager.ShowTutorialHand(screenPos);
        }

        private void HandleSimulationUpdated(SimulationResult result)
        {
            if (currentLevel == null) return;

            uiManager.UpdateSimulationStatus(result, currentLevel);

            // Hide tutorial hand once player interacts
            if (currentLevelNumber == 1 && result.placedMirrorsCount > 0)
            {
                uiManager.HideTutorialHand();
                gridManager.ClearAllHighlights();
            }

            // Check Win Condition
            if (result.isWin && !isLevelCompleted)
            {
                isLevelCompleted = true;
                StartCoroutine(HandleWinSequence(result));
            }
        }

        private IEnumerator HandleWinSequence(SimulationResult result)
        {
            gridManager.SetInputEnabled(false);

            // Play lantern ignite sound
            AudioManager.Instance?.PlayLantern();
            yield return new WaitForSeconds(0.4f);

            // Play victory fanfare
            AudioManager.Instance?.PlayWin();

            // Save progress
            SaveSystem.SetLevelStars(currentLevelNumber, result.stars);
            SaveSystem.UnlockLevel(currentLevelNumber + 1);

            yield return new WaitForSeconds(0.6f);

            // Show win dialog
            uiManager.ShowWinModal(result.stars, result.placedMirrorsCount, currentLevel.minMirrors);
        }
    }
}
