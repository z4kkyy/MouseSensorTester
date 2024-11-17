using MouseSensorTest.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using static MouseSensorTest.Utilities.Win32.NativeMethods;
using static MouseSensorTest.Utilities.Win32.Win32Structures;

namespace MouseSensorTest.Services
{
	public class MouseHardwareService
	{
		public List<MouseDeviceInfo> GetConnectedMouseDevices()
		{
			var devices = new List<MouseDeviceInfo>();
			uint numDevices = 0;
			uint deviceStructSize = (uint)Marshal.SizeOf(typeof(RAWINPUTDEVICELIST));

			GetRawInputDeviceList(null, ref numDevices, deviceStructSize);
			if (numDevices == 0) return devices;

			RAWINPUTDEVICELIST[] deviceList = new RAWINPUTDEVICELIST[numDevices];
			GetRawInputDeviceList(deviceList, ref numDevices, deviceStructSize);

			foreach (var device in deviceList)
			{
				if (device.dwType == RIM_TYPEMOUSE)
				{
					var mouseInfo = GetMouseInfo(device.hDevice);
					if (mouseInfo != null)
					{
						devices.Add(mouseInfo);
					}
				}
			}

			return devices;
		}

		private MouseDeviceInfo GetMouseInfo(IntPtr hDevice)
		{
			uint nameSize = 0;
			GetRawInputDeviceInfo(hDevice, RIDI_DEVICENAME, IntPtr.Zero, ref nameSize);
			if (nameSize == 0) return null;

			IntPtr namePtr = Marshal.AllocHGlobal((int)nameSize);
			try
			{
				GetRawInputDeviceInfo(hDevice, RIDI_DEVICENAME, namePtr, ref nameSize);
				string deviceName = Marshal.PtrToStringAnsi(namePtr);

				var deviceInfo = new RID_DEVICE_INFO
				{
					cbSize = (uint)Marshal.SizeOf(typeof(RID_DEVICE_INFO))
				};
				uint deviceInfoSize = deviceInfo.cbSize;

				GetRawInputDeviceInfo(hDevice, RIDI_DEVICEINFO, ref deviceInfo, ref deviceInfoSize);

				return new MouseDeviceInfo
				{
					DeviceName = deviceName,
					Id = deviceInfo.u.mouse.dwId,
					ButtonCount = deviceInfo.u.mouse.dwNumberOfButtons,
					SampleRate = deviceInfo.u.mouse.dwSampleRate,
					HasHorizontalWheel = deviceInfo.u.mouse.fHasHorizontalWheel
				};
			}
			finally
			{
				Marshal.FreeHGlobal(namePtr);
			}
		}
	}
}
