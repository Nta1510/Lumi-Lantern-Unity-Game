using System.Collections;
using UnityEngine;

namespace LumiLantern
{
    public class TileView : MonoBehaviour
    {
        public GridCoord Coord { get; private set; }
        public CellType CellType { get; private set; }
        public MirrorType PlacedMirror { get; private set; } = MirrorType.None;

        private SpriteRenderer bgRenderer;
        private SpriteRenderer itemRenderer;
        private SpriteRenderer mirrorRenderer;
        private SpriteRenderer highlightRenderer;

        // Sprites cache
        private Sprite spriteLanternOff;
        private Sprite spriteLanternOn;
        private Sprite spriteGateClosed;
        private Sprite spriteGateOpen;
        private Sprite spriteMirrorSlash;
        private Sprite spriteMirrorBackslash;

        private bool isLit = false;
        private Coroutine pulseRoutine;

        public void Initialize(GridCoord coord, CellType cellType, System.Func<string, Sprite> spriteLoader)
        {
            Coord = coord;
            CellType = cellType;

            // 1. Background Renderer
            GameObject bgObj = new GameObject("BG");
            bgObj.transform.SetParent(transform, false);
            bgRenderer = bgObj.AddComponent<SpriteRenderer>();
            bgRenderer.sprite = spriteLoader("tile_bg");
            bgRenderer.sortingOrder = 0;

            // 2. Item Renderer (Wall, Lantern, Source, Filter, etc.)
            GameObject itemObj = new GameObject("Item");
            itemObj.transform.SetParent(transform, false);
            itemRenderer = itemObj.AddComponent<SpriteRenderer>();
            itemRenderer.sortingOrder = 2;

            // 3. Player Mirror Renderer
            GameObject mirrorObj = new GameObject("Mirror");
            mirrorObj.transform.SetParent(transform, false);
            mirrorRenderer = mirrorObj.AddComponent<SpriteRenderer>();
            mirrorRenderer.sortingOrder = 4;

            // 4. Highlight Renderer (Tutorial / Hint dashed box)
            GameObject hlObj = new GameObject("Highlight");
            hlObj.transform.SetParent(transform, false);
            highlightRenderer = hlObj.AddComponent<SpriteRenderer>();
            highlightRenderer.sprite = spriteLoader("dashed_box");
            highlightRenderer.sortingOrder = 10;
            highlightRenderer.enabled = false;

            // Cache standard sprites
            spriteMirrorSlash = spriteLoader("mirror_slash");
            spriteMirrorBackslash = spriteLoader("mirror_backslash");
            spriteGateClosed = spriteLoader("gate_closed");
            spriteGateOpen = spriteLoader("gate_open");

            // Configure item sprite by cell type
            SetupItem(spriteLoader);
        }

