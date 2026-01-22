// Assets/TheWorkingCat/Runtime/Simulation/Agents/GridCharacterController.cs
using System.Collections.Generic;
using TheWorkingCat.World;
using TheWorkingCat.World.Entities;
using UnityEngine;

namespace TheWorkingCat.Simulation.Agents
{
	public sealed class GridCharacterController : MonoBehaviour
	{
		[Header("References")]
		[SerializeField] private SimulationRunner runner;
		[SerializeField] private WorldController worldController;

		[Header("Rules")]
		[SerializeField, Min(1)] private int actorHeight = 2;
		[SerializeField, Min(0)] private int stepHeight = 1;

		[Header("Gravity")]
		[SerializeField] private bool enableGravity = true;
		[SerializeField, Min(1)] private int maxFallPerTick = 1;

		[Header("State")]
		[SerializeField] private Vector3Int feetCell;

		[Header("Visual Motion")]
		[SerializeField] private bool smoothMove = true;

		[Tooltip("Lerp duration for XZ movement (or full position if vertical lerp is disabled).")]
		[SerializeField, Min(0f)] private float moveLerpSeconds = 0.12f;

		[Tooltip("Lerp duration for vertical (Y) only. Kept separate from XZ.")]
		[SerializeField, Min(0f)] private float verticalLerpSeconds = 0.08f;

		[Tooltip("If true, lerp duration is derived from tick rate AND speedMultiplier.")]
		[SerializeField] private bool useTickDurationForLerp = true;

		[Tooltip("If true, when multiple sim ticks happen in one rendered frame, we snap to avoid laggy visuals.")]
		[SerializeField] private bool snapWhenMultipleTicksInFrame = true;

		// Intent queue (behavior writes, tick consumes)
		private readonly Queue<Vector3Int> _moveQueue = new();
		public bool HasQueuedMove => _moveQueue.Count > 0;

		// Behavior state
		public CharacterBehavior CurrentBehavior { get; private set; }

		// Render lerp (XZ)
		private Vector3 _renderFrom, _renderTo;
		private float _renderT, _renderDuration;
		private bool _isInterpolating;

		// Render lerp (Y only - lightweight)
		private float _yTarget;
		private float _yT, _yDuration;
		private bool _isYInterpolating;

		private void Awake()
		{
			if (runner == null) runner = FindFirstObjectByType<SimulationRunner>();
			if (worldController == null) worldController = FindFirstObjectByType<WorldController>();

			// init render position
			Vector3 p = CellToWorld(feetCell);
			transform.position = p;
			_renderFrom = _renderTo = p;

			// init Y lerp state
			_yTarget = p.y;
			_yT = 1f;
			_yDuration = 0f;
			_isYInterpolating = false;

			// default behavior (player input)
			SetBehavior(new PlayerManualBehavior(this));
		}

		private void OnEnable()
		{
			if (runner != null)
				runner.Clock.OnTick += OnTick;
		}

		private void OnDisable()
		{
			if (runner != null)
				runner.Clock.OnTick -= OnTick;
		}

		private void Update()
		{
			// Decision layer (frame-based)
			CurrentBehavior?.TickBehavior(Time.deltaTime);

			// Presentation interpolation MUST use unscaledDeltaTime because the sim is unscaled-driven.
			if (smoothMove)
			{
				float dt = Time.unscaledDeltaTime;

				// XZ lerp (uses your existing vector state)
				if (_isInterpolating)
				{
					if (_renderDuration <= 0f)
					{
						Vector3 p = transform.position;
						p.x = _renderTo.x;
						p.z = _renderTo.z;
						transform.position = p;
						_isInterpolating = false;
					}
					else
					{
						_renderT += dt / _renderDuration;
						float t01 = Mathf.Clamp01(_renderT);
						t01 = t01 * t01 * (3f - 2f * t01); // smoothstep

						Vector3 cur = transform.position;
						Vector3 xz = Vector3.Lerp(_renderFrom, _renderTo, t01);
						cur.x = xz.x;
						cur.z = xz.z;
						transform.position = cur;

						if (_renderT >= 1f)
							_isInterpolating = false;
					}
				}

				// Y lerp (lightweight: just target + duration)
				if (_isYInterpolating)
				{
					if (_yDuration <= 0f)
					{
						Vector3 p = transform.position;
						p.y = _yTarget;
						transform.position = p;
						_isYInterpolating = false;
					}
					else
					{
						_yT += dt / _yDuration;
						float t01 = Mathf.Clamp01(_yT);
						t01 = t01 * t01 * (3f - 2f * t01); // smoothstep

						Vector3 p = transform.position;
						p.y = Mathf.Lerp(p.y, _yTarget, t01); // from "current y" toward target
						transform.position = p;

						if (_yT >= 1f)
							_isYInterpolating = false;
					}
				}
			}
		}

