using UnityEngine;

/// <summary>
/// Handles visibility settings.
/// </summary>
public class Visibility : MonoBehaviour {

	/// <summary> The mesh to toggle visibility with. </summary>
	private GameObject mesh;
	/// <summary> Whether or not the environment mesh is visible. </summary>
	private bool meshVisible;

	/// <summary> Timer to prevent input from occurring too fast. </summary>
	private float keyTimer;
	/// <summary> The time to wait before the visibility key can be pressed again. </summary>
	private float KEYCOOLDOWN = 0.5f;

	/// <summary> Possible visibility settings. </summary>
	private enum Settings {Mesh = 1, Collider, Both};
	/// <summary> The current visibility setting. </summary>
	private static Settings currentSetting = Settings.Both;

	/// <summary>
	/// Checks for input toggling visibility settings.
	/// </summary>
	private void Update() {
		keyTimer -= Time.deltaTime;
		if (keyTimer < 0 && Input.GetKey(KeyCode.Tab)) {
			keyTimer = KEYCOOLDOWN;
			ChangeSetting();
			ApplySetting();
		}
	}

	/// <summary>
	/// Puts the visibility setting into effect.
	/// </summary>
	public void ApplySetting() {
		meshVisible = ((int)currentSetting & 1) > 0;
		Tracker.Instance.logAction("Visibility toggled: " + meshVisible);
		foreach (Transform platform in transform.FindChild("Platforms")) {
			if (platform.name == "Collider") {
				platform.GetComponent<Renderer>().enabled = ((int)currentSetting & 2) > 0;
			}
		}
		if (mesh == null) {
			mesh = GetComponent<MeshChooser>().GetActiveMesh();
		}
		if (mesh != null) {
			mesh.SetActive(meshVisible);
		}
	}

	/// <summary>
	/// Changes the visibility setting to the next one.
	/// </summary>
	private void ChangeSetting() {
		int newSetting = (int)currentSetting + 1;
		if (newSetting > 3) {
			newSetting = 1;
		}
		currentSetting = (Settings)newSetting;
	}
}
