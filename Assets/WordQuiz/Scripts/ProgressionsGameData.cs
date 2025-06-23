using UnityEngine;
using System.Collections.Generic;
using System;
using System.IO;

[CreateAssetMenu(fileName = "ProgressionsGameData", menuName = "Game/ProgressionsGameData")]
public class ProgressionsGameData : ScriptableObject
{
    // Event for progression changes
    public System.Action onProgressionChanged;

    // Core data structures
    [Serializable]
    public class Pair
    {
        public int note;
        public int chordtype;
    }

    [Serializable]
    public class ChordProgression
    {
        public List<Pair> progression = new List<Pair>();
        public string name;

        public ChordProgression(List<Pair> _progression, string _name)
        {
            progression = new List<Pair>();
            progression.AddRange(_progression);
            name = _name;
        }
    }

    [Serializable]
    public class CustomChordType
    {
        public int chordType_index;
        public List<int> intervals = new List<int>();
        public string name;

        public CustomChordType(List<int> _intervals, string _name)
        {
            intervals = new List<int>();
            intervals.AddRange(_intervals);
            name = _name;
        }
    }

    // Data storage
    public List<ChordProgression> savedProgressions = new List<ChordProgression>();
    public List<CustomChordType> customChords = new List<CustomChordType>();
    
    // Question lists for random mode
    public List<int> QuestionList_chordtypes = new List<int>();
    public List<int> QuestionList_customchords = new List<int>();
    
    // Reference data
    public Dictionary<int, string> chordNameList = new Dictionary<int, string>()
    {
        {0, "maj"},
        {1, "min"},
        {2, "dim"},
        {3, "aug"},
        {4, "maj7"},
        {5, "min7"},
        {6, "dom7"},
        {7, "min7b5"},
        {8, "majScale"},
        {9, "harmonicMin"},
        {10, "melodicMin"}
    };

    // Current progression being edited
    public ChordProgression currentProgression;
    [NonSerialized] public int currentProgressionIndex = -1;

    // Default progression (A minor to D major)
    private void OnEnable()
    {
        if (savedProgressions.Count == 0)
        {
            CreateDefaultProgression();
        }
    }

    private void CreateDefaultProgression()
    {
        List<Pair> defaultProgression = new List<Pair>
        {
            new Pair { note = 9, chordtype = 1 },  // A minor
            new Pair { note = 2, chordtype = 0 }   // D major
        };
        
        savedProgressions.Add(new ChordProgression(defaultProgression, "Default Progression"));
        currentProgression = new ChordProgression(defaultProgression, "Default Progression");
        currentProgressionIndex = 0;
    }

    // Methods for progression management
    public void CreateNewProgression()
    {
        if (savedProgressions.Count == 0)
        {
            CreateDefaultProgression();
        }
        else
        {
            currentProgression = new ChordProgression(new List<Pair>(), "New Progression");
            currentProgressionIndex = -1;
        }
        onProgressionChanged?.Invoke();
    }

    public void LoadProgression(int index)
    {
        if (index >= 0 && index < savedProgressions.Count)
        {
            var progressionToLoad = savedProgressions[index];
            Debug.Log($"LoadProgression: Loading '{progressionToLoad.name}' with {progressionToLoad.progression.Count} chords");
            
            // Log each chord in the progression
            for (int i = 0; i < progressionToLoad.progression.Count; i++)
            {
                var chord = progressionToLoad.progression[i];
                Debug.Log($"  Chord {i}: note={chord.note}, chordtype={chord.chordtype}");
            }
            
            currentProgression = new ChordProgression(progressionToLoad.progression, progressionToLoad.name);
            currentProgressionIndex = index;
            onProgressionChanged?.Invoke();
            
            Debug.Log($"LoadProgression: Successfully loaded progression with {currentProgression.progression.Count} chords");
        }
        else
        {
            Debug.LogError($"LoadProgression: Invalid index {index}, total progressions: {savedProgressions.Count}");
        }
    }

