// Assets/TheWorkingCat/Runtime/Core/Time/SimulationTimeConfig.cs
using UnityEngine;

namespace TheWorkingCat.Core.Time
{
	[CreateAssetMenu(menuName = "TheWorkingCat/Time/Simulation Time Config")]
	public sealed class SimulationTimeConfig : ScriptableObject
	{
		[Min(1)] public int targetTicksPerSecond = 60;

		[Tooltip("Multiplier on simulation speed. 1 = normal, 2 = double, 0.5 = half.")]
		[Min(0f)] public float speedMultiplier = 1f;

		[Tooltip("Hard cap to prevent spiral-of-death if a frame stalls.")]
		[Min(1)] public int maxTicksPerFrame = 8;

		public bool startPaused = false;
	}
}
