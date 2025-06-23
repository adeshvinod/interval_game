# Progressions Saving and Loading System

## Overview

This document explains the new progressions saving and loading system that has been implemented to replace the old `settings_arpgame.cs` approach. The new system uses a ScriptableObject-based architecture that provides better organization, persistence, and integration with Unity's serialization system.

## Key Components

### 1. ProgressionsGameData ScriptableObject

**File**: `Assets/WordQuiz/Scripts/ProgressionsGameData.cs`

This is the main data container that manages all progression-related data:

- **Saved Progressions**: List of user-created chord progressions
- **Custom Chords**: List of user-defined custom chord types
- **Current Progression**: The progression currently being edited
- **Question Lists**: Lists for random mode questions

### 2. ChordButton Component

**File**: `Assets/WordQuiz/Scripts/ChordButton.cs`

Manages individual chord buttons in the UI:

- **-1 System**: Uses -1 values to represent empty chords (instead of boolean flags)
- **Dropdown Management**: Handles note and chord type dropdowns
- **Validation**: Ensures only valid chord data (non-negative values) is processed
- **Display Updates**: Shows chord names or "Empty" based on current state

### 3. ProgressionUIManager

**File**: `Assets/WordQuiz/Scripts/ProgressionUIManager.cs`

Manages the overall progression UI:

- **Loading/Saving**: Handles progression loading from dropdown and saving
- **UI Refresh**: Updates all chord buttons based on current progression
- **Validation**: Ensures only valid chords are displayed and saved

## Data Structure

### Chord Representation
```csharp
[Serializable]
public class Pair
{
    public int note;      // 0-11 for notes (A, A#, B, C, C#, D, D#, E, F, F#, G, G#)
    public int chordtype; // 0-10 for chord types (maj, min, dim, aug, maj7, min7, dom7, etc.)
}
```

### Empty Chord System
- **-1 Values**: Both `note` and `chordtype` set to -1 indicate an empty chord
- **Validation**: All methods validate that chord data is non-negative before processing
- **UI Display**: Empty chords show "Empty" text instead of "A maj"

### Progression Structure
```csharp
[Serializable]
public class ChordProgression
{
    public List<Pair> progression = new List<Pair>();
    public string name;
}
```

## Key Features

### 1. Robust Empty Chord Handling
- **-1 System**: Empty chords are represented by -1 values instead of boolean flags
- **Validation**: All chord operations validate data before processing
- **UI Consistency**: Empty slots show "Empty" instead of default chord names

### 2. File Compatibility
- **Backward Compatible**: Works with existing `player.fun` files
- **Data Validation**: Filters out invalid chord data during loading
- **Error Handling**: Gracefully handles corrupted or invalid data

### 3. Real-time UI Updates
- **Event-driven**: UI updates automatically when progression changes
- **Button States**: Properly manages interactive states for all buttons
- **Visual Feedback**: Clear indication of empty vs. filled slots

## Usage Examples

### Creating a New Progression
```csharp
gameData.CreateNewProgression();
gameData.currentProgression.name = "My Progression";
```

### Adding Chords
```csharp
// Valid chord (will be added)
gameData.AddChordToCurrentProgression(0, 0, 0); // A major

// Invalid chord (will be ignored)
gameData.AddChordToCurrentProgression(-1, 0, 0); // Empty note
```

### Loading a Progression
```csharp
gameData.LoadProgression(0); // Loads first saved progression
```

### Saving a Progression
```csharp
gameData.SaveCurrentProgression("My Saved Progression");
```

## File Format

The system maintains compatibility with the existing `player.fun` format:

- **Progressions**: Stored as 2D arrays with length in first column
- **Custom Chords**: Stored as 2D arrays with intervals
- **Names**: Stored as string arrays
- **Validation**: Invalid data (negative values) is filtered during loading

## Testing

The system includes built-in test methods:

- **Test Save/Load Functionality**: Verifies file I/O operations
- **Test -1 Empty Chord System**: Validates empty chord handling

To run tests, right-click on the ProgressionsGameData asset in Unity and select the test method from the context menu.

## Migration from Old System

The old `settings_arpgame.cs` system has been replaced, but the data format remains compatible. Existing saved progressions will be automatically loaded when using the new system.

### Key Improvements
1. **Better Organization**: ScriptableObject-based architecture
2. **Robust Validation**: -1 system prevents invalid data
3. **Cleaner UI**: Proper empty slot handling
4. **Event-driven Updates**: Automatic UI refresh
5. **Better Error Handling**: Graceful handling of invalid data

## Troubleshooting

### Common Issues
1. **Empty slots showing "A maj"**: Ensure ClearChord() doesn't set dropdown values to 0
2. **Invalid chord data**: Check that chord operations validate for non-negative values
3. **UI not updating**: Verify event subscriptions and RefreshUI() calls

### Debug Logging
The system includes comprehensive debug logging to help identify issues:
- Chord loading/saving operations
- UI refresh cycles
- Data validation results
- File I/O operations

## Scene Setup

The `progressions_menu.unity` scene is set up with:
- `ProgressionUIManager` component
- 16 `ChordButton` instances
- Dropdown for loading saved progressions
- Input field for progression names
- Action buttons (Load, Save, Delete, Transpose)

## Future Enhancements

Potential improvements for the system:
- Cloud save integration
- Progression sharing between users
- Advanced progression analysis tools
- Integration with music theory validation
- Export/import functionality for progressions 