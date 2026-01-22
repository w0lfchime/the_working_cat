// Assets/TheWorkingCat/Runtime/World/Blocks/BlockTypes.cs
namespace TheWorkingCat.World
{
	public enum BlockId : byte
	{
		Air = 0,
		Cobble1 = 1,
		Cobble2 = 2,
		Cobble3 = 3,
		Cobble4 = 4,
		Laid1 = 5,
		Laid2 = 6,
		Stone1 = 7,
		Stone2 = 8,
		Grid1 = 9,
		Grid2 = 10,
		Grid3 = 11,
		Grass = 12,
		Dirt1 = 13,
		Dirt2 = 14,
		Leaves1 = 15,
		Leaves2 = 16,
	}

	public enum BlockFace : byte
	{
		North = 0, // +Z
		South = 1, // -Z
		East = 2,  // +X
		West = 3,  // -X
		Up = 4,    // +Y
		Down = 5,  // -Y
	}

	/// <summary>Atlas texturing: each face returns a tile index (0..tileCount-1).</summary>
	[System.Serializable]
	public struct BlockDefinition
	{
		public BlockId id;
		public bool isSolid;

		public int north, south, east, west, up, down;

		public int GetTileIndex(BlockFace face) => face switch
		{
			BlockFace.North => north,
			BlockFace.South => south,
			BlockFace.East => east,
			BlockFace.West => west,
			BlockFace.Up => up,
			BlockFace.Down => down,
			_ => north
		};
	}
}
