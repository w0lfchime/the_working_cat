// Assets/TheWorkingCat/Runtime/World/WorldState.cs
using System.Collections.Generic;
using UnityEngine;

namespace TheWorkingCat.World
{
	public sealed class WorldState
	{
		// For now: dictionary of chunks by coord.
		private readonly Dictionary<Vector2Int, Chunk> _chunks = new Dictionary<Vector2Int, Chunk>();

		public IEnumerable<Chunk> AllChunks => _chunks.Values;

		public bool TryGetChunk(Vector2Int coord, out Chunk chunk) => _chunks.TryGetValue(coord, out chunk);

		public Chunk GetOrCreateChunk(Vector2Int coord)
		{
			if (_chunks.TryGetValue(coord, out var existing))
				return existing;

			var created = new Chunk(coord);
			_chunks.Add(coord, created);
			return created;
		}
	}
}
