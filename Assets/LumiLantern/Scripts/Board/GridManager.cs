using System;
using System.Collections.Generic;
using UnityEngine;

namespace LumiLantern
{
    public struct MoveAction
    {
        public GridCoord coord;
        public MirrorType oldType;
        public MirrorType newType;

        public MoveAction(GridCoord coord, MirrorType oldType, MirrorType newType)
        {
            this.coord = coord;
            this.oldType = oldType;
            this.newType = newType;
        }
    }

    public class GridManager : MonoBehaviour
    {
        public static GridManager Instance { get; private set; }

        [SerializeField] private float cellSize = 1.15f;
        public float CellSize => cellSize;

        private LevelData currentLevel;
        private Dictionary<GridCoord, TileView> tileViews = new Dictionary<GridCoord, TileView>();
        private Dictionary<GridCoord, MirrorType> placedMirrors = new Dictionary<GridCoord, MirrorType>();
        private Stack<MoveAction> undoStack = new Stack<MoveAction>();

        private BeamRenderer beamRenderer;
        private Transform boardRoot;
        private bool isInputEnabled = true;

        public event Action<SimulationResult> OnSimulationUpdated;

        private void Awake()
        {
            Instance = this;
        }

        public void Initialize(BeamRenderer beamRenderer)
        {
            this.beamRenderer = beamRenderer;
            GameObject root = new GameObject("BoardRoot");
            root.transform.SetParent(transform, false);
            boardRoot = root.transform;
        }

        public void LoadLevel(LevelData level, System.Func<string, Sprite> spriteLoader)
        {
            currentLevel = level;
            ClearBoard();

            // Spawn tile views
            for (int r = 1; r <= level.rows; r++)
            {
                for (int c = 1; c <= level.cols; c++)
                {
                    GridCoord coord = new GridCoord(r, c);
                    CellType cellType = level.GetCell(r, c);

                    GameObject tileObj = new GameObject($"Tile_{r}_{c}");
                    tileObj.transform.SetParent(boardRoot, false);

                    Vector2 localPos = LumiSimulation.CoordToWorld(coord, level) * cellSize;
                    tileObj.transform.localPosition = new Vector3(localPos.x, localPos.y, 0);
                    tileObj.transform.localScale = Vector3.one * (cellSize * 0.95f);

                    // Add box collider for touch/click detection
                    BoxCollider2D col = tileObj.AddComponent<BoxCollider2D>();
                    col.size = new Vector2(1f, 1f);

                    TileView view = tileObj.AddComponent<TileView>();
                    view.Initialize(coord, cellType, spriteLoader);
                    tileViews[coord] = view;
                }
            }

            // Adjust Camera position and size
            AdjustCamera(level);

            // Run initial simulation
            RunSimulation();
        }

        private void AdjustCamera(LevelData level)
        {
            Camera cam = Camera.main;
            if (cam == null) return;

            float gridWidth = level.cols * cellSize;
            float gridHeight = level.rows * cellSize;

            if (cam.aspect >= 1.0f)
            {
                // Landscape: offset board upward so bottom controls don't overlap the board
                cam.transform.position = new Vector3(0, 0.75f, -10f);
                float targetOrtho = gridHeight * 0.95f;
                cam.orthographic = true;
                cam.orthographicSize = Mathf.Max(4.2f, targetOrtho);
            }
            else
            {
                // Portrait (mobile)
                cam.transform.position = new Vector3(0, 0.4f, -10f);
                float targetOrtho = Mathf.Max(gridHeight * 0.95f, (gridWidth / cam.aspect) * 0.65f);
                cam.orthographic = true;
                cam.orthographicSize = Mathf.Max(4.2f, targetOrtho);
            }
        }

        private float lastAspect = -1f;

        public void SetInputEnabled(bool enabled)
        {
            isInputEnabled = enabled;
        }

