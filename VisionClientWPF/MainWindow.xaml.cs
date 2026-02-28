using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace VisionClientWPF
{
    public partial class MainWindow : Window
    {
        private DispatcherTimer _timer;
        private byte[]? _frameBuffer;
        private int _currentMode; // 0=Farbe, 1=Gesicht, 2=Kanten, 3=Kreise
        private int _frameCount;
        private readonly Stopwatch _fpsWatch = new();

        public MainWindow()
        {
            InitializeComponent();

            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromMilliseconds(33); // ~30 FPS
            _timer.Tick += Timer_Tick;
        }
                
        // ===== Steuerung =====

        private void Start_Click(object sender, RoutedEventArgs e)
        {
            if (!VisionInterop.StartCamera())
            {
                MessageBox.Show("Kamera konnte nicht gestartet werden.");
                return;
            }

            // Gesichts-Cascade laden (Pfad anpassen!)
            string cascadePath = System.IO.Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                "haarcascade_frontalface_default.xml");

            if (System.IO.File.Exists(cascadePath))
            {
                VisionInterop.LoadFaceCascade(cascadePath);
            }

            NoSignalText.Visibility = Visibility.Collapsed;
            StatusText.Text = "Kamera aktiv";
            _fpsWatch.Restart();
            _frameCount = 0;
            _timer.Start();
        }

        private void Stop_Click(object sender, RoutedEventArgs e)
        {
            _timer.Stop();
            VisionInterop.StopCamera();
            StatusText.Text = "Gestoppt";
            PositionText.Text = "";
            FpsText.Text = "";
            NoSignalText.Visibility = Visibility.Visible;
            OverlayCanvas.Children.Clear();
        }

        private void ModeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _currentMode = ModeComboBox.SelectedIndex;
            OverlayCanvas.Children.Clear();
        }

        // ===== Hauptschleife =====

        private void Timer_Tick(object? sender, EventArgs e)
        {
            RenderLiveFrame();

            switch (_currentMode)
            {
                case 0: RunColorDetection(); break;
                case 1: RunFaceDetection(); break;
                case 2: RunEdgeDetection(); break;
                case 3: RunCircleDetection(); break;
            }

            UpdateFps();
        }

        // ===== Live-Video rendern =====

        private void RenderLiveFrame()
        {
            if (!VisionInterop.GetFrameInfo(out var info))
                return;

            int rgbSize = info.width * info.height * 3;

            if (_frameBuffer == null || _frameBuffer.Length < rgbSize)
                _frameBuffer = new byte[rgbSize];

            if (!VisionInterop.GetFrameBytesRgb(_frameBuffer, rgbSize))
                return;

            int stride = info.width * 3;
            var bitmap = BitmapSource.Create(
                info.width, info.height,
                96, 96,
                PixelFormats.Rgb24,
                null,
                _frameBuffer,
                stride);

            bitmap.Freeze();
            VideoImage.Source = bitmap;
        }

        // ===== Farberkennung (bestehend) =====

        private void RunColorDetection()
        {
            OverlayCanvas.Children.Clear();

            if (!VisionInterop.GetFrame(out var result))
                return;

            if (result.detected)
            {
                PositionText.Text = $"Rot: X={result.x} Y={result.y} " +
                                    $"({result.width}×{result.height})";
                DrawOverlayRect(result, Brushes.Red, "Rot");
            }
            else
            {
                PositionText.Text = "Kein rotes Objekt";
            }
        }

        // ===== Gesichtserkennung =====

        private void RunFaceDetection()
        {
            OverlayCanvas.Children.Clear();

            if (!VisionInterop.DetectFaces(out var result))
            {
                PositionText.Text = "Cascade nicht geladen";
                return;
            }

            PositionText.Text = $"{result.count} Gesicht(er) erkannt";

            for (int i = 0; i < result.count; i++)
            {
                DrawOverlayRect(result.items[i], Brushes.LimeGreen, $"Gesicht {i + 1}");
            }
        }

        // ===== Kantenerkennung =====

        private void RunEdgeDetection()
        {
            OverlayCanvas.Children.Clear();

            if (!VisionInterop.GetFrameInfo(out var info))
                return;

            int bufferSize = info.width * info.height;
            byte[] edgeBuffer = new byte[bufferSize];

            if (!VisionInterop.DetectEdges(edgeBuffer, bufferSize,
                    out int w, out int h))
                return;

            // Kanten-Bild als Overlay statt normalem Frame anzeigen
            var edgeBitmap = BitmapSource.Create(
                w, h, 96, 96,
                PixelFormats.Gray8,
                null,
                edgeBuffer,
                w);

            edgeBitmap.Freeze();
            VideoImage.Source = edgeBitmap;
            PositionText.Text = $"Kanten: {w}×{h}";
        }

        // ===== Kreiserkennung =====

        private void RunCircleDetection()
        {
            OverlayCanvas.Children.Clear();

            if (!VisionInterop.DetectCircles(out var result))
                return;

            PositionText.Text = $"{result.count} Kreis(e) erkannt";

            for (int i = 0; i < result.count; i++)
            {
                var item = result.items[i];
                DrawOverlayEllipse(item, Brushes.Cyan, $"⌀{item.width}");
            }
        }

        // ===== Overlay-Zeichnung =====

        private void DrawOverlayRect(VisionInterop.DetectionResult det,
                                     Brush color, string label)
        {
            if (VideoImage.Source is not BitmapSource bmp)
                return;

            double scaleX = VideoImage.ActualWidth / bmp.PixelWidth;
            double scaleY = VideoImage.ActualHeight / bmp.PixelHeight;

            // Offset berechnen (Uniform Stretch)
            double offsetX = (VideoContainer.ActualWidth - 220 - VideoImage.ActualWidth) / 2;
            double offsetY = (VideoContainer.ActualHeight - VideoImage.ActualHeight) / 2;
            if (offsetX < 0) offsetX = 0;
            if (offsetY < 0) offsetY = 0;

            var rect = new Rectangle
            {
                Width = det.width * scaleX,
                Height = det.height * scaleY,
                Stroke = color,
                StrokeThickness = 2,
                Fill = Brushes.Transparent
            };

            Canvas.SetLeft(rect, det.x * scaleX + offsetX);
            Canvas.SetTop(rect, det.y * scaleY + offsetY);
            OverlayCanvas.Children.Add(rect);

            var text = new TextBlock
            {
                Text = label,
                Foreground = color,
                FontSize = 12,
                FontWeight = FontWeights.Bold
            };

            Canvas.SetLeft(text, det.x * scaleX + offsetX);
            Canvas.SetTop(text, det.y * scaleY + offsetY - 16);
            OverlayCanvas.Children.Add(text);
        }

        private void DrawOverlayEllipse(VisionInterop.DetectionResult det,
                                        Brush color, string label)
        {
            if (VideoImage.Source is not BitmapSource bmp)
                return;

            double scaleX = VideoImage.ActualWidth / bmp.PixelWidth;
            double scaleY = VideoImage.ActualHeight / bmp.PixelHeight;

            var ellipse = new Ellipse
            {
                Width = det.width * scaleX,
                Height = det.height * scaleY,
                Stroke = color,
                StrokeThickness = 2,
                Fill = Brushes.Transparent
            };

            Canvas.SetLeft(ellipse, det.x * scaleX);
            Canvas.SetTop(ellipse, det.y * scaleY);
            OverlayCanvas.Children.Add(ellipse);

            var text = new TextBlock
            {
                Text = label,
                Foreground = color,
                FontSize = 11
            };

            Canvas.SetLeft(text, det.x * scaleX);
            Canvas.SetTop(text, det.y * scaleY - 16);
            OverlayCanvas.Children.Add(text);
        }

        // ===== FPS-Anzeige =====

        private void UpdateFps()
        {
            _frameCount++;
            if (_fpsWatch.ElapsedMilliseconds > 1000)
            {
                double fps = _frameCount / (_fpsWatch.ElapsedMilliseconds / 1000.0);
                FpsText.Text = $"{fps:F1} FPS";
                _frameCount = 0;
                _fpsWatch.Restart();
            }
        }
    }
}