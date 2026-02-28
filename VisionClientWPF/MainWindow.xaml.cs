using System;
using System.Windows;
using System.Windows.Threading;

namespace VisionClientWPF
{
    public partial class MainWindow : Window
    {
        private DispatcherTimer _timer;

        public MainWindow()
        {
            InitializeComponent();

            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromMilliseconds(100);
            _timer.Tick += Timer_Tick;
        }

        private void Start_Click(object sender, RoutedEventArgs e)
        {
            if (VisionInterop.StartCamera())
            {
                _timer.Start();
            }
            else
            {
                MessageBox.Show("Kamera konnte nicht gestartet werden.");
            }
        }

        private void Stop_Click(object sender, RoutedEventArgs e)
        {
            _timer.Stop();
            VisionInterop.StopCamera();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            if (VisionInterop.GetFrame(out VisionInterop.DetectionResult result))
            {
                if (result.detected)
                {
                    PositionText.Text = $"X: {result.x}  Y: {result.y}";
                }
                else
                {
                    PositionText.Text = "Kein Objekt erkannt";
                }
            }
        }
    }
}