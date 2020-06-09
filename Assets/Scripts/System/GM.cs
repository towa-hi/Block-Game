using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using FilePicker;
using UnityEngine;
using Sirenix.OdinInspector;
using Sirenix.Serialization;

public delegate void OnUpdateGameStateHandler(GameState aGameState);

// ReSharper disable once InconsistentNaming
public class GM : SerializedMonoBehaviour {
    public static GM instance;
    public static InputManager inputManager;
    public static BoardManager boardManager;
    public static EditManager editManager;
    public static PlayManager playManager;
    public static Cursor cursor;
    [SerializeField] GameState gameState;
    public GameState currentState {
        get {
            return this.gameState;
        }
    }
    public event OnUpdateGameStateHandler OnUpdateGameState;
    public GameObject coreGameObject;
    public GameObject canvasGameObject;
    public GameObject studPrefab;
    GUIFilePicker filePicker;
    public GameObject playPanel;
    public GameObject editorPanel;
    public GameObject pausePanel;
    
    void Awake() {
        GM.instance = this;
        this.OnUpdateGameState = null;
        this.filePicker = this.canvasGameObject.transform.Find("FilePicker").GetComponent<GUIFilePicker>();
        this.playPanel = this.canvasGameObject.transform.Find("PlayPanel").gameObject;
        this.editorPanel = this.canvasGameObject.transform.Find("EditorPanel").gameObject;
        this.pausePanel = this.canvasGameObject.transform.Find("PausePanel").gameObject;
        GM.inputManager = this.coreGameObject.GetComponent<InputManager>();
        GM.boardManager = this.coreGameObject.GetComponent<BoardManager>();
        GM.editManager = this.coreGameObject.GetComponent<EditManager>();
        GM.playManager = this.coreGameObject.GetComponent<PlayManager>();
        GM.cursor = this.coreGameObject.GetComponentInChildren<Cursor>();
        this.OnUpdateGameState += GM.boardManager.OnUpdateGameState;
        this.OnUpdateGameState += GM.inputManager.OnUpdateGameState;
        this.OnUpdateGameState += GM.editManager.OnUpdateGameState;
        this.OnUpdateGameState += GM.playManager.OnUpdateGameState;
        this.OnUpdateGameState += GM.cursor.OnUpdateGameState;
    }

    void Start() {
        SetGameMode(GameModeEnum.EDITING);
        GM.boardManager.InitializeStartingBoard();
    }
    public void SetGameMode(GameModeEnum aGameMode) {
        GameState newGameState = GameState.SetGameMode(this.gameState, aGameMode);
        UpdateGameState(newGameState);
    }
    
    void UpdateGameState(GameState aGameState) {
        if (this.OnUpdateGameState != null) {
            print("GM - Updating GameState for " + this.OnUpdateGameState.GetInvocationList().Length + " delegates");
        }
        else {
            print("GM - Updating GameState for no delegates");
        }
        this.gameState = aGameState;
        this.OnUpdateGameState?.Invoke(this.currentState);
    }

    // TODO: handle pauses and propagate gameState
    public void SetFilePickerActive(bool aIsActive) {
        this.pausePanel.SetActive(aIsActive);
        this.filePicker.gameObject.SetActive(aIsActive);
    }
    
    public static GameObject LoadEntityPrefabByFilename(string aFilename) {
        return Resources.Load("EntityPrefabs/" + aFilename) as GameObject;
    }

    public static void SaveBoardState(BoardState aBoardState, bool aIsPlaytestTemp = false) {
        string saveFilename = aIsPlaytestTemp ? "PlayTestTemp.board" : aBoardState.title + ".board";
        Debug.Log("SaveBoard - attempting to save " + saveFilename);
        byte[] bytes = SerializationUtility.SerializeValue(aBoardState, DataFormat.Binary);
        File.WriteAllBytes(Config.PATHTOBOARDS + saveFilename, bytes);
    }

    public static BoardState LoadBoardState(string aFilename) {
        if (File.Exists(Config.PATHTOBOARDS + aFilename)) {
            byte[] bytes = File.ReadAllBytes(Config.PATHTOBOARDS + aFilename);
            BoardState loadedBoardState = SerializationUtility.DeserializeValue<BoardState>(bytes, DataFormat.Binary);
            return loadedBoardState;
        }
        else {
            throw new Exception("SaveLoad - .board file with name " + aFilename + " not found!");
        }
    }
}
