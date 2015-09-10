﻿using UnityEngine;
using System;

// Controls movement along the ribbon path.
public class PathMovement : MonoBehaviour {

	// The part of the path where the object begins the level on.
	public PathComponent startPath;
	// Where the object begins the level at.
	Vector3 startPosition;
	// Whether the object has been placed on the ribbon path.
	bool initiated = false;

	// The part of the level that the object is currently on. 
	public PathComponent currentPath;
	// How far along the path the object is.
	public float pathProgress = 0;

	// The speed that the object moves at.
	public float moveSpeed = 0.01f;
	// The initial velocity of the object's jump.
	public float jumpSpeed = 3;
	// The rigid body controlling the object's physics.
	Rigidbody body;
	// The distance from the center of the object to the ground while grounded.
	float groundOffset;
	// The distance from the center of the object to its side.
	float sideOffset;
	// The number of places on the object to check for side collisions.
	const int NUMSIDECHECKS = 30;
	
	// Use this for initialization.
	void Start () {
		body = GetComponent<Rigidbody> ();
		if (startPath != null) {
			ResetPosition ();
		}
		groundOffset = GetComponent<Collider> ().bounds.extents.y;
		sideOffset = GetComponent<Collider> ().bounds.extents.x;
	}

	// Update is called once per frame.
	void Update () {
		if (!initiated && startPath != null) {
			ResetPosition ();
		}
	}

	// Resets the position of the object to its initial position.
	public void ResetPosition () {
		currentPath = startPath;
		pathProgress = 0;
		body.velocity = Vector3.zero;

		// Determine the starting position and save it in case the player resets.
		if (!initiated) {
			RaycastHit hit;
			startPosition = startPath.GetPositionInPath (pathProgress);
			startPosition.y = PathUtil.ceilingHeight;
			Physics.Raycast (startPosition, Vector3.down, out hit, PathUtil.ceilingHeight * 1.1f);
			startPosition.y = hit.point.y + groundOffset + 0.25f;
			initiated = true;
		}
		transform.position = startPosition;
	}

	// Moves the object along the ribbon path.
	// Returns whether the object was able to move.
	public bool MoveAlongPath (bool forward) {
		// Check for side collision.
		Vector3 sidePosition = transform.position + Vector3.down * groundOffset * .99f;
		float sideIncrement = groundOffset * 2 / NUMSIDECHECKS;
		for (int i = 0; i < NUMSIDECHECKS; i++) {
			if (Physics.Raycast (sidePosition, currentPath.GetDirection (forward), sideOffset * 1.01f)) {
				return false;
			}
			sidePosition.y += sideIncrement;
		}

		pathProgress = currentPath.IncrementPathProgress (pathProgress, moveSpeed, forward);

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
				pathProgress = currentPath.GetProgressFromMagnitude(currentPath.GetMagnitude() - underflow);
			}
		}

		// Move the object.
		PathUtil.SetXZ (transform, currentPath.GetPositionInPath (pathProgress));
		return true;
	}

	// Makes the object jump if it is grounded.
	public void jump () {
		if ((Physics.Raycast (transform.position + Vector3.right * sideOffset, Vector3.down, groundOffset + 0.001f) ||
		     Physics.Raycast (transform.position + Vector3.left * sideOffset, Vector3.down, groundOffset + 0.001f) ||
		     Physics.Raycast (transform.position + Vector3.back * sideOffset, Vector3.down, groundOffset + 0.001f) ||
		     Physics.Raycast (transform.position + Vector3.forward * sideOffset, Vector3.down, groundOffset + 0.001f) ||
		     Physics.Raycast (transform.position, Vector3.down, groundOffset + 0.001f)) &&
		    Math.Abs (body.velocity.y) < 0.001) {
			body.velocity += Vector3.up * jumpSpeed;
		}
	}
}
