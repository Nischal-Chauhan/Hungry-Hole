using System;
using System.IO;
using UnityEngine;

namespace HungryHole.Core
{
    /// <summary>
    /// Static local persistence (M8 Stage 1 — Backend Schema section 6): one
    /// small versioned JSON file under Application.persistentDataPath, read and
    /// written only on demand. Missing, malformed, future-versioned or invalid
    /// data falls back to safe defaults with a developer warning — malformed
    /// save data can never crash gameplay. Saves are written atomically
    /// (temporary file, then replace) so a crash mid-write cannot truncate the
    /// real save. No network access, no BinaryFormatter, no third-party packages.
    /// </summary>
    public static class SaveSystem
    {
        private const string SaveFileName = "HungryHoleSave.json";
        private const string TempSuffix = ".tmp";

        /// <summary>
        /// Loads local progress. Any missing/corrupt/unsupported save yields
        /// safe defaults; this never throws into gameplay.
        /// </summary>
        public static SaveData LoadProgress()
        {
            try
            {
                string path = GetSavePath();
                if (!File.Exists(path))
                {
                    return CreateDefaults();
                }

                SaveData data = JsonUtility.FromJson<SaveData>(File.ReadAllText(path));
                if (data == null)
                {
                    Debug.LogWarning("SaveSystem: save file parsed to no data; using defaults.");
                    return CreateDefaults();
                }

                if (data.schemaVersion != SaveData.CurrentSchemaVersion)
                {
                    Debug.LogWarning(
                        "SaveSystem: unsupported save schema version " + data.schemaVersion
                        + "; using defaults.");
                    return CreateDefaults();
                }

                if (data.bestScore < 0)
                {
                    Debug.LogWarning("SaveSystem: invalid best score in save; using defaults.");
                    return CreateDefaults();
                }

                return data;
            }
            catch (Exception exception)
            {
                Debug.LogWarning("SaveSystem: unreadable save file; using defaults. " + exception.Message);
                return CreateDefaults();
            }
        }

        /// <summary>
        /// Writes local progress atomically. Returns false (with a warning) on
        /// any failure; the caller decides whether the failure matters.
        /// </summary>
        public static bool SaveProgress(SaveData data)
        {
            if (data == null)
            {
                Debug.LogWarning("SaveSystem: nothing to save.");
                return false;
            }

            if (data.bestScore < 0)
            {
                Debug.LogWarning("SaveSystem: refusing to save a negative best score.");
                return false;
            }

            data.schemaVersion = SaveData.CurrentSchemaVersion;

            try
            {
                string path = GetSavePath();
                string tempPath = path + TempSuffix;
                File.WriteAllText(tempPath, JsonUtility.ToJson(data));

                if (File.Exists(path))
                {
                    File.Delete(path);
                }

                File.Move(tempPath, path);
                return true;
            }
            catch (Exception exception)
            {
                Debug.LogWarning("SaveSystem: failed to write save file. " + exception.Message);
                return false;
            }
        }

        private static SaveData CreateDefaults()
        {
            return new SaveData { schemaVersion = SaveData.CurrentSchemaVersion, bestScore = 0 };
        }

        private static string GetSavePath()
        {
            return Path.Combine(Application.persistentDataPath, SaveFileName);
        }
    }
}