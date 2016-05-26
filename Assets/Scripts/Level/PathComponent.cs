using UnityEngine;
using System;

/// <summary>
/// A part of the path of the level.
/// </summary>
public class PathComponent : MonoBehaviour {

	/// <summary> The position of the start of the path. </summary>
	[SerializeField]
	[Tooltip("The position of the start of the path.")]
	private Vector3 start;
	/// <summary>
	/// The position of the start of the path.
	/// </summary>
	public Vector3 Start {
		get {return start;}
		set {
			start = value;
			CalculateDimensions();
		}
	}
	/// <summary> The position of the end of the path. </summary>
	[SerializeField]
	[Tooltip("The position of the end of the path.")]
	private Vector3 end;
	/// <summary>
	/// The position of the end of the path.
	/// </summary>
	public Vector3 End {
		get {return end;}
		set {
			end = value;
			CalculateDimensions();
		}
	}

	/// <summary> The vector length of the path. </summary>
	private Vector3 length;
	/// <summary> The vector length of the path. </summary>
	public Vector3 Length {
		get {return length;}
	}
	/// <summary> The normalized direction of the path when moving forward. </summary>
	private Vector3 direction;
	/// <summary> The scalar magnitude of the path's length. </summary>
	private float magnitude;
	/// <summary> The scalar magnitude of the path's length. </summary>
	public float Magnitude {
		get {return magnitude;}
	}

	/// <summary> The path directly after this path. </summary>
	[Tooltip("The path directly after this path.")]
	public PathComponent nextPath;
	/// <summary> The path directly before this path. </summary>
	[Tooltip("The path directly before this path.")]
	public PathComponent previousPath;

	/// <summary> The length of individual segments of the rendered line. </summary>
	private const float LINELENGTH = 0.1f;
	/// <summary> The material to draw the line with. </summary>
	[HideInInspector]
	public Material lineMaterial;
	/// <summary> The first rendered part of the path. </summary>
	[HideInInspector]
	public PathRenderer firstLine;
	/// <summary> The last rendered part of the path. </summary>
	[HideInInspector]
	public PathRenderer lastLine;

	/// <summary>
	/// Initializes the path component.
	/// </summary>
	public void Init() {
		if (magnitude == 0) {
			if (start == end) {
				// If start and end points aren't already set, find them from the center point and rotation.
				magnitude = (float)Math.Pow((double)(transform.localScale.x * transform.localScale.x + transform.localScale.z * transform.localScale.z), 0.5);

				float radius = magnitude / 2;
				Vector3 rotation = transform.eulerAngles * (float)Math.PI / 180;
				Vector3 offset = new Vector3(radius * (float)Math.Cos(rotation.y),
				                              0,
				                              radius * -(float)Math.Sin(rotation.y));
				start = new Vector3(transform.position.x - offset.x, 
				                 transform.position.y - offset.y,
				                 transform.position.z - offset.z);
				end = new Vector3(transform.position.x + offset.x, 
				               transform.position.y + offset.y,
							   transform.position.z + offset.z);

				length = PathUtil.RemoveY(end - start);
				direction = Vector3.Normalize(length);
			} else {
				CalculateDimensions();
			}
		}
		if (lineMaterial != null) {
			DrawLine();
		}
	}

	/// <summary>
	/// Sets both endpoints of the path.
	/// </summary>
	/// <param name="newStart">The start of the path.</param>
	/// <param name="newEnd">The end of the path.</param>
	public void SetPath (Vector3 newStart, Vector3 newEnd) {
		start = newStart;
		end = newEnd;
		CalculateDimensions();
	}
	
	// Gets the magnitude of a fraction of the path.
	/// <summary>
	/// Gets the magnitude of a fraction of the path.
	/// </summary>
	/// <returns>The magnitude of the specified fraction of the path.</returns>
	/// <param name="pathProgress">The fraction of the path to get the magnitude of.</param>
	public float GetMagnitudeFromProgress(float pathProgress) {
		return magnitude * pathProgress;
	}

	/// <summary>
	/// Gets how far along the path something is based on its current distance along the path.
	/// </summary>
	/// <returns>How far along the path something is based on its current distance along the path.</returns>
	/// <param name="currentDistance">The current distance along the path.</param>
	public float GetProgressFromMagnitude(float currentDistance) {
		return currentDistance / magnitude;
	}