    public void SaveCurrentProgression(string name)
    {
        if (currentProgression == null) return;

        currentProgression.name = name;
        
        if (currentProgressionIndex >= 0)
        {
            // Update existing progression
            savedProgressions[currentProgressionIndex] = currentProgression;
        }
        else
        {
            // Add new progression
            savedProgressions.Add(currentProgression);
            currentProgressionIndex = savedProgressions.Count - 1;
        }
        
        // Save to file
        SaveToFile();
    }

    public void DeleteCurrentProgression()
    {
        if (currentProgressionIndex >= 0)
        {
            savedProgressions.RemoveAt(currentProgressionIndex);
            currentProgression = null;
            currentProgressionIndex = -1;
            SaveToFile();
        }
    }

    public void TransposeProgression(int steps)
    {
        if (currentProgression == null) return;

        List<Pair> transposedProgression = new List<Pair>();
        foreach (Pair pair in currentProgression.progression)
        {
            Pair newPair = new Pair
            {
                note = (pair.note + steps + 12) % 12, // Ensure positive modulo
                chordtype = pair.chordtype
            };
            transposedProgression.Add(newPair);
        }
        currentProgression.progression = transposedProgression;
        onProgressionChanged?.Invoke();
    }

    public void AddChordToCurrentProgression(int note, int chordType, int index)
    {
        if (currentProgression == null) return;

        // Validate that we have valid chord data (not -1)
        if (note < 0 || chordType < 0)
        {
            Debug.LogWarning($"Invalid chord data: note={note}, chordType={chordType}. Skipping.");
            return;
        }

        // Check if index is out of bounds
        if (index > currentProgression.progression.Count)
        {
            Debug.LogError($"Index {index} is out of bounds. Current progression length is {currentProgression.progression.Count}");
            return;
        }

        Pair newPair = new Pair { note = note, chordtype = chordType };

        // If index equals length, add new chord
        if (index == currentProgression.progression.Count)
        {
            currentProgression.progression.Add(newPair);
            Debug.Log($"Added new chord at index {index}: {note} {chordType}");
        }
        // If index is within bounds, update existing chord
        else
        {
            currentProgression.progression[index] = newPair;
            Debug.Log($"Updated chord at index {index}: {note} {chordType}");
        }

        onProgressionChanged?.Invoke();
    }

    public void RemoveChordFromCurrentProgression(int index)
    {
        if (currentProgression == null || index < 0 || index >= currentProgression.progression.Count) return;
        currentProgression.progression.RemoveAt(index);
        onProgressionChanged?.Invoke();
    }

    public void UpdateChordInCurrentProgression(int index, int note, int chordType)
    {
        if (currentProgression == null || index < 0 || index >= currentProgression.progression.Count) return;
        
        // Validate that we have valid chord data (not -1)
        if (note < 0 || chordType < 0)
        {
            Debug.LogWarning($"Invalid chord data: note={note}, chordType={chordType}. Cannot update chord at index {index}.");
            return;
        }
        
        currentProgression.progression[index].note = note;
        currentProgression.progression[index].chordtype = chordType;
        onProgressionChanged?.Invoke();
    }

    // ===== SAVING AND LOADING METHODS =====

