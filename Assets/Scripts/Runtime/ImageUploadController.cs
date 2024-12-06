using UnityEngine;
using UnityEngine.UIElements;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine.Networking;
using MegaBook;

/// <summary>
/// Manages image upload and application to book pages.
/// Handles both WebGL and Editor-specific upload mechanisms.
/// </summary>
[Tooltip("Manages image uploads and application to book pages")]
public class ImageUploadController : MonoBehaviour
{
    /// <summary>
    /// Singleton instance for global access.
    /// </summary>
    public static ImageUploadController Instance { get; private set; }

    /// <summary>
    /// Reference to the UI document containing upload controls.
    /// </summary>
    [Tooltip("UI Document containing upload controls")]
    [SerializeField] private UIDocument document;

    /// <summary>
    /// Reference to the book component to apply images to.
    /// </summary>
    [Tooltip("Book component to apply images to")]
    [SerializeField] private MegaBookBuilder bookComponent;

    /// <summary>
    /// Default shader for rendering uploaded images.
    /// </summary>
    [Tooltip("Shader used for rendering uploaded images")]
    [SerializeField] private Shader defaultShader;

    /// <summary>
    /// UI element for drag and drop functionality.
    /// </summary>
    private Box imageDropArea;

    /// <summary>
    /// Cache of uploaded images mapped to page indices.
    /// </summary>
    private Dictionary<int, Texture2D> pageImages = new Dictionary<int, Texture2D>();

    /// <summary>
    /// Currently selected page index.
    /// </summary>
    private int currentPageIndex = -1;

    /// <summary>
    /// Default material instance for rendering images.
    /// </summary>
    private Material defaultMaterial;

    /// <summary>
    /// External JavaScript function for opening file picker in WebGL.
    /// </summary>
    [DllImport("__Internal")]
    private static extern void OpenFilePicker();

    /// <summary>
    /// External JavaScript function for handling drag events in WebGL.
    /// </summary>
    [DllImport("__Internal")]
    private static extern void HandleDragEvent(string eventName);

    /// <summary>
    /// External JavaScript function for showing file input dialog in WebGL.
    /// </summary>
    [DllImport("__Internal")]
    private static extern void ShowFileInput(string gameObjectName, string methodName, string accept);

    /// <summary>
    /// Initializes the singleton instance and default shader/material.
    /// </summary>
    private void Awake()
    {
        Instance = this;
        if (defaultShader == null)
        {
            defaultShader = Shader.Find("Universal Render Pipeline/Lit");
            if (defaultShader == null)
                defaultShader = Shader.Find("Standard");
        }
        defaultMaterial = new Material(defaultShader);
    }

    /// <summary>
    /// Sets up the UI and registers event callbacks.
    /// </summary>
    private void OnEnable()
    {
        if (document == null) document = GetComponent<UIDocument>();
        SetupUI();
        RegisterCallbacks();
    }

    /// <summary>
    /// Updates the current page index and image preview when the page changes.
    /// </summary>
    private void Update()
    {
        if (bookComponent != null)
        {
            int newPageIndex = Mathf.RoundToInt(bookComponent.page * 2);
            if (newPageIndex != currentPageIndex)
            {
                currentPageIndex = newPageIndex;
                UpdateImagePreview();
            }
        }
    }

    /// <summary>
    /// Configures the UI elements and their event handlers.
    /// </summary>
    private void SetupUI()
    {
        var root = document.rootVisualElement;
        imageDropArea = root.Q<Box>("imageDropArea");

        if (imageDropArea != null)
        {
            imageDropArea.RegisterCallback<ClickEvent>(evt =>
            {
#if UNITY_WEBGL && !UNITY_EDITOR
                ShowFileInput(gameObject.name, "OnImageUploaded", ".png,.jpg,.jpeg");
#else
                OnImageDropAreaClick(evt);
#endif
            });
        }
    }

    /// <summary>
    /// Callback for when an image is uploaded via WebGL.
    /// </summary>
    /// <param name="blobUrl">URL of the uploaded image blob.</param>
    public void OnImageUploaded(string blobUrl)
    {
        StartCoroutine(LoadImageFromBlob(blobUrl));
    }

