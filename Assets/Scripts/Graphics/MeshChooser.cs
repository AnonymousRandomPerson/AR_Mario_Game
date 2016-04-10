using UnityEngine;
using System.Collections;

/// <summary>
/// Displays a particular mesh out of several.
/// </summary>
public class MeshChooser : MonoBehaviour {

	/// <summary> All possible meshes. </summary>
	[SerializeField]
	[Tooltip("All possible meshes.")]
	private GameObject[] meshes;
	/// <summary> The index of the active mesh. </summary>
	private int meshIndex = -1;

	/// <summary>
	/// Activates a mesh with a certain index.
	/// </summary>
	/// <param name="index">The index of the mesh to activate.</param>
	public void ActivateMesh(int index) {
		if (index < meshes.Length) {
			meshes[index].SetActive(true);
			meshIndex = index;
		}
	}

	/// <summary>
	/// Gets the currently active mesh.
	/// </summary>
	/// <returns>The active mesh.</returns>
	public GameObject GetActiveMesh() {
		return meshIndex >= 0 ? meshes[meshIndex] : null;
	}
}
