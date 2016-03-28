﻿using UnityEngine;

/// <summary>
/// Handles switching the camera between first-person and third-person.
/// </summary>
public class CameraSetting : MonoBehaviour {

	/// <summary> The cameras to be instantiated when the game starts. </summary>
	[SerializeField]
	[Tooltip("The cameras to be instantiated when the game starts.")]
	private GameObject[] cameraPrefabs;
	/// <summary> The cameras in the scene. </summary>
	private Camera[] cameras;

	/// <summary> The index of the currently selected camera. </summary>
	private int cameraIndex = 0;
	/// <summary> Timer to prevent input from occurring too fast. </summary>
	private int keyTimer;

	/// <summary>
	/// Initializes cameras in the scene.
	/// </summary>
	/// <param name="player">The player in the scene.</param>
	public void InitializeCameras(Player player) {
		cameras = new Camera[cameraPrefabs.Length];

		for (int i = 0; i < cameras.Length; i++) {
			GameObject cameraObject = GameObject.Instantiate(cameraPrefabs[i]);
			CameraOption option = cameraObject.GetComponent<CameraOption>();
			if (option != null) {
				option.SetPlayer(player);
			}
			cameras[i] = cameraObject.GetComponentInChildren<Camera>();
			cameras[i].gameObject.SetActive(i == 0);
		}
	}

	/// <summary>
	/// Watches for camera change input.
	/// </summary>
	private void Update() {
		if (cameras != null && --keyTimer < 0 && Input.GetKey(KeyCode.C)) {
			keyTimer = 2;
			SetCameras();
		}
	}

	/// <summary>
	/// Sets which camera is active.
	/// </summary>
	private void SetCameras() {
		if (cameras.Length <= 1) {
			return;
		}
		cameras[cameraIndex].gameObject.SetActive(false);
		cameraIndex++;
		if (cameraIndex >= cameras.Length) {
			cameraIndex = 0;
		}
		cameras[cameraIndex].gameObject.SetActive(true);
	}
}
