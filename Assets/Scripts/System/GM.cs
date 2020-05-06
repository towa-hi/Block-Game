using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

public class GM : Singleton<GM> {
    public GameObject boardManagerGameObject;
    public static BoardData boardData;
    public static BoardManager2 boardManager;
    public static EditManager2 editManager;
    public LevelData levelToLoad;

    private void Awake() {
        NewBoard(levelToLoad);
    }

    public void NewBoard(LevelData aLevelData) {
        GM.boardData = new BoardData(aLevelData);
        GM.boardManager = this.boardManagerGameObject.GetComponent<BoardManager2>();
        GM.editManager = this.boardManagerGameObject.GetComponent<EditManager2>();
        GM.boardManager.Init(GM.boardData);
    }
}
