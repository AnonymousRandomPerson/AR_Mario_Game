using UnityEngine;

/// <summary>
/// Kills the player instantly (regardless of power-up) upon hitting a trigger.
/// </summary>
public class OHKO : MonoBehaviour {

	/// <summary>
	/// Kills the player instantly upon collision.
	/// </summary>
	/// <param name="collider">The collider that collided with the object.</param>
	private void OnTriggerEnter(Collider collider) {
		if (collider.tag == "Player" && !collider.isTrigger) {
			collider.gameObject.GetComponent<Player>().KillPlayer();
		}
	}
}
