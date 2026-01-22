// Assets/TheWorkingCat/Runtime/Presentation/World/WorldView.cs
using UnityEngine;
using TheWorkingCat.Simulation;
using TheWorkingCat.World;

namespace TheWorkingCat.Presentation.World
{
	public sealed class WorldView : MonoBehaviour
	{
		[Header("Authoritative World")]
		[SerializeField] private WorldController worldController;

		[Header("Chunk View")]
		[SerializeField] private ChunkView chunkViewPrefab;

		[Tooltip("If true, existing child ChunkViews will be destroyed and rebuilt on Awake.")]
		[SerializeField] private bool rebuildOnAwake = true;

		private void Awake()
		{
			Setup();
		}

		public void Setup()
		{
			if (worldController == null)
			{
				Debug.LogError("WorldView: missing WorldController reference.");
				return;
			}

			// Ensure the WorldController has initialized its World before we read it.
			worldController.InitializeIfNeeded();

			if (chunkViewPrefab == null)
			{
				Debug.LogError("WorldView: missing ChunkView prefab reference.");
				return;
			}

			if (rebuildOnAwake)
				ClearExistingChunkViews();

			SpawnViews();
		}

		private void ClearExistingChunkViews()
		{
			// Destroy children that have ChunkView, to avoid duplicates when entering play mode repeatedly.
			for (int i = transform.childCount - 1; i >= 0; i--)
			{
				var child = transform.GetChild(i);
				if (child.GetComponent<ChunkView>() != null)
				{
#if UNITY_EDITOR
					if (!Application.isPlaying) DestroyImmediate(child.gameObject);
					else Destroy(child.gameObject);
#else
					Destroy(child.gameObject);
#endif
				}
			}
		}

		private void SpawnViews()
		{
			var world = worldController.World;
			if (world == null)
			{
				Debug.LogError("WorldView: WorldController.World is null (world not initialized?).");
				return;
			}

			bool any = false;
			foreach (var chunk in world.AllChunks)
			{
				any = true;
				var go = Instantiate(chunkViewPrefab, transform);
				go.name = $"ChunkView_{chunk.ChunkCoord.x}_{chunk.ChunkCoord.y}";
				// Position chunk in world units: chunkCoord * chunk dimensions.
				go.transform.localPosition = new Vector3(
					chunk.ChunkCoord.x * Chunk.SizeX,
					0f,
					chunk.ChunkCoord.y * Chunk.SizeZ
				);
				go.Bind(chunk);
			}

			if (!any)
			{
				Debug.LogWarning("WorldView: no chunks found to spawn. WorldState may be empty.");
			}
		}
	}
}
