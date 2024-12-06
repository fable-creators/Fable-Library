using UnityEngine;
using UnityEngine.UIElements;
using MegaBook;
using System.Collections.Generic;

/// <summary>
/// Controls the UI tree view representation of a book's structure.
/// Manages page selection, navigation, and structure updates.
/// </summary>
public class BookStructureController : MonoBehaviour
{
    /// <summary>
    /// Reference to the UI document containing the tree view and related controls.
    /// </summary>
    [SerializeField] private UIDocument document;

    /// <summary>
    /// Reference to the MegaBook component that handles the book's core functionality.
    /// </summary>
    [SerializeField] private MegaBookBuilder bookComponent;

    private TreeView bookStructureTree;
    private Button addPageButton;

    /// <summary>
    /// Initializes UI elements and sets up event handlers for the book structure interface.
    /// </summary>
    private void OnEnable()
    {
        if (document == null) document = GetComponent<UIDocument>();

        bookStructureTree = document.rootVisualElement.Q<TreeView>("bookStructureTree");
        addPageButton = document.rootVisualElement.Q<Button>("addPageButton");

        if (addPageButton != null)
            addPageButton.clicked += AddPage;

        SetupTreeView();
    }

    /// <summary>
    /// Configures the tree view with fixed height and single selection mode.
    /// Initializes the book structure display.
    /// </summary>
    private void SetupTreeView()
    {
        if (bookStructureTree == null) return;

        bookStructureTree.fixedItemHeight = 30;
        bookStructureTree.selectionType = SelectionType.Single;
        bookStructureTree.selectionChanged += OnTreeSelectionChanged;

        UpdateBookStructureFromBook();
    }

    /// <summary>
    /// Handles page navigation when a tree item is selected.
    /// Converts tree indices to page numbers using the following mapping:
    /// - Index 0: Front cover (-1)
    /// - Index 1: Back cover (NumPages + 2)
    /// - Other indices: Calculated based on page number and side (front/back)
    /// </summary>
    /// <param name="items">Selected items in the tree view.</param>
    private void OnTreeSelectionChanged(IEnumerable<object> items)
    {
        if (bookComponent == null) return;
        int selectedIndex = bookStructureTree.selectedIndex;
        if (selectedIndex >= 0)
        {
            float targetPage;

            if (selectedIndex == 0) // Front cover
            {
                targetPage = -1f;
            }
            else if (selectedIndex == 1) // Back cover
            {
                targetPage = bookComponent.NumPages + 2;
            }
            else
            {
                // For regular pages:
                // 1. Subtract 2 to account for covers
                // 2. Divide by 2 to get page number (since each page has front/back)
                // 3. If it's a back page (odd index), add 0.5 to open to that side
                int adjustedIndex = selectedIndex - 2;
                targetPage = adjustedIndex / 2f + 1;
                if (adjustedIndex % 2 == 1) // If it's a back page
                {
                    targetPage += 0.5f;
                }
            }

            float percentage = targetPage / (bookComponent.NumPages + 2);
            bookComponent.SetPage(percentage);
        }
    }

    /// <summary>
    /// Rebuilds the tree view to reflect the current book structure.
    /// Creates items for front/back covers and all page sides.
    /// </summary>
    public void UpdateBookStructureFromBook()
    {
        if (bookComponent == null || bookStructureTree == null) return;

        var pageItems = new List<TreeViewItemData<string>>();

        // Add covers
        pageItems.Add(new TreeViewItemData<string>(0, "Cover Front"));
        pageItems.Add(new TreeViewItemData<string>(1, "Cover Back"));

        // Add regular pages
        for (int i = 0; i < bookComponent.NumPages; i++)
        {
            int pageIndex = (i * 2) + 2; // Offset by 2 to account for covers
            pageItems.Add(new TreeViewItemData<string>(pageIndex, $"Page {i + 1} Front"));
            pageItems.Add(new TreeViewItemData<string>(pageIndex + 1, $"Page {i + 1} Back"));
        }

        bookStructureTree.SetRootItems(pageItems);
        bookStructureTree.Rebuild();
    }

    /// <summary>
    /// Adds a new page to the book and updates the UI structure.
    /// Triggers necessary mesh and binding updates.
    /// </summary>
    private void AddPage()
    {
        if (bookComponent == null) return;
        bookComponent.NumPages++;
        UpdateBookStructureFromBook();
        bookComponent.rebuildmeshes = true;
        bookComponent.rebuild = true;
        bookComponent.updateBindings = true;
    }

    /// <summary>
    /// Cleans up event subscriptions when the component is disabled.
    /// </summary>
    private void OnDisable()
    {
        if (bookStructureTree != null)
            bookStructureTree.selectionChanged -= OnTreeSelectionChanged;

        if (addPageButton != null)
            addPageButton.clicked -= AddPage;
    }
}