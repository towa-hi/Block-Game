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

    public static void SaveBoard(BoardData aBoardData, bool aIsPlaytestTemp = false) {
        string saveFilename;
        if (aIsPlaytestTemp) {
            saveFilename = "PlaytestTemp.board";
        } else {
            saveFilename = ConvertTitleToSafeFileName(aBoardData.title) + ".board";
        }
        Debug.Log("SaveLoad - attempting to save " + saveFilename);
        byte[] bytes = SerializationUtility.SerializeValue<BoardData>(aBoardData, DataFormat.Binary);
        File.WriteAllBytes(Config.PATHTOBOARDS + saveFilename, bytes);
    }

    public static BoardData LoadBoard(string aFilename) {
        if (!File.Exists(Config.PATHTOBOARDS + aFilename)) {
            Debug.Log("SaveLoad - .board file with name " + aFilename + " not found!");
            return null;
        }
        byte[] bytes = File.ReadAllBytes(Config.PATHTOBOARDS + aFilename);
        mySaveData = SerializationUtility.DeserializeValue<BoardData>(bytes, DataFormat.Binary);
        Debug.Log("SaveLoad - now loading " + aFilename);
        return mySaveData;
    }

    public static string ConvertTitleToSafeFileName(string aTitle) {
        char[] invalidFileNameChars = Path.GetInvalidFileNameChars();
        return new string(aTitle.Where(ch => !invalidFileNameChars.Contains(ch)).ToArray());
    }

    public static bool IsValidSave(BoardData aBoardData) {
        // TODO: make a validation function for save files
        return
            aBoardData.gameGrid != null &&
            aBoardData.entityDataSet != null &&
            aBoardData.playerEntityData != null &&
            aBoardData.title != null &&
            aBoardData.title.Length > 0 &&
            aBoardData.creator != null &&
            aBoardData.creator.Length > 0 &&
            aBoardData.par > 0 &&
            aBoardData.par <= Constants.MAXPAR &&
            aBoardData.size == aBoardData.gameGrid.size;
    }
}
