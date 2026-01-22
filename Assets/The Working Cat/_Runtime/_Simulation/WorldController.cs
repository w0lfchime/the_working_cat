// Assets/TheWorkingCat/Runtime/Simulation/WorldController.cs
using UnityEngine;
using TheWorkingCat.World;
using TheWorkingCat.World.Entities;
using TheWorkingCat.World.Placement;

namespace TheWorkingCat.Simulation
{
	/// <summary>
	/// Scene-level authoritative world bootstrap + simulation hook.
	/// Owns backend world data and advances it on ticks.
	/// </summary>
	public sealed class WorldController : MonoBehaviour
	{
		[Header("References")]
		[SerializeField] private SimulationRunner runner;

		[Header("World Gen")]
		[SerializeField] private int terrainSeed = 12221345;
		[SerializeField] private bool generateTerrain = true;
		[SerializeField] private int chopAboveY = 7;

		[Header("Chunk Grid")]
		[SerializeField, Min(0)] private int chunkRadius = 1; // radius around (0,0) to generate (0 => single chunk)

		public WorldState World { get; private set; }
		public EntityWorld Entities { get; private set; }
		public PlacementService Placement { get; private set; }

		public Chunk MainChunk { get; private set; }

		private bool _initialized;

		private void Awake()
		{
			InitializeIfNeeded();

			if (runner != null)
				runner.Clock.OnTick += OnTick;
		}

		private void OnDestroy()
		{
			if (runner != null)
				runner.Clock.OnTick -= OnTick;
		}

		public void InitializeIfNeeded()
		{
			if (_initialized) return;
			_initialized = true;

			BlockLibrary.EnsureInitialized();

			World = new WorldState();
			Entities = new EntityWorld();
			Placement = new PlacementService(World, Entities);

			// Create a grid of chunks centered at coord (0,0) using chunkRadius.
			// MainChunk remains the center chunk.
			for (int cx = -chunkRadius; cx <= chunkRadius; cx++)
			{
				for (int cz = -chunkRadius; cz <= chunkRadius; cz++)
				{
					var coord = new Vector2Int(cx, cz);
					var chunk = World.GetOrCreateChunk(coord);
					GenerateInitialChunk(chunk);
				}
			}

			MainChunk = World.GetOrCreateChunk(Vector2Int.zero);
		}

		public void GenerateInitialChunk(Chunk chunk)
		{
			chunk.Fill(BlockId.Air);

			//if (!generateTerrain)
			//{
			//	// Flat slab default
			//	chunk.FillBottom(4, BlockId.Dirt);
			//	return;
			//}

			// Use the same seed for every chunk so they sample from the same terrain function
			// (GenerateRandomTerrain should use chunk.ChunkCoord when sampling noise to be coherent across chunks).
			chunk.GenerateRandomTerrain(seed: terrainSeed);
			chunk.ChopAboveY(chopAboveY);
		}

		private void OnTick(long tick)
		{
			// This is where "simulation" goes later:
			// - tick machines
			// - tick agents
			// - process jobs
			//
			// For now: nothing. But the hook is correct.
		}
	}
}
