using UnityEngine;

/// <summary>
/// Rotates the object every frame.
/// </summary>
public class Rotate : MonoBehaviour {

	// The speed that the object is rotated at.
	[SerializeField]
	[Tooltip("The speed that the object is rotated at.")]
	private float rotateSpeed = 1;

	/// <summary>
	/// Rotates the object.
	/// </summary>
	private void FixedUpdate () {
		if (GameMenuUI.paused) {
			return;
		}
		transform.Rotate(new Vector3(0f, rotateSpeed, 0f));
	}
}
