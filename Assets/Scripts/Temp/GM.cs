﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

public delegate void OnUpdateGameStateHandler(GameState aGameState);

public class GM : SerializedMonoBehaviour {

    public static InputManager inputManager;
    public static BoardManager boardManager;
    public static EditManager editManager;
    public static PlayManager playManager;
    
    [SerializeField]
    GameState currentState;
    public GameState gameState {
        get {
            return this.currentState;
        }
    }
    public event OnUpdateGameStateHandler OnUpdateGameState;
    public GameObject coreGameObject;

    void Awake() {
        this.OnUpdateGameState = null;
        GM.inputManager = this.coreGameObject.GetComponent<InputManager>();
        GM.boardManager = this.coreGameObject.GetComponent<BoardManager>();
        GM.editManager = this.coreGameObject.GetComponent<EditManager>();
        GM.playManager = this.coreGameObject.GetComponent<PlayManager>();
        this.OnUpdateGameState += GM.boardManager.OnUpdateGameState;
        this.OnUpdateGameState += GM.inputManager.OnUpdateGameState;
        this.OnUpdateGameState += GM.editManager.OnUpdateGameState;
        this.OnUpdateGameState += GM.playManager.OnUpdateGameState;
        UpdateGameState(GameState.CreateGameState(GameModeEnum.EDITING));
    }

    public void UpdateGameState(GameState aGameState) {
        print("GM - Updating GameState for " + this.OnUpdateGameState.GetInvocationList().Length + " delegates");
        this.currentState = aGameState;
        this.OnUpdateGameState?.Invoke(this.gameState);
    }

    public static GameObject LoadEntityPrefabByFilename(string aFilename) {
        return Resources.Load("EntityPrefabs/" + aFilename) as GameObject;
    }

    public static GameObject LoadBgPrefabByFilename(string aFilename) {
        return Resources.Load("BgPrefabs/" + aFilename) as GameObject;
    }
}