    /// <summary>
    /// Load all saved progressions and custom chords from the player.fun file
    /// </summary>
    public void LoadFromFile()
    {
        string path = Application.persistentDataPath + "/player.fun";

        if (File.Exists(path))
        {
            savedData data = SaveSystem.Loaddata();
            if (data != null)
            {
                // Initialize arrays if they're null
                if (data.savedProgressions_tonics == null)
                {
                    data.savedProgressions_tonics = new int[15, 40];
                    Debug.Log("savedProgressions_tonics was null, initialized");
                }
                if (data.savedProgressions_chordtypes == null)
                {
                    data.savedProgressions_chordtypes = new int[15, 40];
                    Debug.Log("savedProgressions_chordtypes was null, initialized");
                }
                if (data.savedProgression_names == null)
                {
                    data.savedProgression_names = new string[15];
                    Debug.Log("savedProgression_names was null, initialized");
                }
                if (data.savedCustomChordTypes == null)
                {
                    data.savedCustomChordTypes = new int[20, 12];
                    Debug.Log("savedCustomChordTypes was null, initialized");
                }
                if (data.savedCustomChord_names == null)
                {
                    data.savedCustomChord_names = new string[20];
                    Debug.Log("savedCustomChord_names was null, initialized");
                }

                // Convert serialized data to usable format
                ConvertSerializedProgressionsToUsableData(data);
                ConvertSerializedCustomChordsToUsableData(data);
            }
        }
        else
        {
            Debug.Log("No saved data file found at: " + path);
        }
    }

    /// <summary>
    /// Save all progressions and custom chords to the player.fun file
    /// </summary>
    public void SaveToFile()
    {
        savedData data = LoadExistingData();
        if (data == null)
        {
            data = new savedData();
        }

        // Convert current data to serializable format
        ConvertProgressionsToSerializable(data);
        ConvertCustomChordsToSerializable(data);

        // Save to file
        SaveSystem.SavePlayer(data);
    }

    /// <summary>
    /// Load existing data from file without overwriting current ScriptableObject data
    /// </summary>
    private savedData LoadExistingData()
    {
        string path = Application.persistentDataPath + "/player.fun";
        if (File.Exists(path))
        {
            return SaveSystem.Loaddata();
        }
        return null;
    }

    /// <summary>
    /// Convert serialized progression data from savedData to ScriptableObject format
    /// </summary>
    private void ConvertSerializedProgressionsToUsableData(savedData data)
    {
        savedProgressions.Clear();
        
        for (int i = 0; i < 15; i++)
        {
            int progressionLength = data.savedProgressions_chordtypes[i, 0];
            if (progressionLength == 0) break;

            List<Pair> progression = new List<Pair>();
            for (int j = 1; j <= progressionLength; j++)
            {
                int note = data.savedProgressions_tonics[i, j];
                int chordtype = data.savedProgressions_chordtypes[i, j];
                
                // Only add valid chords (non-negative values)
                if (note >= 0 && chordtype >= 0)
                {
                    Pair newPair = new Pair
                    {
                        note = note,
                        chordtype = chordtype
                    };
                    progression.Add(newPair);
                }
                else
                {
                    Debug.LogWarning($"Skipping invalid chord data at position {j}: note={note}, chordtype={chordtype}");
                }
            }

            // Only create progression if it has valid chords
            if (progression.Count > 0)
            {
                ChordProgression chordProgression = new ChordProgression(progression, data.savedProgression_names[i]);
                savedProgressions.Add(chordProgression);
                Debug.Log($"Loaded progression '{data.savedProgression_names[i]}' with {progression.Count} valid chords");
            }
            else
            {
                Debug.LogWarning($"Skipping progression '{data.savedProgression_names[i]}' - no valid chords found");
            }
        }

        Debug.Log($"Loaded {savedProgressions.Count} progressions from file");
    }

    /// <summary>
    /// Convert serialized custom chord data from savedData to ScriptableObject format
    /// </summary>
    private void ConvertSerializedCustomChordsToUsableData(savedData data)
    {
        customChords.Clear();

        for (int i = 0; i < 20; i++)
        {
            List<int> intervals = new List<int>();
            for (int j = 0; j < data.savedCustomChordTypes.GetLength(1); j++)
            {
                int temp = data.savedCustomChordTypes[i, j];
                if (temp != -1)
                {
                    intervals.Add(temp);
                }
                else
                {
                    break;
                }
            }

            if (intervals.Count > 0 && !string.IsNullOrEmpty(data.savedCustomChord_names[i]))
            {
                CustomChordType customChord = new CustomChordType(intervals, data.savedCustomChord_names[i]);
                customChords.Add(customChord);
            }
        }

        Debug.Log($"Loaded {customChords.Count} custom chords from file");
    }

