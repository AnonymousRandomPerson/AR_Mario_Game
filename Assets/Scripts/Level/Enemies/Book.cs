using UnityEngine;

/// <summary>
/// Attempts to float above and squash the player.
/// </summary>
public class Book : EnemyMovement {

	/// <summary> The initial position of the book. </summary>
	private Vector3 startPosition;

	/// <summary> The player the book is targeting. </summary>
	private Player target;
	/// <summary> The book's rigidbody. </summary>
	private Rigidbody body;

	/// <summary> Whether the book is currently chasing the player. </summary>
	private bool targetingPlayer = false;
	/// <summary> The current movement stage of the book. </summary>
	private Stage stage = Stage.Idle;

	/// <summary> The height of the book when on the ground. </summary>
	private float groundedHeight;
	/// <summary> The height that the book will rise to. </summary>
	private float targetHeight;
	/// <summary> The position that the book will move towards. </summary>
	private Vector3 targetPosition;
	/// <summary> The delay after the book falls before it begins moving again. </summary>
	private int fallDelay = 0;

	/// <summary>
	/// Stages in the book's movement.
	/// </summary>
	private enum Stage {Idle, Rise, Chase, Fall};

	/// <summary>
	/// Initializes the book.
	/// </summary>
	new private void Start() {
		base.Start();
		startPosition = transform.position;
		body = GetComponent<Rigidbody>();
	}
	
	// Update is called once per frame.
	protected override void Move() {
		if (stage == Stage.Idle && targetingPlayer) {
			stage = Stage.Rise;
			groundedHeight = transform.position.y;
			body.useGravity = false;
			targetHeight = target.GetComponent<Collider>().bounds.extents.y * 4;
		} else if (stage == Stage.Rise) {
			PathUtil.SetY(transform, transform.position.y + 0.005f);
			if (transform.position.y - groundedHeight > targetHeight) {
				targetPosition = target.transform.position;
				stage = Stage.Chase;
			}
		} else if (stage == Stage.Chase) {
			PathUtil.MoveTowardsXZ(transform, targetPosition, 0.005f);
			if (PathUtil.GetMagnitudeXZ(transform.position - targetPosition) < Mathf.Epsilon) {
				stage = Stage.Fall;
			}
		} else if (stage == Stage.Fall) {
			body.useGravity = true;
			if (Mathf.Abs(body.velocity.y) < 0.01f && fallDelay++ > 20) {
				fallDelay = 0;
				body.useGravity = false;
				stage = Stage.Idle;
			}
		}
	}

	/// <summary>
	/// Causes the book to give up chase if something is in the way.
	/// </summary>
	/// <param name="collision">The collision that the book was involved in.</param>
	private void OnCollisionEnter(Collision collision) {
		if (stage == Stage.Rise || stage == Stage.Chase) {
			stage = Stage.Fall;
		}
	}

	/// <summary>
	/// Checks if the player is within targeting range.
	/// </summary>
	/// <param name="collider">The collider that the book collided with.</param>
	private void OnTriggerEnter(Collider collider) {
		if (collider.tag == "Player") {
			target = collider.GetComponent<Player> ();
			targetingPlayer = true;
		}
	}

	/// <summary>
	/// Checks if the player has stepped outside targeting range.
	/// </summary>
	/// <param name="collider">The collider that the book stopped collided with.</param>
	private void OnTriggerExit(Collider collider) {
		if (collider.tag == "Player") {
			targetingPlayer = false;
		}
	}

	/// <summary>
	/// Resets the book.
	/// </summary>
	public override void Reset() {
		transform.position = startPosition;
		transform.rotation = Quaternion.identity;
		stage = Stage.Idle;
		targetingPlayer = false;
		fallDelay = 0;
	}
}
