using UnityEngine;
using System.Collections.Generic;
using System.Net;

/// <summary>
/// Creates a ribbon path from input.
/// </summary>
public class LevelCreator : MonoBehaviour {

	/// <summary> Path resource to be instantiated from. </summary>
	[SerializeField]
	[Tooltip("Path resource to be instantiated from.")]
	private PathComponent pathPrefab;
	/// <summary> Goal resource to be instantiated from. </summary>
	[SerializeField]
	[Tooltip("Goal resource to be instantiated from.")]
	private Goal goalPrefab;
	/// <summary> Texture for surface paths. </summary>
	[SerializeField]
	[Tooltip("Texture for surface paths.")]
	private Material surfaceMaterial;
	/// <summary> Texture for virtual platforms. </summary>
	[SerializeField]
	[Tooltip("Texture for virtual platforms.")]
	private Material virtualPlatformMaterial;
	/// <summary> Block resources to be instantiated from. </summary>
	[SerializeField]
	[Tooltip("Block resources to be instantiated from.")]
	private Block[] blockPrefabs;

	/// <summary> Item resources to be instantiated from. </summary>
	[SerializeField]
	[Tooltip("Item resources to be instantiated from.")]
	private Item[] itemPrefabs;

	/// <summary> The player. </summary>
	[SerializeField]
	[Tooltip("The player.")]
	private Player playerPrefab;
	/// <summary> Enemy resources to be instantiated from. </summary>
	[SerializeField]
	[Tooltip("Enemy resources to be instantiated from.")]
	private Enemy[] enemyPrefabs;
	
	/// <summary> The material to draw path lines with. </summary>
	[SerializeField]
	[Tooltip("The material to draw path lines with.")]
	private Material lineMaterial;

	/// <summary> JSON file to load the level from. </summary>
	[SerializeField]
	[Tooltip("JSON file to load the level from.")]
	private TextAsset json;

	/// <summary> The height of surface paths. </summary>
	[SerializeField]
	[Tooltip("The height of surface paths.")]
	private float surfaceHeight;
	/// <summary> The thickness of surface paths. </summary>
	[SerializeField]
	[Tooltip("The thickness of surface paths.")]
	private float surfaceThickness;

	/// <summary> The height of virtual platforms. </summary>
	[SerializeField]
	[Tooltip("The height of virtual platforms.")]
	private float platformHeight;
	/// <summary> The thickness of virtual platforms. </summary>
	[SerializeField]
	[Tooltip("The thickness of virtual platforms.")]
	private float platformThickness;

	/// <summary>
	/// Creates the level from either the level AI server or a local JSON.
	/// </summary>
	private void Start() {
		if (json == null) {
			// Connect to the server to get JSON file.
			NetworkingManager.instance.ProcessStringFromURL((jsonText) =>
				{
					CreateLevel(jsonText);
				});
		} else {
			// Hard-coded JSON resource for testing.
			CreateLevel(json.text);
		}
	}

