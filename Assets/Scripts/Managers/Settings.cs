using UnityEngine;

class Settings : MonoBehaviour {

    /// <summary> The singleton settings instance. </summary>
    private static Settings _instance;
    /// <summary> The singleton settings instance. </summary>
    public static Settings instance {
        get { return _instance; }
    }

    /// <summary> The PlayerPrefs key for the path setting. </summary>
    public const string PATH_KEY = "path";
    /// <summary> The PlayerPrefs key for the movement setting. </summary>
    public const string MOVEMENT_KEY = "movement";
    /// <summary> The PlayerPrefs key for the difficulty setting. </summary>
    public const string DIFFICULTY_KEY = "difficulty";

    /// <summary> The length of the level path. </summary>
    private float _pathSetting = 0.5f;
    /// <summary> The length of the level path. </summary>
    public float pathSetting {
        get { return _pathSetting; }
        set { 
            _pathSetting = value;
            PlayerPrefs.SetFloat(PATH_KEY, value);
        }
    }
    /// <summary> The amount of human movement in the level. </summary>
    private float _movementSetting = 0.5f;
    /// <summary> The amount of human movement in the level. </summary>
    public float movementSetting {
        get { return _movementSetting; }
        set { 
            _movementSetting = value;
            PlayerPrefs.SetFloat(MOVEMENT_KEY, value);
        }
    }
    /// <summary> The difficulty of the level. </summary>
    private float _difficultySetting = 0.5f;
    /// <summary> The difficulty of the level. </summary>
    public float difficultySetting {
        get { return _difficultySetting; }
        set { 
			_difficultySetting = value;
			Debug.Log(value);
            PlayerPrefs.SetFloat(DIFFICULTY_KEY, value);
        }
    }

    /// <summary>
    /// Sets the settings instance.
    /// </summary>
    private void Awake() {
        _instance = this;
    }

    /// <summary>
    /// Loads existing settings.
    /// </summary>
    public void LoadSettings() {
        LoadSetting(PATH_KEY, ref _pathSetting);
        LoadSetting(MOVEMENT_KEY, ref _movementSetting);
        LoadSetting(DIFFICULTY_KEY, ref _difficultySetting);
    }

    /// <summary>
    /// Loads a setting if it exists.
    /// </summary>
    /// <param name="key">The key of the setting.</param>
    /// <param name="setting">The setting variable to set.</param>
    private void LoadSetting(string key, ref float setting) {
        if (PlayerPrefs.HasKey(key)) {
            setting = PlayerPrefs.GetFloat(key);
        }
    }
}