        private void Update()
        {
            Camera cam = Camera.main;
            if (cam != null && currentLevel != null)
            {
                if (Mathf.Abs(lastAspect - cam.aspect) > 0.02f)
                {
                    lastAspect = cam.aspect;
                    AdjustCamera(currentLevel);
                }
            }

            if (!isInputEnabled || currentLevel == null) return;

            // PC Keyboard Shortcuts
            HandleKeyboardInput();

            // Unified Touch & Mouse Input
            bool inputTriggered = false;
            Vector3 screenPos = Vector3.zero;

            if (Input.touchCount > 0)
            {
                Touch t = Input.GetTouch(0);
                if (t.phase == TouchPhase.Began)
                {
                    inputTriggered = true;
                    screenPos = t.position;
                }
            }
            else if (Input.GetMouseButtonDown(0))
            {
                inputTriggered = true;
                screenPos = Input.mousePosition;
            }

            if (inputTriggered && cam != null)
            {
                // Only block board clicks if clicking on an interactive Button or modal dialog
                bool isOverInteractiveUI = false;
                if (UnityEngine.EventSystems.EventSystem.current != null)
                {
                    var pointerData = new UnityEngine.EventSystems.PointerEventData(UnityEngine.EventSystems.EventSystem.current)
                    {
                        position = screenPos
                    };
                    var rayResults = new List<UnityEngine.EventSystems.RaycastResult>();
                    UnityEngine.EventSystems.EventSystem.current.RaycastAll(pointerData, rayResults);

                    foreach (var result in rayResults)
                    {
                        if (result.gameObject.GetComponentInParent<UnityEngine.UI.Button>() != null ||
                            result.gameObject.name == "ChapterPanel" ||
                            result.gameObject.name == "WinModal")
                        {
                            isOverInteractiveUI = true;
                            break;
                        }
                    }
                }

                if (isOverInteractiveUI)
                {
                    return;
                }

                Vector3 worldPoint = cam.ScreenToWorldPoint(screenPos);
                Vector2 point2D = new Vector2(worldPoint.x, worldPoint.y);

                Collider2D hit = Physics2D.OverlapPoint(point2D);
                if (hit != null)
                {
                    TileView view = hit.GetComponent<TileView>();
                    if (view != null)
                    {
                        OnTileClicked(view);
                    }
                }
            }
        }

