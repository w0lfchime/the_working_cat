// Runtime/World/Entities/EntityWorld.cs
using System;
using System.Collections.Generic;
using UnityEngine;

namespace TheWorkingCat.World.Entities
{
	public sealed class EntityWorld
	{
		private int _nextId = 1;
		private readonly Dictionary<int, Entity> _entities = new();
		public readonly OccupancyGrid Occupancy = new();

		public event Action<Entity>? OnSpawned;
		public event Action<Entity>? OnDespawned;

		public IEnumerable<Entity> All => _entities.Values;

		public bool TryGet(EntityId id, out Entity e) => _entities.TryGetValue(id.Value, out e);

		public bool TrySpawn(EntityDef def, Vector3Int originCell, out Entity entity)
		{
			entity = null;

			var id = new EntityId(_nextId++);
			var e = new Entity(id, def, originCell);

			if (!Occupancy.TryAdd(e))
				return false;

			_entities.Add(id.Value, e);
			entity = e;
			OnSpawned?.Invoke(e);
			return true;
		}

		public bool Despawn(EntityId id)
		{
			if (!_entities.TryGetValue(id.Value, out var e)) return false;

			Occupancy.Remove(e);
			_entities.Remove(id.Value);
			OnDespawned?.Invoke(e);
			return true;
		}
	}
}
