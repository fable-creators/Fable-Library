using UnityEngine;
using MegaBook;

/// <summary>
/// Manages page turn sound effects by integrating with the MegaBook system.
/// Monitors page position changes and triggers appropriate audio feedback.
/// </summary>
public class BookAudioController : MonoBehaviour
{
    /// <summary>
    /// Reference to the MegaBookBuilder component that handles page mechanics.
    /// </summary>
    [SerializeField] private MegaBookBuilder bookComponent;

    /// <summary>
    /// Reference to the AudioUploadController that handles sound playback.
    /// </summary>
    [SerializeField] private AudioUploadController audioController;

    /// <summary>
    /// Tracks the previous page number to detect meaningful changes in page position.
    /// </summary>
    private float lastPage = 0;

    /// <summary>
    /// Monitors page position changes and triggers page turn sounds when appropriate.
    /// Uses a threshold of 0.1 to prevent multiple triggers during page turn animations.
    /// </summary>
    private void Update()
    {
        if (bookComponent != null && Mathf.Abs(bookComponent.page - lastPage) > 0.1f)
        {
            audioController?.PlayPageTurnSound();
            lastPage = bookComponent.page;
        }
    }
}