        private void SetupItem(System.Func<string, Sprite> spriteLoader)
        {
            itemRenderer.enabled = true;
            itemRenderer.transform.localRotation = Quaternion.identity;

            switch (CellType)
            {
                case CellType.Wall:
                    itemRenderer.sprite = spriteLoader("wall");
                    break;

                case CellType.SourceRight:
                    itemRenderer.sprite = spriteLoader("source");
                    itemRenderer.transform.localRotation = Quaternion.Euler(0, 0, 0);
                    break;
                case CellType.SourceUp:
                    itemRenderer.sprite = spriteLoader("source");
                    itemRenderer.transform.localRotation = Quaternion.Euler(0, 0, 90);
                    break;
                case CellType.SourceLeft:
                    itemRenderer.sprite = spriteLoader("source");
                    itemRenderer.transform.localRotation = Quaternion.Euler(0, 0, 180);
                    break;
                case CellType.SourceDown:
                    itemRenderer.sprite = spriteLoader("source");
                    itemRenderer.transform.localRotation = Quaternion.Euler(0, 0, 270);
                    break;

                case CellType.LanternNormal:
                    spriteLanternOff = spriteLoader("lantern_normal_off");
                    spriteLanternOn = spriteLoader("lantern_normal_on");
                    itemRenderer.sprite = spriteLanternOff;
                    break;
                case CellType.LanternRed:
                    spriteLanternOff = spriteLoader("lantern_red_off");
                    spriteLanternOn = spriteLoader("lantern_red_on");
                    itemRenderer.sprite = spriteLanternOff;
                    break;
                case CellType.LanternBlue:
                    spriteLanternOff = spriteLoader("lantern_blue_off");
                    spriteLanternOn = spriteLoader("lantern_blue_on");
                    itemRenderer.sprite = spriteLanternOff;
                    break;

                case CellType.FilterRed:
                    itemRenderer.sprite = spriteLoader("filter_red");
                    break;
                case CellType.FilterBlue:
                    itemRenderer.sprite = spriteLoader("filter_blue");
                    break;

                case CellType.SplitterSlash:
                    itemRenderer.sprite = spriteLoader("splitter_slash");
                    break;
                case CellType.SplitterBackslash:
                    itemRenderer.sprite = spriteLoader("splitter_backslash");
                    break;

                case CellType.FixedMirrorSlash:
                    itemRenderer.sprite = spriteLoader("fixed_mirror_slash");
                    break;
                case CellType.FixedMirrorBackslash:
                    itemRenderer.sprite = spriteLoader("fixed_mirror_backslash");
                    break;

                case CellType.RhythmGate:
                    itemRenderer.sprite = spriteGateClosed;
                    break;

                default: // Empty
                    itemRenderer.enabled = false;
                    break;
            }
        }

        public void SetLanternLit(bool lit)
        {
            if (spriteLanternOn == null || spriteLanternOff == null) return;
            if (isLit == lit) return;

            isLit = lit;
            itemRenderer.sprite = lit ? spriteLanternOn : spriteLanternOff;

            if (lit)
            {
                StopAllCoroutines();
                StartCoroutine(PunchScale(itemRenderer.transform, 1.25f, 0.25f));
            }
        }

        public void SetGateState(bool isOpen)
        {
            if (CellType != CellType.RhythmGate) return;
            itemRenderer.sprite = isOpen ? spriteGateOpen : spriteGateClosed;
        }

        public void SetPlayerMirror(MirrorType type, bool animate = true)
        {
            PlacedMirror = type;
            if (type == MirrorType.Slash)
            {
                mirrorRenderer.enabled = true;
                mirrorRenderer.sprite = spriteMirrorSlash;
                if (animate) StartCoroutine(PunchScale(mirrorRenderer.transform, 1.25f, 0.18f));
            }
            else if (type == MirrorType.Backslash)
            {
                mirrorRenderer.enabled = true;
                mirrorRenderer.sprite = spriteMirrorBackslash;
                if (animate) StartCoroutine(PunchScale(mirrorRenderer.transform, 1.25f, 0.18f));
            }
            else
            {
                mirrorRenderer.enabled = false;
            }
        }

        public void SetHighlight(bool active)
        {
            highlightRenderer.enabled = active;
            if (active)
            {
                if (pulseRoutine != null) StopCoroutine(pulseRoutine);
                pulseRoutine = StartCoroutine(PulseHighlight());
            }
            else
            {
                if (pulseRoutine != null) StopCoroutine(pulseRoutine);
                highlightRenderer.transform.localScale = Vector3.one;
            }
        }

        private IEnumerator PulseHighlight()
        {
            while (true)
            {
                float t = (Mathf.Sin(Time.time * 5f) + 1f) * 0.5f;
                float s = Mathf.Lerp(0.92f, 1.08f, t);
                highlightRenderer.transform.localScale = new Vector3(s, s, 1f);
                yield return null;
            }
        }

        private IEnumerator PunchScale(Transform target, float maxScale, float duration)
        {
            float elapsed = 0f;
            Vector3 originalScale = Vector3.one;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float progress = elapsed / duration;
                float currentScale = Mathf.Sin(progress * Mathf.PI) * (maxScale - 1f) + 1f;
                target.localScale = originalScale * currentScale;
                yield return null;
            }
            target.localScale = originalScale;
        }
    }
}
