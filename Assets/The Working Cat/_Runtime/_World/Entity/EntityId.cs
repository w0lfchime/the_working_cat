// Runtime/World/Entities/EntityId.cs
namespace TheWorkingCat.World.Entities
{
	public readonly struct EntityId
	{
		public readonly int Value;
		public EntityId(int value) => Value = value;
		public override string ToString() => Value.ToString();
	}
}
