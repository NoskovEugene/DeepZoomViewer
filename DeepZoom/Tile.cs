﻿namespace DeepZoom
{
    /// <summary>
    /// Represents a Tile in a Deep Zoom image.
    /// </summary>
    public struct Tile
    {
        public Tile(int level, int column, int row): this()
        {
            Level = level;
            Row = row;
            Column = column;
        }
        public int Level { get; set; }
        public int Row { get; set; }
        public int Column { get; set; }

        public override string ToString()
        {
            return Level.ToString() + "_" + Row.ToString() + "_" + Column.ToString();
        }
    }
}
