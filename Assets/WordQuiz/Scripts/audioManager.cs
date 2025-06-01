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
            noteSources[source.gameObject.name] = source;
        }
    }

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
        Debug.Log("x coord: " + x_coord + " adjustedShiftPoint: " + adjustedShiftPoint + " baseNote: " + baseNote + " octave: " + octave);
        return noteNames[baseNote] + octave;
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
            instance.noteSources[noteName].Play();
            Debug.Log("Playing note: " + noteName);
        }
        else
        {
            Debug.LogWarning($"No audio source found for note: {noteName}");
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
