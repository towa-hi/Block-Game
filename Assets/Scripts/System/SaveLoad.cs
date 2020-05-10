using System.Collections;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Sirenix.OdinInspector;
using Sirenix.Serialization;

public class SaveLoad {
    // public static string saveFilename = "serializationTestBinary1";
    public static BoardData mySaveData;
    public static string testFilename = "testLevelNew";

    public static void SaveBoard(BoardData aBoardData) {
        // LevelSaveData newSave = new LevelSaveData();
        string saveFilename = ConvertTitleToSafeFileName(aBoardData.title);
        byte[] bytes = SerializationUtility.SerializeValue<BoardData>(aBoardData, DataFormat.Binary);
        // File.WriteAllBytes(Config.PATHTOLEVELJSON + saveFilename, bytes);
        File.WriteAllBytes(Config.PATHTOLEVELJSON + testFilename, bytes);
    }

    public static void LoadBoard() {
        if (!File.Exists(Config.PATHTOLEVELJSON + testFilename)) {
            return;
        }
        byte[] bytes = File.ReadAllBytes(Config.PATHTOLEVELJSON + testFilename);
        mySaveData = SerializationUtility.DeserializeValue<BoardData>(bytes, DataFormat.Binary);
        
        GM.I.LoadBoard(mySaveData);
    }

    public static string ConvertTitleToSafeFileName(string aTitle) {
        char[] invalidFileNameChars = Path.GetInvalidFileNameChars();
        return new string(aTitle.Where(ch => !invalidFileNameChars.Contains(ch)).ToArray());
    }
}
