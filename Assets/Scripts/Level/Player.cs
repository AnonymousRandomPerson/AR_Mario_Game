using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using System.Collections.Generic;

/// <summary>
/// Responds to keypresses to control the character.
/// </summary>
public class Player : MonoBehaviour {

	/// <summary> Controls the player's movement along the ribbon path. </summary>
	private PathMovement pathMovement;
	/// <summary> The rigid body controlling the object's physics. </summary>
	private Rigidbody body;
	/// <summary> Renderer for displaying the player. </summary>
	private Renderer[] playerRenderers;
	
	/// <summary> The normal walking speed of the player. </summary>
	[Tooltip("The normal walking speed of the player.")]
	public float baseMoveSpeed = 0.01f;
	/// <summary> The maximum running speed of the player. </summary>
	[Tooltip("The maximum running speed of the player.")]
	public float runSpeed;
	/// <summary> The amount that speed is incremented every tick when acclerating to run speed. </summary>
	private float speedIncrement;
	/// <summary> Whether the player can move around. </summary>
	[HideInInspector]
	public bool canMove;

	/// <summary> The initial velocity of the player's jump. </summary>
	[Tooltip("The initial velocity of the player's jump.")]
	public float jumpSpeed = 2.5f;
	/// <summary> Threshold for limiting jump height. </summary>
	[Tooltip("Threshold for limiting jump height.")]
	public int baseMaxJumpTimer = 6;
	/// <summary> Timer for varying the player's jump height. </summary>
	private int jumpHeightTimer = 0;
	/// <summary> Used to stop the jump animation if the player's y velocity stays at 0. </summary>
	private bool jumpAnimationCutoff;

	/// <summary> The amount of frames that the player will be invincible after being hit. </summary>
	private const int INVINCIBLEDELAY = 120;
	/// <summary> Timer for the player being invincible. </summary>
	private int invincibleTimer = 0;

	/// <summary> Ticks after the player has reached the goal. </summary>
	private int goalTick;
	/// <summary> The number of ticks to wait before resetting the level after winning. </summary>
	private const int MAXGOALTICKS = 60;

	/// <summary> Whether the player is dead and in a dying animation. </summary>
	private bool dead = false;

	/// <summary> The current score of the player. </summary>
	[Tooltip("The current score of the player.")]
	public int score = 0;
	/// <summary> The current power-ups of the player. </summary>
	private List<Power> powers = new List<Power> ();
	/// <summary> The number of power-ups the player has lost in this tick. </summary>
	private int lostPowers = 0;
	/// <summary> The current scale of the player. </summary>
	private int size = 1;

	/// <summary> Timer for posting the player's position to the server. </summary>
	private int postTimer = 0;
	/// <summary> Time between posting the player's position to the server. </summary>
	[Tooltip("Time between posting the player's position to the server.")]
	public int postInterval = 10;

	/// <summary> The non-trigger collider on the player. </summary>
	private Collider physicsCollider;

	/// <summary> The game state logger. </summary>
	private Tracker tracker;

	/// <summary> The animator controlling the player model's animations. </summary>
	private Animator animator;

	/// <summary> Whether the player is pressing the forward key. </summary>
	private bool forward;
	/// <summary> Whether the player is pressing the backwards key. </summary>
	private bool backward;
	/// <summary> Whether the player is pressing the jump key. </summary>
	private bool jump;
	/// <summary> Whether the player is pressing the reset key. </summary>
	private bool reset;
	/// <summary> Whether the player is pressing the run key. </summary>
	private bool run;
	/// <summary> Whether the player is pressing the power-up key. </summary>
	private bool powerKey;

	/// <summary> The height threshold for stomping on enemies. </summary>
	[Tooltip("The height threshold for stomping on enemies.")]
	public Transform stomp;

	/// <summary>
	/// Initializes the player.
	/// </summary>
	private void Start() {
		pathMovement = GetComponent<PathMovement>();
		body = GetComponent<Rigidbody>();
		playerRenderers = transform.FindChild("Model").GetComponentsInChildren<MeshRenderer>();

		canMove = true;

		pathMovement.moveSpeed = baseMoveSpeed;
		if (runSpeed < baseMoveSpeed) {
			runSpeed = baseMoveSpeed * 1.5f;
		}
		speedIncrement = (runSpeed - baseMoveSpeed) / 30f;
		pathMovement.ResetPosition();
		// Get the non-trigger collider on the player.
		foreach (Collider collider in GetComponents<Collider>()) {
			if (!collider.isTrigger) {
				physicsCollider = collider;
				break;
			}
		}

		Cursor.lockState = CursorLockMode.Locked;
		Cursor.visible = false;

		tracker = Tracker.Instance;
		animator = GetComponentInChildren<Animator>();
	}

