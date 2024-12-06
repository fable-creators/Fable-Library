using UnityEngine;
using UnityEngine.UIElements;
using MegaBook;
using System.Collections.Generic;

/// <summary>
/// Controls the appearance and behavior of different media types (Book, Magazine, Newspaper, etc.).
/// Manages media type switching, presets, and associated visual configurations.
/// </summary>
[Tooltip("Manages different media type configurations and transitions")]
public class MediaTypeController : MonoBehaviour
{
    /// <summary>
    /// Core UI document reference.
    /// </summary>
    [Tooltip("UI Document containing media type controls")]
    [SerializeField] private UIDocument document;

    /// <summary>
    /// Reference to the main book builder component.
    /// </summary>
    [Tooltip("Main book builder component to configure")]
    [SerializeField] private MegaBookBuilder bookComponent;

    /// <summary>
    /// Reference to page settings controller for synchronization.
    /// </summary>
    [Tooltip("Controller for page-specific settings")]
    [SerializeField] private PageSettingsController pageSettingsController;

    /// <summary>
    /// Reference to book structure controller for updates.
    /// </summary>
    [Tooltip("Controller for book structure management")]
    [SerializeField] private BookStructureController bookStructureController;

    /// <summary>
    /// Reference to page slider controller for navigation.
    /// </summary>
    [Tooltip("Controller for page navigation")]
    [SerializeField] private PageSliderController pageSliderController;

    /// <summary>
    /// Reference to materials controller for appearance updates.
    /// </summary>
    [Tooltip("Controller for material management")]
    [SerializeField] private MaterialsController materialsController;

    /// <summary>
    /// Book binding component references.
    /// </summary>
    [Tooltip("References to book binding components")]
    [SerializeField] private GameObject coverObject;
    [SerializeField] private GameObject headband1Object;
    [SerializeField] private GameObject headband2Object;
    [SerializeField] private GameObject spineFabricObject;

    // Cached textures for different media types
    private Texture2D magazineCoverTexture;
    private Texture2D newspaperCoverTexture;
    private Texture2D posterCoverTexture;
    private Texture2D mangaCoverTexture;

    private Texture2D defaultMagazinePageTexture;
    private Texture2D defaultNewspaperPageTexture;
    private Texture2D defaultPosterPageTexture;
    private Texture2D defaultMangaPageTexture;

    private DropdownField mediaTypeDropdown;
    private bool isRightToLeft = false;

    /// <summary>
    /// Initializes UI, loads textures, and sets up initial book state.
    /// </summary>
    private void OnEnable()
    {
        if (document == null) document = GetComponent<UIDocument>();
        LoadPlaceholderTextures();
        mediaTypeDropdown = document.rootVisualElement.Q<DropdownField>("mediaTypeDropdown");

        if (mediaTypeDropdown != null)
        {
            // Update dropdown choices
            mediaTypeDropdown.choices = new List<string> {
                "Book",
                "Magazine",
                "Newspaper",
                "Poster",
                "Manga"
            };

            PreserveAndInitializeBook();
            mediaTypeDropdown.RegisterValueChangedCallback(OnMediaTypeChanged);
            mediaTypeDropdown.value = "Book";
        }

        // Find and store references to cover and binding objects
        if (coverObject == null)
            coverObject = bookComponent?.transform.Find("Cover")?.gameObject;
        if (headband1Object == null)
            headband1Object = bookComponent?.transform.Find("Headband1")?.gameObject;
        if (headband2Object == null)
            headband2Object = bookComponent?.transform.Find("Headband2")?.gameObject;
        if (spineFabricObject == null)
            spineFabricObject = bookComponent?.transform.Find("SpineFabric")?.gameObject;
    }

    /// <summary>
    /// Preserves existing book parameters when changing media types.
    /// Ensures smooth transitions between different media types.
    /// </summary>
    private void PreserveAndInitializeBook()
    {
        if (bookComponent == null) return;

        // Preserve existing page count if available
        int currentPages = bookComponent.NumPages > 0 ? bookComponent.NumPages : 15;

        // Initialize params list if null while preserving existing
        if (bookComponent.pageparams == null)
        {
            bookComponent.pageparams = new List<MegaBookPageParams>();
        }

        // Keep existing params and add more if needed
        int existingCount = bookComponent.pageparams.Count;
        int targetCount = Mathf.Max(60, currentPages); // Use max of 60 or current pages

        // Only add new params for missing slots
        for (int i = existingCount; i < targetCount; i++)
        {
            bookComponent.pageparams.Add(new MegaBookPageParams());
        }

        // Gentle rebuild without forcing values
        bookComponent.updateBindings = true;
    }

