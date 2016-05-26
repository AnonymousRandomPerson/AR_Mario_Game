/// <summary>
/// A type of camera to be used in the scene.
/// </summary>
public interface CameraOption {

	/// <summary>
	/// Sets the player that the camera is following.
	/// </summary>
	/// <param name="player">The player that the camera is following.</param>
	void SetPlayer(Player player);
}
