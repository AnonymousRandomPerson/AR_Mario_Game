using UnityEngine;

/// <summary>
/// An enemy that will kill the player on contact.
/// Landing on it from above will kill it instead.
/// </summary>
public class Stompable : MonoBehaviour {

	/// <summary> The height threshold for killing the enemy. </summary>
	[SerializeField]
	[Tooltip("The height threshold for killing the enemy.")]
	private Transform head;

	/// <summary>
	/// Fires when the enemy hits something.
	/// </summary>
	/// <param name="collision">The collision that the enemy was involved in.</param>
	private void OnCollisionEnter(Collision collision) {
		if (collision.collider.tag == "Player") {
			// Check if the player is high enough to flatten the enemy.
			Player player = collision.gameObject.GetComponent<Player>();
			if (player.stomp.position.y >= head.position.y) {
				player.StompEnemy();
				GetComponent<Enemy>().KillEntity();
			} else {
				// If not, the player gets damaged.
				player.TakeDamage();
			}
		}
	}
}
