# GitHub Copilot Instructions for RimWorld Mod: HelpTab

## Mod Overview and Purpose
The HelpTab mod for RimWorld enhances user experience by integrating a comprehensive help system directly within the game's interface. This system is designed to provide detailed information about various game elements, including items, biomes, and research projects, catering to both new and experienced players. The primary goal is to make in-game information more accessible and organized, ultimately improving gameplay and usability.

## Key Features and Systems
- **Help Categories and Definitions**: The system organizes information into categories and definitions, allowing users to filter and jump to specific items of interest.
- **Dynamic Filtering**: Players can filter categories and definitions based on keywords, ensuring that only relevant information is displayed.
- **User Interface Integration**: The mod extends the game's UI to incorporate a main help menu, providing quick access through the main menu button.
- **Extensible Design**: Through C# extensions, the mod allows additional game definitions to be easily integrated into the help system.

## Key Files and Classes:
The project is organized around several key files and classes, each playing a critical role in extending RimWorld's functionality:

1. **Extensions Classes**: 
   - `BiomeDef_Extensions`, `BuildableDef_Extensions`, `Def_Extensions`, etc., provide extension methods for various `Def` types, facilitating integration with the help system.

2. **Help System Classes**:
   - `HelpBuilder`, `HelpCategoryDef`, `HelpDef`, and `HelpDetailSection` handle the construction, categorization, filtering, and display of help content.
   - `MainTabWindow_ModHelp` manages the user interface for the help system.

3. **Custom Structures**:
   - `DefStringTriplet` and `StringDescTriplet` manage display strings with optional prefixes and suffixes for clear in-game representation.

## Coding Patterns and Conventions
- **Static Extension Classes**: The project makes extensive use of static classes to add functionality to existing RimWorld definitions without modifying original code.
- **OOP Principles**: Utilizes object-oriented principles, such as encapsulation within classes like `HelpCategoryDef` for managing help data.
- **Consistent Naming**: Method and class names are descriptive and follow PascalCase for public methods and classes, ensuring readability and maintainability.

## XML Integration
Though the mod is primarily C#-based, XML files can be used to define additional content or configurations. XML definitions can be structured to map RimWorld's existing schema, and new data can be linked to the help system via extension methods.

## Harmony Patching
- **Dynamic Method Patching**: The project employs Harmony, a library for runtime method patching, to adjust game logic as necessary without direct source code changes. This allows the mod to seamlessly integrate additional help support without conflicting with the base game's functionality.

## Suggestions for Copilot
When working with this project, Copilot can assist by:
- **Autocomplete Tasks**: Suggesting method stubs and implementing repetitive patterns, especially in the extension methods.
- **Code Refactoring**: Enhancing readability and maintaining consistency in the method signatures and logic.
- **Harmony Integration**: Providing code snippets for common Harmony patches to reduce boilerplate code.
- **UI Generation**: Assisting in the creation of UI elements by suggesting common RimWorld UI patterns and layouts.

By following these instructions, developers can work efficiently within the HelpTab mod’s architecture, making contributions that align with its design goals and maintain its extensible and user-friendly nature.
