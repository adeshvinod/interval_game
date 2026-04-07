using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System.Linq;

public enum LearningMode
{
    Intervals,
    Notes,
    Progressions,
    Chords
}

[CreateAssetMenu(fileName = "GameSettings", menuName = "Settings/Game Settings")]
public class GameSettings : ScriptableObject
{
    public static GameSettings instance;

    [Header("Learning Mode")]
    public LearningMode currentLearningMode = LearningMode.Intervals;

    // Events
    public delegate void TuningChangedHandler();
    public event TuningChangedHandler OnTuningChanged;

    public delegate void LearningModeChangedHandler(LearningMode newMode);
    public event LearningModeChangedHandler OnLearningModeChanged;

    private void OnEnable()
    {
        if (!Application.isPlaying) return;

        savedData data = SaveSystem.Loaddata();
        if (data != null && data.transposedNotes_audio != null && data.transposedNotes_audio.Length == 6)
        {
            for (int i = 0; i < 6; i++)
                transposedNotes_audio[i] = data.transposedNotes_audio[i];
        }
        else
        {
            for (int i = 0; i < 6; i++)
                transposedNotes_audio[i] = 0;
        }
    }

    

    public void SetLearningMode(int modeIndex)
    {
        currentLearningMode = (LearningMode)modeIndex;
        OnLearningModeChanged?.Invoke(currentLearningMode);
    }

    [Header("Note Names")]
    public Dictionary<int, string> notename_sharps = new Dictionary<int, string>()
    {
        {0, "A"},
        {1, "A#"},
        {2, "B"},
        {3, "C"},
        {4, "C#"},
        {5, "D"},
        {6, "D#"},
        {7, "E"},
        {8, "F"},
        {9, "F#"},
        {10, "G"},
        {11, "G#"}
    };

     public readonly Dictionary<int,string> notenames_highE = new Dictionary<int,string>(){
        
  
        {0, "E4"},
        {1, "F4"},
        {2, "F#4"},
        {3, "G4"},
        {4, "G#4"},
        {5, "A4"},
        {6, "A#4"},
        {7, "B4"},
        {8, "C5"},
        {9, "C#5"}, 
        {10, "D5"},
        {11, "D#5"},
        {12, "E5"},
        {13, "F5"},
        {14, "F#5"},
        {15, "G5"}, 
        {16, "G#5"},
        {17, "A5"},
        {18, "A#5"},
        {19, "B5"},
        {20, "C6"},
        {21, "C#6"},
        {22, "D6"},
        {23, "D#6"},
        {24, "E6"},
        {25, "F6"},
        {26, "F#6"},
        {27, "G6"},
         {-1,"D#4"}, //-1 is the transposed value for the high E string
        {-2,"D4"}, //-2 is the transposed value for the high E string
        {-3,"C#4"}, //-3 is the transposed value for the high E string
        {-4,"C4"}, //-4 is the transposed value for the high E string
        {-5,"B3"}, //-5 is the transposed value for the high E string
        {-6,"A#3"}, //-6 is the transposed value for the high E string
        {-7,"A3"}, //-7 is the transposed value for the high E string
        {-8,"G#3"}, //-8 is the transposed value for the high E string  
        {-9,"G3"}, //-9 is the transposed value for the high E string
        {-10,"F#3"}, //-10 is the transposed value for the high E string
        {-11,"F3"}, //-11 is the transposed value for the high E string
        {-12,"E3"}, //-12 is the transposed value for the high E string
        {-13,"D#3"}, //-13 is the transposed value for the high E string
        {-14,"D3"}, //-14 is the transposed value for the high E string 
        {-15,"C#3"}, //-15 is the transposed value for the high E string
        {-16,"C3"}, //-16 is the transposed value for the high E string
        {-17,"B2"}, //-17 is the transposed value for the high E string
        {-18,"A#2"}, //-18 is the transposed value for the high E string
        {-19,"A2"}, //-19 is the transposed value for the high E string
        {-20,"G#2"}, //-20 is the transposed value for the high E string    
        {-21,"G2"}, //-21 is the transposed value for the high E string
        {-22,"F#2"}, //-22 is the transposed value for the high E string
        {-23,"F2"}, //-23 is the transposed value for the high E string
        {-24,"E2"}, //-24 is the transposed value for the high E string
        {-25,"D#2"}, //-25 is the transposed value for the high E string
        {-26,"D2"}, //-26 is the transposed value for the high E string
    };
    
