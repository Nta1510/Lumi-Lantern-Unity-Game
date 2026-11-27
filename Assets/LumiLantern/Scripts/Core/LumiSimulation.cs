using System.Collections.Generic;
using UnityEngine;

namespace LumiLantern
{
    public struct BeamSegment
    {
        public Vector2 startPoint;
        public Vector2 endPoint;
        public BeamColor color;

        public BeamSegment(Vector2 start, Vector2 end, BeamColor color)
        {
            this.startPoint = start;
            this.endPoint = end;
            this.color = color;
        }
    }

    public class SimulationResult
    {
        public bool isWin;
        public int stars;
        public int placedMirrorsCount;
        public bool isGateOpen;
        public HashSet<GridCoord> litLanterns = new HashSet<GridCoord>();
        public HashSet<GridCoord> totalLanterns = new HashSet<GridCoord>();
        public HashSet<GridCoord> touchedMirrors = new HashSet<GridCoord>();
        public List<BeamSegment> segments = new List<BeamSegment>();
        public List<Vector2> beamPathPoints = new List<Vector2>(); // all discrete nodes
    }

    public static class LumiSimulation
    {
        private struct RayState
        {
            public GridCoord coord;
            public Direction dir;
            public BeamColor color;

            public RayState(GridCoord c, Direction d, BeamColor col)
            {
                this.coord = c;
                this.dir = d;
                this.color = col;
            }
        }

