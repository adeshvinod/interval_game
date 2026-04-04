using UnityEngine;
using System.IO;

public static class SaveSystem
{
    private static string Path => Application.persistentDataPath + "/player.fun";

    // JsonUtility cannot serialize int[,] multidimensional arrays, so we use this
    // internal wrapper that flattens them to int[] for JSON, then reconstructs
    // them on load. savedData and all callers are unchanged.
    [System.Serializable]
    private class SaveDataWrapper
    {
        public int HighScore;
        public int l1_intervals_highscore;
        public int l2_intervals_highscore;
        public int l3_intervals_highscore;
        public int l4_intervals_highscore;
        public int l1_notes_highscore;
        public int l2_notes_highscore;
        public int l3_notes_highscore;
        public int l4_notes_highscore;
        public int totalProgressions;
        public int totalCustomChordtypes;

        // int[15, 40] flattened to int[600]
        public int[] savedProgressions_tonics_flat;
        public int[] savedProgressions_chordtypes_flat;
        public string[] savedProgression_names;

        // int[20, 12] flattened to int[240]
        public int[] savedCustomChordTypes_flat;
        public string[] savedCustomChord_names;

        private const int MAX_PROGRESSIONS = 15;
        private const int MAX_CHORDS = 40;
        private const int MAX_CUSTOM_CHORDS = 20;
        private const int MAX_INTERVALS = 12;

        public static SaveDataWrapper FromSavedData(savedData d)
        {
            var w = new SaveDataWrapper();

            w.HighScore = d.HighScore;
            w.l1_intervals_highscore = d.l1_intervals_highscore;
            w.l2_intervals_highscore = d.l2_intervals_highscore;
            w.l3_intervals_highscore = d.l3_intervals_highscore;
            w.l4_intervals_highscore = d.l4_intervals_highscore;
            w.l1_notes_highscore = d.l1_notes_highscore;
            w.l2_notes_highscore = d.l2_notes_highscore;
            w.l3_notes_highscore = d.l3_notes_highscore;
            w.l4_notes_highscore = d.l4_notes_highscore;
            w.totalProgressions = d.totalProgressions;
            w.totalCustomChordtypes = d.totalCustomChordtypes;

            w.savedProgressions_tonics_flat = new int[MAX_PROGRESSIONS * MAX_CHORDS];
            w.savedProgressions_chordtypes_flat = new int[MAX_PROGRESSIONS * MAX_CHORDS];
            for (int i = 0; i < MAX_PROGRESSIONS; i++)
                for (int j = 0; j < MAX_CHORDS; j++)
                {
                    w.savedProgressions_tonics_flat[i * MAX_CHORDS + j] = d.savedProgressions_tonics[i, j];
                    w.savedProgressions_chordtypes_flat[i * MAX_CHORDS + j] = d.savedProgressions_chordtypes[i, j];
                }

            w.savedProgression_names = d.savedProgression_names;

            w.savedCustomChordTypes_flat = new int[MAX_CUSTOM_CHORDS * MAX_INTERVALS];
            for (int i = 0; i < MAX_CUSTOM_CHORDS; i++)
                for (int j = 0; j < MAX_INTERVALS; j++)
                    w.savedCustomChordTypes_flat[i * MAX_INTERVALS + j] = d.savedCustomChordTypes[i, j];

            w.savedCustomChord_names = d.savedCustomChord_names;

            return w;
        }

        public savedData ToSavedData()
        {
            var d = new savedData();

            d.HighScore = HighScore;
            d.l1_intervals_highscore = l1_intervals_highscore;
            d.l2_intervals_highscore = l2_intervals_highscore;
            d.l3_intervals_highscore = l3_intervals_highscore;
            d.l4_intervals_highscore = l4_intervals_highscore;
            d.l1_notes_highscore = l1_notes_highscore;
            d.l2_notes_highscore = l2_notes_highscore;
            d.l3_notes_highscore = l3_notes_highscore;
            d.l4_notes_highscore = l4_notes_highscore;
            d.totalProgressions = totalProgressions;
            d.totalCustomChordtypes = totalCustomChordtypes;

            d.savedProgressions_tonics = new int[MAX_PROGRESSIONS, MAX_CHORDS];
            d.savedProgressions_chordtypes = new int[MAX_PROGRESSIONS, MAX_CHORDS];
            for (int i = 0; i < MAX_PROGRESSIONS; i++)
                for (int j = 0; j < MAX_CHORDS; j++)
                {
                    d.savedProgressions_tonics[i, j] = savedProgressions_tonics_flat[i * MAX_CHORDS + j];
                    d.savedProgressions_chordtypes[i, j] = savedProgressions_chordtypes_flat[i * MAX_CHORDS + j];
                }

            d.savedProgression_names = savedProgression_names;

            d.savedCustomChordTypes = new int[MAX_CUSTOM_CHORDS, MAX_INTERVALS];
            for (int i = 0; i < MAX_CUSTOM_CHORDS; i++)
                for (int j = 0; j < MAX_INTERVALS; j++)
                    d.savedCustomChordTypes[i, j] = savedCustomChordTypes_flat[i * MAX_INTERVALS + j];

            d.savedCustomChord_names = savedCustomChord_names;

            return d;
        }
    }

    public static void SavePlayer(savedData modifieddata)
    {
        SaveDataWrapper wrapper = SaveDataWrapper.FromSavedData(modifieddata);
        string json = JsonUtility.ToJson(wrapper);
        File.WriteAllText(Path, json);
    }

    public static void saveProgression()
    {
    }

    public static savedData Loaddata()
    {
        if (!File.Exists(Path))
        {
            Debug.Log("No save file found at " + Path);
            return null;
        }

        string json = File.ReadAllText(Path);
        if (string.IsNullOrWhiteSpace(json))
        {
            Debug.Log("Save file is empty, starting fresh.");
            return null;
        }

        try
        {
            SaveDataWrapper wrapper = JsonUtility.FromJson<SaveDataWrapper>(json);
            if (wrapper == null)
            {
                Debug.LogWarning("Save file could not be parsed, starting fresh.");
                return null;
            }
            return wrapper.ToSavedData();
        }
        catch (System.Exception e)
        {
            Debug.LogWarning("Save file invalid (possibly old binary format), starting fresh. Error: " + e.Message);
            return null;
        }
    }
}
