using UnityEngine;
using UnityEngine.UIElements;
using System.Collections;

/// <summary>
/// Controls scene-wide settings including audio, camera, and rendering options.
/// Manages UI controls for volume, camera positioning, and visual quality settings.
/// </summary>
[Tooltip("Manages global scene settings and controls")]
public class SceneSettingsController : MonoBehaviour
{
    /// <summary>
    /// Reference to the UI document containing settings controls.
    /// </summary>
    [Tooltip("UI Document containing settings controls")]
    [SerializeField] private UIDocument document;

    /// <summary>
    /// Reference to the main camera for view manipulation.
    /// </summary>
    [Tooltip("Main camera to control")]
    [SerializeField] private Camera mainCamera;

    /// <summary>
    /// Reference to the background music audio source.
    /// </summary>
    [Tooltip("Audio source for background music")]
    [SerializeField] private AudioSource musicSource;

    /// <summary>
    /// Reference to the sound effects audio source.
    /// </summary>
    [Tooltip("Audio source for sound effects")]
    [SerializeField] private AudioSource effectsSource;

    private Slider masterVolumeSlider;
    private Slider musicVolumeSlider;
    private Slider effectsVolumeSlider;
    private Button muteButton;
    private SliderInt fovSlider;
    private Slider distanceSlider;
    private DropdownField qualityDropdown;

    private bool isMuted = false;
    private float lastMasterVolume = 1f;

    /// <summary>
    /// Initializes UI elements and registers event handlers.
    /// </summary>
    private void OnEnable()
    {
        if (document == null) document = GetComponent<UIDocument>();
        if (mainCamera == null) mainCamera = Camera.main;

        SetupUI(); // Initializes UI elements
        RegisterCallbacks(); // Registers event callbacks for UI interactions
    }

    /// <summary>
    /// Initializes UI elements by querying them from the UIDocument.
    /// Sets up camera view buttons and debug toggles.
    /// </summary>
    private void SetupUI()
    {
        var root = document.rootVisualElement;

        // Query UI elements by their names
        masterVolumeSlider = root.Q<Slider>("masterVolumeSlider");
        musicVolumeSlider = root.Q<Slider>("musicVolumeSlider");
        effectsVolumeSlider = root.Q<Slider>("effectsVolumeSlider");
        muteButton = root.Q<Button>("muteButton");
        fovSlider = root.Q<SliderInt>("fovSlider");
        distanceSlider = root.Q<Slider>("distanceSlider");
        qualityDropdown = root.Q<DropdownField>("qualityDropdown");

        // Setup camera position buttons with predefined views
        root.Q<Button>("frontViewButton").clicked += () => SetCameraView(Vector3.forward * 5);
        root.Q<Button>("sideViewButton").clicked += () => SetCameraView(Vector3.right * 5);
        root.Q<Button>("topViewButton").clicked += () => SetCameraView(Vector3.up * 5);

        // Setup debug toggles for FPS, wireframe, and colliders
        root.Q<Toggle>("fpsToggle").RegisterValueChangedCallback(evt => ToggleFPS(evt.newValue));
        root.Q<Toggle>("wireframeToggle").RegisterValueChangedCallback(evt => ToggleWireframe(evt.newValue));
        root.Q<Toggle>("collidersToggle").RegisterValueChangedCallback(evt => ToggleColliders(evt.newValue));

        // Setup rendering toggles for shadows and reflections
        root.Q<Toggle>("shadowsToggle").RegisterValueChangedCallback(evt => ToggleShadows(evt.newValue));
        root.Q<Toggle>("reflectionsToggle").RegisterValueChangedCallback(evt => ToggleReflections(evt.newValue));
    }

    /// <summary>
    /// Registers event callbacks for UI interactions.
    /// </summary>
    private void RegisterCallbacks()
    {
        masterVolumeSlider?.RegisterValueChangedCallback(evt => UpdateMasterVolume(evt.newValue));
        musicVolumeSlider?.RegisterValueChangedCallback(evt => UpdateMusicVolume(evt.newValue));
        effectsVolumeSlider?.RegisterValueChangedCallback(evt => UpdateEffectsVolume(evt.newValue));
        muteButton?.RegisterCallback<ClickEvent>(evt => ToggleMute());
        fovSlider?.RegisterValueChangedCallback(evt => UpdateFOV(evt.newValue));
        distanceSlider?.RegisterValueChangedCallback(evt => UpdateCameraDistance(evt.newValue));
        qualityDropdown?.RegisterValueChangedCallback(evt => UpdateQualitySettings(evt.newValue));
    }

