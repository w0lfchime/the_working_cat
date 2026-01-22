// Assets/TheWorkingCat/Runtime/World/Blocks/BlockLibrary.cs
using System;

namespace TheWorkingCat.World
{
	public static class BlockLibrary
	{
		// Indexed by (byte)BlockId. Keep size 256 so BlockId stays a byte.
		private static readonly BlockDefinition[] _defs = new BlockDefinition[256];
		private static bool _initialized;

		public static void EnsureInitialized()
		{
			if (_initialized) return;
			_initialized = true;

			// Air (non-solid, no rendering)
			_defs[(byte)BlockId.Air] = new BlockDefinition
			{
				id = BlockId.Air,
				isSolid = false,
				north = 0,
				south = 0,
				east = 0,
				west = 0,
				up = 0,
				down = 0
			};

			// Tile indices map left->right, then down (0..15)
			_defs[(byte)BlockId.Cobble1] = SolidAllFaces(BlockId.Cobble1, tile: 0);
			_defs[(byte)BlockId.Cobble2] = SolidAllFaces(BlockId.Cobble2, tile: 1);
			_defs[(byte)BlockId.Cobble3] = SolidAllFaces(BlockId.Cobble3, tile: 2);
			_defs[(byte)BlockId.Cobble4] = SolidAllFaces(BlockId.Cobble4, tile: 3);

			_defs[(byte)BlockId.Laid1] = SolidAllFaces(BlockId.Laid1, tile: 4);
			_defs[(byte)BlockId.Laid2] = SolidAllFaces(BlockId.Laid2, tile: 5);

			_defs[(byte)BlockId.Stone1] = SolidAllFaces(BlockId.Stone1, tile: 6);
			_defs[(byte)BlockId.Stone2] = SolidAllFaces(BlockId.Stone2, tile: 7);

			_defs[(byte)BlockId.Grid1] = SolidAllFaces(BlockId.Grid1, tile: 8);
			_defs[(byte)BlockId.Grid2] = SolidAllFaces(BlockId.Grid2, tile: 9);
			_defs[(byte)BlockId.Grid3] = SolidAllFaces(BlockId.Grid3, tile: 10);

			_defs[(byte)BlockId.Grass] = SolidAllFaces(BlockId.Grass, tile: 11);

			_defs[(byte)BlockId.Dirt1] = SolidAllFaces(BlockId.Dirt1, tile: 12);
			_defs[(byte)BlockId.Dirt2] = SolidAllFaces(BlockId.Dirt2, tile: 13);

			_defs[(byte)BlockId.Leaves1] = SolidAllFaces(BlockId.Leaves1, tile: 14);
			_defs[(byte)BlockId.Leaves2] = SolidAllFaces(BlockId.Leaves2, tile: 15);
		}

		public static BlockDefinition Get(BlockId id)
		{
			EnsureInitialized();
			return _defs[(byte)id];
		}

		public static bool IsSolid(BlockId id)
		{
			EnsureInitialized();
			return _defs[(byte)id].isSolid;
		}

		private static BlockDefinition SolidAllFaces(BlockId id, int tile)
		{
			return new BlockDefinition
			{
				id = id,
				isSolid = true,
				north = tile,
				south = tile,
				east = tile,
				west = tile,
				up = tile,
				down = tile
			};
		}
	}
}
