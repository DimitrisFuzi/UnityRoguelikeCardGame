Pozytrum presents: Copy as Text! An essential developer utility to instantly copy GameObject hierarchies and detailed Inspector properties to formatted text. Perfect for debugging, documentation, comparing objects, or feeding data to AI. Boost your workflow with flexible options via the Editor Window and convenient context menus.
Example:
Entire Hierarchy:  (Scene)
Main Camera
Directional Light
▼ Environment
Cube (inactive)
Sphere
▼ Player
Character

Stop manually typing out hierarchy structures or component values! "Copy as Text: Hierarchy & Inspector" by Pozytrum is the ultimate time-saving utility for any Unity developer needing to extract information directly from the editor.
Whether you're debugging complex GameObjects, creating documentation, comparing different component setups, or preparing structured data for external tools (like AI prompts or code generators), this tool provides a fast and flexible way to get the text representation you need, copied directly to your clipboard.
Key Functionality:
1. Hierarchy Copying:

    Full Tree Below: Copy the entire structure of children starting from the selected GameObject(s).
    Selected Only: Copy only the names of the GameObjects currently selected, preserving their relative hierarchical structure through indentation. Perfect for outlining specific parts of your scene.
    Entire Scene/Prefab: Copy the complete hierarchy of the currently active scene or prefab stage.
    Customization: Control indentation style, include/exclude inactive objects, limit recursion depth, and optionally add familiar foldout markers (▼) for clarity.

2. Inspector Property Copying:

    Detailed (Reflection): Dives deeper using code reflection to copy all accessible fields and properties. Great for in-depth debugging and understanding the underlying data. Includes an option to copy non-public members for advanced diagnostics.Two Powerful Modes:
    Visible (Serialized): Mimics the Inspector view, copying properties that Unity serializes and typically displays. Ideal for getting a user-focused view of the data.

    Flexibility: Choose whether to include the often verbose Transform component, and optionally group fields and properties separately in Detailed mode for better organization.

Seamless Workflow:

    Intuitive Editor Window: Access all options through a clear, tabbed interface (Tools > Pozytrum > Hierarchy & Inspector Exporter). Settings are saved for your convenience.
    Lightning-Fast Context Menus: Right-click directly on GameObjects in the Hierarchy or on Component headers in the Inspector to perform common copy operations instantly, using your last saved settings from the window.

Why "Copy as Text"?

    Massive Time Saver: Eliminates tedious manual data entry.
    Improve Debugging: Quickly grab object structures or component states for analysis.
    Streamline Documentation: Easily paste hierarchies or settings into your notes or wikis.
    Enhance Collaboration: Share precise object configurations with team members.
    Power External Tools: Provide structured text input for AI, scripts, or comparison tools.
    Clean & Efficient: Editor-only script with zero runtime overhead. Works across all render pipelines (Built-in, URP, HDRP).

Brought to you by Pozytrum - Tools for efficient Unity development.