    /// <summary>
    /// Convert ScriptableObject progression data to serializable format for saving
    /// </summary>
    private void ConvertProgressionsToSerializable(savedData data)
    {
        // Reset arrays before reassigning
        int rows = data.savedProgressions_tonics.GetLength(0);
        int columns = data.savedProgressions_tonics.GetLength(1);

        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < columns; j++)
            {
                data.savedProgressions_tonics[i, j] = 0;
                data.savedProgressions_chordtypes[i, j] = 0;
            }
            data.savedProgression_names[i] = null;
        }

        // Convert progressions to serializable format
        for (int i = 0; i < savedProgressions.Count; i++)
        {
            data.savedProgressions_chordtypes[i, 0] = savedProgressions[i].progression.Count;
            data.savedProgressions_tonics[i, 0] = savedProgressions[i].progression.Count;

            for (int j = 1; j <= savedProgressions[i].progression.Count; j++)
            {
                data.savedProgressions_tonics[i, j] = savedProgressions[i].progression[j - 1].note;
                data.savedProgressions_chordtypes[i, j] = savedProgressions[i].progression[j - 1].chordtype;
                data.savedProgression_names[i] = savedProgressions[i].name;
            }
        }

        Debug.Log($"Saved {savedProgressions.Count} progressions to file");
    }

    /// <summary>
    /// Convert ScriptableObject custom chord data to serializable format for saving
    /// </summary>
    private void ConvertCustomChordsToSerializable(savedData data)
    {
        // Reset arrays before reassigning
        int rows = data.savedCustomChordTypes.GetLength(0);
        int columns = data.savedCustomChordTypes.GetLength(1);

        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < columns; j++)
            {
                data.savedCustomChordTypes[i, j] = -1;
            }
            data.savedCustomChord_names[i] = null;
        }

        // Convert custom chords to serializable format
        for (int i = 0; i < customChords.Count; i++)
        {
            for (int j = 0; j < customChords[i].intervals.Count; j++)
            {
                data.savedCustomChordTypes[i, j] = customChords[i].intervals[j];
                data.savedCustomChord_names[i] = customChords[i].name;
            }
        }

        Debug.Log($"Saved {customChords.Count} custom chords to file");
    }

    /// <summary>
    /// Add a custom chord type to the list
    /// </summary>
    public void AddCustomChordType(List<int> intervals, string name)
    {
        if (string.IsNullOrEmpty(name))
        {
            Debug.LogError("Custom chord name cannot be empty!");
            return;
        }

        CustomChordType customChord = new CustomChordType(intervals, name);
        customChords.Add(customChord);
        SaveToFile();
    }

    /// <summary>
    /// Remove a custom chord type from the list
    /// </summary>
    public void RemoveCustomChordType(int index)
    {
        if (index >= 0 && index < customChords.Count)
        {
            customChords.RemoveAt(index);
            SaveToFile();
        }
    }

    /// <summary>
    /// Get all saved progression names for dropdown population
    /// </summary>
    public List<string> GetProgressionNames()
    {
        List<string> names = new List<string>();
        foreach (var progression in savedProgressions)
        {
            names.Add(progression.name);
        }
        return names;
    }

    /// <summary>
    /// Get all custom chord names for dropdown population
    /// </summary>
    public List<string> GetCustomChordNames()
    {
        List<string> names = new List<string>();
        foreach (var customChord in customChords)
        {
            names.Add(customChord.name);
        }
        return names;
    }

    /// <summary>
    /// Test method to verify saving and loading functionality
    /// </summary>
    [ContextMenu("Test Save/Load Functionality")]
    public void TestSaveLoadFunctionality()
    {
        Debug.Log("=== Testing Save/Load Functionality ===");
        
        // Clear current data
        savedProgressions.Clear();
        customChords.Clear();
        
        // Create test progression
        List<Pair> testProgression = new List<Pair>
        {
            new Pair { note = 0, chordtype = 0 },  // A major
            new Pair { note = 5, chordtype = 1 },  // D minor
            new Pair { note = 7, chordtype = 0 }   // E major
        };
        
        ChordProgression testProg = new ChordProgression(testProgression, "Test Progression");
        savedProgressions.Add(testProg);
        
        // Create test custom chord
        List<int> testIntervals = new List<int> { 0, 4, 7, 10 }; // maj7 intervals
        CustomChordType testCustomChord = new CustomChordType(testIntervals, "Test Custom Chord");
        customChords.Add(testCustomChord);
        
        Debug.Log($"Created test data: {savedProgressions.Count} progressions, {customChords.Count} custom chords");
        
        // Save to file
        SaveToFile();
        Debug.Log("Data saved to file");
        
        // Clear current data
        savedProgressions.Clear();
        customChords.Clear();
        Debug.Log("Cleared current data");
        
        // Load from file
        LoadFromFile();
        Debug.Log($"Loaded data: {savedProgressions.Count} progressions, {customChords.Count} custom chords");
        
        // Verify data
        if (savedProgressions.Count > 0)
        {
            Debug.Log($"First progression: {savedProgressions[0].name} with {savedProgressions[0].progression.Count} chords");
        }
        
        if (customChords.Count > 0)
        {
            Debug.Log($"First custom chord: {customChords[0].name} with {customChords[0].intervals.Count} intervals");
        }
        
        Debug.Log("=== Test Complete ===");
    }

    /// <summary>
    /// Test method to verify the -1 system for empty chords
    /// </summary>
    [ContextMenu("Test -1 Empty Chord System")]
    public void TestNegativeOneSystem()
    {
        Debug.Log("=== Testing -1 Empty Chord System ===");
        
        // Clear current data
        savedProgressions.Clear();
        
        // Create test progression with some empty slots
        List<Pair> testProgression = new List<Pair>
        {
            new Pair { note = 0, chordtype = 0 },   // A major (valid)
            new Pair { note = -1, chordtype = -1 }, // Empty slot (should be ignored)
            new Pair { note = 5, chordtype = 1 },   // D minor (valid)
            new Pair { note = -1, chordtype = 0 },  // Invalid (note -1, chordtype 0)
            new Pair { note = 7, chordtype = -1 },  // Invalid (note 7, chordtype -1)
            new Pair { note = 9, chordtype = 1 }    // A minor (valid)
        };
        
        ChordProgression testProg = new ChordProgression(testProgression, "Test -1 System");
        savedProgressions.Add(testProg);
        
        Debug.Log($"Created test progression with {testProgression.Count} total pairs");
        
        // Test AddChordToCurrentProgression with invalid data
        CreateNewProgression();
        Debug.Log("Testing AddChordToCurrentProgression with invalid data:");
        AddChordToCurrentProgression(-1, 0, 0);  // Should be ignored
        AddChordToCurrentProgression(0, -1, 0);  // Should be ignored
        AddChordToCurrentProgression(-1, -1, 0); // Should be ignored
        AddChordToCurrentProgression(0, 0, 0);   // Should work
        
        Debug.Log($"Current progression has {currentProgression.progression.Count} chords after adding valid and invalid data");
        
        // Test UpdateChordInCurrentProgression with invalid data
        if (currentProgression.progression.Count > 0)
        {
            Debug.Log("Testing UpdateChordInCurrentProgression with invalid data:");
            UpdateChordInCurrentProgression(0, -1, 0); // Should be ignored
            Debug.Log($"Chord at index 0 after invalid update: note={currentProgression.progression[0].note}, chordtype={currentProgression.progression[0].chordtype}");
        }
        
        Debug.Log("=== -1 System Test Complete ===");
    }
} 