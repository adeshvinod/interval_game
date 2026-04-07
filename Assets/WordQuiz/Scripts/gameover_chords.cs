using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Game-over screen for the Chords game mode.
///
/// Two-stage layout:
///   Stage 1 — Scorecard:  scorecardPanel visible, fretboard hidden.
///             "Continue" button calls GoToLevelSelect() which detects we're still
///             in scorecard stage and calls ShowFeedback() instead.
///
///   Stage 2 — Feedback:   bg_image_gameoverpanel visible, fretboard visible.
///             next_btn / prev_btn cycle through wrong chords.
///             weakspots_text shows the chord name for the currently displayed chord.
///             Clicking "Continue" again (GoToLevelSelect) now goes to chords_levels.
///
/// Wire in Inspector:
///   scorecardPanel        → the parent GO that contains the score stats texts
///   bgImageGameoverPanel  → the feedback overlay GO (contains next_btn, prev_btn, weakspots_text)
///   weakspotsText         → TMP text showing current wrong chord name
///   "Continue" button     → GoToLevelSelect()  (auto-detects stage)
///   next_btn              → ShowNextWrongChord()
///   prev_btn              → ShowPrevWrongChord()
///   "Main Menu" btn       → GoToMainMenu()
///
/// wholeFretboard is auto-found at runtime by name "wholefretboard".
/// </summary>
public class gameover_chords : MonoBehaviour
{
    [SerializeField] private ChordsGameData chordsGameData;
    [SerializeField] private GameSettings   gameSettings;

    // ── Scorecard UI ──────────────────────────────────────────────────────────
    [SerializeField] private TextMeshProUGUI currentScoreText;
    [SerializeField] private TextMeshProUGUI highScoreText;
    [SerializeField] private TextMeshProUGUI correctAnswersText;

    /// <summary>Parent container for all scorecard stats — hidden when entering feedback mode.</summary>
    [SerializeField] private GameObject scorecardPanel;

    // ── Feedback UI ───────────────────────────────────────────────────────────
    /// <summary>The feedback overlay GO containing next_btn, prev_btn, and weakspots_text.</summary>
    [SerializeField] private GameObject bgImageGameoverPanel;

    /// <summary>TMP label that shows the wrong chord name dynamically in feedback mode.</summary>
    [SerializeField] private TextMeshProUGUI weakspotsText;

    // ── Runtime ───────────────────────────────────────────────────────────────
    private GameObject wholeFretboard;
    private prog_button[] progbuttons_;
    private int wrongIndex = 0;
    private bool inFeedbackMode = false;

    private savedData modifiedData = new savedData();

    private static readonly Dictionary<int, string> IntervalNames = new Dictionary<int, string>()
    {
        {0,"R"},{1,"b2"},{2,"M2"},{3,"b3"},{4,"M3"},{5,"P4"},
        {6,"b5"},{7,"P5"},{8,"b6"},{9,"M6"},{10,"b7"},{11,"M7"}
    };

    private static readonly Dictionary<int, string> NoteNamesFlats = new Dictionary<int, string>()
    {
        {0,"A"},{1,"Bb"},{2,"B"},{3,"C"},{4,"Db"},{5,"D"},
        {6,"Eb"},{7,"E"},{8,"F"},{9,"Gb"},{10,"G"},{11,"Ab"}
    };

    // ── Lifecycle ─────────────────────────────────────────────────────────────

    void OnEnable()
    {
        if (chordsGameData == null) return;

        inFeedbackMode = false;

        // Cache prog-button references BEFORE hiding the fretboard
        var pb = GameObject.Find("Prog_buttons");
        if (pb != null)
            progbuttons_ = pb.GetComponentsInChildren<prog_button>();

        // Find the fretboard GO by name (always runtime — no Inspector wiring needed)
        wholeFretboard = GameObject.Find("wholefretboard");

        // Stage 1: show scorecard, hide fretboard and feedback overlay
        if (wholeFretboard       != null) wholeFretboard.SetActive(false);
        if (bgImageGameoverPanel != null) bgImageGameoverPanel.SetActive(false);
        if (scorecardPanel       != null) scorecardPanel.SetActive(true);

        // Display stats
        if (currentScoreText  != null)
            currentScoreText.text = chordsGameData.score.ToString();

        if (correctAnswersText != null)
            correctAnswersText.text = $"Correct: {chordsGameData.correctAnswers} / {chordsGameData.totalQuestions}";

        LoadAndSave();

        if (highScoreText != null)
            highScoreText.text = chordsGameData.highScore.ToString();
    }

    // ── Button callbacks ──────────────────────────────────────────────────────

    /// <summary>
    /// Wire to the "Continue" button.
    /// First call: transitions from scorecard → feedback review panel.
    /// Second call (already in feedback): loads the level-select scene.
    /// </summary>
    public void GoToLevelSelect()
    {
        if (!inFeedbackMode)
        {
            ShowFeedback();
            return;
        }

        SceneManager.LoadScene("chords_levels");
    }

