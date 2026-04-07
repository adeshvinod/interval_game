using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class audioManager : MonoBehaviour
{
    private static audioManager instance;
    private Dictionary<string, AudioSource> noteSources = new Dictionary<string, AudioSource>();

    // Octave shift points for each string in standard tuning
    private readonly int[] octave_shift_point = new int[] {
        8,  // 1st string (E)
        1,  // 2nd string (B)
        5,  // 3rd string (G)
        10, // 4th string (D)
        3,  // 5th string (A)
        8   // 6th string (E)
    };

    // Base octave for each string in standard tuning
    private readonly int[] base_octave = new int[] {
        4,  // 1st string (E)
        3,  // 2nd string (B)
        3,  // 3rd string (G)
        3,  // 4th string (D)
        2,  // 5th string (A)
        2   // 6th string (E)
    };

    //dictioany of audio clips to play on high E string
    private readonly Dictionary<int,string> notenames_highE = new Dictionary<int,string>(){
        
  
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
    
    private readonly Dictionary<int,string> notenames_BString = new Dictionary<int,string>(){
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

    private readonly Dictionary<int,string> notenames_GString = new Dictionary<int,string>(){
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
        {-26,"A1"},
    };

    private readonly Dictionary<int,string> notenames_DString = new Dictionary<int,string>(){
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

    private readonly Dictionary<int,string> notenames_AString = new Dictionary<int,string>(){
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

    private readonly Dictionary<int,string> notenames_lowEString = new Dictionary<int,string>(){
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
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeNoteSources();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void InitializeNoteSources()
    {
        // Get all child AudioSources
        AudioSource[] sources = GetComponentsInChildren<AudioSource>();
        foreach (AudioSource source in sources)
        {
            // Silence on load — only play when explicitly called
            source.playOnAwake = false;
            source.Stop();

            // Optimize AudioSource for low latency
            source.priority = 0;         // Highest priority for immediate playback
            source.bypassEffects = true; // Bypass audio effects for lower latency
            source.bypassListenerEffects = true; // Bypass listener effects
            source.bypassReverbZones = true;     // Bypass reverb zones
            
            noteSources[source.gameObject.name] = source;
        }
    }
/*
    private string GetNoteName(int noteValue, int x_coord, int stringNumber, int transposedStringValue)
    {
        // Get the base note value (0-11)
        int baseNote = noteValue % 12;
        
        // Calculate the adjusted octave shift point based on transposition
        int adjustedShiftPoint = octave_shift_point[stringNumber] - transposedStringValue;
        
        // Determine if we need to handle negative shift point
        bool hasNegativeShift = adjustedShiftPoint < 0;
        if (hasNegativeShift)
        {
            adjustedShiftPoint += 12;
        }

        // Calculate the octave
        int octave = base_octave[stringNumber];
        
        // If we have a negative shift, increment the starting octave
        if (hasNegativeShift)
        {
            octave++;
        }
        
        // If x_coord is past the shift point, increment octave
        if (x_coord >= adjustedShiftPoint)
        {
            octave++;
        }

        // Convert note value to note name
        string[] noteNames = { "A", "A#", "B", "C", "C#", "D", "D#", "E", "F", "F#", "G", "G#" };
        string noteName = noteNames[baseNote] + octave;
        
        #if UNITY_EDITOR
        Debug.Log($"Audio calculation: x_coord={x_coord}, stringNumber={stringNumber}, transposedStringValue={transposedStringValue}, " +
                  $"octave_shift_point={octave_shift_point[stringNumber]}, adjustedShiftPoint={adjustedShiftPoint}, " +
                  $"base_octave={base_octave[stringNumber]}, hasNegativeShift={hasNegativeShift}, octave={octave}, " +
                  $"baseNote={baseNote}, finalNoteName={noteName}");
        #endif
        
        return noteName;
    }
*/
private string GetNoteName(int noteValue, int x_coord, int stringNumber, int transposedStringValue)
{
    int key = x_coord + transposedStringValue;
    
    switch (stringNumber)
    {
        case 0:
            return notenames_highE.TryGetValue(key, out string note0) ? note0 : "E4";
        case 1:
            return notenames_BString.TryGetValue(key, out string note1) ? note1 : "B3";
        case 2:
            return notenames_GString.TryGetValue(key, out string note2) ? note2 : "G3";
        case 3:
            return notenames_DString.TryGetValue(key, out string note3) ? note3 : "D3";
        case 4:
            return notenames_AString.TryGetValue(key, out string note4) ? note4 : "A2";
        case 5:
            return notenames_lowEString.TryGetValue(key, out string note5) ? note5 : "E2";
        default:
            return "E4"; // Default fallback
    }
}
    public static void PlayNote(int noteValue, int x_coord, int stringNumber, int transposedStringValue)
    {
        if (instance == null)
        {
            Debug.LogError("AudioManager instance not found!");
            return;
        }

     /*   // Get the transposition value for this string
        int transposedStringValue = 0;
        if (global_settings.instance != null && global_settings.instance.transposed_notes_dict != null)
        {
            transposedStringValue = global_settings.instance.transposed_notes_dict[stringNumber];
        }
*/
        string noteName = instance.GetNoteName(noteValue, x_coord, stringNumber, transposedStringValue);
        
        if (instance.noteSources.ContainsKey(noteName))
        {
            AudioSource source = instance.noteSources[noteName];
            source.time = 0.2f;  // Skip silence at beginning of audio clip
            source.Play();
            #if UNITY_EDITOR
            Debug.Log("Playing note: " + noteName + " from 0.2 seconds");
            #endif
        }
        else
        {
            #if UNITY_EDITOR
            Debug.LogWarning($"No audio source found for note: {noteName}");
            #endif
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
