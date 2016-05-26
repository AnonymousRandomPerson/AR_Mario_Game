using UnityEngine;
using System;

/// <summary>
/// Controls movement along the ribbon path.
/// </summary>
public class PathMovement : MonoBehaviour {

	/// <summary> The part of the path where the object begins the level on. </summary>
	[Tooltip("The part of the path where the object begins the level on.")]
	public PathComponent startPath;
	/// <summary> How far along the path the object is at the start of the level. </summary>
	[SerializeField]
	[Tooltip("How far along the path the object is at the start of the level.")]
	private float startProgress = 0;
	/// <summary> Where the object begins the level at. </summary>
	private Vector3 startPosition;
	/// <summary> Whether the object has been placed on the ribbon path. </summary>
	private bool initiated = false;

	/// <summary> The part of the level that the object is currently on. </summary>
	[Tooltip("The part of the level that the object is currently on.")]
	public PathComponent currentPath;
	/// <summary> How far along the path the object is. </summary>
	[Tooltip("How far along the path the object is.")]
	public float pathProgress = 0;

	/// <summary> The speed that the object moves at per tick. </summary>
	[Tooltip("The speed that the object moves at per tick.")]
	public float moveSpeed = 0.01f;
	/// <summary> The rigid body controlling the object's physics. </summary>
	private Rigidbody body;
	/// <summary> The distance from the center of the object to the ground while grounded. </summary>
	private float groundOffset;
	/// <summary> The distance from the center of the object to the ground while grounded. </summary>
	public float GroundOffset {
		get {return groundOffset;}
	}
	/// <summary> The distance from the center of the object to its side. </summary>
	private float sideOffset;
	/// <summary> The distance from the center of the object to its side. </summary>
	public float SideOffset {
		get {return sideOffset;}
	}
	/// <summary> The number of places on the object to check for side collisions. </summary>
	private const int NUMSIDECHECKS = 30;
	/// <summary> Extra distance to count as a side collision. </summary>
	private const float COLLISIONOFFSET = 0.001f;
	/// <summary> The layers to detect as collisions. </summary>
	[SerializeField]
	[Tooltip("The layers to detect as collisions.")]
	private LayerMask collisionLayers;
	/// <summary> Whether the object rotates at a constant speed. </summary>
	private bool rotates = false;

	/// <summary> The maximum slope angle that the object can walk on. </summary>
	private float MAXSLOPEANGLE = 60;

	/// <summary>
	/// Initializes the path movement.
	/// </summary>
	private void Start() {
		body = GetComponent<Rigidbody>();
		startProgress = pathProgress;
		if (startPath != null) {
			ResetPosition();
		}
		UpdateGroundOffset();
		sideOffset = GetComponent<Collider>().bounds.extents.z;
		transform.eulerAngles = Vector3.up * Vector3.Angle(Vector3.right, currentPath.GetDirection(true));
		rotates = GetComponent<Rotate>() != null;
		FacePath(true);
	}

	/// <summary>
	/// Checks if the movement needs to be reset.
	/// </summary>
	private void Update() {
		if (!initiated && startPath != null) {
			ResetPosition();
		}
	}

	/// <summary>
	/// Finds the offset from the entity's center to the bottom of the entity.
	/// </summary>
	public void UpdateGroundOffset() {
		groundOffset = GetComponent<Collider>().bounds.extents.y;
	}

	/// <summary>
	/// Gets the side offset of the object in the direction of its current path.
	/// </summary>
	/// <returns>The side offset of the object in the direction of its current path.</returns>
	/// <param name="forward">Whether the object is moving forward.</param>
	private Vector3 GetRotatedSideOffset(bool forward) {
		return sideOffset * currentPath.GetDirection(forward);
	}

	/// <summary>
	/// Resets the position of the object to its initial position.
	/// </summary>
	public void ResetPosition() {
		currentPath = startPath;
		pathProgress = startProgress;
		if (body == null) {
			body = GetComponent<Rigidbody>();
		}
		body.velocity = Vector3.zero;

		// Determine the starting position and save it in case the player resets.
		if (!initiated) {
			startPosition = startPath.GetPositionInPath (pathProgress);
			if (body.useGravity) {
				RaycastHit hit;
				startPosition.y = PathUtil.ceilingHeight;
				Physics.Raycast(startPosition, Vector3.down, out hit, PathUtil.ceilingHeight * 1.1f);
				startPosition.y = hit.point.y + groundOffset + 0.25f;
			} else {
				startPosition.y = transform.position.y;
			}
			initiated = true;
		}
		transform.position = startPosition;
		FacePath(true);
	}

