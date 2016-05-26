using UnityEngine;

/// <summary>
/// The goal that the player needs to reach.
/// </summary>
public class Goal : MonoBehaviour {

	/// <summary> The particles emitted from the goal. </summary>
	private GameObject particles;

	/// <summary>
	/// Initializes the goal particle system.
	/// </summary>
	private void Start() {
		particles = transform.FindChild("Particles").gameObject;
	}

	/// <summary>
	/// Causes the goal to emit particles after .
	/// </summary>
	public void Win() {
		particles.SetActive(true);
	}

	/// <summary>
	/// Disables the goal particles.
	/// </summary>
	public void Reset() {
		particles.SetActive(false);
	}
}
