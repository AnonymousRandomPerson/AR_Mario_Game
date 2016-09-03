using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// The screen that appears when the level is loading.
/// </summary>
class LoadingScreen : MonoBehaviour {

    /// <summary> The camera used to view the loading screen. </summary>
    [SerializeField]
    [Tooltip("The camera used to view the loading screen.")]
    private Camera loadingCamera;
    /// <summary> The text displayed during loading. </summary>
    [SerializeField]
    [Tooltip("The text displayed during loading.")]
    private Text loadingText;
    /// <summary> The ellipses that animate during loading. </summary>
    private string ellipses;
    /// <summary> The loading text without ellipses. </summary>
    private string originalLoadingText;

    /// <summary> Timer for animating the ellipses. </summary>
    private float loadTimer;
    /// <summary> The speed of the ellipses animation. </summary>
    [SerializeField]
    [Tooltip("The speed of the ellipses animation.")]
    private float loadAnimationSpeed;

    /// <summary>
    /// Updates the object.
    /// </summary>
    private void Start() {
        originalLoadingText = loadingText.text;
    }

    /// <summary>
    /// Animates the ellipses.
    /// </summary>
    private void Update() {
        ellipses = "";
        if (loadTimer < loadAnimationSpeed * 4) {
            for (int i = 0; i < (int)(loadTimer / loadAnimationSpeed); i++) {
                ellipses += ".";
            }
        } else {
            loadTimer = 0;
        }
        loadingText.text = originalLoadingText + ellipses;
        loadTimer += Time.deltaTime;

        bool active = LevelManager.Instance.player == null;
        gameObject.SetActive(active);
        loadingCamera.gameObject.SetActive(active);
    }
}