        private void HandleKeyboardInput()
        {
            // Z or U: Undo
            if (Input.GetKeyDown(KeyCode.Z) || Input.GetKeyDown(KeyCode.U))
            {
                Undo();
            }
            // R: Reset board
            if (Input.GetKeyDown(KeyCode.R))
            {
                ResetBoard();
            }
            // H: Request Hint
            if (Input.GetKeyDown(KeyCode.H))
            {
                GameManager.Instance?.RequestHint();
            }
            // Space / Enter: Next level if win modal is open
            if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return))
            {
                if (UIManager.Instance != null && UIManager.Instance.IsWinModalOpen())
                {
                    GameManager.Instance?.GoNextLevel();
                }
            }
            // Escape: Toggle Chapter Select
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (UIManager.Instance != null && UIManager.Instance.IsChapterSelectOpen())
                {
                    GameManager.Instance?.CloseChapterSelect();
                }
                else
                {
                    GameManager.Instance?.OpenChapterSelect();
                }
            }
        }

        private void OnTileClicked(TileView view)
        {
            // Only empty cells can receive player mirrors
            if (view.CellType != CellType.Empty) return;

            GridCoord coord = view.Coord;
            MirrorType current = MirrorType.None;
            placedMirrors.TryGetValue(coord, out current);

            MirrorType next = MirrorType.None;
            int placedCount = GetPlacedMirrorsCount();

            if (current == MirrorType.None)
            {
                // Check max mirrors limit
                if (placedCount >= currentLevel.maxMirrors)
                {
                    // At limit: cannot place new mirror
                    AudioManager.Instance?.PlayRemove();
                    StartCoroutine(WiggleTile(view.transform));
                    return;
                }
                next = MirrorType.Slash;
                AudioManager.Instance?.PlayPlace();
            }
            else if (current == MirrorType.Slash)
            {
                next = MirrorType.Backslash;
                AudioManager.Instance?.PlayRotate();
            }
            else if (current == MirrorType.Backslash)
            {
                next = MirrorType.None;
                AudioManager.Instance?.PlayRemove();
            }

            // Apply move
            undoStack.Push(new MoveAction(coord, current, next));
            SetMirrorAt(coord, next);
            RunSimulation();
        }

        public void Undo()
        {
            if (undoStack.Count == 0) return;

            MoveAction last = undoStack.Pop();
            SetMirrorAt(last.coord, last.oldType);
            AudioManager.Instance?.PlayRotate();
            RunSimulation();
        }

        public void ResetBoard()
        {
            if (placedMirrors.Count == 0) return;

            placedMirrors.Clear();
            undoStack.Clear();
            foreach (var kvp in tileViews)
            {
                if (kvp.Value.CellType == CellType.Empty)
                {
                    kvp.Value.SetPlayerMirror(MirrorType.None, false);
                }
            }
            AudioManager.Instance?.PlayRemove();
            RunSimulation();
        }

        public void ApplyHint(MirrorPlacement hint)
        {
            if (hint == null) return;
            // Place hint mirror directly
            MirrorType current = MirrorType.None;
            placedMirrors.TryGetValue(hint.coord, out current);
            undoStack.Push(new MoveAction(hint.coord, current, hint.type));
            SetMirrorAt(hint.coord, hint.type);
            AudioManager.Instance?.PlayPlace();
            RunSimulation();
        }

        public void HighlightTile(GridCoord coord, bool active)
        {
            if (tileViews.TryGetValue(coord, out var view))
            {
                view.SetHighlight(active);
            }
        }

        public void ClearAllHighlights()
        {
            foreach (var view in tileViews.Values)
            {
                view.SetHighlight(false);
            }
        }

        private void SetMirrorAt(GridCoord coord, MirrorType type)
        {
            if (type == MirrorType.None)
            {
                placedMirrors.Remove(coord);
            }
            else
            {
                placedMirrors[coord] = type;
            }

            if (tileViews.TryGetValue(coord, out var view))
            {
                view.SetPlayerMirror(type, true);
            }
        }

        public int GetPlacedMirrorsCount()
        {
            int count = 0;
            foreach (var kvp in placedMirrors)
            {
                if (kvp.Value != MirrorType.None) count++;
            }
            return count;
        }

        public SimulationResult RunSimulation()
        {
            if (currentLevel == null) return null;

            SimulationResult result = LumiSimulation.Simulate(currentLevel, placedMirrors);

            // Update tiles visuals
            foreach (var kvp in tileViews)
            {
                GridCoord coord = kvp.Key;
                TileView view = kvp.Value;

                // Lantern lit state
                bool isLantern = (view.CellType == CellType.LanternNormal || view.CellType == CellType.LanternRed || view.CellType == CellType.LanternBlue);
                if (isLantern)
                {
                    bool wasLit = result.litLanterns.Contains(coord);
                    view.SetLanternLit(wasLit);
                }

                // Gate state
                if (view.CellType == CellType.RhythmGate)
                {
                    view.SetGateState(result.isGateOpen);
                }
            }

            // Render laser beams
            if (beamRenderer != null)
            {
                beamRenderer.RenderBeams(result.segments, cellSize);
            }

            OnSimulationUpdated?.Invoke(result);
            return result;
        }

        private void ClearBoard()
        {
            placedMirrors.Clear();
            undoStack.Clear();
            tileViews.Clear();
            if (beamRenderer != null) beamRenderer.Clear();

            if (boardRoot != null)
            {
                for (int i = boardRoot.childCount - 1; i >= 0; i--)
                {
                    Destroy(boardRoot.GetChild(i).gameObject);
                }
            }
        }

        private System.Collections.IEnumerator WiggleTile(Transform target)
        {
            Vector3 original = target.localPosition;
            float elapsed = 0f;
            float duration = 0.2f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float xOffset = Mathf.Sin(elapsed * 50f) * 0.08f;
                target.localPosition = original + new Vector3(xOffset, 0, 0);
                yield return null;
            }
            target.localPosition = original;
        }
    }
}
