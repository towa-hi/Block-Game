using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

public class GM : Singleton<GM> {
    public static BoardData boardData;
    public static InputManager inputManager;
    public static BoardManager boardManager;
    public static EditManager editManager;
    public static PlayManager playManager;
    public static GridViewBase gridViewBase;
    public static CursorBase cursorBase;

    public GameModeEnum gameMode;
    [Header("Set In Editor")]
    public GameObject boardManagerGameObject;
    public GameObject editPanel;
    public GameObject pausePanel;
    public GameObject blockPrefabMaster;
    public GameObject playerPrefabMaster;
    public GameObject shufflebotPrefabMaster;

    public bool isFullPaused;

    private void Awake() {
        // set as editing by default
        this.gameMode = GameModeEnum.EDITING;
        NewBoard();
    }

    public void NewBoard() {
        GM.boardData = new BoardData();
        GM.boardManager = this.boardManagerGameObject.GetComponent<BoardManager>();
        GM.editManager = this.boardManagerGameObject.GetComponent<EditManager>();
        GM.inputManager = this.boardManagerGameObject.GetComponent<InputManager>();
        GM.playManager = this.boardManagerGameObject.GetComponent<PlayManager>();
        GM.gridViewBase = this.boardManagerGameObject.GetComponentInChildren<GridViewBase>();
        GM.cursorBase = this.boardManagerGameObject.GetComponentInChildren<CursorBase>();
        GM.boardManager.Init();
        GM.editManager.Init();
    }

    public void LoadBoard(BoardData aBoardData) {
        GM.boardData = aBoardData;
        GM.boardManager.Init();
        GM.editManager.Init();
    }

    public void SetGameMode(GameModeEnum aGameMode) {
        this.gameMode = aGameMode;
        switch (this.gameMode) {
            case GameModeEnum.EDITING:
                this.editPanel.SetActive(true);
                GM.editManager.enabled = true;
                GM.playManager.enabled = false;
                break;
            case GameModeEnum.PLAYING:
                this.editPanel.SetActive(false);
                GM.playManager.enabled = true;
                GM.editManager.enabled = false;
                break;
        }
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