    public readonly Dictionary<int,string> notenames_BString = new Dictionary<int,string>(){
        {0, "B3"},
        {1, "C4"},
        {2, "C#4"},
        {3, "D4"},
        {4, "D#4"},
        {5, "E4"},
        {6, "F4"},
        {7, "F#4"},
        {8, "G4"},
        {9, "G#4"},
        {10, "A4"},
        {11, "A#4"},
        {12, "B4"},
        {13, "C5"},
        {14, "C#5"},
        {15, "D5"},
        {16, "D#5"},
        {17, "E5"},
        {18, "F5"},
        {19, "F#5"},
        {20, "G5"},
        {21, "G#5"},
        {22, "A5"},
        {23, "A#5"},
        {24, "B5"},
        {25, "C6"},
        {26, "C#6"},
        {27, "D6"},
        {28, "D#6"},
        {29, "E6"},
        {30, "F6"},
        {31, "F#6"},
        {-1,"A#3"},
        {-2,"A3"},
        {-3,"G#3"},
        {-4,"G3"},
        {-5,"F#3"},
        {-6,"F3"},
        {-7,"E3"},
        {-8,"D#3"},
        {-9,"D3"},
        {-10,"C#3"},
        {-11,"C3"},
        {-12,"B2"},
        {-13,"A#2"},
        {-14,"A2"},
        {-15,"G#2"},
        {-16,"G2"},
        {-17,"F#2"},
        {-18,"F2"},
        {-19,"E2"}, 
        {-20,"D#2"},
        {-21,"D2"},
        {-22,"C#2"},
        {-23,"C2"},
        {-24,"B1"},
        {-25,"A#1"},
    };  

    public readonly Dictionary<int,string> notenames_GString = new Dictionary<int,string>(){
        {0, "G3"},
        {1, "G#3"},
        {2, "A3"},
        {3, "A#3"},
        {4, "B3"},
        {5, "C4"},
        {6, "C#4"},
        {7, "D4"},
        {8, "D#4"},
        {9, "E4"},
        {10, "F4"},
        {11, "F#4"},
        {12, "G4"},
        {13, "G#4"},
        {14, "A4"},
        {15, "A#4"},
        {16, "B4"},
        {17, "C5"},
        {18, "C#5"},        
        {19, "D5"},
        {20, "D#5"},
        {21, "E5"},
        {22, "F5"},
        {23, "F#5"},
        {24, "G5"},
        {25, "G#5"},
        {-1,"F#3"},
        {-2,"F3"},
        {-3,"E3"},
        {-4,"D#3"},
        {-5,"D3"},
        {-6,"C#3"},
        {-7,"C3"},
        {-8,"B2"},
        {-9,"A#2"},
        {-10,"A2"},
        {-11,"G#2"},
        {-12,"G2"},
        {-13,"F#2"},
        {-14,"F2"},
        {-15,"E2"},
        {-16,"D#2"},
        {-17,"D2"},
        {-18,"C#2"},
        {-19,"C2"},
        {-20,"B1"},
        {-21,"A#1"},
        {-22,"A1"},
        {-23,"G#1"},
        {-24,"G1"},
        {-25,"F#1"},
        {-26,"F1"},
    };

