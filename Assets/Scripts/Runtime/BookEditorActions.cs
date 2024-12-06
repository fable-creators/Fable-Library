using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;

/// <summary>
/// Represents the complete state of the book editor, used for undo/redo and save/load operations.
/// Contains all configurable properties of the book editor interface.
/// </summary>
[System.Serializable]
public class BookState
{
    public string mediaType;
    public float width;
    public float height;
    public float thickness;
    public int pageCount;
    public float pageThickness;
    public float pageStiffness;
    public bool physicsEnabled;
    public string coverMaterial;
    public string pageMaterial;
    public float roughness;
    public float metallic;
    public string contentType;
    public string pageText;
    public string selectedFont;
    public int fontSize;
    public bool isBold;
    public bool isItalic;
    public string pageTurnSound;
    public string backgroundMusic;
    public float effectsVolume;
    public float musicVolume;
    public float pageCurlAmount;
    public float edgeWear;
    public bool dogEarsEnabled;
    public string exportType;
}

/// <summary>
/// Manages the book editor's state, including undo/redo functionality and state persistence.
/// Handles UI element change events and maintains state history.
/// </summary>
public class BookEditorActions : MonoBehaviour
{
    /// <summary>
    /// Reference to the database controller for saving and loading projects.
    /// </summary>
    [SerializeField] private DatabaseController databaseController;
    
    private UIDocument document;
    private Stack<BookState> undoStack = new Stack<BookState>();
    private Stack<BookState> redoStack = new Stack<BookState>();
    private bool isUndoRedoOperation = false;

    /// <summary>
    /// Initializes UI components and registers event handlers for all interactive elements.
    /// </summary>
    private void OnEnable()
    {
        document = GetComponent<UIDocument>();
        if (document == null)
        {
            Debug.LogError("UIDocument component not found!");
            return;
        }

        RegisterChangeEvents(document.rootVisualElement);

        var saveProgressButton = document.rootVisualElement.Q<Button>("saveProgressButton");
        if (saveProgressButton != null)
        {
            saveProgressButton.clicked += async () =>
            {
                await databaseController.SaveProject("Book Project");
                SaveState(); // Also save local state
            };
        }
    }

    /// <summary>
    /// Registers change event handlers for all UI elements and sets up undo/redo functionality.
    /// </summary>
    /// <param name="root">Root visual element containing all UI components.</param>
    private void RegisterChangeEvents(VisualElement root)
    {
        if (root == null) return;

        var fields = new Dictionary<string, EventCallback<ChangeEvent<string>>>()
        {
            { "mediaTypeDropdown", evt => OnUIElementChanged() },
            { "contentTextField", evt => OnUIElementChanged() },
            { "coverMaterialDropdown", evt => OnUIElementChanged() },
            { "pageMaterialDropdown", evt => OnUIElementChanged() },
            { "pageTurnSoundDropdown", evt => OnUIElementChanged() },
            { "backgroundMusicDropdown", evt => OnUIElementChanged() },
            { "exportTypeDropdown", evt => OnUIElementChanged() }
        };

        foreach (var field in fields)
        {
            var element = root.Q<DropdownField>(field.Key);
            if (element != null)
            {
                element.RegisterValueChangedCallback(field.Value);
            }
        }

        // Register float fields and sliders
        var floatControls = new string[]
        {
            "widthField", "heightField", "thicknessField",
            "pageThicknessSlider", "pageStiffnessSlider",
            "roughnessSlider", "metallicSlider",
            "effectsVolumeSlider", "musicVolumeSlider",
            "pageCurlSlider", "edgeWearSlider"
        };

        foreach (var controlName in floatControls)
        {
            var floatField = root.Q<FloatField>(controlName);
            if (floatField != null)
            {
                floatField.RegisterValueChangedCallback(_ => OnUIElementChanged());
            }

            var slider = root.Q<Slider>(controlName);
            if (slider != null)
            {
                slider.RegisterValueChangedCallback(_ => OnUIElementChanged());
            }
        }

        // Register integer fields
        var pageCountField = root.Q<IntegerField>("pageCountField");
        if (pageCountField != null)
        {
            pageCountField.RegisterValueChangedCallback(_ => OnUIElementChanged());
        }

        // Register toggles
        var toggles = new[] { "enablePhysicsToggle", "dogEarsToggle" };
        foreach (var toggle in toggles)
        {
            var element = root.Q<Toggle>(toggle);
            if (element != null)
            {
                element.RegisterValueChangedCallback(_ => OnUIElementChanged());
            }
        }

        // Register undo/redo buttons if they exist
        var undoButton = root.Q<Button>("undoButton");
        if (undoButton != null) undoButton.clicked += Undo;

        var redoButton = root.Q<Button>("redoButton");
        if (redoButton != null) redoButton.clicked += Redo;

        var saveButton = root.Q<Button>("saveButton");
        if (saveButton != null) saveButton.clicked += SaveState;

        var loadButton = root.Q<Button>("loadButton");
        if (loadButton != null) loadButton.clicked += LoadState;
    }

