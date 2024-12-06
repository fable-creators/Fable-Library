using UnityEngine;
using UnityEngine.UIElements;
using System;
using System.IO;
using System.Collections.Generic;
using MegaBook;

public class ImportController : MonoBehaviour
{
    [SerializeField] private UIDocument document;
    [SerializeField] private MegaBookBuilder bookComponent;

    private Button importButton;
    private TextField importPathField;

    private void OnEnable()
    {
        if (document == null) document = GetComponent<UIDocument>();
        
        var root = document.rootVisualElement;
        importButton = root.Q<Button>("importJsonButton");
        importPathField = root.Q<TextField>("importPathField");

        if (importButton != null)
            importButton.clicked += HandleImport;
    }

    private void HandleImport()
    {
        #if UNITY_EDITOR
        string path = UnityEditor.EditorUtility.OpenFilePanel("Select JSON config", "", "json");
        if (!string.IsNullOrEmpty(path))
        {
            string jsonData = File.ReadAllText(path);
            ImportJSON(jsonData);
        }
        #endif
    }

    private void ImportJSON(string jsonData)
    {
        try
        {
            var importedData = JsonUtility.FromJson<BookExportData>(jsonData);
            if (importedData == null)
            {
                Debug.LogError("Failed to parse JSON data");
                return;
            }

            ApplyImportedData(importedData);
        }
        catch (Exception e)
        {
            Debug.LogError($"Import failed: {e.Message}");
        }
    }

    private void ApplyImportedData(BookExportData data)
    {
        if (bookComponent == null)
        {
            Debug.LogError("Book component is not assigned!");
            return;
        }

        try
        {
            // Initialize or clear page parameters
            if (bookComponent.pageparams == null)
            {
                bookComponent.pageparams = new List<MegaBookPageParams>();
            }
            else
            {
                bookComponent.pageparams.Clear();
            }

            // Pre-populate page parameters
            int targetPageCount = Mathf.Max(data.numPages, 1); // Ensure at least one page
            for (int i = 0; i < targetPageCount; i++)
            {
                var pageParam = new MegaBookPageParams();
                
                // Create default materials if needed
                pageParam.frontmat = new Material(Shader.Find("Unlit/Texture"));
                pageParam.backmat = new Material(Shader.Find("Unlit/Texture"));
                
                bookComponent.pageparams.Add(pageParam);
            }

            // Apply dimensions first
            bookComponent.pageWidth = data.pageWidth;
            bookComponent.pageLength = data.pageLength;
            bookComponent.pageHeight = data.pageHeight;
            bookComponent.bookthickness = data.bookThickness;
            bookComponent.pageGap = data.pageGap;
            bookComponent.spineradius = data.spineRadius;

            // Apply UV settings
            bookComponent.edgeUVSize = data.edgeUVSize;
            bookComponent.edgeUVOff = data.edgeUVOffset;

            // Apply cover settings
            bookComponent.autoFit = data.autoFit;
            bookComponent.autoFitSize = data.autoFitSize;
            bookComponent.spineScale = data.spineScale;
            bookComponent.coverScale = data.coverScale;

            // Apply page settings after parameters are initialized
            bookComponent.NumPages = targetPageCount;
            bookComponent.WidthSegs = data.widthSegs;
            bookComponent.LengthSegs = data.lengthSegs;
            bookComponent.HeightSegs = data.heightSegs;

            // Force complete rebuild
            bookComponent.rebuildmeshes = true;
            bookComponent.rebuild = true;
            bookComponent.updateBindings = true;

            Debug.Log($"Import completed successfully. Created {targetPageCount} pages.");
        }
        catch (Exception e)
        {
            Debug.LogError($"Error applying imported data: {e.Message}\nStack trace: {e.StackTrace}");
        }
    }

    private void OnDisable()
    {
        if (importButton != null)
            importButton.clicked -= HandleImport;
    }
}