		// ---------- Tick (authoritative movement resolution) ----------

		private void OnTick(long tick)
		{
			if (!HasWorld()) return;

			// Gravity first
			if (enableGravity && TryFall(maxFallPerTick))
				return;

			// Consume one move intent per tick
			if (_moveQueue.Count == 0) return;

			Vector3Int dir = _moveQueue.Dequeue();
			TryMove(dir);
		}

		private bool HasWorld()
		{
			return worldController != null && worldController.MainChunk != null;
		}

		// ---------- Public API for behaviors ----------

		public void SetBehavior(CharacterBehavior next)
		{
			if (next == null) return;

			CurrentBehavior?.OnExit();
			CurrentBehavior = next;
			CurrentBehavior.OnEnter();
		}

		public void ClearIntents() => _moveQueue.Clear();

		public void QueueMove(Vector3Int dir)
		{
			if (dir == Vector3Int.zero) return;
			_moveQueue.Enqueue(dir);
		}

		public bool InBounds(Vector3Int cell)
		{
			// One chunk world assumption for now
			return (uint)cell.x < Chunk.SizeX && (uint)cell.y < Chunk.SizeY && (uint)cell.z < Chunk.SizeZ;
		}

		public bool IsBlocked(Vector3Int cell)
		{
			if (!InBounds(cell)) return true;

			// Terrain blocks
			var chunk = worldController.MainChunk;
			if (BlockLibrary.IsSolid(chunk.Get(cell.x, cell.y, cell.z)))
				return true;

			// Entity occupancy (treat anything occupying a cell as blocking for now)
			if (worldController.Entities != null && worldController.Entities.Occupancy.IsOccupied(cell))
				return true;

			return false;
		}

		// ---------- Movement resolution ----------

		private bool CanOccupy(Vector3Int newFeet)
		{
			if (!InBounds(newFeet)) return false;
			return HasBodyClearance(newFeet);
		}

		private bool TryFall(int maxSteps)
		{
			bool fell = false;

			for (int i = 0; i < maxSteps; i++)
			{
				var belowFeet = feetCell + Vector3Int.down;
				if (!InBounds(belowFeet)) return fell;

				// If there's ground (blocked) we stop falling.
				if (IsBlocked(belowFeet)) return fell;

				if (!HasBodyClearance(belowFeet)) return fell;

				CommitMove(belowFeet);
				fell = true;
			}

			return fell;
		}

		private bool TryMove(Vector3Int dir)
		{
			if (dir == Vector3Int.zero) return false;

			var target = feetCell + dir;

			// grounded move
			if (CanStandAt(target))
			{
				CommitMove(target);
				return true;
			}

			// ledge move into air (fall next tick)
			if (CanOccupy(target))
			{
				CommitMove(target);
				return true;
			}

			// step up
			for (int h = 1; h <= stepHeight; h++)
			{
				var stepped = feetCell + dir + new Vector3Int(0, h, 0);
				if (CanStandAt(stepped))
				{
					CommitMove(stepped);
					return true;
				}
			}

			return false;
		}

