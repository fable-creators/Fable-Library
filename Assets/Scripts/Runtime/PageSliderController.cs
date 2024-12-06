using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;
using MegaBook;

/// <summary>
/// Manages the slider UI element to control and display the current page in a MegaBook.
/// Handles page navigation and synchronization between the slider and book component.
/// </summary>
[Tooltip("Controls page navigation slider and synchronization")]
public class PageSliderController : MonoBehaviour
{
    /// <summary>
    /// Reference to the UI document containing slider controls.
    /// </summary>
    [Tooltip("UI Document containing slider controls")]
    [SerializeField] private UIDocument document;

    /// <summary>
    /// Reference to the book component for page synchronization.
    /// </summary>
    [Tooltip("Book component to synchronize with slider")]
    [SerializeField] private MegaBookBuilder bookComponent;

    /// <summary>
    /// Reference to the UI slider component for page navigation.
    /// </summary>
    [Tooltip("UI Slider for page navigation")]
    [SerializeField] private UnityEngine.UI.Slider pageSlider;

    /// <summary>
    /// Initializes the page slider and updates its range and value based on the book component.
    /// Attempts to find the slider in the scene if not already assigned.
    /// </summary>
    private void OnEnable()
    {
        if (pageSlider == null)
        {
            pageSlider = GameObject.Find("Page Slider")?.GetComponent<UnityEngine.UI.Slider>();
        }

        UpdateSliderFromBook();
    }

    /// <summary>
    /// Configures the slider's range and initial value based on the number of pages in the book.
    /// Sets the slider to use whole numbers and initializes its value to -1 (before the first page).
    /// </summary>
    public void UpdateSliderFromBook()
    {
        if (pageSlider != null && bookComponent != null)
        {
            int maxPages = bookComponent.NumPages + 1; // Includes an extra page for the end
            pageSlider.minValue = -1; // Represents a state before the first page
            pageSlider.maxValue = maxPages;
            pageSlider.wholeNumbers = true; // Ensures the slider snaps to whole numbers
            pageSlider.value = -1; // Initializes the slider to the starting position
        }
    }
}