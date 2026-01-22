// Assets/TheWorkingCat/Runtime/Simulation/SimulationRunner.cs
using UnityEngine;
using TheWorkingCat.Core.Time;

namespace TheWorkingCat.Simulation
{
	public sealed class SimulationRunner : MonoBehaviour
	{
		[Header("Config")]
		[SerializeField] private SimulationTimeConfig config;

		[Header("Debug")]
		[SerializeField] private bool runInUpdate = true; // switch to FixedUpdate later if you want

		public SimulationClock Clock { get; private set; } = new SimulationClock();

		// Metrics
		public float MeasuredFPS { get; private set; }
		public float LastFrameDeltaTime { get; private set; }
		public int LastFrameTicksExecuted { get; private set; }

		private float _fpsTimer;
		private int _fpsFrames;

		private void Awake()
		{
			if (config != null)
			{
				Clock.Configure(config.targetTicksPerSecond, config.speedMultiplier, config.maxTicksPerFrame);
				Clock.SetPaused(config.startPaused);
			}

			// Hook your future Simulation/Systems here:
			// Clock.OnTick += tick => simulation.Tick();
		}

		private void Update()
		{
			if (!runInUpdate) return;
			Drive(Time.unscaledDeltaTime);
		}

		private void FixedUpdate()
		{
			if (runInUpdate) return;
			Drive(Time.fixedUnscaledDeltaTime);
		}

		private void Drive(float unscaledDt)
		{
			LastFrameDeltaTime = unscaledDt;
			LastFrameTicksExecuted = Clock.Advance(unscaledDt);

			// FPS measurement (unscaled, real frame rate)
			_fpsTimer += unscaledDt;
			_fpsFrames++;
			if (_fpsTimer >= 0.5f) // update twice per second for stability
			{
				MeasuredFPS = _fpsFrames / _fpsTimer;
				_fpsTimer = 0f;
				_fpsFrames = 0;
			}
		}

		// Convenience API for UI
		public void SetTicksPerSecond(float value)
		{
			Clock.Configure(Mathf.RoundToInt(value), Clock.SpeedMultiplier, Clock.MaxTicksPerFrame);
		}

		public void SetSpeedMultiplier(float value)
		{
			Clock.Configure(Clock.TargetTicksPerSecond, value, Clock.MaxTicksPerFrame);
		}

		public void SetMaxTicksPerFrame(float value)
		{
			Clock.Configure(Clock.TargetTicksPerSecond, Clock.SpeedMultiplier, Mathf.RoundToInt(value));
		}

		public void TogglePause() => Clock.TogglePaused();
		public void SetPaused(bool paused) => Clock.SetPaused(paused);
		public void Step() => Clock.StepOneTick();
	}
}
