using MouseSensorTest.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MouseSensorTest.Services
{
	public class MouseTrackingService
	{
		private readonly Stopwatch moveStopwatch = new();
		private readonly Stopwatch pollRateStopwatch = new();
		private readonly Queue<int> pollRateHistory = new();
		private const int POLL_HISTORY_SIZE = 100;

		private Point previousMousePosition;
		private DateTime lastUpdateTime = DateTime.Now;
		private double lastSpeed = 0;
		private int pollCount = 0;
		private int mouseDPI;
		private MouseDeviceInfo deviceInfo;

		// Statistics fields
		private double totalDistance = 0;
		private double currentSpeed = 0;
		private double maxSpeed = 0;
		private double acceleration = 0;
		private double maxAcceleration = 0;
		private int currentPollRate = 0;
		private int maxPollRate = 0;
		private double averagePollRate = 0;
		private int pollRateClass = 0;
		private int maxJumpDistance = 0;
		private int minJumpDistance = int.MaxValue;

		public MouseTrackingService(int initialDPI = 800)
		{
			this.mouseDPI = initialDPI;
			moveStopwatch.Start();
			pollRateStopwatch.Start();
		}

		public void UpdateMouseDevice(MouseDeviceInfo info)
		{
			deviceInfo = info;
		}

		public void UpdateMouseDPI(int newDPI)
		{
			this.mouseDPI = newDPI;
		}

		public MouseStatistics UpdateStatistics(Point currentPosition)
		{
			pollCount++;

			if (previousMousePosition != Point.Empty)
			{
				CalculateMovement(currentPosition);
			}

			UpdatePollRate();

			var stats = new MouseStatistics
			{
				// 既存の統計情報
				CurrentPosition = currentPosition,
				TotalDistance = totalDistance,
				CurrentSpeed = currentSpeed,
				MaxSpeed = maxSpeed,
				Acceleration = acceleration,
				MaxAcceleration = maxAcceleration,
				CurrentPollRate = currentPollRate,
				MaxPollRate = maxPollRate,
				AveragePollRate = averagePollRate,
				PollRateClass = pollRateClass,
				MaxJumpDistance = maxJumpDistance,
				MinJumpDistance = minJumpDistance,
				DPI = mouseDPI,

				// ハードウェア情報を追加
				DeviceName = deviceInfo?.DeviceName ?? "Unknown Device",
				ButtonCount = deviceInfo?.ButtonCount ?? 0,
				DeviceSampleRate = deviceInfo?.SampleRate ?? 0,
				HasHorizontalWheel = deviceInfo?.HasHorizontalWheel ?? false
			};

			previousMousePosition = currentPosition;
			return stats;
		}

		private void CalculateMovement(Point currentPosition)
		{
			double dx = currentPosition.X - previousMousePosition.X;
			double dy = currentPosition.Y - previousMousePosition.Y;
			double distance = Math.Sqrt(dx * dx + dy * dy);

			totalDistance += distance;

			var now = DateTime.Now;
			double deltaTime = (now - lastUpdateTime).TotalSeconds;
			lastUpdateTime = now;

			if (deltaTime > 0)
			{
				currentSpeed = distance / deltaTime;
				acceleration = (currentSpeed - lastSpeed) / deltaTime;
				lastSpeed = currentSpeed;

				maxSpeed = Math.Max(maxSpeed, currentSpeed);
				maxAcceleration = Math.Max(maxAcceleration, Math.Abs(acceleration));
			}

			int jumpDistance = (int)distance;
			maxJumpDistance = Math.Max(maxJumpDistance, jumpDistance);
			if (jumpDistance > 0)
			{
				minJumpDistance = Math.Min(minJumpDistance, jumpDistance);
			}
		}

		private void UpdatePollRate()
		{
			double elapsedSeconds = pollRateStopwatch.ElapsedMilliseconds / 1000.0;
			if (elapsedSeconds >= 1.0)
			{
				currentPollRate = pollCount;
				maxPollRate = Math.Max(maxPollRate, currentPollRate);

				pollRateHistory.Enqueue(currentPollRate);
				if (pollRateHistory.Count > POLL_HISTORY_SIZE)
					pollRateHistory.Dequeue();

				averagePollRate = pollRateHistory.Average();
				pollRateClass = GetPollRateClass(currentPollRate);

				pollCount = 0;
				pollRateStopwatch.Restart();
			}
		}

		private int GetPollRateClass(int pollRate)
		{
			if (pollRate >= 975) return 1000;
			if (pollRate >= 475) return 500;
			if (pollRate >= 225) return 250;
			if (pollRate >= 175) return 200;
			if (pollRate >= 125) return 125;
			return 100;
		}

		public void Reset()
		{
			totalDistance = 0;
			currentSpeed = 0;
			maxSpeed = 0;
			acceleration = 0;
			maxAcceleration = 0;
			maxJumpDistance = 0;
			minJumpDistance = int.MaxValue;
			pollCount = 0;
			currentPollRate = 0;
			maxPollRate = 0;
			pollRateHistory.Clear();
			previousMousePosition = Point.Empty;
		}
	}
}