    /// <summary>
    /// Creates a new book with default parameters.
    /// Used when no existing book state needs to be preserved.
    /// </summary>
    private void InitializeBook()
    {
        if (bookComponent == null) return;

        // Reset core parameters first
        bookComponent.NumPages = 15; // Default starting value
        bookComponent.pageparams = new List<MegaBookPageParams>();

        // Pre-populate params
        for (int i = 0; i < 60; i++) // Use max possible pages to prevent resize issues
        {
            bookComponent.pageparams.Add(new MegaBookPageParams());
        }

        // Force initial rebuild
        bookComponent.rebuildmeshes = true;
        bookComponent.rebuild = true;
        bookComponent.updateBindings = true;
    }

    /// <summary>
    /// Handles media type changes from the dropdown.
    /// Applies appropriate presets and textures for the selected media type.
    /// </summary>
    /// <param name="evt">Change event containing the new media type.</param>
    private void OnMediaTypeChanged(ChangeEvent<string> evt)
    {
        if (bookComponent == null) return;

        EnsurePageParams();

        // Reset to first page for non-manga types
        if (evt.newValue != "Manga" && isRightToLeft)
        {
            isRightToLeft = false;
            bookComponent.page = 0;
        }


        switch (evt.newValue)
        {
            case "Book":
                ApplyBookPreset();
                ClearAllPageTextures();
                ShowCoverAndBinding(true);
                break;
            case "Magazine":
                ApplyMagazinePreset();
                ApplyDefaultPagesToBook(defaultMagazinePageTexture);
                ApplyFrontPageTexture(magazineCoverTexture);

                ShowCoverAndBinding(false);
                break;
            case "Newspaper":
                ApplyNewspaperPreset();
                ApplyDefaultPagesToBook(defaultNewspaperPageTexture);
                ApplyFrontPageTexture(newspaperCoverTexture);

                ShowCoverAndBinding(false);
                break;
            case "Poster":
                ApplyPosterPreset();
                ApplyDefaultPagesToBook(defaultPosterPageTexture);
                ApplyFrontPageTexture(posterCoverTexture);

                ShowCoverAndBinding(false);
                break;
            case "Manga":
                ApplyMangaPreset();
                ApplyDefaultPagesToBook(defaultMangaPageTexture);
                ShowCoverAndBinding(false);
                break;
        }
    }

    /// <summary>
    /// Controls visibility of book binding elements (cover, headbands, spine).
    /// </summary>
    /// <param name="show">Whether to show or hide binding elements.</param>
    private void ShowCoverAndBinding(bool show)
    {
        if (coverObject != null) coverObject.SetActive(show);
        if (headband1Object != null) headband1Object.SetActive(show);
        if (headband2Object != null) headband2Object.SetActive(show);
        if (spineFabricObject != null) spineFabricObject.SetActive(show);
    }

    /// <summary>
    /// Loads placeholder textures for different media types from Resources folder.
    /// </summary>
    private void LoadPlaceholderTextures()
    {
        magazineCoverTexture = Resources.Load<Texture2D>("Textures/MagazinePlaceholder");
        if (magazineCoverTexture == null)
        {
            Debug.LogError("Failed to load magazine placeholder texture from Resources/Textures/MagazinePlaceholder");
        }

        // Load other placeholder textures when you have them
        newspaperCoverTexture = Resources.Load<Texture2D>("Textures/NewspaperPlaceholder");
        posterCoverTexture = Resources.Load<Texture2D>("Textures/PosterPlaceholder");
        mangaCoverTexture = Resources.Load<Texture2D>("Textures/MangaPlaceholder");

        // Load default page textures
        defaultMagazinePageTexture = Resources.Load<Texture2D>("Textures/MagazinePagePlaceholder");
        defaultNewspaperPageTexture = Resources.Load<Texture2D>("Textures/NewspaperPagePlaceholder");
        defaultPosterPageTexture = Resources.Load<Texture2D>("Textures/PosterPagePlaceholder");
        defaultMangaPageTexture = Resources.Load<Texture2D>("Textures/MangaPagePlaceholder");
    }

