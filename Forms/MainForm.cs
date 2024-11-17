using MouseSensorTest.Models;
using MouseSensorTest.Services;
using System.Text;

namespace MouseSensorTest.Forms
{
	public partial class MainForm : Form
	{
		private readonly MouseTrackingService trackingService;
		private readonly Label statsLabel = new();
		private readonly System.Windows.Forms.Timer pollTimer = new();

		private bool enableTrail = false;
		private List<Point> cursorTrail = new();
		private const int MAX_TRAIL_LENGTH = 100;

		public MainForm()
		{
			InitializeComponent();
			trackingService = new MouseTrackingService();

			var hardwareService = new MouseHardwareService();
			var mouseDevices = hardwareService.GetConnectedMouseDevices();
			if (mouseDevices.Any())
			{
				trackingService.UpdateMouseDevice(mouseDevices[0]);
			}

			InitializeUI();
			SetupTimer();
		}

		private void InitializeUI()
		{
			this.KeyPreview = true;
			this.Size = new Size(800, 600);
			this.BackColor = Color.Black;

			// Stats panel setup
			statsLabel.AutoSize = true;
			statsLabel.ForeColor = Color.White;
			statsLabel.BackColor = Color.Black;
			statsLabel.Location = new Point(10, 10);
			statsLabel.Font = new Font("Consolas", 12);
			statsLabel.Padding = new Padding(10);
			this.Controls.Add(statsLabel);

			// DPI input setup
			var dpiLabel = new Label
			{
				Text = "Mouse DPI:",
				ForeColor = Color.White,
				BackColor = Color.Black,
				Location = new Point(10, 300),
				AutoSize = true
			};

			var dpiInput = new NumericUpDown
			{
				Location = new Point(100, 300),
				Minimum = 100,
				Maximum = 32000,
				Value = 800,
				BackColor = Color.Black,
				ForeColor = Color.White
			};
			dpiInput.ValueChanged += (s, e) => trackingService.UpdateMouseDPI((int)dpiInput.Value);

			this.Controls.Add(dpiLabel);
			this.Controls.Add(dpiInput);
		}

		private void SetupTimer()
		{
			pollTimer.Interval = 1;
			pollTimer.Tick += (sender, e) =>
			{
				var currentPosition = this.PointToClient(Control.MousePosition);
				var stats = trackingService.UpdateStatistics(currentPosition);
				UpdateStatisticsDisplay(stats);
				UpdateTrail(currentPosition);
				this.Invalidate();
			};
			pollTimer.Start();
		}

		private void UpdateStatisticsDisplay(MouseStatistics stats)
		{
			var display = new StringBuilder();
			display.AppendLine("MOUSE SENSOR TEST");
			display.AppendLine("----------------");
			display.AppendLine($"Position          {stats.CurrentPosition.X,4} px      {stats.CurrentPosition.Y,4} px");
			display.AppendLine();

			// Hardware info
			display.AppendLine("HARDWARE INFO");
			display.AppendLine($"Device Name:      {stats.DeviceName}");
			display.AppendLine($"Buttons:          {stats.ButtonCount}");
			display.AppendLine($"Sample Rate:      {stats.DeviceSampleRate} Hz");
			display.AppendLine($"Horizontal Wheel: {(stats.HasHorizontalWheel ? "Yes" : "No")}");
			display.AppendLine($"Current DPI:      {stats.DPI}");
			display.AppendLine();

			// Polling rate 
			display.AppendLine("POLLING RATE");
			display.AppendLine($"Current Rate:     {stats.CurrentPollRate,4} Hz");
			display.AppendLine($"Maximum Rate:     {stats.MaxPollRate,4} Hz");
			display.AppendLine($"Average Rate:     {stats.AveragePollRate,4:F0} Hz");
			display.AppendLine($"Rate Class:       {stats.PollRateClass,4} Hz");
			display.AppendLine();

			// Distance info
			display.AppendLine("MOVEMENT");
			display.AppendLine($"Longest Jump:     {stats.MaxJumpDistance,4} px      {(stats.MaxJumpDistance / (double)stats.DPI):F4} inch");
			display.AppendLine($"Shortest Jump:    {stats.MinJumpDistance,4} px      {(stats.MinJumpDistance / (double)stats.DPI):F4} inch");
			display.AppendLine($"Current Speed:    {stats.CurrentSpeed,4:F0} px/s    {(stats.CurrentSpeed / stats.DPI):F4} inch/s");
			display.AppendLine($"Max Speed:        {stats.MaxSpeed,4:F0} px/s    {(stats.MaxSpeed / stats.DPI):F4} inch/s");

			// Acceleration info
			display.AppendLine();
			display.AppendLine("ACCELERATION");
			display.AppendLine($"Current:          {stats.Acceleration,4:F0} px/s²   {(stats.Acceleration / (stats.DPI * 9.81)):F4} g");
			display.AppendLine($"Maximum:          {stats.MaxAcceleration,4:F0} px/s²   {(stats.MaxAcceleration / (stats.DPI * 9.81)):F4} g");

			// System info
			display.AppendLine();
			display.AppendLine("SYSTEM");
			display.AppendLine($"Graphics FPS:     {(1000.0 / pollTimer.Interval):F0} FPS");

			statsLabel.Text = display.ToString();
		}

		private void UpdateTrail(Point currentPosition)
		{
			if (enableTrail)
			{
				cursorTrail.Add(currentPosition);
				if (cursorTrail.Count > MAX_TRAIL_LENGTH)
					cursorTrail.RemoveAt(0);
			}
		}

		protected override void OnPaint(PaintEventArgs e)
		{
			base.OnPaint(e);
			if (enableTrail && cursorTrail.Count > 1)
			{
				using var pen = new Pen(Color.LimeGreen, 4);
				for (int i = 1; i < cursorTrail.Count; i++)
				{
					e.Graphics.DrawLine(pen, cursorTrail[i - 1], cursorTrail[i]);
				}
			}

			using var crosshairPen = new Pen(Color.Red, 1);
			const int size = 10;
			var currentPosition = cursorTrail.LastOrDefault();
			e.Graphics.DrawLine(crosshairPen,
				currentPosition.X - size, currentPosition.Y,
				currentPosition.X + size, currentPosition.Y);
			e.Graphics.DrawLine(crosshairPen,
				currentPosition.X, currentPosition.Y - size,
				currentPosition.X, currentPosition.Y + size);
		}

		protected override void OnKeyDown(KeyEventArgs e)
		{
			base.OnKeyDown(e);
			switch (e.KeyCode)
			{
				case Keys.Escape:
					Application.Exit();
					break;
				case Keys.T:
					enableTrail = !enableTrail;
					if (!enableTrail) cursorTrail.Clear();
					break;
				case Keys.R:
					trackingService.Reset();
					cursorTrail.Clear();
					break;
			}
		}
	}
}