		private bool CanStandAt(Vector3Int newFeet)
		{
			if (!InBounds(newFeet)) return false;

			var ground = newFeet + Vector3Int.down;
			if (!InBounds(ground)) return false;

			// Need solid ground (blocked) to stand
			if (!IsBlocked(ground)) return false;

			return HasBodyClearance(newFeet);
		}

		private bool HasBodyClearance(Vector3Int newFeet)
		{
			for (int i = 0; i < actorHeight; i++)
			{
				var c = newFeet + new Vector3Int(0, i, 0);
				if (!InBounds(c)) return false;
				if (IsBlocked(c)) return false;
			}
			return true;
		}

		// ---------- Visual timing helpers ----------

		private float GetVisualMoveDurationSeconds()
		{
			if (!useTickDurationForLerp || runner == null)
				return moveLerpSeconds;

			if (runner.Clock.IsPaused)
				return 0f;

			float baseSecPerTick = runner.Clock.TickDeltaTime;
			float speed = Mathf.Max(0.0001f, runner.Clock.SpeedMultiplier);
			float realSecPerTick = baseSecPerTick / speed;

			if (snapWhenMultipleTicksInFrame && runner.LastFrameTicksExecuted > 1)
				return 0f;

			return Mathf.Max(0f, realSecPerTick);
		}

		private float GetVisualVerticalDurationSeconds()
		{
			// Vertical uses ONLY its own inspector speed unless you explicitly want it tied to ticks too.
			// If you *do* want it tied, replace this with GetVisualMoveDurationSeconds() * scale.
			if (runner != null && runner.Clock.IsPaused)
				return 0f;

			if (snapWhenMultipleTicksInFrame && runner != null && runner.LastFrameTicksExecuted > 1)
				return 0f;

			return Mathf.Max(0f, verticalLerpSeconds);
		}

		private Vector3 SampleCurrentRenderPositionXZ()
		{
			if (!_isInterpolating || _renderDuration <= 0f)
				return transform.position;

			float t = Mathf.Clamp01(_renderT);
			t = t * t * (3f - 2f * t);
			return Vector3.Lerp(_renderFrom, _renderTo, t);
		}

		private void CommitMove(Vector3Int newFeet)
		{
			feetCell = newFeet;

			Vector3 newPos = CellToWorld(feetCell);

			if (!smoothMove)
			{
				transform.position = newPos;
				_isInterpolating = false;
				_isYInterpolating = false;
				_yTarget = newPos.y;
				return;
			}

			// If multiple ticks happen before Update runs,
			// transform.position is stale. Use our XZ lerp state to get current visual pos for XZ.
			Vector3 currentVisual = SampleCurrentRenderPositionXZ();

			// Snap transform to the latest known visual before starting new lerps.
			// Preserve current Y as-is here; Y is handled by its own interpolator.
			Vector3 tpos = transform.position;
			tpos.x = currentVisual.x;
			tpos.z = currentVisual.z;
			transform.position = tpos;

			// --- XZ lerp ---
			_renderFrom = new Vector3(transform.position.x, 0f, transform.position.z);
			_renderTo = new Vector3(newPos.x, 0f, newPos.z);
			_renderT = 0f;
			_renderDuration = GetVisualMoveDurationSeconds();
			_isInterpolating = _renderDuration > 0f;

			if (!_isInterpolating)
			{
				Vector3 p = transform.position;
				p.x = newPos.x;
				p.z = newPos.z;
				transform.position = p;
			}

			// --- Y lerp (simple) ---
			_yTarget = newPos.y;
			_yT = 0f;
			_yDuration = GetVisualVerticalDurationSeconds();
			_isYInterpolating = _yDuration > 0f;

			if (!_isYInterpolating)
			{
				Vector3 p = transform.position;
				p.y = _yTarget;
				transform.position = p;
			}
		}

		private static Vector3 CellToWorld(Vector3Int cell)
		{
			// Feet cell sits on the ground; keep Y at cell.y
			return new Vector3(cell.x + 0.5f, cell.y, cell.z + 0.5f);
		}
	}
}
