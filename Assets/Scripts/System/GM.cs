using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

public class GM : Singleton<GM> {
    public GameObject boardManagerGameObject;
    public static BoardData boardData;
    public static LevelSchema levelSchemaTest;
    public static BoardManager boardManager;
    public static EditManager editManager;
    public LevelData levelToLoad;

    private void Awake() {
        NewBoard(levelToLoad);
    }

    public void NewBoard(LevelData aLevelData) {
        GM.boardData = ScriptableObject.CreateInstance("BoardData") as BoardData;
        GM.boardData.Init(aLevelData);
        GM.boardManager = this.boardManagerGameObject.GetComponent<BoardManager>();
        GM.editManager = this.boardManagerGameObject.GetComponent<EditManager>();
        GM.boardManager.Init();
    }

    public void LoadLevelSaveData(LevelSaveData aLevelSaveData) {

    }
}