	/// <summary>
	/// Creates the level.
	/// </summary>
	/// <param name="jsonText">The JSON text to create the level with.</param>
	private void CreateLevel(string jsonText) {
		JSONObject input = new JSONObject(jsonText);
		Tracker.Instance.logJSON (jsonText);

		JSONObject meshJSON = input.GetField("mesh");
		int meshIndex = meshJSON != null ? (int)meshJSON.n : 0;
		GetComponent<MeshChooser>().ActivateMesh(meshIndex);

		// Parse the path from JSON.
		JSONObject pathJSON = input.GetField("route");
		if (pathJSON == null) {
			print("Failed to load JSON file.");
			return;
		}
		List<PathInput> pathInput = new List<PathInput>(pathJSON.list.Count);
		foreach (JSONObject pathComponent in pathJSON.list) {
			List<JSONObject> points = pathComponent.GetField("points").list;
			int type = (int)pathComponent.GetField("type").i;
			foreach (JSONObject point in points) {
				pathInput.Add(new PathInput(point, type));
			}
		}

		// Parse enemies from JSON.
		JSONObject enemyJSON = input.GetField("enemies");
		List<EnemyInput> enemyInput;
		if (enemyJSON == null) {
			enemyInput = new List<EnemyInput>(0);
		} else {
			enemyInput = new List<EnemyInput>(enemyJSON.list.Count);
			foreach (JSONObject enemy in enemyJSON.list) {
				enemyInput.Add(new EnemyInput(enemy));
			}
		}

		// Parse collectibles from JSON.
		JSONObject collectibleJSON = input.GetField("collectibles");
		List<CollectibleInput> collectibleInput;
		if (collectibleJSON == null) {
			collectibleInput = new List<CollectibleInput>(0);
		} else {
			collectibleInput = new List<CollectibleInput>(collectibleJSON.list.Count);
			foreach (JSONObject collectible in collectibleJSON.list) {
				collectibleInput.Add(new CollectibleInput (collectible));
			}
		}

		// Parse blocks from JSON.
		JSONObject blockJSON = input.GetField("blocks");
		List<BlockInput> blockInput;
		if (blockJSON == null) {
			blockInput = new List<BlockInput>(0);
		} else {
			blockInput = new List<BlockInput>(blockJSON.list.Count);
			foreach (JSONObject block in blockJSON.list) {
				blockInput.Add(new BlockInput (block));
			}
		}

		CreateLevel(pathInput, enemyInput, collectibleInput, blockInput);
	}