    /// <summary>
    /// Applies a texture to the front page of the book.
    /// </summary>
    /// <param name="texture">Texture to apply to the front page.</param>
    private void ApplyFrontPageTexture(Texture2D texture)
    {
        if (texture == null || bookComponent == null) return;

        // Ensure we have at least one page parameter
        while (bookComponent.pageparams.Count == 0)
        {
            bookComponent.pageparams.Add(new MegaBookPageParams());
        }

        // Create material if needed
        if (bookComponent.pageparams[0].frontmat == null)
        {
            bookComponent.pageparams[0].frontmat = new Material(Shader.Find("Unlit/Texture"));
        }

        // Apply texture to first page (front)
        bookComponent.pageparams[0].front = texture;
        bookComponent.pageparams[0].frontmat.mainTexture = texture;

        // Trigger rebuild
        bookComponent.rebuildmeshes = true;
        bookComponent.rebuild = true;
    }

    /// <summary>
    /// Applies the default texture to all pages in the book.
    /// </summary>
    /// <param name="defaultTexture">Default texture to apply to all pages.</param>
    private void ApplyDefaultPagesToBook(Texture2D defaultTexture)
    {
        if (bookComponent == null || defaultTexture == null) return;

        for (int i = 0; i < bookComponent.pageparams.Count; i++)
        {
            if (i < bookComponent.NumPages)  // Only apply to actual pages
            {
                var pageParam = bookComponent.pageparams[i];

                // Create materials if they don't exist
                if (pageParam.frontmat == null)
                    pageParam.frontmat = new Material(Shader.Find("Unlit/Texture"));
                if (pageParam.backmat == null)
                    pageParam.backmat = new Material(Shader.Find("Unlit/Texture"));

                // Apply textures
                pageParam.front = defaultTexture;
                pageParam.back = defaultTexture;
                pageParam.frontmat.mainTexture = defaultTexture;
                pageParam.backmat.mainTexture = defaultTexture;
            }
        }

        bookComponent.rebuildmeshes = true;
        bookComponent.rebuild = true;
    }

    /// <summary>
    /// Clears all page textures, setting them to white.
    /// </summary>
    private void ClearAllPageTextures()
    {
        if (bookComponent == null) return;

        // Create a white texture for blank pages
        var blankTexture = new Texture2D(1, 1);
        blankTexture.SetPixel(0, 0, Color.white);
        blankTexture.Apply();

        for (int i = 0; i < bookComponent.pageparams.Count; i++)
        {
            if (i < bookComponent.NumPages)
            {
                var pageParam = bookComponent.pageparams[i];

                if (pageParam.frontmat == null)
                    pageParam.frontmat = new Material(Shader.Find("Unlit/Texture"));
                if (pageParam.backmat == null)
                    pageParam.backmat = new Material(Shader.Find("Unlit/Texture"));

                pageParam.front = blankTexture;
                pageParam.back = blankTexture;
                pageParam.frontmat.mainTexture = blankTexture;
                pageParam.backmat.mainTexture = blankTexture;
            }
        }

        bookComponent.rebuildmeshes = true;
        bookComponent.rebuild = true;
    }