    public readonly Dictionary<int,string> notenames_DString = new Dictionary<int,string>(){
        {0, "D3"},
        {1, "D#3"},
        {2, "E3"},
        {3, "F3"},
        {4, "F#3"},
        {5, "G3"},
        {6, "G#3"},
        {7, "A3"},
        {8, "A#3"},
        {9, "B3"},
        {10, "C4"},
        {11, "C#4"},
        {12, "D4"},
        {13, "D#4"},
        {14, "E4"},
        {15, "F4"},
        {16, "F#4"},
        {17, "G4"},
        {18, "G#4"},
        {19, "A4"},
        {20, "A#4"},
        {21, "B4"},
        {22, "C5"},
        {23, "C#5"},
        {24, "D5"},
        {25, "D#5"},
        {26, "E5"},
        {27, "F5"},
        {28, "F#5"},
        {-1,"C#3"},
        {-2,"C3"},
        {-3,"B2"},
        {-4,"A#2"},
        {-5,"A2"},
        {-6,"G#2"},
        {-7,"G2"},  
        {-8,"F#2"},
        {-9,"F2"},
        {-10,"E2"},
        {-11,"D#2"},
        {-12,"D2"},
        {-13,"C#2"},
        {-14,"C2"},
        {-15,"B1"},
        {-16,"A#1"},
        {-17,"A1"},
        {-18,"G#1"},
        {-19,"G1"},
    };

    public readonly Dictionary<int,string> notenames_AString = new Dictionary<int,string>(){
        {0, "A2"},
        {1, "A#2"},
        {2, "B2"},
        {3, "C3"},
        {4, "C#3"},
        {5, "D3"},
        {6, "D#3"},
        {7, "E3"},
        {8, "F3"},
        {9, "F#3"},
        {10, "G3"},
        {11, "G#3"},
        {12, "A3"},
        {13, "A#3"},
        {14, "B3"},
        {15, "C4"},
        {16, "C#4"},
        {17, "D4"},
        {18, "D#4"},
        {19, "E4"},
        {20, "F4"},
        {21, "F#4"},
        {22, "G4"},
        {23, "G#4"},
        {24, "A4"},
        {25, "A#4"},
        {26, "B4"},
        {27, "C5"},
        {28, "C#5"},
        {-1,"G#2"},
        {-2,"G2"},
        {-3,"F#2"},
        {-4,"F2"},
        {-5,"E2"},
        {-6,"D#2"},
        {-7,"D2"},
        {-8,"C#2"},
        {-9,"C2"},
        {-10,"B1"},
        {-11,"A#1"},
        {-12,"A1"},
        {-13,"G#1"},
        {-14,"G1"},
        {-15,"F#1"},
        {-16,"F1"},
        {-17,"E1"},
        {-18,"D#1"},
        {-19,"D1"},
        {-20,"C#1"},
    };

    public readonly Dictionary<int,string> notenames_lowEString = new Dictionary<int,string>(){
        {0, "E2"},
        {1, "F2"},
        {2, "F#2"},
        {3, "G2"},
        {4, "G#2"},
        {5, "A2"},
        {6, "A#2"},
        {7, "B2"},
        {8, "C3"},
        {9, "C#3"},
        {10, "D3"},
        {11, "D#3"},
        {12, "E3"},
        {13, "F3"},
        {14, "F#3"},
        {15, "G3"},
        {16, "G#3"},
        {17, "A3"},
        {18, "A#3"},
        {19, "B3"},
        {20, "C4"},
        {21, "C#4"},
        {22, "D4"},
        {23, "D#4"},
        {24, "E4"},
        {25, "F4"},
        {26, "F#4"},
        {27, "G4"},
        {28, "G#4"},
        {-1,"D#2"},
        {-2,"D2"},
        {-3,"C#2"},
        {-4,"C2"},
        {-5,"B1"},
        {-6,"A#1"},
        {-7,"A1"},
        {-8,"G#1"},
        {-9,"G1"},
        {-10,"F#1"},
        {-11,"F1"},
        {-12,"E1"},
        {-13,"D#1"},
        {-14,"D1"},
        {-15,"C#1"},
        {-16,"C1"},
        {-17,"B1"},
        {-18,"A#1"},
        {-19,"A1"},
        {-20,"G#1"},
    };

