using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// Controls a drag-and-drop UI layout editor with a grid system.
/// Enables visual layout design with snapping and resizing capabilities.
/// </summary>
[Tooltip("Manages drag-and-drop layout editing with grid snapping")]
public class PageLayoutController : MonoBehaviour
{
    /// <summary>
    /// Reference to the layout editor UI document.
    /// </summary>
    [Tooltip("UI Document containing layout editor elements")]
    [SerializeField] private UIDocument layoutDocument;

    /// <summary>
    /// Reference to the main book builder UI document.
    /// </summary>
    [Tooltip("Main book builder UI document")]
    [SerializeField] private UIDocument bookBuilderDocument;

    /// <summary>
    /// Grid constants for layout snapping and minimum sizes.
    /// </summary>
    private const float GRID_SIZE = 50f;  // Size of snap grid in pixels
    private const float MIN_SIZE = 50f;   // Minimum size for components

    private VisualElement layoutGrid;
    private Vector2 dragStartPosition;
    private VisualElement draggedElement;
    private Vector2 mouseStartPosition;
    private int activePointerId = -1;
    private bool isResizing = false;

    /// <summary>
    /// Initializes the layout editor and sets up the toggle button.
    /// </summary>
    private void OnEnable()
    {
        layoutDocument.enabled = false;

        // Setup toggle button
        var editButton = bookBuilderDocument.rootVisualElement.Q<Button>("editLayoutButton");
        editButton.clicked += ToggleLayoutEditor;

    }

    /// <summary>
    /// Toggles the layout editor visibility and initializes the grid when enabled.
    /// </summary>
    private void ToggleLayoutEditor()
    {
        layoutDocument.enabled = !layoutDocument.enabled;
        var root = layoutDocument.rootVisualElement;

        if (layoutDocument.enabled)
        {
            var closeButton = root.Q<Button>("closeButton");
            closeButton.clicked += () => layoutDocument.enabled = false;

            layoutGrid = root.Q<VisualElement>("layoutGrid");

            // Set explicit size for grid
            layoutGrid.style.width = 800;
            layoutGrid.style.height = 400;

            SetupComponents(root);
            SetupGrid();
        }


    }

    /// <summary>
    /// Binds click handlers to component creation buttons.
    /// </summary>
    /// <param name="root">Root visual element containing the buttons.</param>
    private void SetupComponents(VisualElement root)
    {
        root.Q<Button>("addTextButton").clicked += () => AddComponent("Text Block");
        root.Q<Button>("addImageButton").clicked += () => AddComponent("Image");
        root.Q<Button>("addButtonButton").clicked += () => AddComponent("Button");
    }

    /// <summary>
    /// Registers mouse event handlers for the layout grid.
    /// </summary>
    private void SetupGrid()
    {
        if (layoutGrid == null) return;

        // Clear existing event handlers
        layoutGrid.UnregisterCallback<MouseMoveEvent>(OnGridMouseMove);
        layoutGrid.UnregisterCallback<MouseUpEvent>(OnGridMouseUp);

        // Register new event handlers
        layoutGrid.RegisterCallback<MouseMoveEvent>(OnGridMouseMove);
        layoutGrid.RegisterCallback<MouseUpEvent>(OnGridMouseUp);
    }

    /// <summary>
    /// Creates a new draggable component of specified type.
    /// </summary>
    /// <param name="type">Type of component to create (Text Block, Image, or Button).</param>
    private void AddComponent(string type)
    {
        var component = new VisualElement();
        component.AddToClassList("layout-component");
        component.style.position = Position.Absolute;
        component.style.left = 0;
        component.style.top = 0;
        component.style.width = 100;
        component.style.height = 50;
        component.style.backgroundColor = new StyleColor(new Color(0.4f, 0.4f, 0.4f, 0.9f));

        var label = new Label(type);
        component.Add(label);

        var resizeHandle = CreateResizeHandle();
        component.Add(resizeHandle);

        MakeComponentDraggable(component);
        MakeComponentResizable(component);
        layoutGrid.Add(component);
    }

    /// <summary>
    /// Adds drag functionality to a component with grid snapping and boundary constraints.
    /// </summary>
    /// <param name="component">Component to make draggable.</param>
    private void MakeComponentDraggable(VisualElement component)
    {
        component.RegisterCallback<MouseDownEvent>(evt =>
        {
            // Only start drag if we click the component but not the resize handle
            if (evt.button == 0 && evt.target == component)
            {
                draggedElement = component;
                dragStartPosition = new Vector2(component.style.left.value.value,
                                             component.style.top.value.value);
                mouseStartPosition = evt.mousePosition;
                component.AddToClassList("dragging");
                evt.StopPropagation();
            }
        });

        component.RegisterCallback<MouseMoveEvent>(evt =>
        {
            if (draggedElement != component) return;

            var delta = evt.mousePosition - mouseStartPosition;
            var newX = dragStartPosition.x + delta.x;
            var newY = dragStartPosition.y + delta.y;

            // Snap to grid
            newX = Mathf.Round(newX / GRID_SIZE) * GRID_SIZE;
            newY = Mathf.Round(newY / GRID_SIZE) * GRID_SIZE;

            // Constrain to grid boundaries
            newX = Mathf.Clamp(newX, 0, layoutGrid.layout.width - component.layout.width);
            newY = Mathf.Clamp(newY, 0, layoutGrid.layout.height - component.layout.height);

            component.style.left = newX;
            component.style.top = newY;

            Debug.Log($"Moving to: ({newX}, {newY})");
            evt.StopPropagation();
        });

        component.RegisterCallback<MouseUpEvent>(evt =>
        {
            if (draggedElement == component)
            {
                draggedElement.RemoveFromClassList("dragging");
                draggedElement = null;
                evt.StopPropagation();
            }
        });
    }

