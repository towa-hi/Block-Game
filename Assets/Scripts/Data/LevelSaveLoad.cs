using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Sirenix.OdinInspector;

public class LevelSaveLoad {
    public static void SaveLevel(LevelSchema aLevelSchema) {
        Debug.Log("LevelSaveLoad - saving " + aLevelSchema.title + ".json ...");
        string filePath = Application.dataPath + "/LevelJSON/" + aLevelSchema.title + ".json";
        Debug.Log("Destination: " + filePath);
        string levelJson = JsonUtility.ToJson(aLevelSchema);
        File.WriteAllText(filePath, levelJson);
    }
}
