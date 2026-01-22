// Assets/TheWorkingCat/Runtime/World/Chunk.cs
using System;
using UnityEngine;

namespace TheWorkingCat.World
{
	/// <summary>
	/// Backend chunk data. No Unity components. Rendering is handled by ChunkView.
	/// </summary>
	public sealed class Chunk
	{
		// Keep your original dimensions (X,Z,Y).
		public const int SizeX = 32;
		public const int SizeZ = 32;
		public const int SizeY = 16;

		// Chunk grid coord (cx, cz)
		public readonly Vector2Int ChunkCoord;

		// Flattened array for better cache locality than [,,]
		// Index = x + SizeX * (z + SizeZ * y)
		private readonly BlockId[] _blocks = new BlockId[SizeX * SizeZ * SizeY];

		public Chunk(Vector2Int chunkCoord)
		{
			ChunkCoord = chunkCoord;
		}

		public BlockId Get(int x, int y, int z)
		{
			if (!InBounds(x, y, z)) return BlockId.Air;
			return _blocks[ToIndex(x, y, z)];
		}

		public void Set(int x, int y, int z, BlockId id)
		{
			if (!InBounds(x, y, z)) return;
			_blocks[ToIndex(x, y, z)] = id;
		}

		public void Fill(BlockId id)
		{
			Array.Fill(_blocks, id);
		}

		public void FillBottom(int bottomYExclusive, BlockId fillBlock)
		{
			bottomYExclusive = Mathf.Clamp(bottomYExclusive, 0, SizeY);
			for (int y = 0; y < bottomYExclusive; y++)
				for (int z = 0; z < SizeZ; z++)
					for (int x = 0; x < SizeX; x++)
						Set(x, y, z, fillBlock);
		}


		public void ChopAboveY(int maxYInclusive)
		{
			maxYInclusive = Mathf.Clamp(maxYInclusive, -1, SizeY - 1);
			for (int y = maxYInclusive + 1; y < SizeY; y++)
				for (int z = 0; z < SizeZ; z++)
					for (int x = 0; x < SizeX; x++)
						Set(x, y, z, BlockId.Air);
		}

		/// <summary>
		/// Generate a coherent random terrain for this chunk. All chunks that receive the same seed
		/// will sample the same global heightfield because the seed controls the global noise offset.
		/// The chunk position is included in the sampling so adjacent chunks line up seamlessly.
		/// </summary>
		/// <param name="seed">Global terrain seed (same for all chunks to sample the same terrain).</param>
		/// <param name="noiseScale">Perlin noise scaling (smaller = larger features).</param>
		/// <param name="baseHeight">Base height added to noise-derived height.</param>
		/// <param name="heightVariation">Max additional height contributed by noise.</param>
		public void GenerateRandomTerrain(
			int seed,
			float noiseScale = 0.08f,
			int baseHeight = 4,
			int heightVariation = 6)
		{
			// Use the seed to produce a global offset. Using the same seed across chunks
			// yields the same offsetX/offsetZ and therefore a single continuous noise field.
			System.Random rng = new System.Random(seed);
			float offsetX = rng.Next(-100000, 100000);
			float offsetZ = rng.Next(-100000, 100000);

			// For each local cell in this chunk, compute its world X/Z and sample Perlin noise.
			for (int x = 0; x < SizeX; x++)
			{
				for (int z = 0; z < SizeZ; z++)
				{
					// World-space coordinates in block units
					int worldX = ChunkCoord.x * SizeX + x;
					int worldZ = ChunkCoord.y * SizeZ + z;

					float nx = (worldX + offsetX) * noiseScale;
					float nz = (worldZ + offsetZ) * noiseScale;

					float noise = Mathf.PerlinNoise(nx, nz); // 0..1

					int height = baseHeight + Mathf.RoundToInt(noise * heightVariation);
					height = Mathf.Clamp(height, 1, SizeY - 1);

					// Deterministic per-column variant selection so neighbouring chunks match.
					// Use a simple hash of seed + world coords to seed a per-column RNG.
					int colSeed = seed ^ (worldX * 73428767) ^ (worldZ * 91278341);
					var colRng = new System.Random(colSeed);

					// Keep dirt variants randomized per-column
					BlockId dirtVariant = (colRng.Next(0, 2) == 0) ? BlockId.Dirt1 : BlockId.Dirt2;

					// Define stone layering:
					// - y > height -> Air
					// - y == height -> Grass
					// - y >= height - 2 -> dirtVariant
					// - below those (stone layers): use Stone2 for the lower portion and Stone1 for the upper portion
					int stoneLayerMaxY = height - 3; // inclusive top Y index that belongs to stone layers
					int stoneSplit = (stoneLayerMaxY >= 0) ? (stoneLayerMaxY / 2) : -1; // lower half -> Stone2

					for (int y = 0; y < SizeY; y++)
					{
						if (y > height)
						{
							Set(x, y, z, BlockId.Air);
						}
						else if (y == height)
						{
							// Top-most block -> grass (single block id)
							Set(x, y, z, BlockId.Grass);
						}
						else if (y >= height - 2)
						{
							// Near-surface layers -> dirt variant
							Set(x, y, z, dirtVariant);
						}
						else
						{
							// Deep layers -> split into Stone2 (lower majority) and Stone1 (upper)
							if (stoneLayerMaxY < 0)
							{
								// No deep stone layers exist for very low heights; fall back to Stone1
								Set(x, y, z, BlockId.Stone1);
							}
							else if (y <= stoneSplit)
							{
								Set(x, y, z, BlockId.Stone2); // lower half of stone layers
							}
							else
							{
								Set(x, y, z, BlockId.Stone1); // upper half of stone layers
							}
						}
					}
				}
			}
		}

		public static bool InBounds(int x, int y, int z)
		{
			return (uint)x < SizeX && (uint)y < SizeY && (uint)z < SizeZ;
		}

		public static int ToIndex(int x, int y, int z)
		{
			return x + SizeX * (z + SizeZ * y);
		}
	}
}
