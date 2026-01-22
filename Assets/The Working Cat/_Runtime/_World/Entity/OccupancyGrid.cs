// Runtime/World/Entities/OccupancyGrid.cs
using System.Collections.Generic;
using UnityEngine;

namespace TheWorkingCat.World.Entities
{
	public sealed class OccupancyGrid
	{
		private readonly Dictionary<Vector3Int, EntityId> _cellToEntity = new();

		public bool IsOccupied(Vector3Int cell) => _cellToEntity.ContainsKey(cell);

		public bool TryGetOccupant(Vector3Int cell, out EntityId id) => _cellToEntity.TryGetValue(cell, out id);

		public bool TryAdd(Entity entity)
		{
			// Check first (atomic-ish)
			foreach (var cell in entity.Def.Footprint.EnumerateWorldCells(entity.OriginCell))
				if (_cellToEntity.ContainsKey(cell))
					return false;

			// Commit
			foreach (var cell in entity.Def.Footprint.EnumerateWorldCells(entity.OriginCell))
				_cellToEntity[cell] = entity.Id;

			return true;
		}

		public void Remove(Entity entity)
		{
			foreach (var cell in entity.Def.Footprint.EnumerateWorldCells(entity.OriginCell))
			{
				if (_cellToEntity.TryGetValue(cell, out var existing) && existing.Value == entity.Id.Value)
					_cellToEntity.Remove(cell);
			}
		}
	}
}
