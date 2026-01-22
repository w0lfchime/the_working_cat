// Assets/TheWorkingCat/Runtime/Presentation/World/ChunkMesher.cs
using System.Collections.Generic;
using UnityEngine;
using TheWorkingCat.World;

namespace TheWorkingCat.Presentation.World
{
	public static class ChunkMesher
	{
		public static Mesh BuildMesh(Chunk chunk, int atlasTilesPerRow)
		{
			BlockLibrary.EnsureInitialized();

			var verts = new List<Vector3>(4096);
			var tris = new List<int>(8192);
			var uvs = new List<Vector2>(4096);
			var norms = new List<Vector3>(4096);

			int vStart = 0;

			int tpr = Mathf.Max(1, atlasTilesPerRow);
			float step = 1f / tpr;
			float inset = step * 0.02f;

			for (int y = 0; y < Chunk.SizeY; y++)
				for (int z = 0; z < Chunk.SizeZ; z++)
					for (int x = 0; x < Chunk.SizeX; x++)
					{
						BlockId id = chunk.Get(x, y, z);
						if (!BlockLibrary.IsSolid(id)) continue;

						TryFace(BlockFace.North, 0, 0, 1);
						TryFace(BlockFace.South, 0, 0, -1);
						TryFace(BlockFace.East, 1, 0, 0);
						TryFace(BlockFace.West, -1, 0, 0);
						TryFace(BlockFace.Up, 0, 1, 0);
						TryFace(BlockFace.Down, 0, -1, 0);

						void TryFace(BlockFace face, int nx, int ny, int nz)
						{
							BlockId neighbor = chunk.Get(x + nx, y + ny, z + nz);
							if (BlockLibrary.IsSolid(neighbor)) return;
							AddQuad(face, x, y, z, id);
						}

						void AddQuad(BlockFace face, int bx, int by, int bz, BlockId blockId)
						{
							Vector3 p = new Vector3(bx, by, bz);

							Vector3 p0, p1, p2, p3;
							Vector3 n;

							switch (face)
							{
								case BlockFace.North: // +Z
									p0 = p + new Vector3(0, 0, 1);
									p1 = p + new Vector3(1, 0, 1);
									p2 = p + new Vector3(1, 1, 1);
									p3 = p + new Vector3(0, 1, 1);
									n = Vector3.forward;
									break;

								case BlockFace.South: // -Z
									p0 = p + new Vector3(1, 0, 0);
									p1 = p + new Vector3(0, 0, 0);
									p2 = p + new Vector3(0, 1, 0);
									p3 = p + new Vector3(1, 1, 0);
									n = Vector3.back;
									break;

								case BlockFace.East: // +X
									p0 = p + new Vector3(1, 0, 1);
									p1 = p + new Vector3(1, 0, 0);
									p2 = p + new Vector3(1, 1, 0);
									p3 = p + new Vector3(1, 1, 1);
									n = Vector3.right;
									break;

								case BlockFace.West: // -X
									p0 = p + new Vector3(0, 0, 0);
									p1 = p + new Vector3(0, 0, 1);
									p2 = p + new Vector3(0, 1, 1);
									p3 = p + new Vector3(0, 1, 0);
									n = Vector3.left;
									break;

								case BlockFace.Up: // +Y
									p0 = p + new Vector3(0, 1, 1);
									p1 = p + new Vector3(1, 1, 1);
									p2 = p + new Vector3(1, 1, 0);
									p3 = p + new Vector3(0, 1, 0);
									n = Vector3.up;
									break;

								case BlockFace.Down: // -Y
									p0 = p + new Vector3(0, 0, 0);
									p1 = p + new Vector3(1, 0, 0);
									p2 = p + new Vector3(1, 0, 1);
									p3 = p + new Vector3(0, 0, 1);
									n = Vector3.down;
									break;

								default:
									return;
							}

							verts.Add(p0); verts.Add(p1); verts.Add(p2); verts.Add(p3);
							norms.Add(n); norms.Add(n); norms.Add(n); norms.Add(n);

							tris.Add(vStart + 0); tris.Add(vStart + 1); tris.Add(vStart + 2);
							tris.Add(vStart + 0); tris.Add(vStart + 2); tris.Add(vStart + 3);

							int tileIndex = BlockLibrary.Get(blockId).GetTileIndex(face);

							int tx = tileIndex % tpr;
							int ty = (tpr - 1) - (tileIndex / tpr);

							float u0 = tx * step + inset;
							float v0 = ty * step + inset;
							float u1 = (tx + 1) * step - inset;
							float v1 = (ty + 1) * step - inset;

							uvs.Add(new Vector2(u0, v0));
							uvs.Add(new Vector2(u1, v0));
							uvs.Add(new Vector2(u1, v1));
							uvs.Add(new Vector2(u0, v1));

							vStart += 4;
						}
					}

			var mesh = new Mesh();
			mesh.indexFormat = (verts.Count > 65535)
				? UnityEngine.Rendering.IndexFormat.UInt32
				: UnityEngine.Rendering.IndexFormat.UInt16;

			mesh.SetVertices(verts);
			mesh.SetTriangles(tris, 0);
			mesh.SetNormals(norms);
			mesh.SetUVs(0, uvs);
			mesh.RecalculateBounds();
			return mesh;
		}
	}
}
