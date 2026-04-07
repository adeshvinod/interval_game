using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Drives the learnModeChords scene.
/// Shows chord shapes on the 5-fret board.
/// Shift up/down cycles through all 12 chromatic root notes.
/// Clicking any chord option button triggers arpeggio + strum audio.
/// Clicking a visible note on the board plays only that note.
/// </summary>
public class chordsLearnMode : MonoBehaviour
{
    public static chordsLearnMode instance;

    [SerializeField] private ChordsGameData chordsGameData;
    [SerializeField] private GameSettings   gameSettings;

    // Option buttons — assign in Inspector (one per active chord in the level)
    [SerializeField] private chordsOption[] chordOptionButtons;

    public prog_button[] progbuttons_;

    // Current state
    private int selectedChordTypeIndex = -1;  // index into ChordsGameData.AllChords
    private int currentRootNoteValue   = 0;   // 0=A … 11=G#

    private static readonly Dictionary<int, string> NoteNames = new Dictionary<int, string>()
    {
        {0,"A"},{1,"A#"},{2,"B"},{3,"C"},{4,"C#"},{5,"D"},
        {6,"D#"},{7,"E"},{8,"F"},{9,"F#"},{10,"G"},{11,"G#"}
    };

    private static readonly Dictionary<int, string> IntervalNames = new Dictionary<int, string>()
    {
        {0,"R"},{1,"b2"},{2,"M2"},{3,"b3"},{4,"M3"},{5,"P4"},
        {6,"b5"},{7,"P5"},{8,"b6"},{9,"M6"},{10,"b7"},{11,"M7"}
    };

    // Approximate open-string MIDI values for ascending-pitch sort (standard EADGBE tuning)
    // Index = stringnum (0=high E, 5=low E)
    private static readonly int[] STRING_BASE_MIDI = { 64, 59, 55, 50, 45, 40 };

    private Coroutine audioSequenceCoroutine;

    // ── Unity lifecycle ───────────────────────────────────────────────────────
    void Awake()
    {
        instance = this;
    }

    void Start()
    {
        // Collect prog buttons
        var pb = GameObject.Find("Prog_buttons");
        if (pb != null)
            progbuttons_ = pb.GetComponentsInChildren<prog_button>();
        else
            Debug.LogError("chordsLearnMode: Prog_buttons not found");

        // Auto-setup option buttons if not assigned in Inspector
        if ((chordOptionButtons == null || chordOptionButtons.Length == 0) && chordsGameData != null)
        {
            var found = FindObjectsOfType<chordsOption>();
            System.Array.Sort(found, (a, b) => a.chordTypeIndex.CompareTo(b.chordTypeIndex));
            chordOptionButtons = found;
        }

        // Initialise option button labels from active chord types
        if (chordsGameData != null && chordOptionButtons != null)
        {
            for (int i = 0; i < chordOptionButtons.Length && i < chordsGameData.activeChordTypes.Count; i++)
            {
                chordOptionButtons[i].Setup(chordsGameData.activeChordTypes[i]);
                chordOptionButtons[i].SetVisualState(false);
            }
        }

        // Auto-select the first chord (triggers arpeggio too)
        if (chordsGameData != null && chordsGameData.activeChordTypes.Count > 0)
            SelectChord(chordsGameData.activeChordTypes[0]);
    }

    // ── Public API ────────────────────────────────────────────────────────────

    /// <summary>
    /// Called by chordsOption button click.
    /// Refreshes the fretboard and plays the chord arpeggio + strum, even if
    /// the same chord button is tapped again.
    /// </summary>
    public void SelectChord(int chordTypeIndex)
    {
        selectedChordTypeIndex = chordTypeIndex;

        // Update visual state of option buttons
        if (chordOptionButtons != null)
        {
            foreach (var btn in chordOptionButtons)
                btn.SetVisualState(btn.chordTypeIndex == chordTypeIndex);
        }

        RefreshFretboard();

        // Always trigger arpeggio so re-tapping the same button replays it
        if (audioSequenceCoroutine != null) StopCoroutine(audioSequenceCoroutine);
        audioSequenceCoroutine = StartCoroutine(PlayChordArpeggio());
    }

