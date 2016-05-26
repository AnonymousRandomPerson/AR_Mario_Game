using UnityEngine;

/// <summary>
/// Camera fixed at the origin. Zooms into the player.
/// </summary>
public class FixedCamera : MonoBehaviour, CameraOption {

	/// <summary> The player represented by the camera. </summary>
	private Player player;
	/// <summary> The distance from the camera to the player. </summary>
	[SerializeField]
	[Tooltip("The distance from the camera to the player.")]
	private float playerDistanceSetting;
	/// <summary> The maximum distance the player has been away from the camera. </summary>
	private float maxDistance;
	/// <summary> The speed at which the camera can be zoomed per tick. </summary>
	private const float ZOOMSPEED = 0.02f;

	/// <summary>
	/// Initializes the maximum distance.
	/// </summary>
	private void Start() {
		maxDistance = playerDistanceSetting;
	}
	
	/// <summary>
	/// Updates the camera location.
	/// </summary>
	private void FixedUpdate() {
		if (player != null && player.transform.position != Vector3.zero) {
			transform.LookAt(player.transform.position);
			float playerDistance = Vector3.Magnitude(player.transform.position);
			float currentDistance = Mathf.Min(playerDistanceSetting, playerDistance);
			transform.position = player.transform.position - (player.transform.position * currentDistance / playerDistance);
			float distanceIncrement = Input.GetAxis("Mouse ScrollWheel") * ZOOMSPEED;
			if (Input.GetKey(KeyCode.I)) {
				distanceIncrement = -0.02f;
			} else if (Input.GetKey(KeyCode.K)) {
				distanceIncrement = 0.02f;
			}
			if (distanceIncrement < 0) {
				playerDistanceSetting = Mathf.Min(currentDistance, Mathf.Max(0.05f, playerDistanceSetting + distanceIncrement));
			} else {
				playerDistanceSetting += distanceIncrement;
			}
			maxDistance = Mathf.Max(maxDistance, playerDistance);
			playerDistanceSetting = Mathf.Min(playerDistanceSetting, maxDistance);
		}
	}

	/// <summary>
	/// Sets the player that the camera is following.
	/// </summary>
	/// <param name="player">The player that the camera is following.</param>
	public void SetPlayer(Player player) {
		this.player = player;
	}
}
