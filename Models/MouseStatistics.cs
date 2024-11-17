using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MouseSensorTest.Models
{
	public class MouseStatistics
	{
		// Basic statistics fields
		public Point CurrentPosition { get; set; }
		public double TotalDistance { get; set; }
		public double CurrentSpeed { get; set; }
		public double MaxSpeed { get; set; }
		public double Acceleration { get; set; }
		public double MaxAcceleration { get; set; }
		public int CurrentPollRate { get; set; }
		public int MaxPollRate { get; set; }
		public double AveragePollRate { get; set; }
		public int PollRateClass { get; set; }
		public int MaxJumpDistance { get; set; }
		public int MinJumpDistance { get; set; }
		public int DPI { get; set; }

		// Hardware statistics fields
		public string DeviceName { get; set; }
		public uint ButtonCount { get; set; }
		public uint DeviceSampleRate { get; set; }
		public bool HasHorizontalWheel { get; set; }
	}
}