    /// <summary>Shift the root note up (+1) or down (-1) through 12 chromatic notes.</summary>
    public void ShiftRoot(int direction)
    {
        currentRootNoteValue = (currentRootNoteValue + direction + 12) % 12;
        RefreshFretboard();

        // Re-play arpeggio for new root
        if (audioSequenceCoroutine != null) StopCoroutine(audioSequenceCoroutine);
        audioSequenceCoroutine = StartCoroutine(PlayChordArpeggio());
    }

    /// <summary>Load the challenge scene.</summary>
    public void StartGame()
    {
        SceneManager.LoadScene("challengeChords");
    }

    /// <summary>
    /// Called by prog_button when tapped in learn mode.
    /// Only plays the tapped note — no root-note follow-up.
    /// </summary>
    public void PlayButtonAudio(prog_button clickedButton)
    {
        if (clickedButton.circleIndex == 0) return; // invisible button — no sound
        audioManager.PlayNote(clickedButton.notevalue, clickedButton.x_coord, clickedButton.stringnum,
            gameSettings.transposedNotes_audio[clickedButton.stringnum]);
        StartCoroutine(clickedButton.PulseScale());
    }

    // ── Private ───────────────────────────────────────────────────────────────

    /// <summary>
    /// Repaint the fretboard for the current (chord, rootNote).
    /// Root buttons → circle 3. Chord tones → circle 1. Everything else → circle 0.
    /// </summary>
    private void RefreshFretboard()
    {
        if (progbuttons_ == null || selectedChordTypeIndex < 0) return;

        int[] formula = ChordsGameData.AllChords[selectedChordTypeIndex];

        foreach (prog_button btn in progbuttons_)
        {
            if (btn == null) continue;
            int interval = ChordsGameData.GetInterval(currentRootNoteValue, btn.notevalue);

            bool isRoot      = (interval == 0);
            bool isChordTone = System.Array.IndexOf(formula, interval) >= 0;

            if (isRoot)
            {
                btn.SetActiveCircle(3);
                btn.text.text = "R";
            }
            else if (isChordTone)
            {
                btn.SetActiveCircle(1);
                btn.text.text = IntervalNames.ContainsKey(interval) ? IntervalNames[interval] : interval.ToString();
            }
            else
            {
                btn.SetActiveCircle(0);
                btn.text.text = "";
            }
        }
    }

    /// <summary>
    /// Plays all chord tones in ascending pitch order (arpeggio, 0.3 s gaps),
    /// then immediately follows with a quick strum of the same notes (0.1 s gaps).
    /// Each note triggers the button's pulse animation.
    /// </summary>
    private IEnumerator PlayChordArpeggio()
    {
        // Collect visible chord tone buttons (circle 1 = interval, circle 3 = root)
        var chordButtons = new List<prog_button>();
        if (progbuttons_ != null)
        {
            foreach (var btn in progbuttons_)
            {
                if (btn != null && (btn.circleIndex == 1 || btn.circleIndex == 3))
                    chordButtons.Add(btn);
            }
        }

        if (chordButtons.Count == 0)
        {
            audioSequenceCoroutine = null;
            yield break;
        }

        // Sort ascending pitch: low E string (5) → high E string (0), fret ascending within string
        chordButtons.Sort((a, b) =>
        {
            int aMidi = STRING_BASE_MIDI[Mathf.Clamp(a.stringnum, 0, 5)] + a.x_coord;
            int bMidi = STRING_BASE_MIDI[Mathf.Clamp(b.stringnum, 0, 5)] + b.x_coord;
            return aMidi.CompareTo(bMidi);
        });

        // ── Arpeggio ascend (0.3 s between notes) ──────────────────────────
        foreach (var btn in chordButtons)
        {
            audioManager.PlayNote(btn.notevalue, btn.x_coord, btn.stringnum,
                gameSettings.transposedNotes_audio[btn.stringnum]);
            StartCoroutine(btn.PulseScale());
            yield return new WaitForSeconds(0.3f);
        }

        // Brief pause before strum
        yield return new WaitForSeconds(0.1f);

        // ── Chord strum: all notes in ascending order (0.01 s gaps) ───────
        foreach (var btn in chordButtons)
        {
            audioManager.PlayNote(btn.notevalue, btn.x_coord, btn.stringnum,
                gameSettings.transposedNotes_audio[btn.stringnum]);
            StartCoroutine(btn.PulseScale());
            yield return new WaitForSeconds(0.01f);
        }

        audioSequenceCoroutine = null;
    }
}
