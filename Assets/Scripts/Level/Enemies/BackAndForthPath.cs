﻿using UnityEngine;

/// <summary>
/// Moves along the path until hitting a barrier, then turns around.
/// </summary>
public class BackAndForthPath : MonoBehaviour, Movement {

	/// <summary> Controls the entity's movement along the ribbon path. </summary>
	private PathMovement pathMovement;
	/// <summary> Whether the entity is moving forwards. </summary>
	private bool forward;
	/// <summary> The direction the entity is moving at the start of the level. </summary>
	private bool startDirection;

	/// <summary>
	/// Initializes the movement.
	/// </summary>
	private void Start() {
		startDirection = RandomUtil.RandomBoolean();
		forward = startDirection;
		pathMovement = GetComponent<PathMovement>();
	}

	/// <summary>
	/// Moves the object along the path.
	/// </summary>
	private void Update() {
		if (GameMenuUI.paused) {
			return;
		}
		if (!pathMovement.MoveAlongPath(forward)) {
			forward = !forward;
		}
	}

	/// <summary>
	/// Resets the entity's direction and position.
	/// </summary>
	public void Reset() {
		forward = startDirection;
		pathMovement.ResetPosition();
	}
}
