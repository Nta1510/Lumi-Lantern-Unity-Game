using System;

namespace LumiLantern
{
    [Serializable]
    public struct GridCoord : IEquatable<GridCoord>
    {
        public int row; // 1-based row (1 is top)
        public int col; // 1-based col (1 is left)

        public GridCoord(int row, int col)
        {
            this.row = row;
            this.col = col;
        }

        public bool Equals(GridCoord other)
        {
            return row == other.row && col == other.col;
        }

        public override bool Equals(object obj)
        {
            return obj is GridCoord other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (row * 397) ^ col;
            }
        }

        public override string ToString()
        {
            return $"({row}, {col})";
        }

        public static bool operator ==(GridCoord a, GridCoord b)
        {
            return a.row == b.row && a.col == b.col;
        }

        public static bool operator !=(GridCoord a, GridCoord b)
        {
            return !(a == b);
        }

        public GridCoord Move(Direction dir)
        {
            switch (dir)
            {
                case Direction.Up:    return new GridCoord(row - 1, col);
                case Direction.Right: return new GridCoord(row, col + 1);
                case Direction.Down:  return new GridCoord(row + 1, col);
                case Direction.Left:  return new GridCoord(row, col - 1);
                default: return this;
            }
        }
    }
}
