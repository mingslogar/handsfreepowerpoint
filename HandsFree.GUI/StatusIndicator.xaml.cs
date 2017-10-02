using HandsFree.Audio.PowerPoint;
using HandsFree.GUI.Properties;
using HandsFree.GUI.Win32;
using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace HandsFree.GUI
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class StatusIndicator : Window
	{
		#region Constructor

		public StatusIndicator()
		{
			InitializeComponent();

			if (Settings.Default.UpgradeRequired)
			{
				Settings.Default.Upgrade();
				Settings.Default.UpgradeRequired = false;
				Settings.Default.Save();
			}

			Left = Settings.Default.WindowPosition.X;
			Top = Settings.Default.WindowPosition.Y;
		}

		#endregion

		#region Initialization

		private bool _isMonitoring = false;
		private AudioEngine _audioEngine;

		protected override void OnContentRendered(EventArgs e)
		{
			base.OnContentRendered(e);

			if (!Settings.Default.IsMuted)
				StartMonitoring();
		}

		#endregion

		#region Protected Methods

		protected override void OnClosing(CancelEventArgs e)
		{
			base.OnClosing(e);

			Settings.Default.IsMuted = !_isMonitoring;
			Settings.Default.WindowPosition = new Point(Left, Top);
			Settings.Default.Save();
		}

		#endregion

		#region Private Methods

		private void StartMonitoring()
		{
			if (_audioEngine == null)
			{
				_audioEngine = new AudioEngine();
				_audioEngine.CommandExecuted += _audioEngine_CommandExecuted;
				_audioEngine.SpeechDetected += _audioEngine_SpeechDetected;
				_audioEngine.CommandRejected += _audioEngine_CommandRejected;
			}

			_audioEngine.StartListening();

			ShowIsMonitoring(true);
		}

		private void StopMonitoring()
		{
			if (_audioEngine != null)
				_audioEngine.StopListening();

			ShowIsMonitoring(false);
		}

		private void ShowIsMonitoring(bool value)
		{
			_isMonitoring = value;
			status.Text = value ? "\xf130" : "\xf131";
		}

		private void _audioEngine_CommandExecuted(object sender, EventArgs e)
		{
			ResetIndicatorBackground();

			ColorAnimation backgroundAnim = new ColorAnimation(Color.FromRgb(0, 255, 0), new Duration(TimeSpan.FromMilliseconds(400)));
			backgroundAnim.AutoReverse = true;
			indicator.Background.BeginAnimation(SolidColorBrush.ColorProperty, backgroundAnim);
		}

		private void _audioEngine_SpeechDetected(object sender, EventArgs e)
		{
			ColorAnimation backgroundAnim = new ColorAnimation(Color.FromRgb(64, 64, 64), new Duration(TimeSpan.FromMilliseconds(100)));
			indicator.Background.BeginAnimation(SolidColorBrush.ColorProperty, backgroundAnim);
		}

		private void _audioEngine_CommandRejected(object sender, EventArgs e)
		{
			ResetIndicatorBackground();

			ColorAnimation backgroundAnim = new ColorAnimation(Color.FromRgb(255, 0, 0), new Duration(TimeSpan.FromMilliseconds(400)));
			backgroundAnim.AutoReverse = true;
			indicator.Background.BeginAnimation(SolidColorBrush.ColorProperty, backgroundAnim);
		}

		private void ResetIndicatorBackground()
		{
			indicator.Background.BeginAnimation(SolidColorBrush.ColorProperty, null);
		}

		private double Round(double value, int increment)
		{
			double mod = value % increment;

			value -= mod;

			if (mod > increment / 2)
				value += increment;

			return Math.Round(value);
		}

		#endregion

		#region UI

		#region Drag-and-drop

		private Point _dragOffset;
		private bool _isDragging = false;
		private bool _isDown = false;

		private void indicator_PreviewMouseDown(object sender, MouseButtonEventArgs e)
		{
			_isDown = true;
			_dragOffset = Mouse.GetPosition(indicator);
			indicator.CaptureMouse();
		}

		private void indicator_PreviewMouseMove(object sender, MouseEventArgs e)
		{
			if (_isDown)
			{
				if (!_isDragging)
				{
					Vector offset = Mouse.GetPosition(indicator) - _dragOffset;

					if (Math.Abs(offset.X) > SystemParameters.MinimumHorizontalDragDistance
						&& Math.Abs(offset.Y) > SystemParameters.MinimumVerticalDragDistance)
					{
						_isDragging = true;
						indicator.CaptureMouse();
					}
				}

				if (_isDragging && indicator.IsMouseCaptured)
				{
					Point mouse = Mouse.GetPosition(indicator);
					Left += mouse.X - _dragOffset.X;
					Top += mouse.Y - _dragOffset.Y;
				}
			}
		}

		private void indicator_PreviewMouseUp(object sender, MouseButtonEventArgs e)
		{
			_isDown = false;

			if (_isDragging)
			{
				_isDragging = false;
				indicator.ReleaseMouseCapture();

				//
				// Snap to an invisible grid.
				//
				double left = Round(Left, 50);
				double top = Round(Top, 50);

				//
				// Make sure we're still fully on the monitor.
 				//
				Rect monitor = MonitorHelper.MonitorWorkingAreaFromRect(new Rect(left, top, ActualWidth, ActualHeight));

				if (left < monitor.Left + 10)
					left = monitor.Left + 10;
				else if (left > monitor.Right - ActualWidth - 10)
					left = monitor.Right - ActualWidth - 10;

				if (top < monitor.Top + 10)
					top = monitor.Top + 10;
				else if (top > monitor.Bottom - ActualHeight - 10)
					top = monitor.Bottom - ActualHeight - 10;

				DoubleAnimation topAnim = new DoubleAnimation(top, new Duration(TimeSpan.FromMilliseconds(300)), FillBehavior.Stop);
				topAnim.EasingFunction = new QuarticEase();
				topAnim.Completed += (obj, args) => { Top = top; };

				DoubleAnimation leftAnim = new DoubleAnimation(left, new Duration(TimeSpan.FromMilliseconds(300)), FillBehavior.Stop);
				leftAnim.EasingFunction = new QuarticEase();
				leftAnim.Completed += (obj, args) => { Left = left; };

				BeginAnimation(TopProperty, topAnim);
				BeginAnimation(LeftProperty, leftAnim);
			}
		}

		#endregion

		private void closeButton_Click(object sender, RoutedEventArgs e)
		{
			Close();
		}

		private void indicator_Click(object sender, RoutedEventArgs e)
		{
			if (_isMonitoring)
				StopMonitoring();
			else
				StartMonitoring();
		}

		#endregion
	}
}
