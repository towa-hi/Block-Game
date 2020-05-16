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
    public GameObject playPanel;
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
        GM.playManager.Init();
        AddBoundaries();
    }

    public void AddBoundaries() {
        EntityData leftBoundary = new EntityData(EntityPrefabEnum.BLOCKPREFAB, new Vector2Int(1, 20), EntityTypeEnum.BLOCK, new Vector2Int(0, 0), Vector2Int.right, Constants.DEFAULTCOLOR, true, true);
        EntityData rightBoundary = new EntityData(EntityPrefabEnum.BLOCKPREFAB, new Vector2Int(1, 20), EntityTypeEnum.BLOCK, new Vector2Int(39, 0), Vector2Int.right, Constants.DEFAULTCOLOR, true, true);
        EntityData downBoundary = new EntityData(EntityPrefabEnum.BLOCKPREFAB, new Vector2Int(38, 1), EntityTypeEnum.BLOCK, new Vector2Int(1, 0), Vector2Int.right, Constants.DEFAULTCOLOR, true, true);
        EntityData upBoundary = new EntityData(EntityPrefabEnum.BLOCKPREFAB, new Vector2Int(38, 1), EntityTypeEnum.BLOCK, new Vector2Int(1, 19), Vector2Int.right, Constants.DEFAULTCOLOR, true, true);
        EntityData player = new EntityData(EntityPrefabEnum.PLAYERPREFAB, new Vector2Int(2, 3), EntityTypeEnum.PLAYER, new Vector2Int(5, 1), Vector2Int.right, Color.yellow);
        GM.boardManager.CreateEntityFromData(leftBoundary);
        GM.boardManager.CreateEntityFromData(rightBoundary);
        GM.boardManager.CreateEntityFromData(upBoundary);
        GM.boardManager.CreateEntityFromData(downBoundary);
        GM.boardManager.CreateEntityFromData(player);
    }

    public void LoadBoard(BoardData aBoardData) {
        GM.boardData = aBoardData;
        GM.boardManager.Init();
        GM.editManager.Init();
    }

    public void SetGameModeWithInt(int gameModeEnumInt) {
        SetGameMode((GameModeEnum)gameModeEnumInt);
    }
    
    public void SetGameMode(GameModeEnum aGameMode) {
        if (this.gameMode == GameModeEnum.PLAYTESTING) {
            LoadBoard(SaveLoad.LoadBoard("PlaytestTemp.board"));
        }
        this.gameMode = aGameMode;
        switch (this.gameMode) {
            case GameModeEnum.EDITING:
                Time.timeScale = 0;
                this.editPanel.SetActive(true);
                this.playPanel.SetActive(false);
                GM.editManager.enabled = true;
                GM.playManager.enabled = false;
                break;
            case GameModeEnum.PLAYING:
                Time.timeScale = 1;
                this.editPanel.SetActive(false);
                this.playPanel.SetActive(true);
                GM.playManager.enabled = true;
                GM.editManager.enabled = false;
                GM.playManager.SetPlaytest(false);
                break;
            case GameModeEnum.PLAYTESTING:
                Time.timeScale = 1;
                this.editPanel.SetActive(false);
                this.playPanel.SetActive(true);
                GM.playManager.enabled = true;
                GM.editManager.enabled = false;
                GM.playManager.SetPlaytest(true);
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
        this.playPanel.SetActive(!this.isFullPaused);
        if (this.isFullPaused) {
            Time.timeScale = 0;
        } else {
            Time.timeScale = 1;
        }
    }
}
