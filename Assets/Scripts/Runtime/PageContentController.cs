using UnityEngine;
using UnityEngine.UIElements;
using MegaBook;
using System.Collections.Generic;
using TMPro;

/// <summary>
/// Controls the text and content display on book pages.
/// Manages text rendering, content types, and page content synchronization.
/// </summary>
[Tooltip("Manages page content and text rendering")]
public class PageContentController : MonoBehaviour
{
    /// <summary>
    /// Reference to the UI document containing content controls.
    /// </summary>
    [Tooltip("UI Document containing content controls")]
    [SerializeField] private UIDocument document;

    /// <summary>
    /// Reference to the book component for page management.
    /// </summary>
    [Tooltip("Book component to update content on")]
    [SerializeField] private MegaBookBuilder bookComponent;

    /// <summary>
    /// Default font asset for text rendering.
    /// </summary>
    [Tooltip("Default font for text rendering")]
    [SerializeField] private TMP_FontAsset defaultFont;

    /// <summary>
    /// Default shader for rendering content.
    /// </summary>
    [Tooltip("Shader used for content rendering")]
    [SerializeField] private Shader defaultShader;

    private DropdownField contentTypeDropdown;
    private TextField pageTextField;
    private Dictionary<int, string> pageContents = new Dictionary<int, string>();

    private int currentPageIndex = -1;
    private TextMeshPro tmpText;
    private Camera renderCamera;
    private bool isUpdating = false;
    private float lastUpdateTime;
    private const float UPDATE_INTERVAL = 0.1f;

    private Material defaultMaterial;

    /// <summary>
    /// Initializes default shader and material.
    /// </summary>
    private void Awake()
    {
        if (defaultShader == null)
        {
            defaultShader = Shader.Find("Universal Render Pipeline/Lit");
            if (defaultShader == null)
                defaultShader = Shader.Find("Standard");
        }
        defaultMaterial = new Material(defaultShader);
    }

    /// <summary>
    /// Sets up UI and registers event callbacks.
    /// </summary>
    private void OnEnable()
    {
        if (document == null) document = GetComponent<UIDocument>();
        SetupTMP();
        SetupUI();
        RegisterCallbacks();
    }

    /// <summary>
    /// Updates page content based on the current page index.
    /// Throttles updates using UPDATE_INTERVAL.
    /// </summary>
    private void Update()
    {
        if (bookComponent != null && !isUpdating && Time.time - lastUpdateTime > UPDATE_INTERVAL)
        {
            int newPageIndex = Mathf.RoundToInt(bookComponent.page * 2);
            if (newPageIndex != currentPageIndex)
            {
                currentPageIndex = newPageIndex;
                UpdatePageContent();
                StartCoroutine(RefreshBook());
            }
            lastUpdateTime = Time.time;
        }
    }

    /// <summary>
    /// Refreshes the book content asynchronously.
    /// Prevents multiple simultaneous updates.
    /// </summary>
    private System.Collections.IEnumerator RefreshBook()
    {
        if (isUpdating) yield break;
        isUpdating = true;

        if (pageContents.TryGetValue(currentPageIndex, out string content))
        {
            var texture = CreateTextureWithTMP(content);
            if (texture != null)
            {
                ApplyTextureToPage(texture);
            }
        }

        isUpdating = false;
    }

    /// <summary>
    /// Configures the UI elements and content type options.
    /// </summary>
    private void SetupUI()
    {
        var root = document.rootVisualElement;

        contentTypeDropdown = root.Q<DropdownField>("contentTypeDropdown");
        pageTextField = root.Q<TextField>("contentTextField");

        if (contentTypeDropdown != null)
        {
            contentTypeDropdown.choices = new List<string> {
                "Text Only",
                "Image Only",
                "Text + Image Layout",
                "Custom Texture"
            };
            contentTypeDropdown.value = "Text Only";
        }

        Debug.Log($"UI Setup - Dropdown: {contentTypeDropdown != null}, TextField: {pageTextField != null}");
    }

    /// <summary>
    /// Registers callbacks for UI interactions.
    /// </summary>
    private void RegisterCallbacks()
    {
        if (pageTextField != null)
        {
            pageTextField.RegisterValueChangedCallback(evt =>
            {
                if (!isUpdating)
                {
                    OnPageTextChanged(evt.newValue);
                }
            });
        }

        if (contentTypeDropdown != null)
        {
            contentTypeDropdown.RegisterValueChangedCallback(evt => OnContentTypeChanged(evt.newValue));
        }
    }

    /// <summary>
    /// Updates the text content for the current page.
    /// </summary>
    /// <param name="newText">New text content to apply.</param>
    private void OnPageTextChanged(string newText)
    {
        if (currentPageIndex < 0 || bookComponent == null) return;

        Debug.Log($"Updating text for page {currentPageIndex}: {newText}");
        pageContents[currentPageIndex] = newText;
        StartCoroutine(DelayedTextureUpdate(newText));
    }

    /// <summary>
    /// Delays the texture update to ensure UI changes are applied.
    /// </summary>
    /// <param name="text">Text to render as texture.</param>
    private System.Collections.IEnumerator DelayedTextureUpdate(string text)
    {
        yield return null;
        var texture = CreateTextureWithTMP(text);
        if (texture != null)
        {
            ApplyTextureToPage(texture);
        }
    }

