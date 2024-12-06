using UnityEngine;

/// <summary>
/// Handles image upload events and delegates them to the ImageUploadController component.
/// Acts as a bridge between WebGL JavaScript callbacks and Unity components.
/// </summary>
[Tooltip("Handles WebGL image upload callbacks")]
public class MessageReceiver : MonoBehaviour
{
    /// <summary>
    /// Reference to the ImageUploadController component that handles image processing.
    /// </summary>
    private ImageUploadController imageUploadController;

    /// <summary>
    /// Initializes by obtaining the ImageUploadController from the same GameObject.
    /// </summary>
    void Start()
    {
        imageUploadController = GetComponent<ImageUploadController>();
    }

    /// <summary>
    /// Called when an image is uploaded through WebGL.
    /// Passes the blob URL to the ImageUploadController for processing.
    /// </summary>
    /// <param name="blobUrl">The URL of the uploaded image blob in WebGL context.</param>
    public void OnImageUploaded(string blobUrl)
    {
        if (imageUploadController != null)
        {
            imageUploadController.OnImageUploaded(blobUrl);
        }
        else
        {
            Debug.LogError("ImageUploadController not found!");
        }
    }
}