using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using UnityEngine.UI;

public class GUIPlayPanel : SerializedMonoBehaviour {
    public Text titleText;
    public GameObject playtestPanel;
    public Text movesText;
    public Text parText;
    
    void OnEnable() {
        print("GUIPlayPanel enabled");
        GM.boardManager.OnUpdateBoardState += OnUpdateBoardState;
        GM.playManager.OnUpdatePlayState += OnUpdatePlayState;
        SetPlayStateGUI(GM.playManager.currentState);
        SetBoardStateGUI(GM.boardManager.currentState);
    }

    void OnDisable() {
        GM.boardManager.OnUpdateBoardState -= OnUpdateBoardState;
        GM.playManager.OnUpdatePlayState -= OnUpdatePlayState;
    }

    void OnUpdateBoardState(BoardState aBoardState) {
        SetBoardStateGUI(aBoardState);
    }

    void OnUpdatePlayState(PlayState aPlayState) {
        SetPlayStateGUI(aPlayState);
    }

    void SetPlayStateGUI(PlayState aPlayState) {
        this.movesText.text = "Moves: " + aPlayState.moves;
    }

    void SetBoardStateGUI(BoardState aBoardState) {
        this.titleText.text = aBoardState.title;
        this.parText.text = "Par: " + aBoardState.par;
    }
}
