using UnityEngine;
using UnityEngine.UIElements;
using MegaBook;
using System.Collections.Generic;

/// <summary>
/// Controls the materials and appearance of a book in a Unity scene.
/// Manages material selection, properties, and real-time updates for covers and pages.
/// </summary>
[Tooltip("Manages book materials and appearance settings")]
public class MaterialsController : MonoBehaviour
{
    /// <summary>
    /// Reference to the UI document containing material controls.
    /// </summary>
    [Tooltip("UI Document containing material controls")]
    [SerializeField] private UIDocument document;

    /// <summary>
    /// Reference to the book component for updating materials.
    /// </summary>
    [Tooltip("Book component to update materials on")]
    [SerializeField] private MegaBookBuilder bookComponent;

    /// <summary>
    /// Reference to the book cover component for material updates.
    /// </summary>
    [Tooltip("Book cover component for material changes")]
    [SerializeField] private MegaBookCover coverComponent;

    private DropdownField coverMaterialDropdown;
    private DropdownField pageMaterialDropdown;
    private Slider roughnessSlider;
    private Slider metallicSlider;

    /// <summary>
    /// Initializes the UI and registers event callbacks when the component is enabled.
    /// </summary>
    private void OnEnable()
    {
        if (document == null) document = GetComponent<UIDocument>();
        SetupUI();
        RegisterCallbacks();
        ReadInitialValues();
    }

    /// <summary>
    /// Sets up the UI elements and initializes dropdown choices and slider ranges.
    /// </summary>
    private void SetupUI()
    {
        var root = document.rootVisualElement;

        coverMaterialDropdown = root.Q<DropdownField>("coverMaterialDropdown");
        pageMaterialDropdown = root.Q<DropdownField>("pageMaterialDropdown");
        roughnessSlider = root.Q<Slider>("roughnessSlider");
        metallicSlider = root.Q<Slider>("metallicSlider");

        if (coverMaterialDropdown != null)
        {
            coverMaterialDropdown.choices = new List<string>
            {
                "Select to Change Cover",
                "Remove Cover",
                "Fabric",
                "Illustrated History",
                "Leather Blue",
                "Leather Brown",
                "Leather Green",
                "Leather Grey",
                "Leather",
                "Skull Crossbones"
            };
        }

        if (pageMaterialDropdown != null)
        {
            pageMaterialDropdown.choices = new List<string>
            {
                "Standard Paper",
                "Glossy Paper",
                "Newspaper"
            };
        }

        if (roughnessSlider != null)
        {
            roughnessSlider.lowValue = 0f;
            roughnessSlider.highValue = 1f;
        }

        if (metallicSlider != null)
        {
            metallicSlider.lowValue = 0f;
            metallicSlider.highValue = 1f;
        }
    }

    /// <summary>
    /// Reads initial material values from the book component and updates the UI.
    /// </summary>
    private void ReadInitialValues()
    {
        if (bookComponent == null) return;

        var cover = bookComponent.GetComponentInChildren<MegaBookCover>();
        if (cover != null)
        {
            var renderer = cover.GetComponent<Renderer>();
            if (renderer && renderer.material != null)
            {
                string currentCoverMaterial = renderer.material.name.Replace("(Clone)", "").Trim();
                coverMaterialDropdown.value = currentCoverMaterial;
            }
        }

        if (bookComponent.pageparams != null && bookComponent.pageparams.Count > 0)
        {
            if (bookComponent.pageparams[0].frontmat != null)
            {
                string currentPageMaterial = bookComponent.pageparams[0].frontmat.name.Replace("(Clone)", "").Trim();
                pageMaterialDropdown.value = currentPageMaterial;
            }
        }
    }

    /// <summary>
    /// Registers callbacks for UI element value changes.
    /// </summary>
    private void RegisterCallbacks()
    {
        if (coverMaterialDropdown != null)
            coverMaterialDropdown.RegisterValueChangedCallback(evt => OnCoverMaterialChanged(evt.newValue));

        if (pageMaterialDropdown != null)
            pageMaterialDropdown.RegisterValueChangedCallback(evt => OnPageMaterialChanged(evt.newValue));

        if (roughnessSlider != null)
            roughnessSlider.RegisterValueChangedCallback(evt => OnRoughnessChanged(evt.newValue));

        if (metallicSlider != null)
            metallicSlider.RegisterValueChangedCallback(evt => OnMetallicChanged(evt.newValue));
    }

