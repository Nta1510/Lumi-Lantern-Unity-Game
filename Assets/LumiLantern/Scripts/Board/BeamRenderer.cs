using System.Collections.Generic;
using UnityEngine;

namespace LumiLantern
{
    public class BeamRenderer : MonoBehaviour
    {
        private List<LineRenderer> linePool = new List<LineRenderer>();
        private List<SpriteRenderer> dotPool = new List<SpriteRenderer>();
        private Sprite dotSprite;
        private Material lineMaterial;

        public void Initialize(System.Func<string, Sprite> spriteLoader)
        {
            dotSprite = spriteLoader("beam_dot");
            // Standard sprites default material handles vertex colors cleanly
            lineMaterial = new Material(Shader.Find("Sprites/Default"));
        }

        public void Clear()
        {
            foreach (var lr in linePool)
            {
                if (lr != null) lr.gameObject.SetActive(false);
            }
            foreach (var dot in dotPool)
            {
                if (dot != null) dot.gameObject.SetActive(false);
            }
        }

        public void RenderBeams(List<BeamSegment> segments, float boardCellSize = 1.0f)
        {
            Clear();
            if (segments == null || segments.Count == 0) return;

            int lineIndex = 0;
            int dotIndex = 0;

            for (int i = 0; i < segments.Count; i++)
            {
                var seg = segments[i];
                LineRenderer lr = GetLine(lineIndex++);
                lr.gameObject.SetActive(true);

                Color coreColor, glowColor;
                GetColors(seg.color, out coreColor, out glowColor);

                lr.startColor = glowColor;
                lr.endColor = glowColor;
                lr.startWidth = 0.11f * boardCellSize;
                lr.endWidth = 0.11f * boardCellSize;
                lr.positionCount = 2;
                lr.SetPosition(0, new Vector3(seg.startPoint.x * boardCellSize, seg.startPoint.y * boardCellSize, 0));
                lr.SetPosition(1, new Vector3(seg.endPoint.x * boardCellSize, seg.endPoint.y * boardCellSize, 0));

                // Spawn beam dots along the segment
                Vector2 start = seg.startPoint * boardCellSize;
                Vector2 end = seg.endPoint * boardCellSize;
                float dist = Vector2.Distance(start, end);
                int dotsCount = Mathf.Max(1, Mathf.RoundToInt(dist / (boardCellSize * 0.5f)));

                for (int d = 0; d <= dotsCount; d++)
                {
                    float t = (float)d / dotsCount;
                    Vector2 dotPos = Vector2.Lerp(start, end, t);

                    SpriteRenderer dot = GetDot(dotIndex++);
                    dot.gameObject.SetActive(true);
                    dot.transform.position = new Vector3(dotPos.x, dotPos.y, 0);
                    dot.transform.localScale = Vector3.one * (0.32f * boardCellSize);
                    dot.color = coreColor;
                }
            }
        }

        private LineRenderer GetLine(int index)
        {
            while (linePool.Count <= index)
            {
                GameObject obj = new GameObject($"BeamLine_{linePool.Count}");
                obj.transform.SetParent(transform, false);
                LineRenderer lr = obj.AddComponent<LineRenderer>();
                lr.material = lineMaterial;
                lr.useWorldSpace = true;
                lr.sortingOrder = 6;
                lr.numCapVertices = 4;
                lr.numCornerVertices = 4;
                linePool.Add(lr);
            }
            return linePool[index];
        }

        private SpriteRenderer GetDot(int index)
        {
            while (dotPool.Count <= index)
            {
                GameObject obj = new GameObject($"BeamDot_{dotPool.Count}");
                obj.transform.SetParent(transform, false);
                SpriteRenderer sr = obj.AddComponent<SpriteRenderer>();
                sr.sprite = dotSprite;
                sr.sortingOrder = 7;
                dotPool.Add(sr);
            }
            return dotPool[index];
        }

        private void GetColors(BeamColor color, out Color core, out Color glow)
        {
            switch (color)
            {
                case BeamColor.Red:
                    core = new Color(1f, 0.9f, 0.95f, 1f);
                    glow = new Color(1f, 0.3f, 0.42f, 0.95f);
                    break;
                case BeamColor.Blue:
                    core = new Color(0.9f, 0.98f, 1f, 1f);
                    glow = new Color(0f, 0.82f, 1f, 0.95f);
                    break;
                default: // White / Warm Gold
                    core = new Color(1f, 0.98f, 0.9f, 1f);
                    glow = new Color(1f, 0.92f, 0.55f, 0.95f);
                    break;
            }
        }
    }
}
