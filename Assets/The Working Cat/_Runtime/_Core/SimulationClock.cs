// Assets/TheWorkingCat/Runtime/Core/Time/SimulationClock.cs
using System;

namespace TheWorkingCat.Core.Time
{
	/// <summary>
	/// Pure simulation clock: accumulates real time and decides how many ticks to run.
	/// No Unity dependencies.
	/// </summary>
	public sealed class SimulationClock
	{
		public long TickCount { get; private set; }
		public bool IsPaused { get; private set; }

		public int TargetTicksPerSecond { get; private set; } = 60;
		public float SpeedMultiplier { get; private set; } = 1f;
		public int MaxTicksPerFrame { get; private set; } = 8;

		// For UI/metrics
		public float TickDeltaTime => 1f / Math.Max(1, TargetTicksPerSecond);
		public float AccumulatorSeconds => (float)_accumulatorSeconds;

		private double _accumulatorSeconds;

		// Smoothed UPS (ticks actually executed per real second)
		public float MeasuredUPS { get; private set; }

		private double _upsTimer;
		private int _upsTicks;

		public event Action<long>? OnTick; // invoked each tick

		public void Configure(int ticksPerSecond, float speedMultiplier, int maxTicksPerFrame)
		{
			TargetTicksPerSecond = Math.Max(1, ticksPerSecond);
			SpeedMultiplier = Math.Max(0f, speedMultiplier);
			MaxTicksPerFrame = Math.Max(1, maxTicksPerFrame);
		}

		public void SetPaused(bool paused) => IsPaused = paused;

		public void TogglePaused() => IsPaused = !IsPaused;

		/// <summary>Run exactly one tick (used for step while paused).</summary>
		public void StepOneTick()
		{
			// Stepping should work even when paused.
			DoOneTick();
		}

		/// <summary>
		/// Advance the clock by real delta time, and execute up to MaxTicksPerFrame ticks.
		/// Returns ticks executed this frame.
		/// </summary>
		public int Advance(double realDeltaSeconds)
		{
			if (realDeltaSeconds < 0) realDeltaSeconds = 0;

			// Always update UPS timer even if paused (so UI remains stable).
			_upsTimer += realDeltaSeconds;

			if (IsPaused || SpeedMultiplier <= 0f)
			{
				UpdateUPS(0);
				return 0;
			}

			double scaled = realDeltaSeconds * SpeedMultiplier;
			_accumulatorSeconds += scaled;

			double tickDt = 1.0 / Math.Max(1, TargetTicksPerSecond);
			int ticksThisFrame = 0;

			while (_accumulatorSeconds >= tickDt && ticksThisFrame < MaxTicksPerFrame)
			{
				_accumulatorSeconds -= tickDt;
				DoOneTick();
				ticksThisFrame++;
			}

			UpdateUPS(ticksThisFrame);
			return ticksThisFrame;
		}

		private void DoOneTick()
		{
			TickCount++;
			_upsTicks++;
			OnTick?.Invoke(TickCount);
		}

		private void UpdateUPS(int ticksThisFrame)
		{
			// Smooth once per second-ish
			if (_upsTimer >= 1.0)
			{
				MeasuredUPS = (float)(_upsTicks / _upsTimer);
				_upsTicks = 0;
				_upsTimer = 0;
			}
		}
	}
}
