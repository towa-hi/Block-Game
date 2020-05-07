using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using System.Linq;
using Sirenix.OdinInspector;

public class LevelSaveLoad {
    public static void SaveLevel(LevelSchema aLevelSchema) {
        string filename = ConvertTitleToSafeFileName(aLevelSchema.title);
        string filePath = Config.PATHTOLEVELJSON + filename + ".json";
        Debug.Log("LevelSaveLoad - saving " + filePath);
        string levelJson = JsonUtility.ToJson(aLevelSchema);
        File.WriteAllText(filePath, levelJson);
    }

    public static LevelSchema LoadLevel(string aFileName) {
        string filePath = Config.PATHTOLEVELJSON + aFileName;
        if (File.Exists(filePath)) {
            string saveString = File.ReadAllText(filePath);
            LevelSchema loadedSchema = ScriptableObject.CreateInstance<LevelSchema>();
            JsonUtility.FromJsonOverwrite(saveString, loadedSchema);
            return loadedSchema;
        } else {
            throw new System.IO.FileNotFoundException();
        }
    }

    public static string ConvertTitleToSafeFileName(string aTitle) {
        char[] invalidFileNameChars = Path.GetInvalidFileNameChars();
        return new string(aTitle.Where(ch => !invalidFileNameChars.Contains(ch)).ToArray());
    }
}