    /// <summary>
    /// Handles UI element changes by pushing the current state to the undo stack.
    /// Clears redo stack as new changes invalidate previous redo operations.
    /// </summary>
    private void OnUIElementChanged()
    {
        if (!isUndoRedoOperation)
        {
            undoStack.Push(GetCurrentState());
            redoStack.Clear();
        }
    }

    /// <summary>
    /// Captures the current state of all UI elements.
    /// Uses null coalescing to provide default values for missing elements.
    /// </summary>
    /// <returns>A BookState object containing the current state of all UI elements.</returns>
    private BookState GetCurrentState()
    {
        var root = document.rootVisualElement;
        return new BookState
        {
            mediaType = root.Q<DropdownField>("mediaTypeDropdown")?.value,
            width = root.Q<FloatField>("widthField")?.value ?? 0,
            height = root.Q<FloatField>("heightField")?.value ?? 0,
            thickness = root.Q<FloatField>("thicknessField")?.value ?? 0,
            pageCount = root.Q<IntegerField>("pageCountField")?.value ?? 0,
            pageThickness = root.Q<Slider>("pageThicknessSlider")?.value ?? 0,
            pageStiffness = root.Q<Slider>("pageStiffnessSlider")?.value ?? 0,
            physicsEnabled = root.Q<Toggle>("enablePhysicsToggle")?.value ?? false,
            coverMaterial = root.Q<DropdownField>("coverMaterialDropdown")?.value,
            pageMaterial = root.Q<DropdownField>("pageMaterialDropdown")?.value,
            roughness = root.Q<Slider>("roughnessSlider")?.value ?? 0,
            metallic = root.Q<Slider>("metallicSlider")?.value ?? 0,
            contentType = root.Q<DropdownField>("contentTypeDropdown")?.value,
            pageText = root.Q<TextField>("contentTextField")?.value,
            selectedFont = root.Q<DropdownField>("fontDropdown")?.value,
            fontSize = root.Q<IntegerField>("fontSizeField")?.value ?? 12,
            pageTurnSound = root.Q<DropdownField>("pageTurnSoundDropdown")?.value,
            backgroundMusic = root.Q<DropdownField>("backgroundMusicDropdown")?.value,
            effectsVolume = root.Q<Slider>("effectsVolumeSlider")?.value ?? 0,
            musicVolume = root.Q<Slider>("musicVolumeSlider")?.value ?? 0,
            pageCurlAmount = root.Q<Slider>("pageCurlSlider")?.value ?? 0,
            edgeWear = root.Q<Slider>("edgeWearSlider")?.value ?? 0,
            dogEarsEnabled = root.Q<Toggle>("dogEarsToggle")?.value ?? false,
            exportType = root.Q<DropdownField>("exportTypeDropdown")?.value
        };
    }