    /// <summary>
    /// Applies standard book dimensions and parameters.
    /// Sets up traditional book binding elements.
    /// </summary>
    private void ApplyBookPreset()
    {
        // General
        bookComponent.NumPages = 15;
        bookComponent.pageLength = 0.3f;
        bookComponent.pageWidth = 0.4f;
        bookComponent.pageHeight = 0.00325f;
        bookComponent.WidthSegs = 30;
        bookComponent.LengthSegs = 8;
        bookComponent.HeightSegs = 1;
        bookComponent.pageGap = 0.0032f;
        bookComponent.spineradius = 0.485f;
        bookComponent.bookthickness = 0.0448f;
        bookComponent.edgeUVSize = 0.22f;
        bookComponent.edgeUVOff = 0.21f;

        // Cover parameters
        bookComponent.autoFit = true;
        bookComponent.autoFitSize = new Vector3(1.03f, 1.20f, 1.03f);
        bookComponent.spineScale = 1f;
        bookComponent.coverScale = Vector3.one;

        float bindingVerticalOffset = bookComponent.pageLength * 0.5f;

        // Headband settings
        if (bookComponent.headband1 != null)
        {
            bookComponent.headband1.length = 1.0f;
            bookComponent.headband1.radius = 0.01f;
            bookComponent.headband1.segs = 8;
            bookComponent.headband1.sides = 8;
            bookComponent.headband1.transform.localPosition = new Vector3(0f, bindingVerticalOffset, 0f);
            bookComponent.headband1.BuildMesh();
        }

        if (bookComponent.headband2 != null)
        {
            bookComponent.headband2.length = 1.0f;
            bookComponent.headband2.radius = 0.01f;
            bookComponent.headband2.segs = 8;
            bookComponent.headband2.sides = 8;
            bookComponent.headband2.transform.localPosition = new Vector3(0f, -bindingVerticalOffset, 0f);
            bookComponent.headband2.BuildMesh();
        }

        // Spine Fabric
        if (bookComponent.spineFabric != null)
        {
            bookComponent.spineFabric.length = 1.0f;
            bookComponent.spineFabric.width = bookComponent.pageLength;
            bookComponent.spineFabric.thickness = 0.01f;
            bookComponent.spineFabric.segs = 8;
            bookComponent.spineFabric.transform.localPosition = Vector3.zero;
            bookComponent.spineFabric.BuildMesh();
        }

        // Set appropriate materials
        if (materialsController != null)
        {
            materialsController.SetMaterialsForType("Book", "Standard Paper");
        }

        UpdateBook();

        if (pageSettingsController != null)
        {
            pageSettingsController.UpdateUIFromBook();
        }
        if (bookStructureController != null)
        {
            bookStructureController.UpdateBookStructureFromBook();
        }
        if (pageSliderController != null)
        {
            pageSliderController.UpdateSliderFromBook();
        }
        // Update materials UI
        if (materialsController != null)
        {
            materialsController.OnPresetChanged();
        }

    }

    /// <summary>
    /// Applies magazine-specific dimensions and parameters.
    /// Configures for glossy paper and tighter binding.
    /// </summary>
    private void ApplyMagazinePreset()
    {
        // Standard US Magazine size (approximately 8.5" x 11")
        bookComponent.NumPages = 84;  // Typical magazine length
        bookComponent.pageLength = 0.28f;  // ~11 inches
        bookComponent.pageWidth = 0.216f;  // ~8.5 inches
        bookComponent.pageHeight = 0.00015f;  // Thinner, glossier pages

        // Higher segment count for smoother page turning
        bookComponent.WidthSegs = 35;  // More segments for smoother bending
        bookComponent.LengthSegs = 10;
        bookComponent.HeightSegs = 1;

        // Tighter binding for magazine-style feel
        bookComponent.pageGap = 0.00025f;  // Tighter page spacing
        bookComponent.spineradius = 0.35f;  // Less curved spine than a book
        bookComponent.bookthickness = 0.015f;  // Thinner overall profile
        bookComponent.edgeUVSize = 0.15f;
        bookComponent.edgeUVOff = 0.14f;

        // Cover parameters (though cover will be hidden)
        bookComponent.autoFit = false;
        bookComponent.autoFitSize = Vector3.one;
        bookComponent.spineScale = 0f;
        bookComponent.coverScale = Vector3.zero;

        // Keep minimal but valid dimensions for binding elements
        float bindingVerticalOffset = bookComponent.pageLength * 0.45f;

        // Minimal but valid headband settings
        if (bookComponent.headband1 != null)
        {
            bookComponent.headband1.length = 0.001f;  // Minimal but non-zero length
            bookComponent.headband1.radius = 0.001f;  // Minimal but non-zero radius
            bookComponent.headband1.segs = 4;         // Minimum segments for valid mesh
            bookComponent.headband1.sides = 4;        // Minimum sides for valid mesh
            bookComponent.headband1.transform.localPosition = new Vector3(0f, bindingVerticalOffset, -0.001f);
            bookComponent.headband1.BuildMesh();
        }

        if (bookComponent.headband2 != null)
        {
            bookComponent.headband2.length = 0.001f;  // Minimal but non-zero length
            bookComponent.headband2.radius = 0.001f;  // Minimal but non-zero radius
            bookComponent.headband2.segs = 4;         // Minimum segments for valid mesh
            bookComponent.headband2.sides = 4;        // Minimum sides for valid mesh
            bookComponent.headband2.transform.localPosition = new Vector3(0f, -bindingVerticalOffset, -0.001f);
            bookComponent.headband2.BuildMesh();
        }

        // Minimal but valid spine fabric settings
        if (bookComponent.spineFabric != null)
        {
            bookComponent.spineFabric.length = 0.001f;     // Minimal but non-zero length
            bookComponent.spineFabric.width = 0.001f;      // Minimal but non-zero width
            bookComponent.spineFabric.thickness = 0.001f;  // Minimal but non-zero thickness
            bookComponent.spineFabric.segs = 4;            // Minimum segments for valid mesh
            bookComponent.spineFabric.transform.localPosition = new Vector3(0f, 0f, -0.002f);
            bookComponent.spineFabric.BuildMesh();
        }
        // Set appropriate materials
        if (materialsController != null)
        {
            materialsController.SetMaterialsForType("Magazine", "Glossy Paper");
        }

        UpdateBookAndUI();
    }

