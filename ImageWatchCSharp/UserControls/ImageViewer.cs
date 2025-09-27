using ImageWatchCSharp.ViewModels;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Threading;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ImageWatchCSharp.UserControls
{
    public class ImageViewer : FrameworkElement
    {
        #region DependencyProperty

        public static readonly DependencyProperty WatchImageProperty =
            DependencyProperty.Register("WatchImage", typeof(WatchImage), typeof(ImageViewer),
                new PropertyMetadata(null, OnWatchImageChanged));

        public static readonly DependencyProperty ZoomFactorProperty =
            DependencyProperty.Register("ZoomFactor", typeof(double), typeof(ImageViewer),
                new PropertyMetadata(1.0, OnZoomFactorChanged));

        public static readonly DependencyProperty OffsetXProperty =
            DependencyProperty.Register("OffsetX", typeof(double), typeof(ImageViewer),
                new PropertyMetadata(0.0, OnOffsetChanged));

        public static readonly DependencyProperty OffsetYProperty =
            DependencyProperty.Register("OffsetY", typeof(double), typeof(ImageViewer),
                new PropertyMetadata(0.0, OnOffsetChanged));

        public static readonly DependencyProperty CurrentPixelInfoProperty =
            DependencyProperty.Register("CurrentPixelInfo", typeof(string), typeof(ImageViewer),
                new PropertyMetadata(string.Empty));

        #endregion

        #region Prop

        public WatchImage WatchImage
        {
            get 
            {
                return (WatchImage)GetValue(WatchImageProperty);
            }
            set 
            {
                bool isFirstLoad = GetValue(WatchImageProperty) == null && value != null;
                SetValue(WatchImageProperty, value);

                if (isFirstLoad)
                {
                   ThreadHelper.JoinableTaskFactory.Run(async delegate {
                        await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
                        FitToWindow();
                    });
                }
            }
        }

        public double ZoomFactor
        {
            get { return (double)GetValue(ZoomFactorProperty); }
            set { SetValue(ZoomFactorProperty, Math.Max(0.1, Math.Min(10.0, value))); }
        }

        public double OffsetX
        {
            get { return (double)GetValue(OffsetXProperty); }
            set { SetValue(OffsetXProperty, value); }
        }

        public double OffsetY
        {
            get { return (double)GetValue(OffsetYProperty); }
            set { SetValue(OffsetYProperty, value); }
        }

        public string CurrentPixelInfo
        {
            get { return (string)GetValue(CurrentPixelInfoProperty); }
            set { SetValue(CurrentPixelInfoProperty, value); }
        }

        #endregion

        #region private

        private bool _isDragging;
        private Point _lastMousePosition;
        private Point _dragStartPosition;
        private double _dragStartOffsetX;
        private double _dragStartOffsetY;

        #endregion

        #region ctor

        public ImageViewer()
        {
            this.Focusable = true;
            
            this.MouseMove += OnMouseMove;
            this.MouseDown += OnMouseDown;
            this.MouseUp += OnMouseUp;
            this.MouseWheel += OnMouseWheel;
            this.MouseLeave += OnMouseLeave;
            this.LostMouseCapture += OnLostMouseCapture;
            this.Loaded += OnLoaded;
            this.SizeChanged += OnSizeChanged;
        }

        #endregion

        #region event
        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            FitToWindow();
        }

        private void OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (e.PreviousSize.Width <= 0 || e.PreviousSize.Height <= 0)
            {
                FitToWindow();
            }
        }
        private static void OnWatchImageChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var viewer = (ImageViewer)d;
            if (e.OldValue is WatchImage oldImage)
            {
                oldImage.PropertyChanged -= viewer.OnWatchImagePropertyChanged;
            }
            if (e.NewValue is WatchImage newImage)
            {
                newImage.PropertyChanged += viewer.OnWatchImagePropertyChanged;

                if (newImage.Image != null && viewer.ActualWidth > 0 && viewer.ActualHeight > 0)
                {
                    ThreadHelper.JoinableTaskFactory.Run(async delegate {
                        await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
                        viewer.FitToWindow();
                    });
               
                }
            }
            viewer.InvalidateVisual();
        }

        private static void OnZoomFactorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var viewer = (ImageViewer)d;
            viewer.SetOffsetWithConstraints(viewer.OffsetX, viewer.OffsetY);
            viewer.InvalidateVisual();
        }

        private static void OnOffsetChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((ImageViewer)d).InvalidateVisual();
        }

        private void OnWatchImagePropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(WatchImage.Image))
            {

                ThreadHelper.JoinableTaskFactory.Run(async delegate {
                    await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
                    FitToWindow();
                });
               
            }
        }

        #endregion

        #region mouse event

        private void OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                _isDragging = true;
                _dragStartPosition = e.GetPosition(this);
                _dragStartOffsetX = OffsetX;
                _dragStartOffsetY = OffsetY;
                _lastMousePosition = _dragStartPosition;
                
                this.CaptureMouse();
                e.Handled = true;
            }
        }

        private void OnMouseMove(object sender, MouseEventArgs e)
        {
            var currentPosition = e.GetPosition(this);
            
            if (_isDragging && e.LeftButton == MouseButtonState.Pressed)
            {
                var deltaX = currentPosition.X - _dragStartPosition.X;
                var deltaY = currentPosition.Y - _dragStartPosition.Y;
                
                var newOffsetX = _dragStartOffsetX + deltaX;
                var newOffsetY = _dragStartOffsetY + deltaY;
                
                SetOffsetWithConstraints(newOffsetX, newOffsetY);
            }
            UpdatePixelInfo(currentPosition);
            _lastMousePosition = currentPosition;
        }

        private void OnMouseUp(object sender, MouseButtonEventArgs e)
        {
            if (_isDragging)
            {
                _isDragging = false;
                this.ReleaseMouseCapture();
                e.Handled = true;
            }
        }

        private void OnMouseWheel(object sender, MouseWheelEventArgs e)
        {
            var mousePosition = e.GetPosition(this);
            var zoomDelta = e.Delta > 0 ? 1.2 : 1.0 / 1.2;
            
            ZoomAt(mousePosition, zoomDelta);
            e.Handled = true;
        }

        private void OnMouseLeave(object sender, MouseEventArgs e)
        {
            CurrentPixelInfo = string.Empty;
        }

        private void OnLostMouseCapture(object sender, MouseEventArgs e)
        {
            _isDragging = false;
        }

        #endregion

        #region 缩放和平移


        private void SetOffsetWithConstraints(double newOffsetX, double newOffsetY)
        {
            if (WatchImage?.Image == null) return;
            
            OffsetX = newOffsetX;
            OffsetY = newOffsetY;
        }

        private void ZoomAt(Point center, double zoomDelta)
        {
            var oldZoom = ZoomFactor;
            var newZoom = Math.Max(0.1, Math.Min(10.0, oldZoom * zoomDelta));
            
            if (Math.Abs(newZoom - oldZoom) < 0.001) return;
            
            var mouseImagePosX = (center.X - OffsetX) / oldZoom;
            var mouseImagePosY = (center.Y - OffsetY) / oldZoom;
            
            ZoomFactor = newZoom;
            
            var newOffsetX = center.X - mouseImagePosX * newZoom;
            var newOffsetY = center.Y - mouseImagePosY * newZoom;
            
            SetOffsetWithConstraints(newOffsetX, newOffsetY);
        }

        #endregion

        #region 像素信息

        private void UpdatePixelInfo(Point mousePosition)
        {
            if (WatchImage?.Image == null)
            {
                CurrentPixelInfo = string.Empty;
                return;
            }

            var image = WatchImage.Image;
            
            var imageX = (int)((mousePosition.X - OffsetX) / ZoomFactor);
            var imageY = (int)((mousePosition.Y - OffsetY) / ZoomFactor);
            
            if (imageX < 0 || imageY < 0 || imageX >= image.PixelWidth || imageY >= image.PixelHeight)
            {
                CurrentPixelInfo = string.Empty;
                return;
            }

            try
            {
                var pixelInfo = GetPixelValue(image, imageX, imageY);
                CurrentPixelInfo = $"({imageX}, {imageY}) = {pixelInfo}";
            }
            catch (Exception ex)
            {
                CurrentPixelInfo = $"({imageX}, {imageY}) = Error: {ex.Message}";
            }
        }

        private string GetPixelValue(BitmapSource image, int x, int y)
        {
            if (image == null) return "N/A";
            try
            {

                return WatchImage.GetImagePixel(x, y);
            }
            catch
            {
                return "N/A";
            }
        }

        #endregion

        #region OnRender

        protected override void OnRender(DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);
            
            drawingContext.DrawRectangle(Brushes.Gray, null, new Rect(0, 0, ActualWidth, ActualHeight));
            
            if (WatchImage?.Image == null) return;
            
            var image = WatchImage.Image;
            
            var imageWidth = image.PixelWidth * ZoomFactor;
            var imageHeight = image.PixelHeight * ZoomFactor;
            
            var imageRect = new Rect(OffsetX, OffsetY, imageWidth, imageHeight);
            
            var clipGeometry = new RectangleGeometry(new Rect(0, 0, ActualWidth, ActualHeight));
            
            drawingContext.PushClip(clipGeometry);
            
            drawingContext.DrawImage(image, imageRect);
            
            var borderPen = new Pen(Brushes.Black, 1.0);
            drawingContext.DrawRectangle(null, borderPen, imageRect);
            
            if (ZoomFactor > 4.0)
            {
                DrawGrid(drawingContext, imageRect, image.PixelWidth, image.PixelHeight);
            }
            drawingContext.Pop();
        }

        private void DrawGrid(DrawingContext drawingContext, Rect imageRect, int pixelWidth, int pixelHeight)
        {
            var gridPen = new Pen(Brushes.LightGray, 0.5);
            
            for (int x = 0; x <= pixelWidth; x++)
            {
                var screenX = imageRect.X + x * ZoomFactor;
                if (screenX >= 0 && screenX <= ActualWidth)
                {
                    drawingContext.DrawLine(gridPen, 
                        new Point(screenX, Math.Max(0, imageRect.Y)), 
                        new Point(screenX, Math.Min(ActualHeight, imageRect.Bottom)));
                }
            }
            
            for (int y = 0; y <= pixelHeight; y++)
            {
                var screenY = imageRect.Y + y * ZoomFactor;
                if (screenY >= 0 && screenY <= ActualHeight)
                {
                    drawingContext.DrawLine(gridPen, 
                        new Point(Math.Max(0, imageRect.X), screenY), 
                        new Point(Math.Min(ActualWidth, imageRect.Right), screenY));
                }
            }
        }

        #endregion

        #region public

        public void FitToWindow()
        {
            if (WatchImage?.Image == null || ActualWidth <= 0 || ActualHeight <= 0) return;
            
            var image = WatchImage.Image;
            var scaleX = ActualWidth / image.PixelWidth;
            var scaleY = ActualHeight / image.PixelHeight;
            var scale = Math.Min(scaleX, scaleY);
            
            ZoomFactor = Math.Max(0.1, Math.Min(10.0, scale));
            
            double scaledWidth = image.PixelWidth * ZoomFactor;
            double scaledHeight = image.PixelHeight * ZoomFactor;
            
            double offsetX = (ActualWidth - scaledWidth) / 2;
            double offsetY = (ActualHeight - scaledHeight) / 2;
            
            SetOffsetWithConstraints(offsetX, offsetY);
        }

        #endregion
    } 
}