	/// <summary>
	/// Creates a level from the given input.
	/// </summary>
	/// <param name="pathInput">The path for the level.</param>
	/// <param name="enemyInput">Enemies in the level.</param>
	/// <param name="collectibleInput">Collectibles in the level.</param>
	/// <param name="blockInput">Blocks in the level.</param>
	public void CreateLevel(List<PathInput> pathInput, List<EnemyInput> enemyInput, List<CollectibleInput> collectibleInput, List<BlockInput> blockInput) {
		LevelManager levelManager = LevelManager.Instance;
		
		// Construct the path from the input points.
		List<PathComponent> fullPath = new List<PathComponent>(pathInput.Count - 1);

		// Construct virtual platforms to represent the colliders.
		for (int i = 0; i < fullPath.Capacity; i++) {
			if (pathInput[i].type != PlatformType.Gap) {
				List<Vector3> platform = new List<Vector3>();
				Vector3 direction = pathInput[i + 1].position - pathInput[i].position;
				Vector3 flatDirection = PathUtil.RemoveY(direction);
				float thickness = pathInput[i].type == PlatformType.Surface ? surfaceThickness : platformThickness;
				if (flatDirection == Vector3.zero) {
					// Wall.
					if (i > 0) {
						flatDirection = Vector3.Normalize(PathUtil.RemoveY(pathInput[i].position - pathInput[i - 1].position)) * thickness;
						Vector3 directionRotate = new Vector3 (flatDirection.z, 0, -flatDirection.x);
						Vector3 top = pathInput[i + 1].position.y > pathInput[i].position.y ? pathInput[i + 1].position : pathInput[i].position;
						platform.Add(top + directionRotate);
						platform.Add(top + directionRotate + flatDirection);
						platform.Add(top - directionRotate + flatDirection);
						platform.Add(top - directionRotate);
						CreatePlatform(new PlatformInput(platform, pathInput[i].type), Mathf.Abs(pathInput[i + 1].position.y - pathInput[i].position.y));
					}
				} else {
					Vector3 flatDirectionNorm = Vector3.Normalize(flatDirection);
					Vector3 directionRotate = new Vector3(flatDirectionNorm.z, 0, -flatDirectionNorm.x) * thickness;
					platform.Add(pathInput[i + 1].position + directionRotate);
					platform.Add(pathInput[i + 1].position - directionRotate);
					platform.Add(pathInput[i].position - directionRotate);
					platform.Add(pathInput[i].position + directionRotate);
					CreatePlatform(new PlatformInput(platform, pathInput[i].type));
				}
			}
		}

		for (int i = 0; i < fullPath.Capacity; i++) {
			// Make and position the path component.
			PathComponent pathComponent = CreatePath(pathInput[i], pathInput[i + 1]);
			fullPath.Add(pathComponent);
			pathComponent.lineMaterial = lineMaterial;

			// Link paths together.
			if (i > 0) {
				pathComponent.previousPath = fullPath[i - 1];
				fullPath[i - 1].nextPath = pathComponent;
			}

			pathComponent.Init();
		}

		if (fullPath.Count == 0) {
			print ("Invalid path.");
			return;
		}

		GetComponent<Visibility>().ApplySetting();

		// Set the player on a path.
		Player player = Instantiate(playerPrefab) as Player;
		PathMovement playerMovement = player.GetComponent<PathMovement>();
		playerMovement.currentPath = fullPath[0];
		playerMovement.startPath = fullPath[0];
		foreach (PathComponent pathComponent in fullPath) {
			pathComponent.transform.parent = player.transform;
		}

		levelManager.GetComponent<CameraSetting>().InitializeCameras(player);
		levelManager.player = player;
		
		// Create the goal at the end of the path.
		Goal goal = Instantiate(goalPrefab);
		goal.transform.parent = levelManager.transform.FindChild("Platforms").transform;
		Vector3 pathEnd = PathUtil.SetY(fullPath[fullPath.Count - 1].End, PathUtil.ceilingHeight);
		RaycastHit hit;
		if (Physics.Raycast(pathEnd, Vector3.down, out hit, PathUtil.ceilingHeight * 1.1f)) {
			goal.transform.position = hit.point + Vector3.up * 0.025f;
		} else {
			goal.transform.position = fullPath[fullPath.Count - 1].End;
		}
		levelManager.goal = goal;

		// Create enemies from the input.
		List<Enemy> enemies = new List<Enemy>(enemyInput.Count);
		foreach (EnemyInput input in enemyInput) {
			if (input.enemyIndex < enemyPrefabs.Length) {
				Enemy enemy = Instantiate(enemyPrefabs[input.enemyIndex]) as Enemy;
				enemies.Add(enemy);
				enemy.transform.parent = levelManager.transform.FindChild("Enemies").transform;
				if (enemy.GetComponent<PathMovement>()) {
					// Create the enemy path.
					int pathLength = input.path.Count - 1;
					List<PathComponent> enemyPath = new List<PathComponent>(pathLength);
					for (int i = 0; i < pathLength; i++) {
						PathComponent pathComponent = CreatePath(input.path[i], input.path[i + 1]);
						enemyPath.Add(pathComponent);
						if (i > 0) {
							pathComponent.previousPath = enemyPath[i - 1];
							enemyPath[i - 1].nextPath = pathComponent;
						}
						pathComponent.transform.parent = enemy.transform;
					}
					// Allow enemy paths to be circular.
					if (input.path[0].Equals(input.path[input.path.Count - 1])) {
						enemyPath[0].previousPath = enemyPath[pathLength - 1];
						enemyPath[pathLength - 1].nextPath = enemyPath[0];
					}
					enemy.GetComponent<PathMovement>().currentPath = enemyPath[0];
					enemy.GetComponent<PathMovement>().startPath = enemyPath[0];
				} else {
					enemy.transform.position = input.path[0].position;
				}
				// Make sure the enemy is above ground.
				Collider enemyCollider = null;
				foreach (Collider collider in enemy.GetComponents<Collider>()) {
					if (!collider.isTrigger) {
						enemyCollider = collider;
						break;
					}
				}
				float offset = enemyCollider.bounds.extents.y;
				Physics.Raycast(enemy.transform.position + Vector3.up * offset, Vector3.down, out hit, offset);
				enemy.transform.position = hit.point + Vector3.up * offset / 2;
			}
		}

		// Create collectibles from the input.
		List<Item> items = new List<Item>(collectibleInput.Count);
		foreach (CollectibleInput input in collectibleInput) {
			Item item = null;
			if (input.type == "coin") {
				item = Instantiate(itemPrefabs[(int)Items.Coin]) as Item;
			} else if (input.type == "power_up_size") {
				item = Instantiate(itemPrefabs[(int)Items.Mushroom]) as Item;
			} else if (input.type == "power_up_speed") {
				item = Instantiate(itemPrefabs[(int)Items.Coffee]) as Item;
			} else if (input.type == "power_up_range") {
				item = Instantiate(itemPrefabs[(int)Items.Toothpick]) as Item;
			} else if (input.type == "power_up_melee") {
				item = Instantiate(itemPrefabs[(int)Items.FlySwatter]) as Item;
			}

			if (item != null) {
				item.transform.parent = levelManager.transform.FindChild("Items").transform;
				item.transform.position = input.position;
				item.SetInitPosition(input.position);
			}
		}

		// Create blocks from the input.
		List<Block> blocks = new List<Block>(blockInput.Count);
		foreach (BlockInput input in blockInput) {
			Block block = Instantiate(blockPrefabs[input.blockIndex]) as Block;
			if (input.contentIndex != -1) {
				block.contents = itemPrefabs[input.contentIndex];
			}
			block.transform.parent = levelManager.transform.FindChild("Blocks").transform;
			block.transform.position = input.position;
			blocks.Add(block);
		}

		// Pass the needed data to the level manager to store.
		levelManager.fullPath = fullPath;
		levelManager.InitializePathRenderer();
		levelManager.enemies = enemies;
		levelManager.items = items;
		levelManager.blocks = blocks;
	}