    /// <summary>
    /// Applies newspaper-specific dimensions and parameters.
    /// Configures for thin pages and large format.
    /// </summary>
    private void ApplyNewspaperPreset()
    {
        // Larger format, thin pages
        bookComponent.NumPages = 8;
        bookComponent.pageLength = 0.58f; // Broadsheet format
        bookComponent.pageWidth = 0.38f;
        bookComponent.pageHeight = 0.0001f; // Very thin pages
        bookComponent.WidthSegs = 20;
        bookComponent.LengthSegs = 8;
        bookComponent.HeightSegs = 1;
        bookComponent.pageGap = 0.0002f;
        bookComponent.spineradius = 0.3f;
        bookComponent.bookthickness = 0.005f;
        bookComponent.edgeUVSize = 0.15f;
        bookComponent.edgeUVOff = 0.14f;

        // Simple cover parameters
        bookComponent.autoFit = false;
        bookComponent.autoFitSize = Vector3.one;
        bookComponent.spineScale = 0f;
        bookComponent.coverScale = Vector3.zero;

        // Set appropriate materials
        if (materialsController != null)
        {
            materialsController.SetMaterialsForType("Newspaper", "Newspaper");
        }
        UpdateBookAndUI();
    }

    /// <summary>
    /// Applies poster-specific dimensions and parameters.
    /// Configures for single-sheet display.
    /// </summary>
    private void ApplyPosterPreset()
    {
        // Single large sheet
        bookComponent.NumPages = 1; // Front and back only
        bookComponent.pageLength = 0.84f; // A1 size ratio
        bookComponent.pageWidth = 0.59f;
        bookComponent.pageHeight = 0.00005f; // Extra thin
        bookComponent.WidthSegs = 15;
        bookComponent.LengthSegs = 8;
        bookComponent.HeightSegs = 1;
        bookComponent.pageGap = 0.0001f;
        bookComponent.spineradius = 0.2f;
        bookComponent.bookthickness = 0.001f;
        bookComponent.edgeUVSize = 0.1f;
        bookComponent.edgeUVOff = 0.09f;

        // No cover parameters
        bookComponent.autoFit = false;
        bookComponent.autoFitSize = Vector3.one;
        bookComponent.spineScale = 0f;
        bookComponent.coverScale = Vector3.zero;

        // Set appropriate materials
        if (materialsController != null)
        {
            materialsController.SetMaterialsForType("Poster", "Glossy Paper");
        }
        UpdateBookAndUI();
    }

    /// <summary>
    /// Applies manga-specific dimensions and parameters.
    /// Configures for right-to-left reading and manga-style binding.
    /// </summary>
    private void ApplyMangaPreset()
    {
        // Set right-to-left page turning
        isRightToLeft = true;

        // Typical manga dimensions
        bookComponent.NumPages = 80;
        bookComponent.pageLength = 0.18f; // B6 size approximation
        bookComponent.pageWidth = 0.13f;
        bookComponent.pageHeight = 0.0002f;
        bookComponent.WidthSegs = 25;
        bookComponent.LengthSegs = 8;
        bookComponent.HeightSegs = 1;
        bookComponent.pageGap = 0.0003f;
        bookComponent.spineradius = 0.35f;
        bookComponent.bookthickness = 0.025f;
        bookComponent.edgeUVSize = 0.18f;
        bookComponent.edgeUVOff = 0.17f;

        // Simple binding parameters
        bookComponent.autoFit = false;
        bookComponent.autoFitSize = Vector3.one;
        bookComponent.spineScale = 0f;
        bookComponent.coverScale = Vector3.zero;

        // Set appropriate materials
        if (materialsController != null)
        {
            materialsController.SetMaterialsForType("Manga", "Standard Paper");
        }
        // Keep minimal but valid dimensions for binding elements
        SetMinimalBindingElements();

        // Apply the manga cover to the last page and start there
        if (mangaCoverTexture != null)
        {
            ApplyTextureToLastPage(mangaCoverTexture);
        }

        // Set to last page
        bookComponent.page = bookComponent.NumPages;

        UpdateBookAndUI();
    }

