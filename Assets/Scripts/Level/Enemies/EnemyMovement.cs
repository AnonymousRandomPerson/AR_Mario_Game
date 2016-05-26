using UnityEngine;

/// <summary>
/// Movement of enemies.
/// </summary>
public abstract class EnemyMovement : MonoBehaviour, Movement {

	/// <summary> The enemy following this behavior. </summary>
	private Enemy enemy;

	/// <summary>
	/// Registers the enemy following this behavior.
	/// </summary>
	protected void Start() {
		enemy = GetComponent<Enemy>();
	}

	/// <summary>
	/// Moves the enemy if it is alive.
	/// </summary>
	private void FixedUpdate() {
		if (!GameMenuUI.paused && !enemy.dead) {
			Move();
		}
	}

	/// <summary>
	/// Moves the enemy every tick.
	/// </summary>
	protected abstract void Move();

	/// <summary>
	/// Resets the movement of the entity.
	/// </summary>
	public abstract void Reset();
}
