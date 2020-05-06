using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

public class GM : Singleton<GM> {
    public GameObject boardManagerGameObject;
    public static BoardData boardData;
    public static BoardManager boardManager;
    public static EditManager editManager;
    public LevelData levelToLoad;

    private void Awake() {
        NewBoard(levelToLoad);
    }

    public void NewBoard(LevelData aLevelData) {
        GM.boardData = new BoardData(aLevelData);
        GM.boardManager = this.boardManagerGameObject.GetComponent<BoardManager>();
        GM.editManager = this.boardManagerGameObject.GetComponent<EditManager>();
        GM.boardManager.Init(GM.boardData);
    }
}
