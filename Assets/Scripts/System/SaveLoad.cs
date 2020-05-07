using System.Collections;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using Sirenix.Serialization;

public class SaveLoad {
    public static string saveFilename = "serializationTestBinary1";
    public static LevelSaveData mySaveData;

    public static void SaveBoard(BoardData aBoardData) {
        LevelSaveData newSave = new LevelSaveData();
        byte[] bytes = SerializationUtility.SerializeValue<LevelSaveData>(newSave, DataFormat.Binary);
        File.WriteAllBytes(Config.PATHTOLEVELJSON + saveFilename, bytes);

    }

    public static void LoadBoard() {
        if (!File.Exists(Config.PATHTOLEVELJSON + saveFilename)) {
            return;
        }
        byte[] bytes = File.ReadAllBytes(Config.PATHTOLEVELJSON + saveFilename);
        mySaveData = SerializationUtility.DeserializeValue<LevelSaveData>(bytes, DataFormat.Binary);

    }
}
