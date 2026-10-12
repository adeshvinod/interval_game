using System;
using System.IO;
using UnityEngine;

/// <summary>
/// Reads and writes <see cref="savedData"/> as JSON at
/// <c>{persistentDataPath}/player.fun</c>.
///
/// <see cref="Loaddata"/> returns null when there is no usable save; every
/// caller treats null as "fresh install" via <c>?? new savedData()</c>.
/// </summary>
public static class SaveSystem
{
    private static string Path => Application.persistentDataPath + "/player.fun";

    public static void SavePlayer(savedData data)
    {
        if (data == null) return;
        File.WriteAllText(Path, JsonUtility.ToJson(data));
    }

    /// Returns the save, or null if it is missing, empty or unreadable.
    public static savedData Loaddata()
    {
        if (!File.Exists(Path)) return null;

        try
        {
            string json = File.ReadAllText(Path);
            if (string.IsNullOrWhiteSpace(json)) return null;

            savedData data = JsonUtility.FromJson<savedData>(json);
            if (data == null) return null;

            // A save written before a field existed comes back with that field
            // null or the wrong length. Repair rather than discard the file.
            if (data.transposedNotes_audio == null ||
                data.transposedNotes_audio.Length != savedData.STRING_COUNT)
                data.transposedNotes_audio = new int[savedData.STRING_COUNT];

            if (data.unlockedLevels == null ||
                data.unlockedLevels.Length != savedData.MODE_COUNT * savedData.LEVELS_PER_MODE)
                data.unlockedLevels = savedData.DefaultUnlockedLevels();

            return data;
        }
        catch (Exception e)
        {
            Debug.LogWarning("Save file unreadable, starting fresh. " + e.Message);
            return null;
        }
    }

    /// Wipes the save so the next launch behaves like a fresh install.
    public static void DeleteSave()
    {
        if (File.Exists(Path)) File.Delete(Path);
    }
}
