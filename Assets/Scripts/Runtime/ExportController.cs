using UnityEngine;
using UnityEngine.UIElements;
using System;
using System.Text;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.IO;
using MegaBook;

/// <summary>
/// Data structure for serializing book configuration.
/// Contains all essential measurements and settings for book reconstruction.
/// </summary>
[Serializable]
public class BookExportData
{
    /// <summary>
    /// Type of media being exported (e.g., "Custom", "Standard").
    /// </summary>
    public string mediaType;

    /// <summary>
    /// Basic book dimensions in Unity units.
    /// </summary>
    public float pageWidth;
    public float pageLength;
    public float pageHeight;
    public float bookThickness;
    public float pageGap;
    public float spineRadius;

    /// <summary>
    /// Page configuration settings.
    /// </summary>
    public int numPages;
    public int widthSegs;
    public int lengthSegs;
    public int heightSegs;

    /// <summary>
    /// Cover and spine configuration.
    /// </summary>
    public bool autoFit;
    public Vector3 autoFitSize;
    public float spineScale;
    public Vector3 coverScale;

    /// <summary>
    /// UV mapping configuration.
    /// </summary>
    public float edgeUVSize;
    public float edgeUVOffset;

    /// <summary>
    /// Book binding configuration.
    /// </summary>
    public float headbandLength;
    public float headbandRadius;
    public int headbandSegs;
    public int headbandSides;
    public float spineFabricLength;
    public float spineFabricWidth;
    public float spineFabricThickness;

    // Added fields for complete book data
    public string id;
    public string name;
    public List<PageAssetData> pageAssets;
    public List<AudioAssetData> audioAssets;
    public DateTime savedAt;

    /// <summary>
    /// Creates export data from a MegaBookBuilder instance.
    /// Uses null coalescing for optional components (headband and spine fabric).
    /// </summary>
    /// <param name="book">MegaBookBuilder instance to export.</param>
    /// <returns>BookExportData containing all book configuration.</returns>
    public static BookExportData FromBookBuilder(MegaBookBuilder book)
    {
        return new BookExportData
        {
            mediaType = "Custom",
            pageWidth = book.pageWidth,
            pageLength = book.pageLength,
            pageHeight = book.pageHeight,
            bookThickness = book.bookthickness,
            pageGap = book.pageGap,
            spineRadius = book.spineradius,
            numPages = book.NumPages,
            widthSegs = book.WidthSegs,
            lengthSegs = book.LengthSegs,
            heightSegs = book.HeightSegs,
            autoFit = book.autoFit,
            autoFitSize = book.autoFitSize,
            spineScale = book.spineScale,
            coverScale = book.coverScale,
            edgeUVSize = book.edgeUVSize,
            edgeUVOffset = book.edgeUVOff,
            headbandLength = book.headband1?.length ?? 1.0f,
            headbandRadius = book.headband1?.radius ?? 0.01f,
            headbandSegs = book.headband1?.segs ?? 8,
            headbandSides = book.headband1?.sides ?? 8,
            spineFabricLength = book.spineFabric?.length ?? 1.0f,
            spineFabricWidth = book.spineFabric?.width ?? 1.0f,
            spineFabricThickness = book.spineFabric?.thickness ?? 0.01f
        };
    }

    public Dictionary<string, object> ToSerializableDict()
    {
        return new Dictionary<string, object>
        {
            {"pageWidth", pageWidth},
            {"pageLength", pageLength},
            // ... rest of the fields ...
        };
    }
}

/// <summary>
/// Handles the UI and logic for exporting book configurations.
/// Supports JSON export with future support for other formats.
/// </summary>
[Tooltip("Manages book configuration export functionality")]
public class ExportController : MonoBehaviour
{
    /// <summary>
    /// Reference to the UI document containing export controls.
    /// </summary>
    [Tooltip("UI Document containing export controls")]
    [SerializeField] private UIDocument document;

    /// <summary>
    /// Reference to the book component to export.
    /// </summary>
    [Tooltip("Book component to export")]
    [SerializeField] private MegaBookBuilder bookComponent;

    private DropdownField exportTypeDropdown;
    private Button exportButton;

    // JavaScript bridge for WebGL builds to trigger file downloads
#if UNITY_WEBGL && !UNITY_EDITOR
    [DllImport("__Internal")]
    private static extern void DownloadFile(string filename, string data);
#endif

    private void OnEnable()
    {
        if (document == null) document = GetComponent<UIDocument>();

        exportTypeDropdown = document.rootVisualElement.Q<DropdownField>("exportTypeDropdown");
        exportButton = document.rootVisualElement.Q<Button>("exportButton");

        if (exportButton != null)
            exportButton.clicked += HandleExport;

        SetupExportTypes();
    }

    private void SetupExportTypes()
    {
        exportTypeDropdown.choices = new List<string> {
            "Export to JSON",
            "Unity Prefab (Coming Soon)",
            "FBX/GLB (Coming Soon)"
        };
        exportTypeDropdown.value = "Export to JSON";
    }

    private void HandleExport()
    {
        if (bookComponent == null)
        {
            Debug.LogError("No book component found to export!");
            return;
        }

        ExportJSON();
    }

    private void ExportJSON()
    {
        if (bookComponent == null)
        {
            Debug.LogError("No book component found to export!");
            return;
        }

        var bookData = BookExportData.FromBookBuilder(bookComponent);
        ExportBookData(bookData);
    }

    public void ExportBookData(BookExportData bookData)
    {
        try
        {
            string jsonData = JsonUtility.ToJson(bookData, true);

#if UNITY_WEBGL && !UNITY_EDITOR
            byte[] bytes = Encoding.UTF8.GetBytes(jsonData);
            string base64 = Convert.ToBase64String(bytes);
            DownloadFile($"book-{bookData.name}-{bookData.id}.json", base64);
#else
            // In editor or non-WebGL builds, use system file dialog
            string path = UnityEngine.Application.dataPath + "/../ExportedBooks/";
            Directory.CreateDirectory(path); // Create directory if it doesn't exist

            string filePath = path + $"book-{bookData.name}-{bookData.id}.json";
            File.WriteAllText(filePath, jsonData);

            Debug.Log($"File exported to: {filePath}");

            // Open the folder in file explorer
            Application.OpenURL("file://" + path);
#endif

            Debug.Log("Export successful!");
            Debug.Log("Exported JSON preview:");
            Debug.Log(jsonData);
        }
        catch (Exception e)
        {
            Debug.LogError($"Export failed: {e.Message}");
        }
    }

    // Debug method to verify export format by attempting to deserialize JSON
    public void TestImport(string jsonData)
    {
        try
        {
            var importedData = JsonUtility.FromJson<BookExportData>(jsonData);
            Debug.Log("Import test successful!");
            Debug.Log($"Imported book width: {importedData.pageWidth}");
            Debug.Log($"Imported book length: {importedData.pageLength}");
            // Add more verification logs as needed
        }
        catch (Exception e)
        {
            Debug.LogError($"Import test failed: {e.Message}");
        }
    }


}
