using UnityEngine;
using UnityEngine.UIElements;
using MegaBook;

/// <summary>
/// Controls the UI elements that modify MegaBook's page settings and physics properties.
/// Manages page count, thickness, stiffness, and physics simulation settings.
/// </summary>
[Tooltip("Manages book page settings and physics properties")]
public class PageSettingsController : MonoBehaviour
{
    /// <summary>
    /// Reference to the UI document containing page settings controls.
    /// </summary>
    [Tooltip("UI Document containing page settings controls")]
    [SerializeField] private UIDocument document;

    /// <summary>
    /// Reference to the book component for applying settings.
    /// </summary>
    [Tooltip("Book component to update with settings changes")]
    [SerializeField] private MegaBookBuilder bookComponent;

    private IntegerField pageCountField;
    private Slider pageThicknessSlider;
    private Slider pageStiffnessSlider;
    private Toggle enablePhysicsToggle;

    /// <summary>
    /// Initializes UI element references and sets up the initial state.
    /// </summary>
    private void OnEnable()
    {
        if (document == null) document = GetComponent<UIDocument>();

        pageCountField = document.rootVisualElement.Q<IntegerField>("pageCountField");
        pageThicknessSlider = document.rootVisualElement.Q<Slider>("pageThicknessSlider");
        pageStiffnessSlider = document.rootVisualElement.Q<Slider>("pageStiffnessSlider");
        enablePhysicsToggle = document.rootVisualElement.Q<Toggle>("enablePhysicsToggle");

        RegisterCallbacks();
        UpdateUIFromBook();
    }

    /// <summary>
    /// Configures UI element callbacks and sets default ranges for sliders.
    /// Physics is enabled by default.
    /// </summary>
    private void RegisterCallbacks()
    {
        if (pageCountField != null)
            pageCountField.RegisterValueChangedCallback(OnPageCountChanged);
        if (pageThicknessSlider != null)
            pageThicknessSlider.RegisterValueChangedCallback(OnPageThicknessChanged);
        if (pageStiffnessSlider != null)
        {
            pageStiffnessSlider.lowValue = 1;
            pageStiffnessSlider.highValue = 50;
            pageStiffnessSlider.RegisterValueChangedCallback(OnPageStiffnessChanged);
        }
        if (enablePhysicsToggle != null)
        {
            enablePhysicsToggle.value = true;
            enablePhysicsToggle.RegisterValueChangedCallback(OnEnablePhysicsChanged);
        }

        /*         // Listen for preset changes
                document.rootVisualElement.Q<DropdownField>("mediaTypeDropdown")
                    ?.RegisterValueChangedCallback(evt => UpdateUIFromBook()); */
    }

    /// <summary>
    /// Synchronizes UI element values with the current MegaBook component settings.
    /// </summary>
    public void UpdateUIFromBook()
    {
        if (bookComponent == null) return;

        if (pageCountField != null)
            pageCountField.value = bookComponent.NumPages;
        if (pageThicknessSlider != null)
            pageThicknessSlider.value = bookComponent.pageHeight;
        if (pageStiffnessSlider != null)
            pageStiffnessSlider.value = bookComponent.WidthSegs;
    }

    /// <summary>
    /// Updates the total number of pages in the book.
    /// Triggers a mesh rebuild to reflect changes.
    /// </summary>
    /// <param name="evt">Change event containing the new page count.</param>
    private void OnPageCountChanged(ChangeEvent<int> evt)
    {
        if (bookComponent == null) return;
        bookComponent.NumPages = evt.newValue;
        UpdateBook();
    }

    /// <summary>
    /// Adjusts the thickness of individual pages.
    /// Triggers a mesh rebuild to reflect changes.
    /// </summary>
    /// <param name="evt">Change event containing the new thickness value.</param>
    private void OnPageThicknessChanged(ChangeEvent<float> evt)
    {
        if (bookComponent == null) return;
        bookComponent.pageHeight = evt.newValue;
        UpdateBook();
    }

    /// <summary>
    /// Controls page flexibility by adjusting the number of width segments in the mesh.
    /// Higher values create more flexible pages, lower values make pages more rigid.
    /// </summary>
    /// <param name="evt">Change event containing the new stiffness value.</param>
    private void OnPageStiffnessChanged(ChangeEvent<float> evt)
    {
        if (bookComponent == null) return;
        int widthSegs = Mathf.RoundToInt(evt.newValue);
        bookComponent.WidthSegs = widthSegs;
        UpdateBook();
    }

    /// <summary>
    /// Toggles between physics-enabled (segmented) and non-physics (simple) mesh modes.
    /// Physics mode uses multiple segments for realistic page movement.
    /// Non-physics mode uses single segments for basic page turning.
    /// </summary>
    /// <param name="evt">Change event containing the new physics state.</param>
    private void OnEnablePhysicsChanged(ChangeEvent<bool> evt)
    {
        if (bookComponent == null) return;

        if (evt.newValue)
        {
            bookComponent.WidthSegs = Mathf.RoundToInt(pageStiffnessSlider.value);
            bookComponent.LengthSegs = 8;
            bookComponent.HeightSegs = 1;
        }
        else
        {
            bookComponent.WidthSegs = 1;
            bookComponent.LengthSegs = 1;
            bookComponent.HeightSegs = 1;
        }

        UpdateBook();
    }

    /// <summary>
    /// Triggers MegaBook to rebuild its meshes and update bindings.
    /// </summary>
    private void UpdateBook()
    {
        bookComponent.rebuildmeshes = true;
        bookComponent.rebuild = true;
        bookComponent.updateBindings = true;
    }
}