        public static SimulationResult Simulate(LevelData level, Dictionary<GridCoord, MirrorType> placedMirrors)
        {
            var result = new SimulationResult();

            // Count placed mirrors (excluding None)
            int placedCount = 0;
            if (placedMirrors != null)
            {
                foreach (var kvp in placedMirrors)
                {
                    if (kvp.Value != MirrorType.None) placedCount++;
                }
            }
            result.placedMirrorsCount = placedCount;

            // Rhythm gate state: Odd = Open, Even = Closed
            result.isGateOpen = (placedCount % 2 != 0);

            // Collect all lanterns
            for (int r = 1; r <= level.rows; r++)
            {
                for (int c = 1; c <= level.cols; c++)
                {
                    var cell = level.GetCell(r, c);
                    if (cell == CellType.LanternNormal || cell == CellType.LanternRed || cell == CellType.LanternBlue)
                    {
                        result.totalLanterns.Add(new GridCoord(r, c));
                    }
                }
            }

            // Find all sources
            var rayQueue = new Queue<RayState>();
            for (int r = 1; r <= level.rows; r++)
            {
                for (int c = 1; c <= level.cols; c++)
                {
                    var cell = level.GetCell(r, c);
                    Direction? srcDir = null;
                    if (cell == CellType.SourceRight) srcDir = Direction.Right;
                    else if (cell == CellType.SourceUp) srcDir = Direction.Up;
                    else if (cell == CellType.SourceDown) srcDir = Direction.Down;
                    else if (cell == CellType.SourceLeft) srcDir = Direction.Left;

                    if (srcDir.HasValue)
                    {
                        rayQueue.Enqueue(new RayState(new GridCoord(r, c), srcDir.Value, BeamColor.White));
                    }
                }
            }

            // Visited set for cycle detection: (coord, dir, color)
            var visited = new HashSet<(int r, int c, int d, int col)>();

            int maxIterations = 200;
            int iteration = 0;

            while (rayQueue.Count > 0 && iteration++ < maxIterations)
            {
                var ray = rayQueue.Dequeue();
                GridCoord curr = ray.coord;
                Direction dir = ray.dir;
                BeamColor col = ray.color;

                Vector2 segStart = CoordToWorld(curr, level);
                result.beamPathPoints.Add(segStart);

                bool continueRay = true;
                while (continueRay && iteration++ < maxIterations)
                {
                    GridCoord next = curr.Move(dir);

                    // Check bounds: ray leaving the board
                    if (next.row < 1 || next.row > level.rows || next.col < 1 || next.col > level.cols)
                    {
                        Vector2 edgePoint = CoordToWorld(curr, level) + DirToVec(dir) * 0.5f;
                        result.segments.Add(new BeamSegment(segStart, edgePoint, col));
                        result.beamPathPoints.Add(edgePoint);
                        break;
                    }

                    // Cycle detection
                    if (!visited.Add((next.row, next.col, (int)dir, (int)col)))
                    {
                        Vector2 end = CoordToWorld(next, level);
                        result.segments.Add(new BeamSegment(segStart, end, col));
                        result.beamPathPoints.Add(end);
                        break;
                    }

                    Vector2 nextCenter = CoordToWorld(next, level);

                    // Check player placed mirror at next cell first
                    MirrorType playerMirror = MirrorType.None;
                    if (placedMirrors != null && placedMirrors.TryGetValue(next, out var pm))
                    {
                        playerMirror = pm;
                    }

                    if (playerMirror == MirrorType.Slash)
                    {
                        result.touchedMirrors.Add(next);
                        result.segments.Add(new BeamSegment(segStart, nextCenter, col));
                        result.beamPathPoints.Add(nextCenter);
                        dir = ReflectSlash(dir);
                        curr = next;
                        segStart = nextCenter;
                        continue;
                    }
                    else if (playerMirror == MirrorType.Backslash)
                    {
                        result.touchedMirrors.Add(next);
                        result.segments.Add(new BeamSegment(segStart, nextCenter, col));
                        result.beamPathPoints.Add(nextCenter);
                        dir = ReflectBackslash(dir);
                        curr = next;
                        segStart = nextCenter;
                        continue;
                    }

                    // Check level cell
                    CellType cell = level.GetCell(next.row, next.col);
                    switch (cell)
                    {
                        case CellType.Wall:
                            result.segments.Add(new BeamSegment(segStart, nextCenter, col));
                            result.beamPathPoints.Add(nextCenter);
                            continueRay = false;
                            break;

                        case CellType.RhythmGate:
                            if (!result.isGateOpen)
                            {
                                // Closed gate blocks beam
                                result.segments.Add(new BeamSegment(segStart, nextCenter, col));
                                result.beamPathPoints.Add(nextCenter);
                                continueRay = false;
                            }
                            else
                            {
                                // Open gate: beam passes through
                                curr = next;
                            }
                            break;

                        case CellType.FilterRed:
                            if (col == BeamColor.White || col == BeamColor.Red)
                            {
                                result.segments.Add(new BeamSegment(segStart, nextCenter, col));
                                result.beamPathPoints.Add(nextCenter);
                                col = BeamColor.Red;
                                curr = next;
                                segStart = nextCenter;
                            }
                            else
                            {
                                // Blue beam blocked by red filter
                                result.segments.Add(new BeamSegment(segStart, nextCenter, col));
                                result.beamPathPoints.Add(nextCenter);
                                continueRay = false;
                            }
                            break;

                        case CellType.FilterBlue:
                            if (col == BeamColor.White || col == BeamColor.Blue)
                            {
                                result.segments.Add(new BeamSegment(segStart, nextCenter, col));
                                result.beamPathPoints.Add(nextCenter);
                                col = BeamColor.Blue;
                                curr = next;
                                segStart = nextCenter;
                            }
                            else
                            {
                                // Red beam blocked by blue filter
                                result.segments.Add(new BeamSegment(segStart, nextCenter, col));
                                result.beamPathPoints.Add(nextCenter);
                                continueRay = false;
                            }
                            break;

                        case CellType.FixedMirrorSlash: // m
                            result.segments.Add(new BeamSegment(segStart, nextCenter, col));
                            result.beamPathPoints.Add(nextCenter);
                            dir = ReflectSlash(dir);
                            curr = next;
                            segStart = nextCenter;
                            break;

                        case CellType.FixedMirrorBackslash: // n
                            result.segments.Add(new BeamSegment(segStart, nextCenter, col));
                            result.beamPathPoints.Add(nextCenter);
                            dir = ReflectBackslash(dir);
                            curr = next;
                            segStart = nextCenter;
                            break;

                        case CellType.SplitterSlash: // P
                            result.segments.Add(new BeamSegment(segStart, nextCenter, col));
                            result.beamPathPoints.Add(nextCenter);
                            // Branch 1: straight
                            rayQueue.Enqueue(new RayState(next, dir, col));
                            // Branch 2: reflected
                            Direction splitDirSlash = ReflectSlash(dir);
                            rayQueue.Enqueue(new RayState(next, splitDirSlash, col));
                            continueRay = false;
                            break;

                        case CellType.SplitterBackslash: // Q
                            result.segments.Add(new BeamSegment(segStart, nextCenter, col));
                            result.beamPathPoints.Add(nextCenter);
                            // Branch 1: straight
                            rayQueue.Enqueue(new RayState(next, dir, col));
                            // Branch 2: reflected
                            Direction splitDirBack = ReflectBackslash(dir);
                            rayQueue.Enqueue(new RayState(next, splitDirBack, col));
                            continueRay = false;
                            break;

                        case CellType.LanternNormal:
                            result.litLanterns.Add(next);
                            curr = next;
                            break;

                        case CellType.LanternRed:
                            if (col == BeamColor.Red) result.litLanterns.Add(next);
                            curr = next;
                            break;

                        case CellType.LanternBlue:
                            if (col == BeamColor.Blue) result.litLanterns.Add(next);
                            curr = next;
                            break;

                        case CellType.SourceRight:
                        case CellType.SourceUp:
                        case CellType.SourceDown:
                        case CellType.SourceLeft:
                            // Hits a light source body
                            result.segments.Add(new BeamSegment(segStart, nextCenter, col));
                            result.beamPathPoints.Add(nextCenter);
                            continueRay = false;
                            break;

                        default: // Empty
                            curr = next;
                            break;
                    }
                }
            }

            // Check Win condition: all lanterns lit
            bool allLit = result.totalLanterns.Count > 0 && result.litLanterns.Count == result.totalLanterns.Count;
            result.isWin = allLit;

            if (allLit)
            {
                if (placedCount <= level.minMirrors)
                    result.stars = 3;
                else if (placedCount <= level.minMirrors + 1)
                    result.stars = 2;
                else
                    result.stars = 1;
            }
            else
            {
                result.stars = 0;
            }

            return result;
        }

