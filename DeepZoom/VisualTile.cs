﻿using System.ComponentModel;
using System.Windows.Media;

namespace DeepZoom
{
    /// <summary>
    /// Represents a tile that displays an image in the screen.
    /// </summary>
    public class VisualTile : INotifyPropertyChanged
    {
        private ImageSource _source;

        public VisualTile(Tile tile, MultiScaleTileSource tileSource)
        {
            ZIndex = tile.Level;
            Scale = 1 / tileSource.ScaleAtLevel(tile.Level);
            var position = tileSource.GetTilePosition(tile.Column, tile.Row);
            Left = position.X * Scale;
            Top = position.Y * Scale;
        }

        public VisualTile(Tile tile, MultiScaleTileSource tileSource, ImageSource source)
            : this(tile, tileSource)
        {
            Source = source;
        }

        public int ZIndex { get; private set; }

        public double Left { get; private set; }

        public double Top { get; private set; }

        public double Scale { get; private set; }

        public ImageSource Source
        {
            get { return _source; }
            internal set
            {
                _source = value;
                RaisePropertyChanged("Source");
            }
        }

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;
        protected void RaisePropertyChanged(string name)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(name));
        }

        #endregion
    }
}