    /// <summary>
    /// Applies a saved state to all UI elements.
    /// Temporarily disables undo/redo operations during state application.
    /// </summary>
    /// <param name="state">The BookState to apply to the UI elements.</param>
    private void ApplyState(BookState state)
    {
        if (state == null || document == null) return;

        isUndoRedoOperation = true;
        var root = document.rootVisualElement;

        void SetFieldValue<T>(string name, T value, System.Action<VisualElement, T> setter)
        {
            var element = root.Q<VisualElement>(name);
            if (element != null) setter(element, value);
        }

        SetFieldValue("mediaTypeDropdown", state.mediaType, (e, v) => ((DropdownField)e).value = v);
        SetFieldValue("widthField", state.width, (e, v) => ((FloatField)e).value = v);
        SetFieldValue("heightField", state.height, (e, v) => ((FloatField)e).value = v);
        SetFieldValue("thicknessField", state.thickness, (e, v) => ((FloatField)e).value = v);
        SetFieldValue("pageCountField", state.pageCount, (e, v) => ((IntegerField)e).value = v);
        SetFieldValue("pageThicknessSlider", state.pageThickness, (e, v) => ((Slider)e).value = v);
        SetFieldValue("pageStiffnessSlider", state.pageStiffness, (e, v) => ((Slider)e).value = v);
        SetFieldValue("enablePhysicsToggle", state.physicsEnabled, (e, v) => ((Toggle)e).value = v);
        SetFieldValue("coverMaterialDropdown", state.coverMaterial, (e, v) => ((DropdownField)e).value = v);
        SetFieldValue("pageMaterialDropdown", state.pageMaterial, (e, v) => ((DropdownField)e).value = v);
        SetFieldValue("roughnessSlider", state.roughness, (e, v) => ((Slider)e).value = v);
        SetFieldValue("metallicSlider", state.metallic, (e, v) => ((Slider)e).value = v);
        SetFieldValue("contentTypeDropdown", state.contentType, (e, v) => ((DropdownField)e).value = v);
        SetFieldValue("contentTextField", state.pageText, (e, v) => ((TextField)e).value = v);
        SetFieldValue("fontDropdown", state.selectedFont, (e, v) => ((DropdownField)e).value = v);
        SetFieldValue("fontSizeField", state.fontSize, (e, v) => ((IntegerField)e).value = v);
        SetFieldValue("pageTurnSoundDropdown", state.pageTurnSound, (e, v) => ((DropdownField)e).value = v);
        SetFieldValue("backgroundMusicDropdown", state.backgroundMusic, (e, v) => ((DropdownField)e).value = v);
        SetFieldValue("effectsVolumeSlider", state.effectsVolume, (e, v) => ((Slider)e).value = v);
        SetFieldValue("musicVolumeSlider", state.musicVolume, (e, v) => ((Slider)e).value = v);
        SetFieldValue("pageCurlSlider", state.pageCurlAmount, (e, v) => ((Slider)e).value = v);
        SetFieldValue("edgeWearSlider", state.edgeWear, (e, v) => ((Slider)e).value = v);
        SetFieldValue("dogEarsToggle", state.dogEarsEnabled, (e, v) => ((Toggle)e).value = v);
        SetFieldValue("exportTypeDropdown", state.exportType, (e, v) => ((DropdownField)e).value = v);

        isUndoRedoOperation = false;
    }

    /// <summary>
    /// Implements undo by swapping current state with previous state.
    /// Current state is preserved in redo stack.
    /// </summary>
    private void Undo()
    {
        if (undoStack.Count == 0) return;

        redoStack.Push(GetCurrentState());
        ApplyState(undoStack.Pop());
    }

    /// <summary>
    /// Implements redo by restoring previously undone state.
    /// Current state is preserved in undo stack.
    /// </summary>
    private void Redo()
    {
        if (redoStack.Count == 0) return;

        undoStack.Push(GetCurrentState());
        ApplyState(redoStack.Pop());
    }

    /// <summary>
    /// Persists current editor state to PlayerPrefs as JSON.
    /// </summary>
    private void SaveState()
    {
        var state = GetCurrentState();
        var json = JsonUtility.ToJson(state);
        PlayerPrefs.SetString("BookEditorState", json);
        PlayerPrefs.Save();
    }

    /// <summary>
    /// Restores editor state from PlayerPrefs.
    /// Silently fails if no saved state exists.
    /// </summary>
    private void LoadState()
    {
        var json = PlayerPrefs.GetString("BookEditorState");
        if (string.IsNullOrEmpty(json)) return;

        var state = JsonUtility.FromJson<BookState>(json);
        ApplyState(state);
    }
}