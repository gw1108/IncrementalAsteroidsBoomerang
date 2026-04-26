#if !(UNITY_STANDALONE_WIN || UNITY_STANDALONE_LINUX || UNITY_STANDALONE_OSX || STEAMWORKS_WIN || STEAMWORKS_LIN_OSX)
#define DISABLESTEAMWORKS
#endif

using System.IO;
using UnityEngine;

#if !DISABLESTEAMWORKS
using Steamworks;
#endif

[System.Serializable]
public class RunData
{
    // TODO save data for your save file.
    //public int day;
}

public class SaveSystem
{
    private const string c_saveFilePath = "GoodLaundryGreatLaundry.txt";

    private string _saveFilePath;

    public static string GetSaveFilePath()
    {
        string path = Application.persistentDataPath;

#if PLATFORM_WEBGL
        path = "idbfs/GoodLaundryGreatLaundry";
#endif

#if !DISABLESTEAMWORKS
            string steamPath;
            try
            {
                steamPath = SteamUser.GetSteamID().ToString(); // {64BitSteamID}
                path += "/" + steamPath;
            }
            catch
            {
                // Do nothing. Error usually caused by windows standalone not run from steam.
            }
#endif

        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }

        return path;
    }

    public SaveSystem()
    {
        string path = GetSaveFilePath();
        Debug.Log("Saving player save/load to : " + path + " filename: player_data.json");
        _saveFilePath = Path.Combine(path, "player_data.json");
    }

    /// <summary>
    /// Called when starting a dungeon level.
    /// </summary>
    public void SaveGame()
    {
        RunData data = new RunData();
        string json = JsonUtility.ToJson(data);
        File.WriteAllText(_saveFilePath, json);
    }

    /// <summary>
    /// HACK ALERT !!!
    /// This tries to recreate the state of the dungeon at a specific level with player items 're-added' back.
    /// This means we are limited to what we can and cannot do mid-rounds.
    /// Anything an item does will always be re-done when added back, so rely on Purchase trigger and not much else.
    /// </summary>
    public void LoadGame()
    {
        if (File.Exists(_saveFilePath))
        {
            string json = File.ReadAllText(_saveFilePath);
            RunData data = JsonUtility.FromJson<RunData>(json);
        }
        else
        {
            Debug.LogWarning("No save file found!");
        }
    }

    public void WipeRun()
    {
        File.Delete(_saveFilePath);
    }

    public bool HasSave()
    {
        return File.Exists(_saveFilePath);
    }
}