	/// <summary>
	/// Gets player input.
	/// </summary>
	private void Update() {
		forward = GetKey(KeyCode.RightArrow, KeyCode.D);
		backward = GetKey(KeyCode.LeftArrow, KeyCode.A);
		jump = GetKey(KeyCode.Space, KeyCode.UpArrow, KeyCode.W);
		reset = GetKey(KeyCode.R);
		run = GetKey(KeyCode.LeftControl, KeyCode.RightControl);
		powerKey = GetKey(KeyCode.Return);

		if (reset) {
			if (GetKey(KeyCode.LeftShift)) {
				SceneManager.LoadScene(SceneManager.GetActiveScene().name);
			} else {
				LevelManager.Instance.ResetLevel();
			}
		}
	}

	/// <summary>
	/// Moves the player according to input.
	/// </summary>
    private void FixedUpdate() {
		if (GameMenuUI.paused) {
			return;
		}
		if (++postTimer > postInterval) {
			postTimer = 0;
			NetworkingManager.instance.PostPositionInURL(transform.position);
		}
		if (goalTick > 0) {
			// Wait for the win animation before resetting the level.
			if (goalTick > MAXGOALTICKS) {
				goalTick = 0;
				SceneManager.LoadScene(SceneManager.GetActiveScene().name);
			} else {
				Color playerColor = new Color();
				foreach (Renderer playerRenderer in playerRenderers) {
					playerColor = playerRenderer.material.color;
					playerColor.a = (MAXGOALTICKS - goalTick) / (float)MAXGOALTICKS;
					playerRenderer.material.color = playerColor;
				}
				goalTick++;
			}
		} else if (dead) {
			Color playerColor = new Color();
			foreach (Renderer playerRenderer in playerRenderers) {
				playerColor = playerRenderer.material.color;
				playerColor.a -= 0.05f;
				playerRenderer.material.color = playerColor;
			}
			body.constraints = body.constraints | RigidbodyConstraints.FreezePositionY;
			if (playerColor.a < Mathf.Epsilon) {
				LevelManager.Instance.ResetLevel();
			}
        } else {
			bool moving = false;
			if (canMove) {
				if (forward ^ backward) {
					moving = pathMovement.MoveAlongPath (forward);
					LevelManager.Instance.pathRendererList.CheckList(this, forward);
				}

				UpdateRun(run && (forward ^ backward));

                Jump(jump);
			}
			animator.SetBool("moving", moving);
			animator.SetBool("running", run && moving);

			UpdateInvincible();

			// Terminal velocity
			if (-body.velocity.y > jumpSpeed) {
                body.velocity = PathUtil.SetY(body.velocity, -jumpSpeed);
			}

			lostPowers = 0;
			if (powerKey) {
				for (int i = 0; i < powers.Count; i++) {
					Power power = powers[i];
					power.PowerKey();
					if (lostPowers > 0) {
						i -= lostPowers;
						lostPowers = 0;
					}
				}
			}

			if (PathUtil.OnFloor(gameObject)) {
				KillPlayer();
            }
		}
	}

	/// <summary>
	/// Checks if any of the given keys are pressed.
	/// </summary>
	/// <returns>Whether any of the given keys are pressed.</returns>
	/// <param name="keys">The keys to check for being pressed.</param>
	private bool GetKey(params KeyCode[] keys) {
		foreach (KeyCode key in keys) {
			if (Input.GetKey(key)) {
				return true;
			}
		}
		return false;
	}

	/// <summary>
	/// Checks when the player collides with certain objects.
	/// </summary>
	/// <param name="collision">The collision that the player was involved in.</param>
	private void OnCollisionEnter(Collision collision) {
		if (collision.collider.tag == "Finish") {
			HitGoal();
		}
	}

	/// <summary>
	/// Handles the player jumping.
	/// </summary>
	/// <param name="isJumping">Whether the jump key is pressed.</param>
	private void Jump(bool isJumping) {
		// Varies the player's jump height based on movement speed and held jump button.
        bool onGround = Physics.Raycast(transform.position, Vector3.down, physicsCollider.bounds.extents.y * 1.1f, 1 << 9);
		if (isJumping && jumpHeightTimer < baseMaxJumpTimer * Mathf.Min(1.5f, pathMovement.moveSpeed / baseMoveSpeed)) {
			bool incremented = false;
            if (onGround) {
				jumpHeightTimer++;
				incremented = true;
				tracker.logAction("Jumped.");
				animator.SetBool("jumping", true);
                transform.position += Vector3.up * physicsCollider.bounds.extents.y / 10f;
            }
			if (jumpHeightTimer > 0) {
                body.velocity = PathUtil.SetY(body.velocity, jumpSpeed);
				if (!incremented) {
					jumpHeightTimer++;
				}
			} else {
				jumpHeightTimer = 0;
			}
		} else {
			jumpHeightTimer = 0;
		}
        if (Mathf.Abs(body.velocity.y) < 1 && onGround) {
			if (jumpAnimationCutoff) {
				animator.SetBool("jumping", false);
			} else {
				jumpAnimationCutoff = true;
			}
		}
	}

