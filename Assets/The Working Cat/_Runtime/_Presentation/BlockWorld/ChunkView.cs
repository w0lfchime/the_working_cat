// Assets/TheWorkingCat/Runtime/Presentation/World/ChunkView.cs
using UnityEngine;
using TheWorkingCat.World;

namespace TheWorkingCat.Presentation.World
{
	[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
	public sealed class ChunkView : MonoBehaviour
	{
		[SerializeField] private Material material;
		[SerializeField] private int atlasTilesPerRow = 4;

		private MeshFilter _mf;
		private MeshRenderer _mr;

		public Chunk Chunk { get; private set; }

		private void Awake()
		{
			_mf = GetComponent<MeshFilter>();
			_mr = GetComponent<MeshRenderer>();
			if (material) _mr.sharedMaterial = material;
		}

		public void Bind(Chunk chunk)
		{
			Chunk = chunk;
			RebuildMesh();
		}

		public void RebuildMesh()
		{
			if (Chunk == null) return;

			var mesh = ChunkMesher.BuildMesh(Chunk, atlasTilesPerRow);
			_mf.sharedMesh = mesh;
		}
	}
}
