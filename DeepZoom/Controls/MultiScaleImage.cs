﻿using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using Blake.NUI.WPF.Touch;

namespace DeepZoom.Controls
{
    /// <summary>
    /// Enables users to open a multi-resolution image, which can be zoomed in on and panned across. 
    /// </summary>
    [TemplatePart(Name = "PART_ItemsControl", Type = typeof(ItemsControl))]
    public class MultiScaleImage : Control
    {
        private const int ScaleAnimationRelativeDuration = 400;
        private const double MinScaleRelativeToMinSize = 0.8;
        private const int ThrottleIntervalMilliseconds = 200;

        private ItemsControl _itemsControl;
        private ZoomableCanvas _zoomableCanvas;
        private MultiScaleImageSpatialItemsSource _spatialSource;
        private double _originalScale;
        private int _desiredLevel;
        private readonly DispatcherTimer _levelChangeThrottle;


        public ZoomableCanvas GetZoomable()
        {
            return _zoomableCanvas;
        }

        public MultiScaleImageSpatialItemsSource GetSource()
        {
            return _spatialSource;
        }

        public ItemsControl GetItems()
        {
            return _itemsControl;
        }

        static MultiScaleImage()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(MultiScaleImage), new FrameworkPropertyMetadata(typeof(MultiScaleImage)));
        }

        public MultiScaleImage()
        {
            MouseTouchDevice.RegisterEvents(this);
            _levelChangeThrottle = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(ThrottleIntervalMilliseconds), IsEnabled = false };
            _levelChangeThrottle.Tick += (s, e) =>
            {
                _spatialSource.CurrentLevel = _desiredLevel;
                _levelChangeThrottle.IsEnabled = false;
            };
            //AbsoluteHeigh = Source.abso

        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            IsManipulationEnabled = true;
            _itemsControl = GetTemplateChild("PART_ItemsControl") as ItemsControl;
            if (_itemsControl == null) return;

            _itemsControl.ApplyTemplate();

            var factoryPanel = new FrameworkElementFactory(typeof(ZoomableCanvas));
            factoryPanel.AddHandler(LoadedEvent, new RoutedEventHandler(ZoomableCanvasLoaded));
            _itemsControl.ItemsPanel = new ItemsPanelTemplate(factoryPanel);

            if (_spatialSource != null)
                _itemsControl.ItemsSource = _spatialSource;
        }

        private void ZoomableCanvasLoaded(object sender, RoutedEventArgs e)
        {
            _zoomableCanvas = sender as ZoomableCanvas;
            if (_zoomableCanvas != null)
            {
                _zoomableCanvas.RealizationPriority = DispatcherPriority.Send;
                _zoomableCanvas.RealizationRate = 10;
                InitializeCanvas();
            }
        }

        #region Public methods

        /// <summary>
        /// Enables a user to zoom in on a point of the MultiScaleImage.
        /// </summary>
        /// <param name="zoomIncrementFactor">Specifies the zoom. This number is greater than 0. A value of 1 specifies that the image fit the allotted page size exactly. A number greater than 1 specifies to zoom in. If a value of 0 or less is used, failure is returned and no zoom changes are applied. </param>
        /// <param name="zoomCenterLogicalX">X coordinate for the point on the MultiScaleImage that is zoomed in on. This is a logical point (between 0 and 1). </param>
        /// <param name="zoomCenterLogicalY">Y coordinate for the point on the MultiScaleImage that is zoomed in on. This is a logical point (between 0 and 1).</param>
        public void ZoomAboutLogicalPoint(double zoomIncrementFactor, double zoomCenterLogicalX, double zoomCenterLogicalY)
        {
            var logicalPoint = new Point(zoomCenterLogicalX, zoomCenterLogicalY);
            ScaleCanvas(zoomIncrementFactor, LogicalToElementPoint(logicalPoint), true);
        }

        /// <summary>
        /// Gets a point with logical coordinates (values between 0 and 1) from a point of the MultiScaleImage. 
        /// </summary>
        /// <param name="elementPoint">The point on the MultiScaleImage to translate into a point with logical coordinates (values between 0 and 1).</param>
        /// <returns>The logical point translated from the elementPoint.</returns>
        public Point ElementToLogicalPoint(Point elementPoint)
        {
            var absoluteCanvasPoint = _zoomableCanvas.GetCanvasPoint(elementPoint);
            return new Point(absoluteCanvasPoint.X / _zoomableCanvas.Extent.Width,
                             absoluteCanvasPoint.Y / _zoomableCanvas.Extent.Height);
        }

        /// <summary>
        /// Gets a point with pixel coordinates relative to the MultiScaleImage from a logical point (values between 0 and 1).
        /// </summary>
        /// <param name="logicalPoint">The logical point to translate into pixel coordinates relative to the MultiScaleImage.</param>
        /// <returns>A point with pixel coordinates relative to the MultiScaleImage translated from logicalPoint.</returns>
        public Point LogicalToElementPoint(Point logicalPoint)
        {
            var absoluteCanvasPoint = new Point(
                logicalPoint.X * _zoomableCanvas.Extent.Width,
                logicalPoint.Y * _zoomableCanvas.Extent.Height
            );
            return _zoomableCanvas.GetVisualPoint(absoluteCanvasPoint);
        }

        #endregion

        #region Dependency Properties

        #region Source

        /// <summary>
        /// Source Dependency Property
        /// </summary>
        public static readonly DependencyProperty SourceProperty =
            DependencyProperty.Register("Source", typeof(MultiScaleTileSource), typeof(MultiScaleImage),
                new FrameworkPropertyMetadata(null,
                    new PropertyChangedCallback(OnSourceChanged)));

        /// <summary>
        /// Gets or sets the Source property. This dependency property 
        /// indicates the tile source for this MultiScaleImage.
        /// </summary>
        public MultiScaleTileSource Source
        {
            get { return (MultiScaleTileSource)GetValue(SourceProperty); }
            set { SetValue(SourceProperty, value); }
        }

        /// <summary>
        /// Handles changes to the Source property.
        /// </summary>
        private static void OnSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            MultiScaleImage target = (MultiScaleImage)d;
            MultiScaleTileSource oldSource = (MultiScaleTileSource)e.OldValue;
            MultiScaleTileSource newSource = target.Source;
            target.OnSourceChanged(oldSource, newSource);
        }

        /// <summary>
        /// Provides derived classes an opportunity to handle changes to the Source property.
        /// </summary>
        protected virtual void OnSourceChanged(MultiScaleTileSource oldSource, MultiScaleTileSource newSource)
        {
            if (newSource == null)
            {
                _spatialSource = null;
                return;
            }

            _spatialSource = new MultiScaleImageSpatialItemsSource(newSource);

            

            if (_itemsControl != null)
                _itemsControl.ItemsSource = _spatialSource;

            if (_zoomableCanvas != null)
                InitializeCanvas();
        }

        #endregion

        #region AspectRatio

        /// <summary>
        /// AspectRatio Read-Only Dependency Property
        /// </summary>
        private static readonly DependencyPropertyKey AspectRatioPropertyKey
            = DependencyProperty.RegisterReadOnly("AspectRatio", typeof(double), typeof(MultiScaleImage),
                new FrameworkPropertyMetadata(1.0));

        public static readonly DependencyProperty AspectRatioProperty
            = AspectRatioPropertyKey.DependencyProperty;

        private double MaxScaleRelativeToMaxSize;

        /// <summary>
        /// Gets the aspect ratio of the image used as the source of the MultiScaleImage. 
        /// The aspect ratio is the width of the image divided by its height.
        /// </summary>
        public double AspectRatio
        {
            get { return (double)GetValue(AspectRatioProperty); }
        }

        /// <summary>
        /// Provides a secure method for setting the AspectRatio property.  
        /// The aspect ratio is the width of the image divided by its height.
        /// </summary>
        /// <param name="value">The new value for the property.</param>
        protected void SetAspectRatio(double value)
        {
            SetValue(AspectRatioPropertyKey, value);
        }

        #endregion

        #endregion

        #region Overriden Input Event Handlers

        protected override void OnManipulationDelta(ManipulationDeltaEventArgs e)
        {
            base.OnManipulationDelta(e);

            var oldScale = _zoomableCanvas.Scale;
            _zoomableCanvas.ApplyAnimationClock(ZoomableCanvas.ScaleProperty, null);
            _zoomableCanvas.Scale = oldScale;

            var oldOffset = _zoomableCanvas.Offset;
            _zoomableCanvas.ApplyAnimationClock(ZoomableCanvas.OffsetProperty, null);
            _zoomableCanvas.Offset = oldOffset;

            var scale = e.DeltaManipulation.Scale.X;
            ScaleCanvas(scale, e.ManipulationOrigin);

            _zoomableCanvas.Offset -= e.DeltaManipulation.Translation;
            e.Handled = true;
        }

        protected override void OnManipulationInertiaStarting(ManipulationInertiaStartingEventArgs e)
        {
            base.OnManipulationInertiaStarting(e);
            e.TranslationBehavior = new InertiaTranslationBehavior { DesiredDeceleration = 0.0096 };
            e.ExpansionBehavior = new InertiaExpansionBehavior { DesiredDeceleration = 0.000096 };
            e.Handled = true;
        }

        protected override void OnPreviewMouseWheel(MouseWheelEventArgs e)
        {
            var relativeScale = Math.Pow(2, (double)e.Delta / Mouse.MouseWheelDeltaForOneLine);
            var position = e.GetPosition(_itemsControl);

            ScaleCanvas(relativeScale, position, true);

            e.Handled = true;
        }

        #endregion

        #region Private helpers

        private void InitializeCanvas()
        {
            var level = Source.GetLevel(_zoomableCanvas.ActualWidth, _zoomableCanvas.ActualHeight);
            _spatialSource.CurrentLevel = level;

            var imageSize = Source.ImageSize;
            var relativeScale = Math.Min(_itemsControl.ActualWidth / imageSize.Width,
                                         _itemsControl.ActualHeight / imageSize.Height);

            _originalScale = relativeScale;

            _zoomableCanvas.Scale = _originalScale;
            _zoomableCanvas.Offset =
                new Point(imageSize.Width * 0.5 * relativeScale - _zoomableCanvas.ActualWidth * 0.5,
                          imageSize.Height * 0.5 * relativeScale - _zoomableCanvas.ActualHeight * 0.5);
            _zoomableCanvas.Clip = new RectangleGeometry(
                new Rect(0, 0,
                    imageSize.Width,
                    imageSize.Height));

            SetAspectRatio(_spatialSource.Extent.Width / _spatialSource.Extent.Height);

            _spatialSource.InvalidateSource();
        }

        public void ScaleCanvas(double relativeScale, Point center, bool animate = false)
        {
            var scale = _zoomableCanvas.Scale;

            if (scale <= 0) return;

            // minimum size = 80% of size where the whole image is visible
            // maximum size = Max(120% of full resolution of the image, 120% of original scale)

            MaxScaleRelativeToMaxSize = 1.2;
            relativeScale = relativeScale.Clamp(
                MinScaleRelativeToMinSize * _originalScale / scale,
                Math.Max(MaxScaleRelativeToMaxSize, MaxScaleRelativeToMaxSize * _originalScale) / scale);

            var targetScale = scale * relativeScale;

            var newLevel = Source.GetLevel(targetScale);
            var level = _spatialSource.CurrentLevel;
            if (newLevel != level)
            {
                // If it's zooming in, throttle
                if (newLevel > level)
                    ThrottleChangeLevel(newLevel);
                else
                    _spatialSource.CurrentLevel = newLevel;
            }

            if (targetScale != scale)
            {
                var position = (Vector)center;
                var targetOffset = (Point)((Vector)(_zoomableCanvas.Offset + position) * relativeScale - position);

                if (animate)
                {
                    if (relativeScale < 1)
                        relativeScale = 1 / relativeScale;
                    var duration = TimeSpan.FromMilliseconds(relativeScale * ScaleAnimationRelativeDuration);
                    var easing = new CubicEase();
                    _zoomableCanvas.BeginAnimation(ZoomableCanvas.ScaleProperty, new DoubleAnimation(targetScale, duration) { EasingFunction = easing }, HandoffBehavior.Compose);
                    _zoomableCanvas.BeginAnimation(ZoomableCanvas.OffsetProperty, new PointAnimation(targetOffset, duration) { EasingFunction = easing }, HandoffBehavior.Compose);
                }
                else
                {
                    _zoomableCanvas.Scale = targetScale;
                    _zoomableCanvas.Offset = targetOffset;
                }
            }
        }

        /// <summary>
        /// Returns actual panorama size
        /// </summary>
        /// <returns></returns>
        public Size GetActualSize()
        {
            double scale = _zoomableCanvas.Scale;
            Size ret = new Size(Source.AbsoluteWidth * scale, Source.AbsoluteHeight * scale);
            return ret;
        }

        /// <summary>
        /// Translate the point from image to view box
        /// </summary>
        /// <param name="Position">Value should be between 0 and 1</param>
        public void SetPosition(Point Position)
        {
            Size ActualSize = GetActualSize();
            Point PosOnView = new Point(Position.X * ActualSize.Width, Position.Y * ActualSize.Height);
            _zoomableCanvas.Offset = PosOnView;
        }

        public void SetPositionToCenter(Point Position,Size ElementSize)
        {
            Size Actualsize = GetActualSize();
            Point PosOnVew = new Point(Position.X * Actualsize.Width - ElementSize.Width / 2, Position.Y * Actualsize.Height - ElementSize.Height / 2);
            _zoomableCanvas.Offset = PosOnVew;
        }

        public Point GetPositionOnProcent(Size ElementSize)
        {
            Size ActPos = GetActualSize();
            Point NowPos = _zoomableCanvas.Offset;
            return new Point(NowPos.X / ActPos.Width, NowPos.Y / ActPos.Height);
        }

        public Point GetCenterPositionOnProcent(Size ElementSize)
        {
            Size actpos = GetActualSize();
            Point NowPos = new Point(_zoomableCanvas.Offset.X + ElementSize.Width / 2,_zoomableCanvas.Offset.Y + ElementSize.Height / 2);
            return new Point(NowPos.X / actpos.Width, NowPos.Y / actpos.Height);
        }

        private void ThrottleChangeLevel(int newLevel)
        {
            _desiredLevel = newLevel;

            if (_levelChangeThrottle.IsEnabled)
                _levelChangeThrottle.Stop();

            _levelChangeThrottle.Start();
        }

        #endregion

    }
}
