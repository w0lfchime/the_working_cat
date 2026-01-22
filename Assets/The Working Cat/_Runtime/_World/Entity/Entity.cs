// Runtime/World/Entities/Entity.cs
using UnityEngine;

namespace TheWorkingCat.World.Entities
{
	public sealed class Entity
	{
		public EntityId Id { get; }
		public EntityDef Def { get; }

		// Grid placement
		public Vector3Int OriginCell { get; private set; }

		public Entity(EntityId id, EntityDef def, Vector3Int originCell)
		{
			Id = id;
			Def = def;
			OriginCell = originCell;
		}

		public void SetOrigin(Vector3Int cell) => OriginCell = cell;
	}
}
