// Assets/TheWorkingCat/Runtime/Simulation/Agents/PlayerManualBehavior.cs
using UnityEngine;

namespace TheWorkingCat.Simulation.Agents
{
	public sealed class PlayerManualBehavior : CharacterBehavior
	{
		// Repeat tuning (seconds, not ticks)
		private readonly float repeatDelay = 0.15f;

		private Vector3Int _heldDir = Vector3Int.zero;
		private float _nextRepeatTime = 0f;

		public PlayerManualBehavior(GridCharacterController controller) : base(controller) { }

		public override void TickBehavior(float dt)
		{
			Vector3Int dir = ReadHeldDir();

			// Released
			if (dir == Vector3Int.zero)
			{
				_heldDir = Vector3Int.zero;
				return;
			}

			// New direction pressed
			if (dir != _heldDir)
			{
				_heldDir = dir;
				_nextRepeatTime = Time.time; // immediate
			}

			// Gate by time + queue
			if (Time.time < _nextRepeatTime)
				return;

			if (controller.HasQueuedMove)
				return;

			controller.QueueMove(dir);
			_nextRepeatTime = Time.time + repeatDelay;
		}

		private static Vector3Int ReadHeldDir()
		{
			if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow)) return new Vector3Int(0, 0, 1);
			if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow)) return new Vector3Int(0, 0, -1);
			if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) return new Vector3Int(1, 0, 0);
			if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)) return new Vector3Int(-1, 0, 0);
			return Vector3Int.zero;
		}
	}
}