	/// <summary>
	/// Creates a path component from a start point and end point.
	/// </summary>
	/// <returns>A new path component from the specified start and end points.</returns>
	/// <param name="startInput">The start point of the path component.</param>
	/// <param name="endInput">The end point of the path component.</param>
	private PathComponent CreatePath (PathInput startInput, PathInput endInput) {
		Vector3 start = startInput.position;
		Vector3 end = endInput.position;
		Vector3 center = (start + end) / 2;
		PathComponent path = Instantiate(pathPrefab, center, Quaternion.LookRotation(end - start, Vector3.up)) as PathComponent;
		path.SetPath(start, end);
		path.transform.Rotate(new Vector3 (0, -90, 0));
		Vector3 tempScale = path.transform.localScale;
		tempScale.x *= Vector3.Magnitude(end - start);
		path.transform.localScale = tempScale;
		return path;
	}

	/// <summary>
	/// Creates a virtual platform from its top vertices.
	/// </summary>
	/// <returns>A new virtual platform from the specified top vertices.</returns>
	/// <param name="input">The top vertices of the platform.</param>
	/// <param name="height">The thickness of the platform.</param>
	private GameObject CreatePlatform(PlatformInput input, float height = 0) {
		if (height == 0) {
			height = input.type == PlatformType.Surface ? surfaceHeight : platformHeight;
		}
		List<Vector3> bottom = new List<Vector3>(input.vertices.Count);
		for (int i = 0; i < input.vertices.Count; i++) {
			bottom.Add(PathUtil.SetY(input.vertices[i], input.vertices[i].y - height));
		}
		return CreatePlatform(input, new PlatformInput(bottom, input.type));
	}

