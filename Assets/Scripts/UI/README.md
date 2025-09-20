# World Space UI System

This system allows UI elements to follow 3D objects in the game world, such as displaying information panels, name tags, health bars, or chat bubbles above characters.

## Components

### UIWorldSpacePanel.cs

The core component that handles positioning a UI element to follow a 3D object in world space. Features include:

- Works with all Canvas render modes (Screen Space - Overlay, Screen Space - Camera, World Space)
- Configurable offset from the target
- Option to always face the camera
- Smooth following with adjustable speed
- Automatic hiding when target is behind the camera

### WorldSpaceInfoPanel.cs

A component to attach to any 3D object that needs an information panel above it. Features:

- Automatically creates and manages a UI panel
- Configurable offset and following behavior
- Methods to show/hide the panel and update its content

### ChatBubblePanel.cs

A specialized implementation for chat bubbles above characters. Features:

- Listens for chat messages and displays them above characters
- Configurable display duration
- Manages multiple chat bubbles for different characters
- Automatically sizes bubbles to fit text content

### ChatBubblePrefab.cs

Handles the layout and appearance of individual chat bubbles:

- Automatically sizes the bubble background to fit the text
- Configurable maximum width and padding
- Support for both Text and TextMeshPro components

## Usage

### Basic Setup

1. Add a Canvas to your scene (any render mode works)
2. Create your UI panel prefab with the desired layout
3. Add the appropriate components to your 3D objects and UI elements

### For Chat Bubbles

See the `ChatBubbleSetupGuide.txt` file for detailed setup instructions.

### For Custom Info Panels

1. Create a UI panel prefab with your desired layout
2. Add the WorldSpaceInfoPanel component to your 3D object
3. Assign the panel prefab and configure the settings
4. Use the ShowPanel(), HidePanel(), and SetTextContent() methods to control the panel

## Example

```csharp
// Get the WorldSpaceInfoPanel component on a character
var infoPanel = character.GetComponent<WorldSpaceInfoPanel>();

// Show the panel with some text
infoPanel.ShowPanel();
infoPanel.SetTextContent("Character Name\nHealth: 100/100");

// Later, hide the panel
infoPanel.HidePanel();
```

## Advanced Customization

For more complex UI panels, you can extend the base classes or create your own implementations using UIWorldSpacePanel as a foundation.
