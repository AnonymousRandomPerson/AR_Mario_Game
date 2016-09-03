using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Event listeners for the main menu.
/// </summary>
public class MainMenuUI : MonoBehaviour {

	/// <summary> The name of the scene that the start button will take the user to. </summary>
	[Tooltip("The name of the scene that the start button will take the user to.")]
	public string startScene = "";

	/// <summary> Slider for the path setting. </summary>
	[SerializeField]
	[Tooltip("Slider for the path setting.")]
	private Slider pathSlider;
	/// <summary> Slider for the movement setting. </summary>
	[SerializeField]
	[Tooltip("Slider for the movement setting.")]
	private Slider movementSlider;
	/// <summary> Slider for the difficulty setting. </summary>
	[SerializeField]
	[Tooltip("Slider for the difficulty setting.")]
	private Slider difficultySlider;

	/// <summary> The settings instance. </summary>
	Settings settings;

	/// <summary>
	/// Loads the level settings.
	/// </summary>
	private void Start() {
		settings = Settings.instance;
		settings.LoadSettings();
		pathSlider.value = settings.pathSetting;
		movementSlider.value = settings.movementSetting;
		difficultySlider.value = settings.difficultySetting;
	}

	/// <summary>
	/// Takes the user to the level.
	/// </summary>
	public void StartLevel() {
		SceneManager.LoadScene(startScene);
	}
}
