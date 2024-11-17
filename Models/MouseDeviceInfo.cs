using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MouseSensorTest.Models
{
	public class MouseDeviceInfo
	{
		public string DeviceName { get; set; }
		public uint Id { get; set; }
		public uint ButtonCount { get; set; }
		public uint SampleRate { get; set; }
		public bool HasHorizontalWheel { get; set; }
		public string ManufacturerName { get; set; }
		public string ProductName { get; set; }
	}
}
