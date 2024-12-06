using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;
using System;

/// <summary>
/// Manages the workflow steps in a UI, allowing navigation and validation of each step.
/// Controls the progression through the book creation process and ensures proper validation.
/// </summary>
[Tooltip("Controls workflow progression and step validation")]
public class WorkflowController : MonoBehaviour
{
    /// <summary>
    /// Reference to the UI document containing workflow controls.
    /// </summary>
    [Tooltip("UI Document containing workflow steps")]
    [SerializeField] private UIDocument document;

    /// <summary>
    /// List of step identifiers in order of progression.
    /// </summary>
    private List<string> steps = new List<string> {
        "step1",  // Main Menu
        "step2",  // Initial Settings
        "step3",  // Load JSON
        "step4",  // Choose Media Type
        "step5",  // Materials & Colors
        "step6",  // Book Structure/Page Content
        "step7",  // Audio Settings
        "step8"   // Export
    };

    private int currentStepIndex = 0;
    private VisualElement root;
    private Label currentStepLabel;
    private Button prevStepButton;
    private Button nextStepButton;
    private Dictionary<string, bool> stepValidation = new Dictionary<string, bool>();

    // Button references
    private Button startButton;
    private Button loadButton;
    private Button optionsButton;
    private Button saveProgressButton;
    private Button cancelButton;

    /// <summary>
    /// Initializes memory management for WebGL builds.
    /// </summary>
    private void Start()
    {
        #if UNITY_WEBGL
            Application.lowMemory += OnLowMemory;
        #endif
    }

    /// <summary>
    /// Handles low memory situations in WebGL builds.
    /// </summary>
    private void OnLowMemory()
    {
        Debug.Log("Low memory detected - cleaning up resources");
        Resources.UnloadUnusedAssets();
        System.GC.Collect();
    }

    /// <summary>
    /// Initializes the workflow, sets up UI elements, and registers event callbacks.
    /// </summary>
    private void OnEnable()
    {
        if (document == null)
        {
            Debug.LogError("UIDocument not assigned to WorkflowController!");
            return;
        }

        root = document.rootVisualElement;
        if (root == null)
        {
            Debug.LogError("Could not find root VisualElement!");
            return;
        }

        InitializeButtons();
        SetupNavigation();
        InitializeStepValidation();
        UpdateStepVisibility();
        RegisterCallbacks();

        // Start with only step1 visible
        ShowOnlyStep("step1");
        Debug.Log("WorkflowController initialized");
    }

    /// <summary>
    /// Sets up navigation buttons and their event handlers.
    /// </summary>
    private void SetupNavigation()
    {
        currentStepLabel = root.Q<Label>("currentStepLabel");
        prevStepButton = root.Q<Button>("prevStepButton");
        nextStepButton = root.Q<Button>("nextStepButton");

        if (prevStepButton != null)
            prevStepButton.clicked += PreviousStep;
        if (nextStepButton != null)
            nextStepButton.clicked += NextStep;

        UpdateNavigationButtons();
    }

    /// <summary>
    /// Initializes step validation states, marking the first step as valid by default.
    /// </summary>
    private void InitializeStepValidation()
    {
        stepValidation = new Dictionary<string, bool>();

        // Initialize all steps as invalid first
        foreach (string step in steps)
        {
            stepValidation[step] = false;
            Debug.Log($"Initialized validation for {step}: {stepValidation[step]}");
        }

        // Set initial step as valid
        stepValidation["step1"] = true;

        // Set default validation rules for each step
        SetDefaultStepValidation();
    }

    /// <summary>
    /// Sets default validation rules for each workflow step.
    /// </summary>
    private void SetDefaultStepValidation()
    {
        // Step 2 (Initial Settings) - Valid by default
        stepValidation["step2"] = true;

        // Step 3 (Load JSON) - Valid if coming from start
        stepValidation["step3"] = true;

        // Step 4 (Media Type) requires selection
        stepValidation["step4"] = false;

        // Step 5 (Materials) valid by default
        stepValidation["step5"] = true;

        // Step 6 (Book Structure) valid by default
        stepValidation["step6"] = true;

        // Step 7 (Audio) valid by default
        stepValidation["step7"] = true;

        // Step 8 (Export) valid by default
        stepValidation["step8"] = true;
    }

