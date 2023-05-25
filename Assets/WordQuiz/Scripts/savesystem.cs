using UnityEngine;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

public static class SaveSystem
{
    public static void SavePlayer(gameover Gameover_)
    {
        BinaryFormatter formatter = new BinaryFormatter();
        string path = Application.persistentDataPath + "/player.fun";
        FileStream stream = new FileStream(path, FileMode.Create);
        savedData data = new savedData(Gameover_);

        formatter.Serialize(stream, data);
        stream.Close();
    }

    public static savedData Loaddata()
    {
        string path = Application.persistentDataPath + "/player.fun";
        if(File.Exists(path))
        {
            BinaryFormatter formatter = new BinaryFormatter();
            FileStream stream = new FileStream(path, FileMode.Open);
            savedData data=formatter.Deserialize(stream) as savedData;
            stream.Close();
            return data;
        }
        else
        {
            Debug.LogError("saved file not found in" + path);
            return null;
        }
    }
}