        public static Direction ReflectSlash(Direction d)
        {
            switch (d)
            {
                case Direction.Right: return Direction.Up;
                case Direction.Up:    return Direction.Right;
                case Direction.Left:  return Direction.Down;
                case Direction.Down:  return Direction.Left;
                default: return d;
            }
        }

        public static Direction ReflectBackslash(Direction d)
        {
            switch (d)
            {
                case Direction.Right: return Direction.Down;
                case Direction.Down:  return Direction.Right;
                case Direction.Left:  return Direction.Up;
                case Direction.Up:    return Direction.Left;
                default: return d;
            }
        }

        public static Vector2 CoordToWorld(GridCoord coord, LevelData level)
        {
            // Center the grid: row 1 at top, col 1 at left
            float x = (coord.col - 1) - (level.cols - 1) * 0.5f;
            float y = ((level.rows - 1) * 0.5f) - (coord.row - 1);
            return new Vector2(x, y);
        }

        public static Vector2 DirToVec(Direction dir)
        {
            switch (dir)
            {
                case Direction.Up:    return new Vector2(0, 1);
                case Direction.Right: return new Vector2(1, 0);
                case Direction.Down:  return new Vector2(0, -1);
                case Direction.Left:  return new Vector2(-1, 0);
                default: return Vector2.zero;
            }
        }
    }
}
