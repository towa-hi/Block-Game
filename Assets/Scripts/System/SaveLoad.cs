using System.Collections;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using Sirenix.Serialization;

public class SaveLoad {
    public static BoardData myBoardData;
    public static void SaveBoard(BoardData aBoardData) {
        byte[] bytes = SerializationUtility.SerializeValue<BoardData>(aBoardData, DataFormat.JSON);
        File.WriteAllBytes(Config.PATHTOLEVELJSON + "serializationTestJson", bytes);

    }

    public static void LoadBoard() {
        if (!File.Exists(Config.PATHTOLEVELJSON + "serializationTestJson")) {
            return;
        }
        byte[] bytes = File.ReadAllBytes(Config.PATHTOLEVELJSON + "serializationTestJson");
        myBoardData = SerializationUtility.DeserializeValue<BoardData>(bytes, DataFormat.JSON);

    }
}