    [Header("Tuning Settings")]
    public int[] standardTuningNotes = new int[] { 7, 2, 10, 5, 0, 7 }; // E, B, G, D, A, E
    public int[] transposedNotes = new int[6]; // Will store the transposition for each string

    public int[] transposedNotes_audio = new int[6];// Will store the transposition for each string for audio, these value can be negative numbers

    [Header("Pre-calculated Note Values")]
    public Dictionary<int, int> fretboardNoteValues = new Dictionary<int, int>(); // buttonNumber -> noteValue (for Notes/Progressions mode)
    public Dictionary<int, int> fretboardNoteValues_IntervalsMode = new Dictionary<int, int>(); // buttonNumber -> noteValue (for Intervals mode)
    public Dictionary<int, int> fretboardNoteValues_ChordsMode = new Dictionary<int, int>(); // buttonNumber -> noteValue (for Chords mode, 5-fret stride)

    public int GetTransposedNote(int stringNumber)
    {
        //int transposedValue = (standardTuningNotes[stringNumber] + transposedNotes_audio[stringNumber]) % 12;
        //return transposedValue < 0 ? transposedValue + 12 : transposedValue;
        
        int transposedValue = transposedNotes_audio[stringNumber];    
//Debug.Log("Transposed value is: " + transposedValue+" standardTuningNotes[stringNumber] is: "+standardTuningNotes[stringNumber]+" transposedNotes_audio[stringNumber] is: "+transposedNotes_audio[stringNumber]);
        return transposedValue;
    }

    public void InitializeFretboard()
    {
        fretboardNoteValues.Clear();
        fretboardNoteValues_IntervalsMode.Clear();
        
        // Initialize Notes/Progressions mode (78 buttons: 13 frets × 6 strings)
        for (int buttonNumber = 0; buttonNumber < 78; buttonNumber++)
        {
            int x_coord = buttonNumber % 13;  // fret position (0-12)
            int stringNumber = buttonNumber / 13;  // string number (0-5)
            
            int noteValue = GetNoteValue(x_coord, stringNumber);
            fretboardNoteValues[buttonNumber] = noteValue;
        }
        
        // Initialize Intervals mode (42 buttons: 7 frets × 6 strings)
        for (int buttonNumber = 0; buttonNumber < 42; buttonNumber++)
        {
            int x_coord = buttonNumber % 7;   // fret position (0-6)
            int stringNumber = buttonNumber / 7;   // string number (0-5)

            int noteValue = GetNoteValue(x_coord, stringNumber);
            fretboardNoteValues_IntervalsMode[buttonNumber] = noteValue;
        }

        // Initialize Chords mode (30 buttons: 5 frets × 6 strings)
        fretboardNoteValues_ChordsMode.Clear();
        for (int buttonNumber = 0; buttonNumber < 30; buttonNumber++)
        {
            int x_coord      = buttonNumber % 5;  // fret position (0-4)
            int stringNumber = buttonNumber / 5;  // string number (0-5)

            int noteValue = GetNoteValue(x_coord, stringNumber);
            fretboardNoteValues_ChordsMode[buttonNumber] = noteValue;
        }

        Debug.Log($"Fretboard initialized - Notes/Progressions: {fretboardNoteValues.Count}, Intervals: {fretboardNoteValues_IntervalsMode.Count}, Chords: {fretboardNoteValues_ChordsMode.Count}");
    }

    public int GetFretboardNoteValue(int buttonNumber)
    {
        // Determine which dictionary to use based on current learning mode
        Dictionary<int, int> targetDictionary;
        
        if (currentLearningMode == LearningMode.Intervals)
        {
            targetDictionary = fretboardNoteValues_IntervalsMode;
        }
        else if (currentLearningMode == LearningMode.Chords)
        {
            if (fretboardNoteValues_ChordsMode == null || fretboardNoteValues_ChordsMode.Count == 0)
                InitializeFretboard();
            targetDictionary = fretboardNoteValues_ChordsMode;
        }
        else
        {
            targetDictionary = fretboardNoteValues;
        }
        
        if (targetDictionary.TryGetValue(buttonNumber, out int noteValue))
        {
            return noteValue;
        }
        else
        {
            Debug.LogError($"Note value not found for button {buttonNumber} in {currentLearningMode} mode");
            return -1;
        }
    }