    /// <summary>
    /// Creates a resize handle element for components.
    /// </summary>
    /// <returns>Visual element configured as a resize handle.</returns>
    private VisualElement CreateResizeHandle()
    {
        var handle = new VisualElement();
        handle.name = "resize-handle";
        handle.AddToClassList("resize-handle");
        handle.style.position = Position.Absolute;
        handle.style.right = 0;
        handle.style.bottom = 0;
        handle.style.width = 10;
        handle.style.height = 10;
        return handle;
    }

    /// <summary>
    /// Adds resize functionality to a component with grid snapping and minimum size constraints.
    /// </summary>
    /// <param name="component">Component to make resizable.</param>
    private void MakeComponentResizable(VisualElement component)
    {
        var handle = component.Q<VisualElement>("resize-handle");
        if (handle == null) return;

        Vector2 startSize = Vector2.zero;
        Vector2 startMousePos = Vector2.zero;

        handle.RegisterCallback<MouseDownEvent>(evt =>
        {
            Debug.Log("Mouse down on resize handle");
            if (evt.button == 0)
            {
                isResizing = true;
                startSize = new Vector2(component.layout.width, component.layout.height);
                startMousePos = evt.mousePosition;
                handle.CapturePointer(0); // Capture pointer
                evt.StopPropagation();
                Debug.Log("Started resize");
            }
        });

        handle.RegisterCallback<MouseMoveEvent>(evt =>
        {
            if (!isResizing) return;

            var delta = evt.mousePosition - startMousePos;
            var newWidth = Mathf.Max(MIN_SIZE, startSize.x + delta.x);
            var newHeight = Mathf.Max(MIN_SIZE, startSize.y + delta.y);

            newWidth = Mathf.Round(newWidth / GRID_SIZE) * GRID_SIZE;
            newHeight = Mathf.Round(newHeight / GRID_SIZE) * GRID_SIZE;

            component.style.width = newWidth;
            component.style.height = newHeight;

            evt.StopPropagation();
            Debug.Log($"Resizing: {newWidth}x{newHeight}");
        });

        handle.RegisterCallback<MouseUpEvent>(evt =>
        {
            if (isResizing)
            {
                isResizing = false;
                handle.ReleasePointer(0); // Release pointer
                evt.StopPropagation();
                Debug.Log("Ended resize");
            }
        });
    }

    /// <summary>
    /// Handles component dragging within the grid boundaries.
    /// </summary>
    /// <param name="evt">Mouse move event data.</param>
    private void OnGridMouseMove(MouseMoveEvent evt)
    {
        if (draggedElement == null) return;

        var gridRect = layoutGrid.worldBound;
        var newX = dragStartPosition.x + (evt.mousePosition.x - mouseStartPosition.x);
        var newY = dragStartPosition.y + (evt.mousePosition.y - mouseStartPosition.y);

        newX = Mathf.Clamp(newX, 0, gridRect.width - draggedElement.worldBound.width);
        newY = Mathf.Clamp(newY, 0, gridRect.height - draggedElement.worldBound.height);

        newX = Mathf.Round(newX / GRID_SIZE) * GRID_SIZE;
        newY = Mathf.Round(newY / GRID_SIZE) * GRID_SIZE;

        draggedElement.style.left = newX;
        draggedElement.style.top = newY;
    }

    /// <summary>
    /// Cleans up dragging state when mouse is released.
    /// </summary>
    /// <param name="evt">Mouse up event data.</param>
    private void OnGridMouseUp(MouseUpEvent evt)
    {
        if (draggedElement != null)
        {
            draggedElement.RemoveFromClassList("dragging");
            draggedElement.ReleasePointer(0);
            draggedElement = null;
            activePointerId = -1;
        }
    }

    /// <summary>
    /// Cleans up event handlers when component is disabled.
    /// </summary>
    private void OnDisable()
    {
        if (layoutGrid != null)
        {
            layoutGrid.UnregisterCallback<MouseMoveEvent>(OnGridMouseMove);
            layoutGrid.UnregisterCallback<MouseUpEvent>(OnGridMouseUp);
        }
    }
}