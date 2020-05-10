using System.Collections;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Sirenix.OdinInspector;
using Sirenix.Serialization;

public class SaveLoad {
    // public static string saveFilename = "serializationTestBinary1";
    public static LevelSaveData mySaveData;

    public static void SaveBoard() {
        LevelSaveData newSave = new LevelSaveData();
        string saveFilename = ConvertTitleToSafeFileName(BoardData.title);
        byte[] bytes = SerializationUtility.SerializeValue<LevelSaveData>(newSave, DataFormat.Binary);
        File.WriteAllBytes(Config.PATHTOLEVELJSON + saveFilename, bytes);

    }

    public static void LoadBoard(string saveFilename) {
        if (!File.Exists(Config.PATHTOLEVELJSON + saveFilename)) {
            return;
        }
        byte[] bytes = File.ReadAllBytes(Config.PATHTOLEVELJSON + saveFilename);
        mySaveData = SerializationUtility.DeserializeValue<LevelSaveData>(bytes, DataFormat.Binary);
        GM.LoadLevelSaveData(mySaveData);
    }

    public static string ConvertTitleToSafeFileName(string aTitle) {
        char[] invalidFileNameChars = Path.GetInvalidFileNameChars();
        return new string(aTitle.Where(ch => !invalidFileNameChars.Contains(ch)).ToArray());
    }
}
