using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

public class GM : Singleton<GM> {
    public GameObject boardManagerGameObject;
    public static BoardData boardData;
    public static InputManager inputManager;
    public static BoardManager boardManager;
    public static EditManager editManager;
    public static GridViewBase gridViewBase;

    public GameObject editPanel;
    public GameObject pausePanel;
    public GameObject blockPrefabMaster;
    public GameObject playerPrefabMaster;
    public GameObject shufflebotPrefabMaster;

    public bool isFullPaused;

    private void Awake() {
        NewBoard();
    }

    public void NewBoard() {
        GM.boardData = new BoardData();
        GM.boardManager = this.boardManagerGameObject.GetComponent<BoardManager>();
        GM.editManager = this.boardManagerGameObject.GetComponent<EditManager>();
        GM.inputManager = this.boardManagerGameObject.GetComponent<InputManager>();
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

    public void ToggleFullPauseGame(bool aIsFullPaused) {
        this.isFullPaused = aIsFullPaused;
        this.pausePanel.SetActive(this.isFullPaused);
        AudioListener.pause = this.isFullPaused;
        this.editPanel.SetActive(!this.isFullPaused);
        if (this.isFullPaused) {
            Time.timeScale = 0;
        } else {
            Time.timeScale = 1;
        }
    }
}