    /// <summary>
    /// Registers callbacks for UI elements to handle user interactions.
    /// </summary>
    private void RegisterCallbacks()
    {

        // Step 2 - Initial Settings
        var presetRoomDropdown = root.Q<DropdownField>("presetRoomDropdown");
        if (presetRoomDropdown != null)
        {
            presetRoomDropdown.RegisterValueChangedCallback(evt =>
            {
                stepValidation["step2"] = !string.IsNullOrEmpty(evt.newValue);
                UpdateNavigationButtons();
            });
        }

        // Step 3 - JSON Loading
        var importJsonButton = root.Q<Button>("importJsonButton");
        if (importJsonButton != null)
        {
            importJsonButton.clicked += () =>
            {
                stepValidation["step3"] = true;
                UpdateNavigationButtons();
            };
        }

        // Step 4 - Media Type
        var mediaTypeDropdown = root.Q<DropdownField>("mediaTypeDropdown");
        if (mediaTypeDropdown != null)
        {
            mediaTypeDropdown.RegisterValueChangedCallback(evt =>
            {
                stepValidation["step4"] = !string.IsNullOrEmpty(evt.newValue);
                UpdateNavigationButtons();
                UpdateDimensionsBasedOnMediaType(evt.newValue);
            });
        }
    }

    /// <summary>
    /// Updates UI dimensions based on the selected media type.
    /// </summary>
    /// <param name="mediaType">Selected media type (Book, Magazine, etc.).</param>
    private void UpdateDimensionsBasedOnMediaType(string mediaType)
    {
        var widthField = root.Q<FloatField>("widthField");
        var heightField = root.Q<FloatField>("heightField");
        var thicknessField = root.Q<FloatField>("thicknessField");

        if (widthField == null || heightField == null || thicknessField == null)
            return;

        switch (mediaType)
        {
            case "Book":
                widthField.value = 21f;  // Standard book dimensions
                heightField.value = 29.7f;
                thicknessField.value = 2.5f;
                break;
            case "Magazine":
                widthField.value = 21f;
                heightField.value = 29.7f;
                thicknessField.value = 0.5f;
                break;
            case "Newspaper":
                widthField.value = 35f;
                heightField.value = 50f;
                thicknessField.value = 0.1f;
                break;
            case "Poster":
                widthField.value = 42f;
                heightField.value = 59.4f;
                thicknessField.value = 0.05f;
                break;
            case "Manga":
                widthField.value = 18.2f;
                heightField.value = 25.7f;
                thicknessField.value = 1.5f;
                break;
        }
    }

    /// <summary>
    /// Initializes button references and logs their initialization status.
    /// </summary>
    private void InitializeButtons()
    {
        // Main menu buttons
        startButton = root.Q<Button>("startButton");
        loadButton = root.Q<Button>("loadButton");
        optionsButton = root.Q<Button>("optionsButton");

        // Global action buttons
        saveProgressButton = root.Q<Button>("saveProgressButton");
        cancelButton = root.Q<Button>("cancelButton");

        // Log button initialization status
        Debug.Log($"Buttons found - Start: {startButton != null}, Load: {loadButton != null}, " +
                 $"Options: {optionsButton != null}, Save: {saveProgressButton != null}, " +
                 $"Cancel: {cancelButton != null}");

        RegisterButtonCallbacks();
    }

    /// <summary>
    /// Registers button click event handlers for main menu actions.
    /// </summary>
    private void RegisterButtonCallbacks()
    {
        if (startButton != null)
        {
            startButton.clicked += OnStartButtonClicked;
            Debug.Log("Start button callback registered");
        }

        if (loadButton != null)
        {
            loadButton.clicked += OnLoadButtonClicked;
            Debug.Log("Load button callback registered");
        }

        if (optionsButton != null)
        {
            optionsButton.clicked += OnOptionsButtonClicked;
            Debug.Log("Options button callback registered");
        }

        if (saveProgressButton != null)
        {
            saveProgressButton.clicked += OnSaveProgressClicked;
            Debug.Log("Save progress button callback registered");
        }

        if (cancelButton != null)
        {
            cancelButton.clicked += OnCancelClicked;
            Debug.Log("Cancel button callback registered");
        }
    }

    /// <summary>
    /// Handles the start button click, moving to the next step.
    /// </summary>
    private void OnStartButtonClicked()
    {
        Debug.Log("Start button clicked");
        stepValidation["step1"] = true;
        currentStepIndex = 1; // Move to step 2
        UpdateStepVisibility();
    }

