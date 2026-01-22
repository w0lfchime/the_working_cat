// Runtime/World/Entities/Footprint.cs
using System.Collections.Generic;
using UnityEngine;

namespace TheWorkingCat.World.Entities
{
	public sealed class Footprint
	{
		// List of occupied local cells relative to origin.
		public readonly Vector3Int[] Cells;

		public Footprint(params Vector3Int[] cells)
		{
			Cells = cells ?? new Vector3Int[0];
		}

		public IEnumerable<Vector3Int> EnumerateWorldCells(Vector3Int origin)
		{
			for (int i = 0; i < Cells.Length; i++)
				yield return origin + Cells[i];
		}

		public static Footprint SingleBlock() => new Footprint(Vector3Int.zero);

		// Example helpers
		public static Footprint Box(int sizeX, int sizeY, int sizeZ)
		{
			var list = new List<Vector3Int>(sizeX * sizeY * sizeZ);
			for (int y = 0; y < sizeY; y++)
				for (int z = 0; z < sizeZ; z++)
					for (int x = 0; x < sizeX; x++)
						list.Add(new Vector3Int(x, y, z));
			return new Footprint(list.ToArray());
		}
	}
}