    /// <summary>
    /// Sets up TextMeshPro for rendering text.
    /// Configures text rendering parameters and camera.
    /// </summary>
    private void SetupTMP()
    {
        var tmpObj = new GameObject("TMP_Renderer");
        tmpObj.transform.parent = transform;
        tmpText = tmpObj.AddComponent<TextMeshPro>();
        tmpText.font = defaultFont;
        tmpText.fontSize = 144;
        tmpText.alignment = TextAlignmentOptions.Center;
        tmpText.textWrappingMode = TextWrappingModes.Normal;
        tmpText.overflowMode = TextOverflowModes.Overflow;
        tmpText.rectTransform.sizeDelta = new Vector2(1024, 512);
        tmpText.margin = new Vector4(25, 25, 25, 25);
        tmpText.color = Color.black;
        tmpText.fontStyle = FontStyles.Normal;
        tmpText.lineSpacing = 10;
        tmpText.paragraphSpacing = 20;
        tmpObj.SetActive(false);

        var cameraObj = new GameObject("TMP_Camera");
        cameraObj.transform.parent = transform;
        renderCamera = cameraObj.AddComponent<Camera>();
        renderCamera.orthographic = true;
        renderCamera.orthographicSize = 256;
        renderCamera.clearFlags = CameraClearFlags.SolidColor;
        renderCamera.backgroundColor = Color.white;
        cameraObj.SetActive(false);
    }

    /// <summary>
    /// Creates a texture from text using TextMeshPro.
    /// </summary>
    /// <param name="text">Text to render.</param>
    /// <returns>Texture2D containing rendered text.</returns>
    private Texture2D CreateTextureWithTMP(string text)
    {
        try
        {
            Debug.Log("Creating texture with TMP...");
            int width = 1024;
            int height = 512;

            RenderTexture rt = RenderTexture.GetTemporary(width, height, 24);
            rt.antiAliasing = 1;

            var cameraObj = new GameObject("TempCamera");
            var tempCamera = cameraObj.AddComponent<Camera>();
            tempCamera.orthographic = true;
            tempCamera.orthographicSize = height / 2f;
            tempCamera.backgroundColor = Color.white;
            tempCamera.clearFlags = CameraClearFlags.SolidColor;
            tempCamera.cullingMask = LayerMask.GetMask("UI");
            tempCamera.transform.position = new Vector3(0, 0, -10);
            tempCamera.targetTexture = rt;

            var textObj = new GameObject("TempText");
            textObj.layer = LayerMask.NameToLayer("UI");
            var textComponent = textObj.AddComponent<TextMeshPro>();
            textComponent.font = defaultFont;
            textComponent.fontSize = 144;
            textComponent.alignment = TextAlignmentOptions.Center;
            textComponent.textWrappingMode = TextWrappingModes.Normal;
            textComponent.rectTransform.sizeDelta = new Vector2(width, height);
            textComponent.text = text;
            textComponent.color = Color.black;

            textObj.transform.position = new Vector3(0, 0, 0);

            tempCamera.Render();

            Texture2D texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
            RenderTexture.active = rt;
            texture.ReadPixels(new Rect(0, 0, width, height), 0, 0);
            texture.Apply();

            RenderTexture.active = null;
            tempCamera.targetTexture = null;
            RenderTexture.ReleaseTemporary(rt);
            GameObject.DestroyImmediate(cameraObj);
            GameObject.DestroyImmediate(textObj);

            Debug.Log("Texture created successfully");
            return texture;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error creating texture: {e.Message}\n{e.StackTrace}");
            return null;
        }
    }

    /// <summary>
    /// Applies the generated texture to the current page.
    /// Creates or updates materials as needed.
    /// </summary>
    /// <param name="texture">Texture to apply to the page.</param>
    private void ApplyTextureToPage(Texture2D texture)
    {
        bool isFrontPage = (currentPageIndex % 2) == 0;
        int pageParamIndex = currentPageIndex / 2;

        Debug.Log($"Applying texture to page {pageParamIndex} ({(isFrontPage ? "Front" : "Back")})");

        while (bookComponent.pageparams.Count <= pageParamIndex)
        {
            bookComponent.pageparams.Add(new MegaBookPageParams());
        }

        var pageParam = bookComponent.pageparams[pageParamIndex];

        if (isFrontPage)
        {
            if (pageParam.frontmat == null)
            {
                pageParam.frontmat = new Material(defaultShader);
            }
            pageParam.front = texture;
            pageParam.frontmat.mainTexture = texture;
            pageParam.frontmat.SetTexture("_MainTex", texture);
            pageParam.frontmat.SetTexture("_BaseMap", texture);
        }
        else
        {
            if (pageParam.backmat == null)
            {
                pageParam.backmat = new Material(defaultShader);
            }
            pageParam.back = texture;
            pageParam.backmat.mainTexture = texture;
            pageParam.backmat.SetTexture("_MainTex", texture);
            pageParam.backmat.SetTexture("_BaseMap", texture);
        }

        bookComponent.rebuildmeshes = true;
        bookComponent.rebuild = true;
        bookComponent.updateBindings = true;
    }

    /// <summary>
    /// Updates the page content in the UI.
    /// </summary>
    private void UpdatePageContent()
    {
        if (pageTextField != null && !isUpdating)
        {
            if (pageContents.TryGetValue(currentPageIndex, out string content))
            {
                pageTextField.value = content;
            }
            else
            {
                pageTextField.value = string.Empty;
            }
        }
    }

    /// <summary>
    /// Handles changes in content type selection.
    /// Adjusts UI visibility based on content type.
    /// </summary>
    /// <param name="newType">Selected content type.</param>
    private void OnContentTypeChanged(string newType)
    {
        pageTextField.style.display = newType == "Text Only" ? DisplayStyle.Flex : DisplayStyle.None;
        Debug.Log($"Content type changed to: {newType}");
    }

    /// <summary>
    /// Cleans up resources when the component is disabled.
    /// </summary>
    private void OnDisable()
    {
        if (tmpText != null) DestroyImmediate(tmpText.gameObject);
        if (renderCamera != null) DestroyImmediate(renderCamera.gameObject);
    }
}