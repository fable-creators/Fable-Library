# DocumentationGenerator

Inherits from: `EditorWindow`

## Properties

### dataModeController
- Type: `IDataModeController`
- Access: Public

### rootVisualElement
- Type: `VisualElement`
- Access: Public

### overlayCanvas
- Type: `OverlayCanvas`
- Access: Public

### wantsMouseMove
- Type: `Boolean`
- Access: Public

### wantsMouseEnterLeaveWindow
- Type: `Boolean`
- Access: Public

### wantsLessLayoutEvents
- Type: `Boolean`
- Access: Public

### autoRepaintOnSceneChange
- Type: `Boolean`
- Access: Public

### maximized
- Type: `Boolean`
- Access: Public

### hasFocus
- Type: `Boolean`
- Access: Public

### docked
- Type: `Boolean`
- Access: Public

### hasUnsavedChanges
- Type: `Boolean`
- Access: Public

### saveChangesMessage
- Type: `String`
- Access: Public

### minSize
- Type: `Vector2`
- Access: Public

### maxSize
- Type: `Vector2`
- Access: Public

### title
- Type: `String`
- Access: Public

### titleContent
- Type: `GUIContent`
- Access: Public

### depthBufferBits
- Type: `Int32`
- Access: Public

### antiAlias
- Type: `Int32`
- Access: Public

### position
- Type: `Rect`
- Access: Public

### name
- Type: `String`
- Access: Public

### hideFlags
- Type: `HideFlags`
- Access: Public

## Methods

### ShowWindow
```csharp
public static Void ShowWindow()
```

### BeginWindows
```csharp
public Void BeginWindows()
```

### EndWindows
```csharp
public Void EndWindows()
```

### ShowNotification
```csharp
public Void ShowNotification(GUIContent notification)
```

### ShowNotification
```csharp
public Void ShowNotification(GUIContent notification, Double fadeoutWait)
```

### RemoveNotification
```csharp
public Void RemoveNotification()
```

### ShowTab
```csharp
public Void ShowTab()
```

### Focus
```csharp
public Void Focus()
```

### ShowUtility
```csharp
public Void ShowUtility()
```

### ShowPopup
```csharp
public Void ShowPopup()
```

### ShowModalUtility
```csharp
public Void ShowModalUtility()
```

### ShowAsDropDown
```csharp
public Void ShowAsDropDown(Rect buttonRect, Vector2 windowSize)
```

### Show
```csharp
public Void Show()
```

### Show
```csharp
public Void Show(Boolean immediateDisplay)
```

### ShowAuxWindow
```csharp
public Void ShowAuxWindow()
```

### ShowModal
```csharp
public Void ShowModal()
```

### SaveChanges
```csharp
public Void SaveChanges()
```

### DiscardChanges
```csharp
public Void DiscardChanges()
```

### Close
```csharp
public Void Close()
```

### Repaint
```csharp
public Void Repaint()
```

### SendEvent
```csharp
public Boolean SendEvent(Event e)
```

### GetExtraPaneTypes
```csharp
public IEnumerable`1 GetExtraPaneTypes()
```

### TryGetOverlay
```csharp
public Boolean TryGetOverlay(String id, Overlay& match)
```

### SetDirty
```csharp
public Void SetDirty()
```

### GetInstanceID
```csharp
public Int32 GetInstanceID()
```

### GetHashCode
```csharp
public Int32 GetHashCode()
```

### Equals
```csharp
public Boolean Equals(Object other)
```

### ToString
```csharp
public String ToString()
```

### GetType
```csharp
public Type GetType()
```

