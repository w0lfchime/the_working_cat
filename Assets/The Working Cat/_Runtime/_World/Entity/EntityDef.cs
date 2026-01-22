// Runtime/World/Entities/EntityDef.cs
namespace TheWorkingCat.World.Entities
{
	[System.Flags]
	public enum OccupancyMask : uint
	{
		None = 0,
		Solid = 1 << 0, // blocks movement
		Wall = 1 << 1, // affects adjacency / room logic later
		Support = 1 << 2, // can support other placements
		Interaction = 1 << 3, // has interaction points
	}

	public sealed class EntityDef
	{
		public readonly string Id;            // "StoneWall", "Assembler", etc.
		public readonly Footprint Footprint;
		public readonly OccupancyMask Occupancy;

		public EntityDef(string id, Footprint footprint, OccupancyMask occupancy)
		{
			Id = id;
			Footprint = footprint;
			Occupancy = occupancy;
		}
	}
}
