using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

public class GM : Singleton<GM> {
    public GameObject boardManagerGameObject;
    public static BoardData boardData;
    public static BoardManager boardManager;
    public static EditManager editManager;
    public static GridViewBase gridViewBase;

    public GameObject blockPrefabMaster;
    public GameObject playerPrefabMaster;
    public GameObject shufflebotPrefabMaster;

    private void Awake() {
        NewBoard();
    }

    public void NewBoard() {
        GM.boardData = new BoardData();
        GM.boardManager = this.boardManagerGameObject.GetComponent<BoardManager>();
        GM.editManager = this.boardManagerGameObject.GetComponent<EditManager>();
        GM.boardManager.Init();
        GM.editManager.Init();
    }

    public void LoadBoard(BoardData aBoardData) {
        GM.boardData = aBoardData;
        GM.boardManager.Init();
        GM.editManager.Init();
    }

    public void LoadLevelSaveData(LevelSaveData aLevelSaveData) {

    }

    public static GameObject EntityPrefabEnumToPrefab(EntityPrefabEnum aEntityPrefabEnum) {
        switch (aEntityPrefabEnum) {
            case EntityPrefabEnum.BLOCKPREFAB:
                return GM.I.blockPrefabMaster;
            case EntityPrefabEnum.PLAYERPREFAB:
                return GM.I.playerPrefabMaster;
            case EntityPrefabEnum.SHUFFLEBOTPREFAB:
                return GM.I.shufflebotPrefabMaster;
        }
        throw new System.Exception("Unknown entity prefab enum");
    }
}
