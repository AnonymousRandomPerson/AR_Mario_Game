using UnityEngine;

/// <summary>
/// An entity that tries to kill the player.
/// </summary>
public class Enemy : MonoBehaviour {

	/// <summary> Whether the enemy can be destroyed. </summary>
	[SerializeField]
	[Tooltip("Whether the enemy can be destroyed.")]
	private bool invincible = false;

	/// <summary> Whether the enemy is dead. </summary>
	public bool dead;
	/// <summary> Timer for running the enemy's death animation. </summary>
	private float deadTimer;
	/// <summary> The time taken for the enemy's death animation to play. </summary>
	private const float DEATHTIME = 0.5f;

	/// <summary> Renderer for displaying the enemy. </summary>
	private Renderer[] renderers;
	/// <summary> Colliders on the enemy. </summary>
	private Collider[] colliders;

	private void Start() {
		renderers = GetComponentsInChildren<MeshRenderer>();
		colliders = GetComponentsInChildren<Collider>();
	}

	/// <summary>
	/// Checks if the enemy is touching the floor, and kills it if it is.
	/// </summary>
	private void Update() {
		if (dead) {
			deadTimer -= Time.deltaTime;
			if (deadTimer <= 0) {
				gameObject.SetActive(false);
			} else {
				Color meshColor;
				foreach (MeshRenderer renderer in renderers) {
					meshColor = renderer.material.color;
					meshColor.a = deadTimer / DEATHTIME;
					renderer.material.color = meshColor;
				}
			}
		}
		if (PathUtil.OnFloor(gameObject)) {
			KillEntity();
		}
	}

	/// <summary>
	/// Resets the position and direction of the enemy.
	/// </summary>
	public void Reset() {
		gameObject.SetActive(true);
		dead = false;
		Movement movement = GetComponent(typeof(Movement)) as Movement;
		if (movement != null) {
			movement.Reset();
		}
		Color meshColor;
		foreach (MeshRenderer renderer in renderers) {
			meshColor = renderer.material.color;
			meshColor.a = 1;
			renderer.material.color = meshColor;
		}
		foreach (Collider collider in colliders) {
			collider.enabled = true;
		}
	}

	/// <summary>
	/// Kills the enemy.
	/// </summary>
	public void KillEntity() {
		if (!invincible) {
			Tracker.Instance.logAction("Enemy killed: " + this.name.Replace("(Clone)", ""));
			dead = true;
			deadTimer = DEATHTIME;
			foreach (Collider collider in colliders) {
				collider.enabled = false;
			}
		}
	}
}