	/// <summary>
	/// Moves the path progress forward or backward depending on movement speed.
	/// </summary>
	/// <returns>The modified path progress</returns>
	/// <param name="pathProgress">The current path progress.</param></param>
	/// <param name="moveSpeed">The distance to move along the path.</param>
	/// <param name="forward">Whether to move forward.</param>
	public float IncrementPathProgress(float pathProgress, float moveSpeed, bool forward) {
		return forward ? pathProgress + moveSpeed / magnitude : pathProgress - moveSpeed / magnitude;
	}

	/// <summary>
	/// Gets a position at a specified part (fraction) of the path.
	/// </summary>
	/// <returns>A position at a specified part (fraction) of the path.</returns>
	/// <param name="pathProgress">The current progress along the path.</param>
	public Vector3 GetPositionInPath(float pathProgress) {
		if (magnitude == 0) {
			Init();
		}
		return start + length * pathProgress;
	}

	/// <summary>
	/// Gets a position at a specified length in the path.
	/// </summary>
	/// <returns>A position at the specified length in the path.</returns>
	/// <param name="magnitudeProgress">The length along the path.</param>
	public Vector3 GetPositionInPathMagnitude(float magnitudeProgress) {
		return start + length * magnitudeProgress / magnitude;
	}

	/// <summary>
	/// Gets a normalized vector representing the direction of the path, forward or backward.
	/// </summary>
	/// <returns>A normalized vector representing the direction of the path, forward or backward.</returns>
	/// <param name="forward">Whether to get a vector pointing forward.</param>
	public Vector3 GetDirection(bool forward) {
		return forward ? direction : -direction;
	}

	/// <summary>
	/// Draws a dotted line along the ribbon path.
	/// </summary>
	public void DrawLine() {
		if (firstLine != null) {
			return;
		}
		if (magnitude < Mathf.Epsilon) {
			firstLine = new PathRenderer(this, start, end, lineMaterial);
			lastLine = firstLine;
			return;
		}

		int numLines = (int)(magnitude / LINELENGTH / 2) + 1;
		PathRenderer prevPathRenderer = null;
		for (int i = 0; i < numLines; i++) {
			// Get the start and end points of the segment.
			Vector3 lineStart = GetPositionInPathMagnitude(i * 2 * LINELENGTH);
			Vector3 lineEnd;
			// Check if a shorter segment needs to be drawn at the end of the line.
			if (i == numLines - 1) {
				if (i * 2 * LINELENGTH > magnitude) {
					break;
				} else {
					lineEnd = GetPositionInPathMagnitude(magnitude);
				}
			} else {
				lineEnd = GetPositionInPathMagnitude((i * 2 + 1) * LINELENGTH);
			}

			// Find the height to draw the segment at.
			RaycastHit hit;
			float heightDifference = Mathf.Abs(lineStart.y - lineEnd.y) + 1;
			float centerHeight = (lineStart.y + lineEnd.y) / 2;
			if (Physics.Raycast(lineStart + Vector3.up * heightDifference, Vector3.down, out hit, PathUtil.ceilingHeight * 1.1f * 100, ~(1 << 11))) {
				lineStart.y = hit.point.y;
			} else {
				lineStart.y = centerHeight;
			}
			if (Physics.Raycast(lineEnd + Vector3.up * heightDifference, Vector3.down, out hit, PathUtil.ceilingHeight * 1.1f * 100, ~(1 << 11))) {
				lineEnd.y = hit.point.y;
			} else {
				lineEnd.y = centerHeight;
			}

			// Draw the segment.
			PathRenderer pathRenderer = new PathRenderer(this, lineStart, lineEnd, lineMaterial);
			if (prevPathRenderer != null) {
				pathRenderer.prevLine = prevPathRenderer;
				prevPathRenderer.nextLine = pathRenderer;
			}
			if (i == 0) {
				firstLine = pathRenderer;
			}
			prevPathRenderer = pathRenderer;
		}
		if (firstLine == null) {
			firstLine = new PathRenderer(this, start, end, lineMaterial);
			lastLine = firstLine;
		} else {
			lastLine = prevPathRenderer;
		}
		if (previousPath != null) {
			firstLine.prevLine = previousPath.lastLine;
			previousPath.lastLine.nextLine = firstLine;
		}
	}

	/// <summary>
	/// Calculates certain variables in the path.
	/// </summary>
	private void CalculateDimensions() {
		length = PathUtil.RemoveY(end - start);
		direction = Vector3.Normalize(length);
		magnitude = Vector3.Magnitude(length);
		// Take the component out of the path if its length is 0.
		if (magnitude < Mathf.Epsilon) {
			if (previousPath != null) {
				previousPath.nextPath = nextPath;
			}
			if (nextPath != null) {
				nextPath.previousPath = previousPath;
			}
		}
	}
}