	/// <summary>
	/// Speeds up the player if running. Reverts back to normal speed if not.
	/// </summary>
	/// <param name="isRunning">Whether the run key is pressed.</param>
	private void UpdateRun(bool isRunning) {
		if (isRunning) {
			if (pathMovement.moveSpeed < runSpeed) {
				pathMovement.moveSpeed += speedIncrement;
			}
		} else {
			if (pathMovement.moveSpeed > baseMoveSpeed) {
				pathMovement.moveSpeed -= speedIncrement;
			}
		}
	}

	/// <summary>
	/// Handles invincibility frames after being hit.
	/// </summary>
	private void UpdateInvincible() {
		if (invincibleTimer > 0) {
			invincibleTimer--;
			bool rendererEnabled;
			if (invincibleTimer > INVINCIBLEDELAY / 2) {
				rendererEnabled = invincibleTimer / 20 % 2 == 1;
			} else {
				rendererEnabled = invincibleTimer / 10 % 2 == 0;
			}
			foreach (Renderer playerRenderer in playerRenderers) {
				playerRenderer.enabled = rendererEnabled;
			}
		}
	}

	/// <summary>
	/// Causes the player to bounce after stomping on an enemy.
	/// Also increases the player's score.
	/// </summary>
	public void StompEnemy() {
		Vector3 setVelocity = new Vector3(body.velocity.x, jumpSpeed, body.velocity.z);
		body.velocity = setVelocity;
		jumpHeightTimer++;
		score += 100;
	}

	/// <summary>
	/// Plays an animation upon reaching the goal.
	/// </summary>
	private void HitGoal() {
		tracker.logAction("Won level.");
		goalTick = 1;
		LevelManager.Instance.goal.Win();
		animator.SetBool("won", true);
	}

	/// <summary>
	/// Sets the size of the player.
	/// </summary>
	/// <param name="newSize">The new size of the player.</param>
	public void SetSize(int newSize) {
		float ratio = (float)newSize / (float)size;
		PathUtil.ScaleY(transform, ratio);
		// Adjust the position of the player so that the lower bound is still at the same height.
		PathUtil.SetY(transform, transform.position.y + physicsCollider.bounds.extents.y * Mathf.Log(ratio, 2) * 1.01f);
		size = newSize;
		pathMovement.UpdateGroundOffset();
	}

	/// <summary>
	/// Reduces the player's size. Kills the player if the size is at a minimum.
	/// </summary>
	public void TakeDamage() {
		if (invincibleTimer == 0) {
			if (powers.Count == 0) {
				KillPlayer();
			} else {
				tracker.logAction("Damaged player.");
				int removeIndex = RandomUtil.RandomInt(0, powers.Count);
				LosePower(powers[removeIndex]);
				invincibleTimer = INVINCIBLEDELAY;
			}
		}
	}

	/// <summary>
	/// Kills the player and resets the level.
	/// </summary>
	public void KillPlayer() {
		tracker.logAction("Killed player");
		dead = true;
		animator.speed = 0;
        LevelManager.numDeaths++;
	}

	/// <summary>
	/// Adds a power-up to the player.
	/// </summary>
	/// <param name="newPower">The power-up to add to the player.</param>
	public void AddPower(Power newPower) {
		tracker.logAction(newPower.ToString() + " power added.");
		powers.Add(newPower);
	}

	/// <summary>
	/// Removes a power from the player.
	/// </summary>
	/// <param name="power">The power to remove from the player.</param>
	public void LosePower(Power power) {
		if (power != null) {
			power.OnRemove();
			tracker.logAction(power.ToString() + " power removed.");
			powers.Remove(power);
			lostPowers++;
			Destroy((MonoBehaviour)power);
		}
	}

	/// <summary>
	/// Returns true if the player has won the level.
	/// </summary>
	/// <returns>True if the player has won the level.</returns>
	private bool HasWon() {
		return goalTick > 0;
	}

	/// <summary>
	/// Gets the current path that the player is on.
	/// </summary>
	/// <returns>The current path that the player is on.</returns>
	public PathComponent GetCurrentPath() {
		return pathMovement.currentPath;
	}

	/// <summary>
	/// Resets the player when the level restarts.
	/// </summary>
	public void Reset() {
		pathMovement.ResetPosition();
		body.useGravity = true;
		body.velocity = Vector3.zero;
		body.constraints = body.constraints & ~RigidbodyConstraints.FreezePositionY;
		score = 0;
		jumpHeightTimer = 0;
		foreach (Power power in powers) {
			power.OnReset();
			Destroy((MonoBehaviour)power);
		}
		powers.Clear();
		invincibleTimer = 0;
		foreach (Renderer playerRenderer in playerRenderers) {
			playerRenderer.enabled = true;
			Color playerColor = playerRenderer.material.color;
			playerColor.a = 1;
			playerRenderer.material.color = playerColor;
		}
		canMove = true;
		dead = false;
		physicsCollider.enabled = true;
		ResetAnimation();
	}

	/// <summary>
	/// Resets the player's animation to an idle state.
	/// </summary>
	private void ResetAnimation() {
		animator.SetBool("moving", false);
		animator.SetBool("running", false);
		animator.SetBool("jumping", false);
		animator.SetBool("won", false);
		animator.SetTrigger("reset");
		animator.speed = 1;
	}
}