    /// <summary>
    /// Handles the load button click, moving to the JSON loading step.
    /// </summary>
    private void OnLoadButtonClicked()
    {
        Debug.Log("Load button clicked");
        stepValidation["step1"] = true;
        currentStepIndex = 2; // Move to step 3 (JSON loading)
        UpdateStepVisibility();
    }

    /// <summary>
    /// Handles the options button click, intended for future options menu functionality.
    /// </summary>
    private void OnOptionsButtonClicked()
    {
        Debug.Log("Options button clicked");
        // Implement options menu functionality
    }

    /// <summary>
    /// Handles the save progress button click, triggering save functionality.
    /// </summary>
    private void OnSaveProgressClicked()
    {
        Debug.Log("Save progress clicked");
        SaveProgress();
    }

    /// <summary>
    /// Handles the cancel button click, resetting to the first step.
    /// </summary>
    private void OnCancelClicked()
    {
        Debug.Log("Cancel clicked");
        // Reset to first step
        currentStepIndex = 0;
        UpdateStepVisibility();
    }

    /// <summary>
    /// Shows only the specified step, hiding all others.
    /// </summary>
    /// <param name="stepName">Name of the step to show.</param>
    private void ShowOnlyStep(string stepName)
    {
        foreach (string step in steps)
        {
            var stepContainer = root.Q<VisualElement>(step);
            if (stepContainer != null)
            {
                stepContainer.style.display = (step == stepName) ? DisplayStyle.Flex : DisplayStyle.None;
            }
        }
    }

    /// <summary>
    /// Returns a human-readable name for a given step ID.
    /// </summary>
    /// <param name="stepId">Internal step identifier.</param>
    /// <returns>Human-readable step name.</returns>
    private string GetStepName(string stepId)
    {
        switch (stepId)
        {
            case "step1": return "Main Menu";
            case "step2": return "Initial Settings";
            case "step3": return "Load Settings";
            case "step4": return "Choose Media Type";
            case "step5": return "Materials & Colors";
            case "step6": return "Book Structure & Content";
            case "step7": return "Audio Settings";
            case "step8": return "Export Options";
            default: return "Unknown Step";
        }
    }

    /// <summary>
    /// Logs step transitions for debugging purposes.
    /// </summary>
    /// <param name="fromStep">Index of the step being left.</param>
    /// <param name="toStep">Index of the step being entered.</param>
    private void LogStepTransition(int fromStep, int toStep)
    {
        Debug.Log($"Step Transition: {GetStepName(steps[fromStep])} -> {GetStepName(steps[toStep])}");
    }

    /// <summary>
    /// Advances to the next step if validation allows, logging the transition.
    /// </summary>
    private void NextStep()
    {
        if (currentStepIndex < steps.Count - 1)
        {
            int previousStep = currentStepIndex;
            currentStepIndex++;
            LogStepTransition(previousStep, currentStepIndex);
            UpdateStepVisibility();
        }
    }

    /// <summary>
    /// Moves to the previous step, logging the transition.
    /// </summary>
    private void PreviousStep()
    {
        if (currentStepIndex > 0)
        {
            int previousStep = currentStepIndex;
            currentStepIndex--;
            LogStepTransition(previousStep, currentStepIndex);
            UpdateStepVisibility();
        }
    }

    /// <summary>
    /// Logs current step information for debugging.
    /// </summary>
    public void LogCurrentStepInfo()
    {
        Debug.Log($"Current Step Index: {currentStepIndex}");
        Debug.Log($"Current Step Name: {GetStepName(steps[currentStepIndex])}");
        Debug.Log($"Step Count: {steps.Count}");
        Debug.Log($"Validation State: {ValidateCurrentStep()}");
    }

#if UNITY_EDITOR
[ContextMenu("Log Current Step Info")]
private void LogCurrentStepInfoMenu()
{
    LogCurrentStepInfo();
}
#endif

    /// <summary>
    /// Validates the current step based on specific criteria.
    /// </summary>
    /// <returns>True if the current step is valid, false otherwise.</returns>
    private bool ValidateCurrentStep()
    {
        string currentStep = steps[currentStepIndex];

        // Additional validation logic based on the current step
        switch (currentStep)
        {
            case "step4": // Media Type
                var mediaType = root.Q<DropdownField>("mediaTypeDropdown")?.value;
                var widthField = root.Q<FloatField>("widthField")?.value ?? 0;
                var heightField = root.Q<FloatField>("heightField")?.value ?? 0;

                return !string.IsNullOrEmpty(mediaType) && widthField > 0 && heightField > 0;

            case "step6": // Book Structure
                var bookStructure = root.Q<TreeView>("bookStructureTree");
                return bookStructure != null && bookStructure.childCount > 0;

            default:
                return stepValidation[currentStep];
        }
    }

