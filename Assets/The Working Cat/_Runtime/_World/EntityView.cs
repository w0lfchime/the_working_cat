// Runtime/Presentation/Entities/EntityView.cs
using UnityEngine;
using TheWorkingCat.World.Entities;

namespace TheWorkingCat.Presentation.Entities
{
	public sealed class EntityView : MonoBehaviour
	{
		public Entity Entity { get; private set; }

		public void Bind(Entity entity)
		{
			Entity = entity;
			SyncTransform();
		}

		public void SyncTransform()
		{
			if (Entity == null) return;
			transform.localPosition = (Vector3)Entity.OriginCell;
		}
	}
}
