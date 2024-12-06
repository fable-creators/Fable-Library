using UnityEngine;
using UnityEngine.UIElements;
using MegaBook;

/// <summary>
/// Controls the UI inputs for book dimensions and synchronizes them with the MegaBook builder component.
/// Handles conversion between display units (centimeters) and internal units.
/// </summary>
[Tooltip("Manages book dimension inputs and unit conversion")]
public class DimensionsController : MonoBehaviour
{
    /// <summary>
    /// Reference to the UI document containing dimension input fields.
    /// </summary>
    [Tooltip("UI Document containing dimension controls")]
    [SerializeField] private UIDocument document;

    /// <summary>
    /// Reference to the MegaBook component for updating book dimensions.
    /// </summary>
    [Tooltip("MegaBook component to update with dimension changes")]
    [SerializeField] private MegaBookBuilder bookComponent;

    /// <summary>
    /// Scale factor to convert between internal units and centimeter display values.
    /// </summary>
    [Tooltip("Conversion factor between internal units and centimeters")]
    private const float DISPLAY_SCALE = 100f;

    private FloatField widthField;
    private FloatField heightField;
    private FloatField thicknessField;

    /// <summary>
    /// Initializes UI elements and sets up initial event listeners.
    /// </summary>
    private void OnEnable()
    {
        if (document == null) document = GetComponent<UIDocument>();

        widthField = document.rootVisualElement.Q<FloatField>("widthField");
        heightField = document.rootVisualElement.Q<FloatField>("heightField");
        thicknessField = document.rootVisualElement.Q<FloatField>("thicknessField");

        if (widthField != null)
            widthField.RegisterValueChangedCallback(OnWidthChanged);
        if (heightField != null)
            heightField.RegisterValueChangedCallback(OnHeightChanged);
        if (thicknessField != null)
            thicknessField.RegisterValueChangedCallback(OnThicknessChanged);
        RegisterCallbacks();
        UpdateFieldsFromBook();
    }

    /// <summary>
    /// Sets up event listeners for dimension fields and preset dropdown changes.
    /// </summary>
    private void RegisterCallbacks()
    {
        if (widthField != null)
            widthField.RegisterValueChangedCallback(OnWidthChanged);
        if (heightField != null)
            heightField.RegisterValueChangedCallback(OnHeightChanged);
        if (thicknessField != null)
            thicknessField.RegisterValueChangedCallback(OnThicknessChanged);

        var mediaTypeDropdown = document.rootVisualElement.Q<DropdownField>("mediaTypeDropdown");
        if (mediaTypeDropdown != null)
            mediaTypeDropdown.RegisterValueChangedCallback(evt => UpdateFieldsFromBook());
    }

    /// <summary>
    /// Updates UI fields with current book dimensions, converting from internal units to centimeters.
    /// </summary>
    private void UpdateFieldsFromBook()
    {
        if (bookComponent == null) return;

        widthField.value = bookComponent.pageWidth * DISPLAY_SCALE;
        heightField.value = bookComponent.pageLength * DISPLAY_SCALE;
        thicknessField.value = bookComponent.bookthickness * DISPLAY_SCALE;
    }

    /// <summary>
    /// Handles width changes from UI, converts to internal units and updates the book.
    /// </summary>
    /// <param name="evt">Change event containing the new width value.</param>
    private void OnWidthChanged(ChangeEvent<float> evt)
    {
        if (bookComponent == null) return;
        bookComponent.pageWidth = evt.newValue / DISPLAY_SCALE;
        UpdateBook();
    }

    /// <summary>
    /// Handles height changes from UI, converts to internal units and updates the book.
    /// Also updates spine fabric dimensions if present.
    /// </summary>
    /// <param name="evt">Change event containing the new height value.</param>
    private void OnHeightChanged(ChangeEvent<float> evt)
    {
        if (bookComponent == null) return;
        bookComponent.pageLength = evt.newValue / DISPLAY_SCALE;

        if (bookComponent.spineFabric != null)
        {
            bookComponent.spineFabric.width = bookComponent.pageLength;
            bookComponent.spineFabric.BuildMesh();
        }

        UpdateBook();
    }

    /// <summary>
    /// Handles thickness changes from UI, converts to internal units and updates the book.
    /// </summary>
    /// <param name="evt">Change event containing the new thickness value.</param>
    private void OnThicknessChanged(ChangeEvent<float> evt)
    {
        if (bookComponent == null) return;
        bookComponent.bookthickness = evt.newValue / DISPLAY_SCALE;
        UpdateBook();
    }

    /// <summary>
    /// Triggers a rebuild of the book meshes and bindings.
    /// </summary>
    private void UpdateBook()
    {
        bookComponent.rebuildmeshes = true;
        bookComponent.rebuild = true;
        bookComponent.updateBindings = true;
    }
}