	/// <summary>
	/// Moves the object forward along the ribbon path.
	/// </summary>
	/// <returns>Whether the object was able to move.</returns>
	private bool MoveAlongPath() {
		return MoveAlongPath(true);
	}

	/// <summary>
	/// Moves the object along the ribbon path.
	/// </summary>
	/// <returns>Whether the object was able to move.</returns>
	/// <param name="forward">Whether to move the object forward along the path.</param>
	public bool MoveAlongPath(bool forward) {
		float moveDistance = moveSpeed;

		// Check for side collision.
		Vector3 sidePosition = transform.position + Vector3.down * (groundOffset - COLLISIONOFFSET) + GetRotatedSideOffset(forward);
		float sideIncrement = groundOffset * 2 / NUMSIDECHECKS + 2 * COLLISIONOFFSET;
		RaycastHit hit;
		int loopStart = 1;
		// Test for sloped ground.
		if (Physics.Raycast(sidePosition, currentPath.GetDirection(forward), out hit, moveSpeed + COLLISIONOFFSET, collisionLayers)) {
			float groundDistance = hit.distance;
			sidePosition.y += sideIncrement;
			loopStart++;
			if (Physics.Raycast(sidePosition, currentPath.GetDirection(forward), out hit, moveSpeed + COLLISIONOFFSET, collisionLayers)) {
				if (hit.distance <= groundDistance || Mathf.Atan(sideIncrement / (hit.distance - groundDistance)) > MAXSLOPEANGLE * Mathf.Deg2Rad) {
					return false;
				}
			}
		}
		// Test the rest of the vertical distance for blockage.
		for (int i = loopStart; i < NUMSIDECHECKS; i++) {
			if (Physics.Raycast(sidePosition, currentPath.GetDirection(forward), out hit, moveSpeed + COLLISIONOFFSET, collisionLayers)) {
				moveDistance = Mathf.Min(moveDistance, hit.distance - COLLISIONOFFSET);
				if (moveDistance < Mathf.Epsilon) {
					return false;
				}
			}
			sidePosition.y += sideIncrement;
		}

		pathProgress = currentPath.IncrementPathProgress(pathProgress, moveDistance, forward);

		// Check if the object has moved past the path's bounds.
		// Switch to the next/previous path if so.
		while (pathProgress > 1) {
			if (currentPath.nextPath == null) {
				pathProgress = 1;
				return false;
			} else {
				float overflow = currentPath.GetMagnitudeFromProgress(pathProgress - 1);
				currentPath = currentPath.nextPath;
				pathProgress = currentPath.GetProgressFromMagnitude(overflow);
			}
		}
		while (pathProgress < 0) {
			if (currentPath.previousPath == null) {
				pathProgress = 0;
				return false;
			} else {
				float underflow = currentPath.GetMagnitudeFromProgress(-pathProgress);
				currentPath = currentPath.previousPath;
				pathProgress = currentPath.GetProgressFromMagnitude(currentPath.Magnitude - underflow);
			}
		}

		// Move the object.
		PathUtil.SetXZ(transform, currentPath.GetPositionInPath(pathProgress));
		Vector3 sideCenter = transform.position + GetRotatedSideOffset(forward);
		if (Physics.Raycast(sideCenter, Vector3.down, out hit, groundOffset, collisionLayers)) {
			PathUtil.SetY(transform, hit.point.y + groundOffset);
		}
		// Rotate the object to face forward.
		if (!rotates) {
			FacePath(forward);
		}
		return true;
	}

	/// <summary>
	/// Rotates the entity to face the direction of the path.
	/// </summary>
	/// <param name="forward">Whether to face forward.</param>
	public void FacePath(bool forward) {
		Vector3 direction = currentPath.GetDirection(forward);
		float angle = Mathf.Atan2 (direction.x, direction.z) * 180 / Mathf.PI;
		Vector3 facing = Vector3.up * angle;
		transform.eulerAngles = facing;
	}
}