    /// <summary>
    /// Updates the master volume level, respecting mute state.
    /// </summary>
    /// <param name="value">New volume value between 0 and 1.</param>
    private void UpdateMasterVolume(float value)
    {
        if (!isMuted)
        {
            AudioListener.volume = value;
            lastMasterVolume = value;
        }
    }

    /// <summary>
    /// Updates the music volume level.
    /// </summary>
    /// <param name="value">New volume value between 0 and 1.</param>
    private void UpdateMusicVolume(float value)
    {
        if (musicSource != null)
            musicSource.volume = value;
    }

    /// <summary>
    /// Updates the sound effects volume level.
    /// </summary>
    /// <param name="value">New volume value between 0 and 1.</param>
    private void UpdateEffectsVolume(float value)
    {
        if (effectsSource != null)
            effectsSource.volume = value;
    }

    /// <summary>
    /// Toggles audio mute state and updates UI accordingly.
    /// </summary>
    private void ToggleMute()
    {
        isMuted = !isMuted;
        if (isMuted)
        {
            lastMasterVolume = AudioListener.volume;
            AudioListener.volume = 0;
            muteButton.AddToClassList("muted");
        }
        else
        {
            AudioListener.volume = lastMasterVolume;
            muteButton.RemoveFromClassList("muted");
        }
    }

    /// <summary>
    /// Updates the camera's field of view.
    /// </summary>
    /// <param name="value">New FOV value in degrees.</param>
    private void UpdateFOV(int value)
    {
        if (mainCamera != null)
            mainCamera.fieldOfView = value;
    }

    /// <summary>
    /// Updates the camera's distance from the target.
    /// </summary>
    /// <param name="value">New distance value.</param>
    private void UpdateCameraDistance(float value)
    {
        if (mainCamera != null)
            mainCamera.transform.position = mainCamera.transform.position.normalized * value;
    }

    /// <summary>
    /// Sets the camera to a specific view position.
    /// </summary>
    /// <param name="position">Target position for the camera.</param>
    private void SetCameraView(Vector3 position)
    {
        if (mainCamera != null)
        {
            mainCamera.transform.position = position;
            mainCamera.transform.LookAt(Vector3.zero);
        }
    }

    /// <summary>
    /// Updates the quality settings based on the selected preset.
    /// </summary>
    /// <param name="value">Quality preset name.</param>
    private void UpdateQualitySettings(string value)
    {
        int qualityLevel = value switch
        {
            "Low" => 0,
            "Medium" => 1,
            "High" => 2,
            "Ultra" => 3,
            _ => 1
        };
        QualitySettings.SetQualityLevel(qualityLevel, true);
    }

    /// <summary>
    /// Toggles the FPS counter display.
    /// </summary>
    /// <param name="enabled">Whether to show the FPS counter.</param>
    private void ToggleFPS(bool enabled)
    {
        StartCoroutine(enabled ? UpdateFPSCounter() : null);
    }

    /// <summary>
    /// Updates the FPS counter display.
    /// </summary>
    private IEnumerator UpdateFPSCounter()
    {
        var fpsText = document.rootVisualElement.Q<Label>("fpsCounter");
        while (true)
        {
            if (fpsText != null)
                fpsText.text = $"FPS: {(int)(1f / Time.deltaTime)}";
            yield return new WaitForSeconds(0.5f);
        }
    }

    /// <summary>
    /// Toggles wireframe rendering mode.
    /// </summary>
    /// <param name="enabled">Whether to enable wireframe mode.</param>
    private void ToggleWireframe(bool enabled)
    {
        if (enabled)
            GL.wireframe = true;
        else
            GL.wireframe = false;
    }

    /// <summary>
    /// Toggles visibility of colliders in the scene.
    /// </summary>
    /// <param name="enabled">Whether to show colliders.</param>
    private void ToggleColliders(bool enabled)
    {
        var colliders = FindObjectsByType<Collider>(FindObjectsSortMode.None);
        foreach (var collider in colliders)
        {
            if (collider.TryGetComponent<MeshRenderer>(out var renderer))
                renderer.enabled = enabled;
        }
    }

    /// <summary>
    /// Toggles shadow quality settings.
    /// </summary>
    /// <param name="enabled">Whether to enable shadows.</param>
    private void ToggleShadows(bool enabled)
    {
        QualitySettings.shadows = enabled ? ShadowQuality.All : ShadowQuality.Disable;
    }

    /// <summary>
    /// Toggles real-time reflection probes.
    /// </summary>
    /// <param name="enabled">Whether to enable real-time reflections.</param>
    private void ToggleReflections(bool enabled)
    {
        QualitySettings.realtimeReflectionProbes = enabled;
    }

    /// <summary>
    /// Cleans up coroutines when the component is disabled.
    /// </summary>
    private void OnDisable()
    {
        StopAllCoroutines();
    }
}