	/// <summary>
	/// Creates a virtual platform from both top and bottom vertices.
	/// </summary>
	/// <returns>A new virtual platform from the specified vertices.</returns>
	/// <param name="top">The top vertices of the platform.</param>
	/// <param name="bottom">The bottom vertices of the platform.</param>
	private GameObject CreatePlatform(PlatformInput top, PlatformInput bottom) {
		GameObject virtualPlatform = new GameObject();
		virtualPlatform.layer = LayerMask.NameToLayer("Virtual");
		virtualPlatform.AddComponent<MeshFilter>();
		MeshRenderer renderer = virtualPlatform.AddComponent<MeshRenderer>();
		if (top.type == PlatformType.Surface) {
			virtualPlatform.name = "Collider";
			renderer.material = surfaceMaterial;
		} else {
			virtualPlatform.name = "Virtual Platform";
			renderer.material = virtualPlatformMaterial;
		}
		Mesh mesh = virtualPlatform.GetComponent<MeshFilter>().mesh;

		// Create the vertices of the platform.
		Vector3[] vertices = new Vector3[top.vertices.Count * 6];

		// Used to determine clockwise/counter-clockwise.
		float edgeSum = 0;
		for (int i = 0; i < top.vertices.Count; i++) {
			vertices[i] = top.vertices[i];
			vertices[i + top.vertices.Count] = bottom.vertices[i];
			if (i < top.vertices.Count - 1) {
				edgeSum += (top.vertices[i + 1].x - top.vertices[i].x) * (top.vertices[i + 1].z + top.vertices[i].z);
			} else {
				edgeSum += (top.vertices[0].x - top.vertices[i].x) * (top.vertices[0].z + top.vertices[i].z);
			}
		}
		bool clockwise = edgeSum > 0;

		// Find the triangles that can make up the top and bottom faces of the platform mesh.
		Triangulator triangulator = new Triangulator(top.vertices.ToArray());
		int[] topTriangles = triangulator.Triangulate();
		int[] triangles = new int[topTriangles.Length * 2 + top.vertices.Count * 6];
		for (int i = 0; i < topTriangles.Length; i += 3) {
			triangles[i] = topTriangles[i];
			triangles[i + 1] = topTriangles[i + 1];
			triangles[i + 2] = topTriangles[i + 2];
			triangles[topTriangles.Length + i] = topTriangles[i + 2] + top.vertices.Count;
			triangles[topTriangles.Length + i + 1] = topTriangles[i + 1] + top.vertices.Count;
			triangles[topTriangles.Length + i + 2] = topTriangles[i] + top.vertices.Count;
		}

		// Find the triangles for the sides of the platform.
		for (int i = 0; i < top.vertices.Count; i++) {
			int triangleOffset = topTriangles.Length * 2 + i * 6;
			int nextIndex = i < top.vertices.Count - 1 ? i + 1 : 0;

			int vertexOffset = top.vertices.Count * 2 + i * 4;
			vertices[vertexOffset] = vertices[i];
			vertices[vertexOffset + 1] = vertices[nextIndex];
			vertices[vertexOffset + 2] = vertices[top.vertices.Count + i];
			vertices[vertexOffset + 3] = vertices[top.vertices.Count + nextIndex];

			if (!clockwise) {
				triangles[triangleOffset] = vertexOffset;
				triangles[triangleOffset + 1] = vertexOffset + 1;
				triangles[triangleOffset + 2] = vertexOffset + 2;
				triangles[triangleOffset + 3] = vertexOffset + 3;
				triangles[triangleOffset + 4] = vertexOffset + 2;
				triangles[triangleOffset + 5] = vertexOffset + 1;
			} else {
				triangles[triangleOffset + 5] = vertexOffset;
				triangles[triangleOffset + 4] = vertexOffset + 1;
				triangles[triangleOffset + 3] = vertexOffset + 2;
				triangles[triangleOffset + 2] = vertexOffset + 3;
				triangles[triangleOffset + 1] = vertexOffset + 2;
				triangles[triangleOffset] = vertexOffset + 1;
			}
		}

		mesh.vertices = vertices;
		mesh.triangles = triangles;
		mesh.RecalculateNormals();

		virtualPlatform.AddComponent<MeshCollider>();
		virtualPlatform.GetComponent<MeshCollider>().sharedMesh = mesh;
		
		virtualPlatform.transform.parent = LevelManager.Instance.transform.FindChild("Platforms").transform;

		return virtualPlatform;
	}

	/// <summary>
	/// Calculates a surface normal from three points.
	/// </summary>
	/// <returns>A surface normal for the three points.</returns>
	/// <param name="point1">A point on the surface.</param>
	/// <param name="point2">A point on the surface.</param>
	/// <param name="point3">A point on the surface.</param>
	private Vector3 CalculateNormal(Vector3 point1, Vector3 point2, Vector3 point3) {
		Vector3 flat1 = point2 - point1;
		Vector3 flat2 = point3 - point2;
		return Vector3.Cross(flat1, flat2);
	}
}
