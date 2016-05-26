using UnityEngine;

/// <summary>
/// Moves like BackAndForthPath, but takes a long time to make turns.
/// </summary>
public class Pencil : EnemyMovement {
	
	/// <summary> Controls the entity's movement along the ribbon path. </summary>
	private PathMovement pathMovement;
	/// <summary> Whether the entity is moving forwards. </summary>
	private bool forward;
	/// <summary> The direction the entity is moving at the start of the level. </summary>
	private bool startDirection;

	/// <summary> The rotation of the pencil in the previous frame. </summary>
	private float prevRotation = -1;
	/// <summary> The direction the pencil is turning. </summary>
	private int turningMode = 0;
	/// <summary> The rotation the pencil is turning towards. </summary>
	private float targetRotation;
	/// <summary> The initial rotation of the pencil when the level begins. </summary>
	private float startRotation;

	/// <summary> The speed at which the pencil rotates. </summary>
	[SerializeField]
	[Tooltip("The speed at which the pencil rotates.")]
	private float turnSpeed = 3;

	/// <summary>
	/// Initializes the pencil.
	/// </summary>
	new private void Start() {
		base.Start();
		startDirection = RandomUtil.RandomBoolean();
		forward = startDirection;
		pathMovement = GetComponent<PathMovement>();
	}

	/// <summary>
	/// Moves the pencil.
	/// </summary>
	protected override void Move() {
		if (turningMode == 0) {
			if (!pathMovement.MoveAlongPath(forward)) {
				forward = !forward;
			}
			float difference = gameObject.transform.eulerAngles.y - prevRotation;
			if (Mathf.Abs(difference) > 0.05f) {
				if (prevRotation == -1) {
					prevRotation = startRotation = gameObject.transform.eulerAngles.y;
				} else {
					if (difference < 0) {
						difference += 360;
					}
					turningMode = difference <= 180 ? 1 : -1;
					targetRotation = gameObject.transform.eulerAngles.y;
					gameObject.transform.eulerAngles = PathUtil.SetY(gameObject.transform.eulerAngles, prevRotation);
				}
			}
		} else {
			float newY = gameObject.transform.eulerAngles.y + turningMode * turnSpeed;
			if ((turningMode > 0 && newY >= targetRotation ||
			    turningMode < 0 && newY <= targetRotation) &&
			    Mathf.Abs((newY - targetRotation) % 360) <= turnSpeed) {
				turningMode = 0;
				newY = targetRotation;
				prevRotation = newY;
			}
			gameObject.transform.eulerAngles = PathUtil.SetY(gameObject.transform.eulerAngles, newY);
		}
	}

	/// <summary>
	/// Resets the entity's direction and position.
	/// </summary>
	public override void Reset() {
		forward = startDirection;
		pathMovement.ResetPosition();
		gameObject.transform.eulerAngles = PathUtil.SetY(gameObject.transform.eulerAngles, startRotation);
		prevRotation = startRotation;
		turningMode = 0;
	}
}
