// Runtime/World/Placement/PlacementService.cs
using UnityEngine;
using TheWorkingCat.World.Entities;

namespace TheWorkingCat.World.Placement
{
	public sealed class PlacementService
	{
		private readonly WorldState _world;
		private readonly EntityWorld _entities;

		public PlacementService(WorldState world, EntityWorld entities)
		{
			_world = world;
			_entities = entities;
		}

		public bool CanPlace(EntityDef def, Vector3Int originCell)
		{
			// 1) Entity collision check
			foreach (var cell in def.Footprint.EnumerateWorldCells(originCell))
				if (_entities.Occupancy.IsOccupied(cell))
					return false;

			// 2) Terrain rule check (example: must not be Air at base)
			// For now, require each occupied cell to be inside some chunk and have support below (optional)
			foreach (var cell in def.Footprint.EnumerateWorldCells(originCell))
			{
				// Must be inside a chunk we have
				var block = GetBlockSafe(cell);
				// Example rule: cannot place inside solid terrain (later you’ll have "carve" rules)
				if (BlockLibrary.IsSolid(block))
					return false;
			}

			return true;
		}

		public bool TryPlace(EntityDef def, Vector3Int originCell, out Entity placed)
		{
			placed = null;
			if (!CanPlace(def, originCell)) return false;
			return _entities.TrySpawn(def, originCell, out placed);
		}

		private BlockId GetBlockSafe(Vector3Int cell)
		{
			// Map world cell -> chunk coord + local cell inside that chunk.
			int cx = Mathf.FloorToInt((float)cell.x / Chunk.SizeX);
			int cz = Mathf.FloorToInt((float)cell.z / Chunk.SizeZ);

			int localX = cell.x - cx * Chunk.SizeX;
			int localZ = cell.z - cz * Chunk.SizeZ;
			int localY = cell.y;

			var chunkCoord = new Vector2Int(cx, cz);

			if (!_world.TryGetChunk(chunkCoord, out var chunk))
				return BlockId.Air;

			if (!Chunk.InBounds(localX, localY, localZ))
				return BlockId.Air;

			return chunk.Get(localX, localY, localZ);
		}
	}
}