    /// <summary>
    /// Handles changes to the cover material dropdown and updates the book cover.
    /// </summary>
    /// <param name="materialName">The name of the selected cover material.</param>
    private void OnCoverMaterialChanged(string materialName)
    {
        if (bookComponent == null || coverComponent == null || string.IsNullOrEmpty(materialName)) return;
        if (materialName == "Select to Change Cover") return;

        GameObject coverPrefab = Resources.Load<GameObject>($"MegaBook/Cover Styles/{materialName}");
        if (coverPrefab != null)
        {
            Transform parent = coverComponent.transform.parent;
            int siblingIndex = coverComponent.transform.GetSiblingIndex();

            DestroyImmediate(coverComponent.gameObject);

            GameObject newCover = Instantiate(coverPrefab, parent);
            newCover.transform.SetSiblingIndex(siblingIndex);

            var newCoverComponent = newCover.GetComponent<MegaBookCover>();
            if (newCoverComponent != null)
            {
                coverComponent = newCoverComponent;
                newCoverComponent.book = bookComponent;
                bookComponent.bookCover = newCoverComponent;

                newCover.transform.localRotation = Quaternion.Euler(0, 180, 0);

                if (newCoverComponent.front)
                    newCoverComponent.front.localRotation = Quaternion.Euler(0, 180, 0);
                if (newCoverComponent.back)
                    newCoverComponent.back.localRotation = Quaternion.Euler(0, 180, 0);
                if (newCoverComponent.spine)
                    newCoverComponent.spine.localRotation = Quaternion.Euler(0, 180, 0);
            }
        }

        bookComponent.rebuildmeshes = true;
        bookComponent.rebuild = true;
    }

    /// <summary>
    /// Handles changes to the page material dropdown and updates all page materials.
    /// </summary>
    /// <param name="materialName">The name of the selected page material.</param>
    private void OnPageMaterialChanged(string materialName)
    {
        if (bookComponent == null || string.IsNullOrEmpty(materialName)) return;

        Material newMaterial = Resources.Load<Material>($"Materials/Pages/{materialName}");
        if (newMaterial != null)
        {
            for (int i = 0; i < bookComponent.pageparams.Count; i++)
            {
                bookComponent.pageparams[i].frontmat = new Material(newMaterial);
                bookComponent.pageparams[i].backmat = new Material(newMaterial);
            }
        }

        bookComponent.rebuildmeshes = true;
        bookComponent.rebuild = true;
    }

    /// <summary>
    /// Handles changes to the roughness slider.
    /// </summary>
    /// <param name="value">New roughness value between 0 and 1.</param>
    private void OnRoughnessChanged(float value)
    {
        if (bookComponent == null) return;
        //bookComponent.coverMaterial.SetFloat("_Roughness", value);
    }

    /// <summary>
    /// Handles changes to the metallic slider.
    /// </summary>
    /// <param name="value">New metallic value between 0 and 1.</param>
    private void OnMetallicChanged(float value)
    {
        if (bookComponent == null) return;
        //bookComponent.coverMaterial.SetFloat("_Metallic", value);
    }

    /// <summary>
    /// Updates material values when a preset is changed.
    /// </summary>
    public void OnPresetChanged()
    {
        ReadInitialValues();
    }

    /// <summary>
    /// Sets materials based on the media type and page material type.
    /// </summary>
    /// <param name="mediaType">Type of media (Book, Magazine, Newspaper, etc.)</param>
    /// <param name="pageMaterialType">Type of page material to apply</param>
    public void SetMaterialsForType(string mediaType, string pageMaterialType)
    {
        if (bookComponent == null) return;

        if (pageMaterialDropdown != null)
        {
            pageMaterialDropdown.value = pageMaterialType;
        }

        if (coverMaterialDropdown != null)
        {
            switch (mediaType)
            {
                case "Book":
                    coverMaterialDropdown.value = "Leather";
                    break;
                case "Magazine":
                case "Newspaper":
                case "Poster":
                case "Manga":
                    coverMaterialDropdown.value = "Remove Cover";
                    break;
            }
        }

        Material pageMaterial = Resources.Load<Material>($"Materials/Pages/{pageMaterialType}");
        if (pageMaterial != null)
        {
            for (int i = 0; i < bookComponent.pageparams.Count; i++)
            {
                if (bookComponent.pageparams[i].frontmat == null)
                {
                    bookComponent.pageparams[i].frontmat = new Material(pageMaterial);
                }
                else
                {
                    bookComponent.pageparams[i].frontmat.CopyPropertiesFromMaterial(pageMaterial);
                }

                if (bookComponent.pageparams[i].backmat == null)
                {
                    bookComponent.pageparams[i].backmat = new Material(pageMaterial);
                }
                else
                {
                    bookComponent.pageparams[i].backmat.CopyPropertiesFromMaterial(pageMaterial);
                }
            }
        }

        bookComponent.rebuildmeshes = true;
        bookComponent.rebuild = true;
    }
}