    public void TransposeUp(int stringNumber)
    {
        if(transposedNotes_audio[stringNumber]==12)
        {
            Debug.LogError("Transposed value is 12, cannot transpose up");
            return;
        }

        transposedNotes_audio[stringNumber] = (transposedNotes_audio[stringNumber] + 1);
        InitializeFretboard(); // Recalculate all note values
        OnTuningChanged?.Invoke();
        SaveTuning();
    }

    public void TransposeDown(int stringNumber)
    {
        if(transposedNotes_audio[stringNumber]==-12)
        {
            Debug.LogError("Transposed value is -12, cannot transpose down");
            return;
        }
        transposedNotes_audio[stringNumber] = transposedNotes_audio[stringNumber] - 1;
        InitializeFretboard(); // Recalculate all note values
        OnTuningChanged?.Invoke();
        SaveTuning();
    }

    private void SaveTuning()
    {
        savedData data = SaveSystem.Loaddata() ?? new savedData();
        data.transposedNotes_audio = (int[])transposedNotes_audio.Clone();
        SaveSystem.SavePlayer(data);
    }

    public string GetNoteName(int noteValue)
    {
        return notename_sharps[noteValue];
    }

    //this function will return the notevalue integer value  and will take in x coordinate, string number and transposed string value. xcoordinate can be negative and we will use the dictionaries to get the note names and return the notevalue int, so filter out the octave numbers
    public int GetNoteValue(int x_coord, int stringNumber)
    {
        string notename;
        string noteNameOnly;
        int notevalue;
        
        switch (stringNumber)
        {
            case 0:  
                notename = notenames_highE[x_coord+transposedNotes_audio[stringNumber]];
                //we need to filter out the octave number from the notename and return the notevalue int
                noteNameOnly = notename.Substring(0, notename.Length - 1);
                //now we need to convert the noteNameOnly to an integer value using the notename_sharps dictionary
                notevalue = notename_sharps.FirstOrDefault(x => x.Value == noteNameOnly).Key;
                return notevalue;      
            case 1:
                notename = notenames_BString[x_coord+transposedNotes_audio[stringNumber]];
                noteNameOnly = notename.Substring(0, notename.Length - 1);
                notevalue = notename_sharps.FirstOrDefault(x => x.Value == noteNameOnly).Key;
                return notevalue;
            case 2:
                notename = notenames_GString[x_coord+transposedNotes_audio[stringNumber]];
                noteNameOnly = notename.Substring(0, notename.Length - 1);
                notevalue = notename_sharps.FirstOrDefault(x => x.Value == noteNameOnly).Key;
                return notevalue;
            case 3:
                notename = notenames_DString[x_coord+transposedNotes_audio[stringNumber]];
                noteNameOnly = notename.Substring(0, notename.Length - 1);
                notevalue = notename_sharps.FirstOrDefault(x => x.Value == noteNameOnly).Key;
                return notevalue;
            case 4:
                notename = notenames_AString[x_coord+transposedNotes_audio[stringNumber]];
                noteNameOnly = notename.Substring(0, notename.Length - 1);
                notevalue = notename_sharps.FirstOrDefault(x => x.Value == noteNameOnly).Key;
                return notevalue;
            case 5:
                notename = notenames_lowEString[x_coord+transposedNotes_audio[stringNumber]];
                noteNameOnly = notename.Substring(0, notename.Length - 1);
                notevalue = notename_sharps.FirstOrDefault(x => x.Value == noteNameOnly).Key;
                return notevalue;
            default:
                Debug.Log("Invalid string number, failed to get note value in switch statement in gameSettings.cs");
                return -1;
        }
    }

    void Start()
    {
        for(int i=0;i<6;i++)
        {
            transposedNotes_audio[i]=0;
        }
        
        InitializeFretboard(); // Initialize the fretboard on start
    }
} 