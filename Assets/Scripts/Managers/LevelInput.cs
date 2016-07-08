using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Types of platforms.
/// </summary>
public enum PlatformType {
	Surface,
	Gap,
	Virtual
}

/// <summary>
/// Used to add a path for a level.
/// </summary>
public struct PathInput {
	/// <summary> The position of a path vertex. </summary>
	public Vector3 position;
	/// <summary> The type of platform to use. </summary>
	public PlatformType type;
	/// <summary> Y offset to raise points by. </summary>
	private static float YOFFSET = 0.04f;

	/// <summary>
	/// Creates a path input.
	/// </summary>
	/// <param name="x">The x coordinate of the path vertex.</param>
	/// <param name="y">The y coordinate of the path vertex.</param>
	/// <param name="z">The z coordinate of the path vertex.</param>
	/// <param name="type">The type of platform to use.</param>
	public PathInput(float x, float y, float z, int type) {
		position = new Vector3(-x, y + YOFFSET, z);
		this.type = (PlatformType)type;
	}

	/// <summary>
	/// Creates a path input.
	/// </summary>
	/// <param name="json">JSON data for the path vertex.</param>
	/// <param name="type">The type of platform to use.</param>
	public PathInput(JSONObject json, int type) {
		position = PathUtil.MakeVectorFromJSON(json);
		position.x = -position.x;
		position.y += YOFFSET;
		this.type = (PlatformType)type;
	}
}

/// <summary>
/// Used to add virtual platforms to a level.
/// </summary>
public struct PlatformInput {
	/// <summary> The positions of the corners of the platform. </summary>
	public List<Vector3> vertices;
	/// <summary> The type of platform to use. </summary>
	public PlatformType type;

	/// <summary>
	/// Creates a platform input.
	/// </summary>
	/// <param name="vertices">The positions of the corners of the platform.</param>
	/// <param name="type">The type of platform to use.</param>
	public PlatformInput(List<Vector3> vertices, PlatformType type) {
		this.vertices = vertices;
		this.type = type;
	}
}

/// <summary>
/// Used to add enemies into a level.
/// </summary>
public struct EnemyInput {
	/// <summary> The index of the type of enemy. </summary>
	public int enemyIndex;
	/// <summary> The vertices of the path the enemy will follow. </summary>
	public List<PathInput> path;

	/// <summary>
	/// Creates an enemy input.
	/// </summary>
	/// <param name="enemyIndex">The index of the type of enemy.</param>
	/// <param name="path">The vertices of the path the enemy will follow.</param>
	public EnemyInput(int enemyIndex, List<PathInput> path) {
		this.enemyIndex = enemyIndex;
		this.path = path;
	}

	/// <summary>
	/// Creates an enemy input.
	/// </summary>
	/// <param name="json">JSON data for the enemy.</param>
	public EnemyInput(JSONObject json) {
		JSONObject enemyPathJSON = json.GetField("enemy_path");
		path = new List<PathInput>(enemyPathJSON.list.Count);
		foreach (JSONObject pathComponent in enemyPathJSON.list) {
			path.Add(new PathInput(pathComponent, (int)PlatformType.Surface));
		}
		enemyIndex = (int)json.GetField("enemy_type").n;
	}
}

/// <summary>
/// Used to add collectibles into a level.
/// </summary>
public struct CollectibleInput {
	/// <summary> The type of collectible. </summary>
	public string type;
	/// <summary> The position of the collectible. </summary>
	public Vector3 position;

	/// <summary>
	/// Creates a collectible input.
	/// </summary>
	/// <param name="x">The x coordinate of the position of the collectible.</param>
	/// <param name="y">The y coordinate of the position of the collectible.</param>
	/// <param name="z">The z coordinate of the position of the collectible.</param>
	public CollectibleInput(float x, float y, float z) {
		type = "coin";
		position = new Vector3(x, y, z);
	}
		
	/// <summary>
	/// Creates a collectible input.
	/// </summary>
	/// <param name="json">JSON data for the collectible.</param>
	public CollectibleInput(JSONObject json) {
		type = json.GetField("collectible_type").str;
		position = PathUtil.MakeVectorFromJSON(json.GetField("position"));
	}
}

/// <summary>
/// Mapping of items to array indices.
/// </summary>
public enum Items {Coin, Mushroom, Coffee, Toothpick, FlySwatter};

/// <summary>
/// Used to add blocks into a level.
/// </summary>
public struct BlockInput {
	/// <summary> The index of the type of block. </summary>
	public int blockIndex;
	/// <summary> The index of the type of item inside the block. </summary>
	public int contentIndex;
	/// <summary> The position of the block. </summary>
	public Vector3 position;

	/// <summary>
	/// Creates a block input.
	/// </summary>
	/// <param name="blockIndex">The index of the type of block.</param>
	/// <param name="contentIndex">The index of the type of item inside the block.</param>
	/// <param name="x">The x coordinate of the position of the block.</param>
	/// <param name="y">The y coordinate of the position of the block.</param>
	/// <param name="z">The z coordinate of the position of the block.</param>
	public BlockInput (int blockIndex, int contentIndex, float x, float y, float z) {
		this.blockIndex = blockIndex;
		this.contentIndex = contentIndex;
		position = new Vector3 (x, y, z);
	}

	/// <summary>
	/// Creates a block input.
	/// </summary>
	/// <param name="json">JSON data for the block.</param>
	public BlockInput(JSONObject json) {
		this.blockIndex = (int)json.GetField("type").n;
		this.contentIndex = (int)json.GetField("contents").n;
		position = PathUtil.MakeVectorFromJSON(json.GetField("position"));
	}
}