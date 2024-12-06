using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// Controls the visibility of the settings panel through a toggle button.
/// Provides a simple show/hide mechanism for the settings interface.
/// </summary>
[Tooltip("Manages settings panel visibility")]
public class SettingsToggleController : MonoBehaviour
{
    /// <summary>
    /// Reference to the UI document containing settings controls.
    /// </summary>
    private UIDocument document;

    /// <summary>
    /// Container element for all settings controls.
    /// </summary>
    private VisualElement settingsContainer;

    /// <summary>
    /// Button that toggles settings visibility.
    /// </summary>
    private Button settingsToggle;

    /// <summary>
    /// Initializes UI references and sets up the toggle button click handler.
    /// </summary>
    private void OnEnable()
    {
        document = GetComponent<UIDocument>();
        if (document == null) return;

        var root = document.rootVisualElement;
        settingsContainer = root.Q("settingsContainer");
        settingsToggle = root.Q<Button>("settingsToggle");

        if (settingsToggle != null)
        {
            settingsToggle.clicked += ToggleSettings;
        }
    }

    /// <summary>
    /// Toggles the settings panel visibility by adding/removing the "hidden" USS class.
    /// </summary>
    private void ToggleSettings()
    {
        settingsContainer.ToggleInClassList("hidden");
    }
}
