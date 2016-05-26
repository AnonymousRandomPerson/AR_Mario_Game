using UnityEngine;

/// <summary>
/// Controls the position of the first-person camera.
/// </summary>
public class FirstPersonCamera : MonoBehaviour, CameraOption {

	/// <summary> The player represented by the camera. </summary>
	private Player player;

	/// <summary> The speed at which the camera rotates when the player turns. </summary>
	[SerializeField]
	[Tooltip("The speed at which the camera rotates when the player turns.")]
	private float rotateSpeed;
	/// <summary> The limit to the horizontal rotation angle. </summary>
	[SerializeField]
	[Tooltip("The limit to the horizontal rotation angle.")]
	private float rotateHorizontalLimit;
	/// <summary> The limit to the vertical rotation angle. </summary>
	[SerializeField]
	[Tooltip("The limit to the vertical rotation angle")]
	private float rotateVerticalLimit;
	/// <summary> The offset to the rotation of the camera. </summary>
	private Vector3 rotateOffset = new Vector3();
	/// <summary> The offset of the player's head to its center. </summary>
	private Vector3 headOffset = new Vector3(0, 0.0625f, 0);

	/// <summary>
	/// Sets the player that the camera is following.
	/// </summary>
	/// <param name="player">The player that the camera is following.</param>
	public void SetPlayer(Player player) {
		this.player = player;
		transform.position = player.transform.position + headOffset;
		transform.rotation = player.transform.rotation;
	}
	
	/// <summary>
	/// Moves the camera according to the player's position and rotation.
	/// </summary>
	private void Update () {
		if (player != null) {
			// Keyboard/mouse input to change look rotation.
			float x = Input.GetAxis("Mouse X") * 3;
			float y = Input.GetAxis("Mouse Y") * -3;
			if (Input.GetKey("l")) {
				x -= 1;
			}
			if (Input.GetKey("j")) {
				x += 1;
			}
			if (Input.GetKey("i")) {
				y -= 1;
			}
			if (Input.GetKey("k")) {
				y += 1;
			}

			rotateOffset.x += x;
			rotateOffset.y += y;
			rotateOffset.x = Mathf.Clamp(rotateOffset.x, -rotateHorizontalLimit, rotateHorizontalLimit);
			rotateOffset.y = Mathf.Clamp(rotateOffset.y, -rotateVerticalLimit, rotateVerticalLimit);

			// Change angle according to player and rotation offset.
			transform.position = player.transform.position + headOffset;
			Vector3 targetRotation = player.transform.rotation.eulerAngles;
			targetRotation.x += rotateOffset.y;
			targetRotation.y += rotateOffset.x;
			Quaternion targetQuaternion = Quaternion.Euler(targetRotation);
			transform.rotation = Quaternion.RotateTowards(transform.rotation, targetQuaternion, rotateSpeed);
		}
	}
}