    /// <summary>
    /// Loads an image from a blob URL and applies it to the current page.
    /// </summary>
    /// <param name="blobUrl">URL of the image blob to load.</param>
    private IEnumerator LoadImageFromBlob(string blobUrl)
    {
        using (UnityWebRequest www = UnityWebRequestTexture.GetTexture(blobUrl))
        {
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                Texture2D texture = DownloadHandlerTexture.GetContent(www);
                if (texture.width != 1024 || texture.height != 512)
                {
                    texture = ResizeTexture(texture, 1024, 512);
                }
                ApplyImageToPage(texture);
                pageImages[currentPageIndex] = texture;
            }
            else
            {
                Debug.LogError($"Error loading image: {www.error}");
            }
        }
    }

    /// <summary>
    /// Registers UI callbacks for content type changes.
    /// </summary>
    private void RegisterCallbacks()
    {
        var contentTypeDropdown = document.rootVisualElement.Q<DropdownField>("contentTypeDropdown");
        if (contentTypeDropdown != null)
        {
            contentTypeDropdown.RegisterValueChangedCallback(evt => OnContentTypeChanged(evt.newValue));
        }
    }

    /// <summary>
    /// Adjusts the visibility of the image drop area based on the selected content type.
    /// </summary>
    /// <param name="newType">Selected content type.</param>
    private void OnContentTypeChanged(string newType)
    {
        if (imageDropArea != null)
        {
            imageDropArea.style.display = (newType == "Image Only" || newType == "Text + Image Layout")
                ? DisplayStyle.Flex
                : DisplayStyle.None;
        }
    }

    /// <summary>
    /// Handles image selection in the Unity Editor.
    /// </summary>
    private void OnImageDropAreaClick(ClickEvent evt)
    {
#if UNITY_EDITOR
        string path = UnityEditor.EditorUtility.OpenFilePanel("Select Image", "", "png,jpg,jpeg");
        if (!string.IsNullOrEmpty(path))
        {
            StartCoroutine(LoadImageFromFile(path));
        }
#endif
    }

    #if UNITY_EDITOR
    // Loads an image from a file path and applies it to the current page
    private IEnumerator LoadImageFromFile(string path)
    {
        if (string.IsNullOrEmpty(path)) yield break;
        
        UnityWebRequest www = null;
        try
        {
            www = UnityWebRequestTexture.GetTexture("file://" + path);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error creating web request: {e.Message}");
            yield break;
        }

        using (www)
        {
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                try
                {
                    Texture2D texture = DownloadHandlerTexture.GetContent(www);
                    
                    if (texture.width != 1024 || texture.height != 512)
                    {
                        texture = ResizeTexture(texture, 1024, 512);
                    }
                    
                    ApplyImageToPage(texture);
                    pageImages[currentPageIndex] = texture;
                    
                    Debug.Log($"Image successfully loaded for page {currentPageIndex}");
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"Error processing image: {e.Message}");
                }
            }
            else
            {
                Debug.LogError($"Error loading image: {www.error}");
            }
        }
    }
#endif

    // Resizes a texture to the specified dimensions


    /// <summary>
    /// Resizes a texture to the specified dimensions.
    /// </summary>
    /// <param name="source">Source texture to resize.</param>
    /// <param name="targetWidth">Target width in pixels.</param>
    /// <param name="targetHeight">Target height in pixels.</param>
    /// <returns>Resized texture.</returns>
    private Texture2D ResizeTexture(Texture2D source, int targetWidth, int targetHeight)
    {
        RenderTexture rt = RenderTexture.GetTemporary(targetWidth, targetHeight);
        RenderTexture.active = rt;
        Graphics.Blit(source, rt);

        Texture2D result = new Texture2D(targetWidth, targetHeight);
        result.ReadPixels(new Rect(0, 0, targetWidth, targetHeight), 0, 0);
        result.Apply();

        RenderTexture.ReleaseTemporary(rt);
        return result;
    }

    /// <summary>
    /// Applies a texture to the current page in the book component.
    /// Creates or updates materials as needed.
    /// </summary>
    /// <param name="texture">Texture to apply to the page.</param>
    public void ApplyImageToPage(Texture2D texture)
    {
        bool isFrontPage = (currentPageIndex % 2) == 0;
        int pageParamIndex = currentPageIndex / 2;

        while (bookComponent.pageparams.Count <= pageParamIndex)
        {
            bookComponent.pageparams.Add(new MegaBookPageParams());
        }

        var pageParam = bookComponent.pageparams[pageParamIndex];

        if (isFrontPage)
        {
            pageParam.front = texture;
            if (pageParam.frontmat == null)
            {
                pageParam.frontmat = new Material(defaultShader);
            }
            pageParam.frontmat.mainTexture = texture;
            pageParam.frontmat.SetTexture("_MainTex", texture);
            pageParam.frontmat.SetTexture("_BaseMap", texture);
        }
        else
        {
            pageParam.back = texture;
            if (pageParam.backmat == null)
            {
                pageParam.backmat = new Material(defaultShader);
            }
            pageParam.backmat.mainTexture = texture;
            pageParam.backmat.SetTexture("_MainTex", texture);
            pageParam.backmat.SetTexture("_BaseMap", texture);
        }

        bookComponent.rebuildmeshes = true;
        bookComponent.rebuild = true;
        bookComponent.updateBindings = true;
    }

    /// <summary>
    /// Updates the image preview in the UI based on the current page.
    /// </summary>
    private void UpdateImagePreview()
    {
        if (pageImages.TryGetValue(currentPageIndex, out Texture2D texture))
        {
            imageDropArea.style.backgroundImage = new StyleBackground(texture);
        }
        else
        {
            imageDropArea.style.backgroundImage = null;
        }
    }

    /// <summary>
    /// Cleans up textures when the component is disabled.
    /// </summary>
    private void OnDisable()
    {
        foreach (var texture in pageImages.Values)
        {
            if (texture != null)
            {
                Destroy(texture);
            }
        }
        pageImages.Clear();
    }
}