    /// <summary>
    /// Updates book and related UI components.
    /// </summary>
    private void UpdateBookAndUI()
    {
        UpdateBook();

        if (pageSettingsController != null)
            pageSettingsController.UpdateUIFromBook();
        if (bookStructureController != null)
            bookStructureController.UpdateBookStructureFromBook();
        if (pageSliderController != null)
            pageSliderController.UpdateSliderFromBook();
        if (materialsController != null)
            materialsController.OnPresetChanged();
    }

    /// <summary>
    /// Sets minimal dimensions for binding elements.
    /// Used for media types that don't need full binding visualization.
    /// </summary>
    private void SetMinimalBindingElements()
    {
        float bindingVerticalOffset = bookComponent.pageLength * 0.45f;

        if (bookComponent.headband1 != null)
        {
            bookComponent.headband1.length = 0.001f;
            bookComponent.headband1.radius = 0.001f;
            bookComponent.headband1.segs = 4;
            bookComponent.headband1.sides = 4;
            bookComponent.headband1.transform.localPosition = new Vector3(0f, bindingVerticalOffset, -0.001f);
            bookComponent.headband1.BuildMesh();
        }

        if (bookComponent.headband2 != null)
        {
            bookComponent.headband2.length = 0.001f;
            bookComponent.headband2.radius = 0.001f;
            bookComponent.headband2.segs = 4;
            bookComponent.headband2.sides = 4;
            bookComponent.headband2.transform.localPosition = new Vector3(0f, -bindingVerticalOffset, -0.001f);
            bookComponent.headband2.BuildMesh();
        }

        if (bookComponent.spineFabric != null)
        {
            bookComponent.spineFabric.length = 0.001f;
            bookComponent.spineFabric.width = 0.001f;
            bookComponent.spineFabric.thickness = 0.001f;
            bookComponent.spineFabric.segs = 4;
            bookComponent.spineFabric.transform.localPosition = new Vector3(0f, 0f, -0.002f);
            bookComponent.spineFabric.BuildMesh();
        }
    }

    /// <summary>
    /// Applies a texture to the last page.
    /// Specifically used for manga covers which appear at the end.
    /// </summary>
    /// <param name="texture">Texture to apply to the last page.</param>
    private void ApplyTextureToLastPage(Texture2D texture)
    {
        if (texture == null || bookComponent == null) return;

        int lastPageIndex = (bookComponent.NumPages) - 1;

        // Ensure we have enough page parameters
        while (bookComponent.pageparams.Count <= lastPageIndex)
        {
            bookComponent.pageparams.Add(new MegaBookPageParams());
        }

        // Create material if needed
        if (bookComponent.pageparams[lastPageIndex].backmat == null)
        {
            bookComponent.pageparams[lastPageIndex].backmat = new Material(Shader.Find("Unlit/Texture"));
        }

        // Apply texture to the back of the last page
        bookComponent.pageparams[lastPageIndex].back = texture;
        bookComponent.pageparams[lastPageIndex].backmat.mainTexture = texture;

        Debug.Log($"Applied manga cover to page index: {lastPageIndex}");

        // Trigger rebuild
        bookComponent.rebuildmeshes = true;
        bookComponent.rebuild = true;
    }

    /// <summary>
    /// Triggers book mesh rebuild and binding update.
    /// </summary>
    private void UpdateBook()
    {
        bookComponent.rebuildmeshes = true;
        bookComponent.rebuild = true;
        bookComponent.updateBindings = true;
    }

    /// <summary>
    /// Ensures sufficient page parameters exist for the current configuration.
    /// </summary>
    private void EnsurePageParams()
    {
        if (bookComponent.pageparams == null || bookComponent.pageparams.Count == 0)
        {
            Debug.Log("Empty Book Found");
            InitializeBook();
            return;
        }

        while (bookComponent.pageparams.Count < bookComponent.NumPages)
        {
            bookComponent.pageparams.Add(new MegaBookPageParams());
        }
    }
}