    /// <summary>Transitions from scorecard to fretboard feedback review.</summary>
    public void ShowFeedback()
    {
        inFeedbackMode = true;

        if (scorecardPanel       != null) scorecardPanel.SetActive(false);
        if (wholeFretboard       != null) wholeFretboard.SetActive(true);
        if (bgImageGameoverPanel != null) bgImageGameoverPanel.SetActive(true);

        wrongIndex = 0;
        DisplayCurrentWrongChord();
    }

    /// <summary>Wire to next_btn — cycles to the next wrong chord.</summary>
    public void ShowNextWrongChord()
    {
        if (chordsGameData.wrongChordIndices == null || chordsGameData.wrongChordIndices.Count == 0) return;
        wrongIndex = (wrongIndex + 1) % chordsGameData.wrongChordIndices.Count;
        DisplayCurrentWrongChord();
    }

    /// <summary>Wire to prev_btn — cycles to the previous wrong chord.</summary>
    public void ShowPrevWrongChord()
    {
        if (chordsGameData.wrongChordIndices == null || chordsGameData.wrongChordIndices.Count == 0) return;
        wrongIndex = (wrongIndex - 1 + chordsGameData.wrongChordIndices.Count) % chordsGameData.wrongChordIndices.Count;
        DisplayCurrentWrongChord();
    }

    /// <summary>Wire to a "Main Menu" button.</summary>
    public void GoToMainMenu()
    {
        SceneManager.LoadScene("chords_levels");
    }

    // ── Private helpers ───────────────────────────────────────────────────────

    private void DisplayCurrentWrongChord()
    {
        if (chordsGameData.wrongChordIndices == null || chordsGameData.wrongChordIndices.Count == 0)
        {
            if (weakspotsText != null) weakspotsText.text = "No weak spots!";
            return;
        }

        int chordIdx  = chordsGameData.wrongChordIndices[wrongIndex];
        int rootNote  = chordsGameData.wrongRootNotes[wrongIndex];

        string chordName = ChordsGameData.ChordNames[chordIdx];
        string rootName  = NoteNamesFlats[rootNote];

        if (weakspotsText != null)
            weakspotsText.text = $"{rootName} {chordName}";

        DrawChordOnFretboard(chordIdx, rootNote);
    }

    private void DrawChordOnFretboard(int chordIdx, int rootNote)
    {
        if (progbuttons_ == null) return;

        int[] formula = ChordsGameData.AllChords[chordIdx];

        foreach (var btn in progbuttons_)
        {
            if (btn == null) continue;

            int interval    = ChordsGameData.GetInterval(rootNote, btn.notevalue);
            bool isRoot     = (interval == 0);
            bool isChordTone = System.Array.IndexOf(formula, interval) >= 0;

            if (isRoot)
            {
                btn.SetActiveCircle(3, false);
                btn.text.text = "R";
            }
            else if (isChordTone)
            {
                btn.SetActiveCircle(1, false);
                btn.text.text = IntervalNames.ContainsKey(interval) ? IntervalNames[interval] : interval.ToString();
            }
            else
            {
                btn.SetActiveCircle(0, false);
                btn.text.text = "";
            }
        }
    }

    private void LoadAndSave()
    {
        string path = Application.persistentDataPath + "/player.fun";

        if (File.Exists(path))
        {
            savedData loaded = SaveSystem.Loaddata();
            if (loaded != null) modifiedData = loaded;
        }

        if (chordsGameData == null) return;

        int current = chordsGameData.score;

        switch (chordsGameData.currentChordLevel)
        {
            case ChordLevel.level1:
                chordsGameData.highScore = modifiedData.l1_chords_highscore;
                if (current > modifiedData.l1_chords_highscore)
                {
                    modifiedData.l1_chords_highscore = current;
                    chordsGameData.highScore = current;
                }
                break;
            case ChordLevel.level2:
                chordsGameData.highScore = modifiedData.l2_chords_highscore;
                if (current > modifiedData.l2_chords_highscore)
                {
                    modifiedData.l2_chords_highscore = current;
                    chordsGameData.highScore = current;
                }
                break;
            case ChordLevel.level3:
                chordsGameData.highScore = modifiedData.l3_chords_highscore;
                if (current > modifiedData.l3_chords_highscore)
                {
                    modifiedData.l3_chords_highscore = current;
                    chordsGameData.highScore = current;
                }
                break;
            case ChordLevel.level4:
                chordsGameData.highScore = modifiedData.l4_chords_highscore;
                if (current > modifiedData.l4_chords_highscore)
                {
                    modifiedData.l4_chords_highscore = current;
                    chordsGameData.highScore = current;
                }
                break;
        }

        SaveSystem.SavePlayer(modifiedData);
    }
}
