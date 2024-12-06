# MediaBuilder - Interactive Book Creation Tool

A Unity-based tool for creating and customizing interactive books, magazines, newspapers, posters, and manga. This document provides a comprehensive overview of the codebase structure and component interactions.

## System Architecture

### Core Components Overview

The system is built using a modular architecture where each controller handles a specific aspect of the book creation process:

```
Controllers/
├── AudioUploadController     # Audio file management and playback
├── BookStructureController   # Page organization and navigation
├── DatabaseController        # Data persistence and Supabase integration
├── DimensionsController      # Physical dimensions management
├── ExportController         # Project export functionality
├── ImageUploadController    # Image handling and page textures
├── MaterialsController      # Material and appearance settings
├── MediaTypeController      # Media type presets and configuration
├── PageContentController    # Page content and text rendering
├── PageLayoutController     # UI layout and grid system
├── PageSettingsController   # Page-specific settings
├── PageSliderController     # Page navigation interface
├── SceneSettingsController  # Global scene settings
├── SettingsToggleController # Settings UI visibility
└── WorkflowController       # Step-by-step process management
```

### Component Interactions

#### 1. Core Book Management
- **BookStructureController** manages the overall book structure
  - Interacts with **PageContentController** for content management
  - Works with **PageSettingsController** for page configuration
  - Communicates with **PageSliderController** for navigation

#### 2. Media Management
- **AudioUploadController** handles audio files
  - Uses `AudioUploadPlugin.jslib` for WebGL file handling
  - Manages both page turn sounds and background music
  - Coordinates with **MaterialsController** for audio feedback

- **ImageUploadController** manages images
  - Handles image uploads and texture management
  - Works with **PageContentController** for content display
  - Uses WebGL-specific handling for browser uploads

#### 3. UI and Workflow
- **WorkflowController** orchestrates the entire process
  - Manages step progression and validation
  - Coordinates with all other controllers
  - Handles state management and navigation

#### 4. Data Persistence
- **DatabaseController** manages data storage
  - Integrates with Supabase backend
  - Handles project saving/loading
  - Manages asset uploads and retrieval

### Detailed Component Descriptions

#### AudioUploadController
```csharp
public class AudioUploadController : MonoBehaviour
{
    // Handles audio file uploads and playback
    // Key methods:
    // - OnImageUploaded(string blobUrl)
    // - PlayPageTurnSound()
    // - HandleLoadedAudioClip(AudioClip clip, bool isPageTurnSound)
}
```
- Manages audio file uploads
- Handles both page turn sounds and background music
- Provides WebGL compatibility through plugins

#### BookStructureController
```csharp
public class BookStructureController : MonoBehaviour
{
    // Manages book structure and navigation
    // Key methods:
    // - UpdateBookStructureFromBook()
    // - OnTreeSelectionChanged(IEnumerable<object> items)
    // - AddPage()
}
```
- Controls the tree view representation of book pages
- Handles page navigation and selection
- Manages page addition and organization

#### MaterialsController
```csharp
public class MaterialsController : MonoBehaviour
{
    // Manages materials and appearance
    // Key methods:
    // - OnCoverMaterialChanged(string materialName)
    // - OnPageMaterialChanged(string materialName)
    // - SetMaterialsForType(string mediaType, string pageMaterialType)
}
```
- Handles material assignments for covers and pages
- Manages material properties (roughness, metallic)
- Coordinates with media type presets

#### MediaTypeController
```csharp
public class MediaTypeController : MonoBehaviour
{
    // Manages different media type configurations
    // Key methods:
    // - ApplyBookPreset()
    // - ApplyMagazinePreset()
    // - UpdateDimensionsBasedOnMediaType(string mediaType)
}
```
- Handles different media type configurations
- Applies appropriate presets and dimensions
- Manages binding elements visibility

### WebGL Integration

The system uses two key JavaScript plugins for WebGL functionality:

#### AudioUploadPlugin.jslib
```javascript
// Handles audio file uploads in WebGL builds
// Key function: ShowFileInput(gameObjectName, methodName, accept)
```

#### ExportPlugin.jslib
```javascript
// Handles file downloads in WebGL builds
// Key function: DownloadFile(filename, data)
```

### State Management

The system uses several mechanisms for state management:

1. **Step Validation**
   - Managed by WorkflowController
   - Ensures proper progression through steps
   - Validates required inputs before proceeding

2. **Data Persistence**
   - Handled by DatabaseController
   - Uses Supabase for backend storage
   - Manages asset uploads and project data

3. **UI State**
   - Controlled by individual controllers
   - Uses Unity UI Toolkit for interface
   - Maintains synchronization between components

### Setup Instructions

1. **Unity Configuration**
   ```
   - Unity 2021.3 or later required
   - Install TextMeshPro package
   - Install UI Toolkit package
   ```

2. **Supabase Setup**
   ```
   - Configure SupabaseSettings ScriptableObject
   - Set URL and API key
   - Initialize storage buckets
   ```

3. **WebGL Configuration**
   ```
   - Enable WebGL compression
   - Configure memory settings
   - Add required plugins
   ```

### Common Workflows

1. **Adding New Pages**
   ```csharp
   BookStructureController.AddPage()
   PageSettingsController.UpdateUIFromBook()
   MaterialsController.OnPresetChanged()
   ```

2. **Changing Media Type**
   ```csharp
   MediaTypeController.UpdateDimensionsBasedOnMediaType()
   MaterialsController.SetMaterialsForType()
   BookStructureController.UpdateBookStructureFromBook()
   ```

3. **Uploading Content**
   ```csharp
   ImageUploadController.OnImageUploaded()
   AudioUploadController.OnAudioUploaded()
   PageContentController.UpdatePageContent()
   ```

### Error Handling

- Each controller implements appropriate error checking
- WebGL-specific error handling for file operations
- Validation checks throughout the workflow
- Detailed logging for debugging

### Performance Considerations

- Texture management for memory efficiency
- Asynchronous loading of assets
- WebGL memory management
- UI optimization for large books

### Future Extensibility

The modular architecture allows for:
- New media type additions
- Additional export formats
- Enhanced material options
- Extended content types

## Support and Maintenance

For technical support or questions about the implementation, please contact:
[Contact Information]

## License

[License Information]
