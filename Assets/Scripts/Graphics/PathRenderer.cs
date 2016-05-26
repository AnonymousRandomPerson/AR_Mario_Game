using UnityEngine;

/// <summary>
/// Displays a line tracing a part of the path.
/// </summary>
public class PathRenderer {
	
	/// <summary> The thickness of the rendered line. </summary>
	private const float LINETHICKNESS = 0.002f;

	/// <summary> The path component that this line is displaying. </summary>
	private PathComponent pathComponent;
	/// <summary> The line being rendered. </summary>
	private LineRenderer line;

	/// <summary> The line immediately before this line in the path. </summary>
	public PathRenderer prevLine;
	/// <summary> The line immediately after this line in the path. </summary>
	public PathRenderer nextLine;

	/// <summary> The position of the start of the line. </summary>
	private Vector3 lineStart;
	/// <summary> The position of the end of the line. </summary>
	private Vector3 lineEnd;

	/// <summary>
	/// Creates a path line renderer.
	/// </summary>
	/// <param name="pathComponent">The path component that this line is tracing.</param>
	/// <param name="lineStart">The start of the line.</param>
	/// <param name="lineEnd">The end of the line.</param>
	/// <param name="lineMaterial">The material used to render the line.</param>
	public PathRenderer(PathComponent pathComponent, Vector3 lineStart, Vector3 lineEnd, Material lineMaterial) {
		this.pathComponent = pathComponent;
		this.lineStart = lineStart;
		this.lineEnd = lineEnd;

		line = new GameObject().AddComponent<LineRenderer>() as LineRenderer;
		line.gameObject.name = "PathRenderer";
		line.material = lineMaterial;
		line.transform.parent = pathComponent.transform;
		line.SetWidth(LINETHICKNESS, LINETHICKNESS);
		line.SetVertexCount(2);

		line.SetPosition(0, lineStart);
		line.SetPosition(1, lineEnd);
		SetVisible(false);
	}

	/// <summary>
	/// Sets the line as visible or invisible.
	/// </summary>
	/// <param name="visible">Whether the line should be visible.</param>
	public void SetVisible(bool visible) {
		line.enabled = visible;
	}

	/// <summary>
	/// Gets the distance from the player to the front of the line, following the path.
	/// </summary>
	/// <returns>The distance from the player to the front of the line, following the path.</returns>
	/// <param name="player">The player to get a distance from.</param>
	public float GetFrontDistance(Player player) {
		PathComponent currentPath = pathComponent;
		PathComponent targetPath = player.GetCurrentPath();
		float currentDistance = 0;
		if (currentPath != targetPath) {
			currentDistance += PathUtil.DistanceXZ(lineEnd, currentPath.End);
			currentPath = currentPath.nextPath;
			while (currentPath != null && currentPath != targetPath) {
				currentDistance += currentPath.Magnitude;
				currentPath = currentPath.nextPath;
			}
			if (currentPath != null) {
				currentDistance += PathUtil.DistanceXZ(currentPath.Start, player.transform.position);
			}
		} else {
			currentDistance = PathUtil.DistanceXZ(lineEnd, player.transform.position);
		}
		return currentDistance;
	}

	/// <summary>
	/// Gets the distance from the player to the back of the line, following the path.
	/// </summary>
	/// <returns>The distance from the player to the back of the line, following the path.</returns>
	/// <param name="player">The player to get a distance from.</param>
	public float GetBackDistance(Player player) {
		PathComponent currentPath = pathComponent;
		PathComponent targetPath = player.GetCurrentPath();
		float currentDistance = 0;
		if (currentPath != targetPath) {
			currentDistance += PathUtil.DistanceXZ(lineStart, currentPath.Start);
			currentPath = currentPath.previousPath;
			while (currentPath != null && currentPath != targetPath) {
				currentDistance += currentPath.Magnitude;
				currentPath = currentPath.previousPath;
			}
			currentDistance += PathUtil.DistanceXZ(currentPath.End, player.transform.position);
		} else {
			currentDistance = PathUtil.DistanceXZ(lineStart, player.transform.position);
		}
		return currentDistance;
	}
}
