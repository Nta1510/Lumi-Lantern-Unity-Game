using System;
using System.Collections.Generic;

namespace LumiLantern
{
    [Serializable]
    public class MirrorPlacement
    {
        public GridCoord coord;
        public MirrorType type;

        public MirrorPlacement(int r, int c, MirrorType type)
        {
            this.coord = new GridCoord(r, c);
            this.type = type;
        }
    }

    [Serializable]
    public class LevelData
    {
        public int levelNumber;
        public string levelName;
        public int chapterNumber;
        public string chapterName;
        public int rows;
        public int cols;
        public int maxMirrors;
        public int minMirrors;
        public string gridAscii;
        public string tutorialText;
        public List<MirrorPlacement> solution = new List<MirrorPlacement>();

        // Parsed grid: 1-based indexing [row, col]
        private CellType[,] parsedGrid;

        public CellType GetCell(int r, int c)
        {
            if (parsedGrid == null) ParseGrid();
            if (r < 1 || r > rows || c < 1 || c > cols) return CellType.Wall;
            return parsedGrid[r - 1, c - 1];
        }

        public void ParseGrid()
        {
            parsedGrid = new CellType[rows, cols];
            string clean = gridAscii.Replace("\r", "").Trim();
            string[] lines = clean.Split('\n');
            for (int r = 0; r < rows; r++)
            {
                string line = r < lines.Length ? lines[r].Replace(" ", "") : "";
                for (int c = 0; c < cols; c++)
                {
                    char ch = c < line.Length ? line[c] : '.';
                    parsedGrid[r, c] = CharToCellType(ch);
                }
            }
        }

        public static CellType CharToCellType(char ch)
        {
            switch (ch)
            {
                case '#': return CellType.Wall;
                case '>': return CellType.SourceRight;
                case '^': return CellType.SourceUp;
                case 'v': return CellType.SourceDown;
                case '<': return CellType.SourceLeft;
                case 'L': return CellType.LanternNormal;
                case 'R': return CellType.LanternRed;
                case 'B': return CellType.LanternBlue;
                case 'r': return CellType.FilterRed;
                case 'b': return CellType.FilterBlue;
                case 'P': return CellType.SplitterSlash;
                case 'Q': return CellType.SplitterBackslash;
                case 'm': return CellType.FixedMirrorSlash;
                case 'n': return CellType.FixedMirrorBackslash;
                case 'G': return CellType.RhythmGate;
                default:  return CellType.Empty;
            }
        }
    }
}
