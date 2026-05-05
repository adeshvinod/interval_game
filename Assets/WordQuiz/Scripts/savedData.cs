using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class savedData
{
    public int HighScore = 0;

    public float[] ReactionTimes;
    public float[] Acurracies;

    public int l1_intervals_highscore = 0;
    public int l2_intervals_highscore = 0;
    public int l3_intervals_highscore = 0;
    public int l4_intervals_highscore = 0;

    public int l1_notes_highscore = 0;
    public int l2_notes_highscore = 0;
    public int l3_notes_highscore = 0;
    public int l4_notes_highscore = 0;

    public int l1_chords_highscore = 0;
    public int l2_chords_highscore = 0;
    public int l3_chords_highscore = 0;
    public int l4_chords_highscore = 0;

    // public List<settings_arpgame.chord_Progression> savedProgressions;
    public int[,] savedProgressions_tonics;   //tonics here does not refer to the key centres but rather just the Root notes of the chords  
    public int[,] savedProgressions_chordtypes;
    public string[] savedProgression_names;
    public int totalProgressions = 0;

    public int[,] savedCustomChordTypes;
    public string[] savedCustomChord_names;
    public int totalCustomChordtypes = 0;

    public int[] transposedNotes_audio = new int[6];

    // ── Engagement system ─────────────────────────────────────────────────
    // totalXP grows monotonically, playerLevel derived from it on save.
    // unlockedLevels: length 16 (4 modes × 4 levels), index = (int)mode*4 + (level-1).
    //   0 = locked, 1 = unlocked. Defaults seed L1 of each mode unlocked.
    // unlockedCollectibles: list of GuitarDexItem.id values that have been unlocked.
    public int totalXP = 0;
    public int playerLevel = 1;
    public int[] unlockedLevels;
    public int[] unlockedCollectibles;
    // Stored as yyyyMMdd int (UTC). 0 = never claimed.
    public int lastDailyBonusYmd = 0;

    public savedData(savedData _modifieddata)
    {
        HighScore = _modifieddata.HighScore;

        l1_intervals_highscore = _modifieddata.l1_intervals_highscore;
        l2_intervals_highscore = _modifieddata.l2_intervals_highscore;
        l3_intervals_highscore = _modifieddata.l3_intervals_highscore;
        l4_intervals_highscore = _modifieddata.l4_intervals_highscore;

        l1_notes_highscore = _modifieddata.l1_notes_highscore;
        l2_notes_highscore = _modifieddata.l2_notes_highscore;
        l3_notes_highscore = _modifieddata.l3_notes_highscore;
        l4_notes_highscore = _modifieddata.l4_notes_highscore;

        l1_chords_highscore = _modifieddata.l1_chords_highscore;
        l2_chords_highscore = _modifieddata.l2_chords_highscore;
        l3_chords_highscore = _modifieddata.l3_chords_highscore;
        l4_chords_highscore = _modifieddata.l4_chords_highscore;




        savedProgressions_tonics = _modifieddata.savedProgressions_tonics;
        savedProgressions_chordtypes = _modifieddata.savedProgressions_chordtypes;
        savedProgression_names = _modifieddata.savedProgression_names;
        totalProgressions = _modifieddata.totalProgressions;

        savedCustomChordTypes=_modifieddata.savedCustomChordTypes;
        savedCustomChord_names=_modifieddata.savedCustomChord_names;
        totalCustomChordtypes = _modifieddata.totalCustomChordtypes;

        transposedNotes_audio = _modifieddata.transposedNotes_audio != null
            ? (int[])_modifieddata.transposedNotes_audio.Clone()
            : new int[6];

        totalXP = _modifieddata.totalXP;
        playerLevel = _modifieddata.playerLevel;
        unlockedLevels = _modifieddata.unlockedLevels != null
            ? (int[])_modifieddata.unlockedLevels.Clone()
            : DefaultUnlockedLevels();
        unlockedCollectibles = _modifieddata.unlockedCollectibles != null
            ? (int[])_modifieddata.unlockedCollectibles.Clone()
            : DefaultUnlockedCollectibles();
        lastDailyBonusYmd = _modifieddata.lastDailyBonusYmd;
}

    
    public savedData()
    {
        int maximum_progressions = 15;
        int maximum_chords_in_prog = 40;
         savedProgressions_tonics = new int[maximum_progressions, maximum_chords_in_prog];
    savedProgressions_chordtypes = new int[maximum_progressions, maximum_chords_in_prog];
     savedProgression_names = new string[maximum_progressions];
    Debug.Log("saved data constructor called)+ "+savedProgressions_tonics[0,0]);
        
        int maximum_custom_chords = 20;
     savedCustomChordTypes=new int[maximum_custom_chords,12];
    savedCustomChord_names=new string[maximum_custom_chords];

        for(int i=0;i<maximum_progressions;i++)
        {
            for(int j=0;j<maximum_chords_in_prog;j++)
            {
                savedProgressions_chordtypes[i,j] = 0;
                savedProgressions_tonics[i, j] = 0;
                

            }
            savedProgression_names[i] = null;
        }

        for(int i=0;i<maximum_custom_chords;i++)
        {
            for(int j=0;j<12;j++)
            {
                savedCustomChordTypes[i, j] = -1;
            }
            savedCustomChord_names[i] = null;
        }

        unlockedLevels = DefaultUnlockedLevels();
        unlockedCollectibles = DefaultUnlockedCollectibles();
}

    // L1 of each mode unlocked at fresh start. Index = (int)LearningMode * 4 + (level-1).
    // Enum order: Intervals=0, Notes=1, Progressions=2, Chords=3.
    public static int[] DefaultUnlockedLevels()
    {
        var arr = new int[16];
        arr[0]  = 1; // Intervals L1
        arr[4]  = 1; // Notes L1
        arr[8]  = 1; // Progressions L1
        arr[12] = 1; // Chords L1
        return arr;
    }

    // Items 1-6 are pre-unlocked to mirror current 6-of-14-collected state.
    public static int[] DefaultUnlockedCollectibles()
    {
        return new int[] { 1, 2, 3, 4, 5, 6 };
    }
   

    public void start()
    {

    }
}