    /// <summary>
    /// Updates the state of navigation buttons based on the current step.
    /// </summary>
    private void UpdateNavigationButtons()
    {
        if (prevStepButton != null)
        {
            prevStepButton.SetEnabled(currentStepIndex > 0);
        }

        if (nextStepButton != null)
        {
            bool isLastStep = currentStepIndex == steps.Count - 1;
            nextStepButton.text = isLastStep ? "Finish" : "Next";
            nextStepButton.SetEnabled(!isLastStep);
        }
    }

    /// <summary>
    /// Updates the label displaying the current step information.
    /// </summary>
    private void UpdateStepLabel()
    {
        if (currentStepLabel != null)
        {
            string stepName = GetStepName(steps[currentStepIndex]);
            currentStepLabel.text = $"Step {currentStepIndex + 1} of {steps.Count}: {stepName}";
        }
    }

    /// <summary>
    /// Updates the visibility of steps based on the current step index.
    /// Handles transition animations and UI updates.
    /// </summary>
    private void UpdateStepVisibility()
    {
        foreach (string step in steps)
        {
            var stepContainer = root.Q<VisualElement>(step);
            if (stepContainer != null)
            {
                bool isCurrentStep = steps[currentStepIndex] == step;
                stepContainer.RemoveFromClassList("fade-enter");
                stepContainer.RemoveFromClassList("fade-exit");
                stepContainer.RemoveFromClassList("active");

                if (isCurrentStep)
                {
                    stepContainer.style.display = DisplayStyle.Flex;
                    stepContainer.AddToClassList("active");
                    stepContainer.AddToClassList("fade-enter");
                    Debug.Log($"Showing step: {step}");
                }
                else
                {
                    stepContainer.style.display = DisplayStyle.None;
                    stepContainer.AddToClassList("fade-exit");
                }
            }
        }

        UpdateNavigationButtons();
        UpdateStepLabel();
    }

    /// <summary>
    /// Resets the workflow to the initial state.
    /// Clears all validation states except for the first step.
    /// </summary>
    public void ResetWorkflow()
    {
        currentStepIndex = 0;
        foreach (string step in steps)
        {
            stepValidation[step] = false;
        }
        stepValidation["step1"] = true;
        UpdateStepVisibility();
    }

    /// <summary>
    /// Cleans up event handlers and WebGL-specific handlers when disabled.
    /// </summary>
    private void OnDisable()
    {
#if UNITY_WEBGL
            Application.lowMemory -= OnLowMemory;
#endif

        // Clean up navigation button events
        if (prevStepButton != null)
            prevStepButton.clicked -= PreviousStep;
        if (nextStepButton != null)
            nextStepButton.clicked -= NextStep;

        // Clean up main menu button events
        if (startButton != null)
            startButton.clicked -= OnStartButtonClicked;
        if (loadButton != null)
            loadButton.clicked -= OnLoadButtonClicked;
        if (optionsButton != null)
            optionsButton.clicked -= OnOptionsButtonClicked;
        if (saveProgressButton != null)
            saveProgressButton.clicked -= OnSaveProgressClicked;
        if (cancelButton != null)
            cancelButton.clicked -= OnCancelClicked;
    }

    /// <summary>
    /// Logs the state of buttons for debugging purposes.
    /// </summary>
    public void DebugButtonStates()
    {
        Debug.Log("=== Button States ===");
        Debug.Log($"Start Button: {(startButton != null ? "Found" : "Missing")}");
        Debug.Log($"Load Button: {(loadButton != null ? "Found" : "Missing")}");
        Debug.Log($"Options Button: {(optionsButton != null ? "Found" : "Missing")}");
        Debug.Log($"Save Progress Button: {(saveProgressButton != null ? "Found" : "Missing")}");
        Debug.Log($"Cancel Button: {(cancelButton != null ? "Found" : "Missing")}");
    }

    /// <summary>
    /// Saves the current progress of the workflow.
    /// </summary>
    public void SaveProgress()
    {
        // Implement save functionality
        Debug.Log("Saving progress...");
    }

    /// <summary>
    /// Loads previously saved progress.
    /// </summary>
    public void LoadProgress()
    {
        // Implement load functionality
        Debug.Log("Loading progress...");
    }
}