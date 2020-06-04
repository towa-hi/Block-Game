using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

[RequireComponent(typeof(BoardManager))]
public class PlayManager : SerializedMonoBehaviour {
    public TimeStateEnum time;
    public PlayStateEnum play;
    
    void OnEnable() {
        this.play = PlayStateEnum.INITIALIZATION;
    }

    void Update() {
        switch (GM.instance.currentState.gameMode) {
            case GameModeEnum.PLAYING:
                PlayingUpdate();
                break;
            case GameModeEnum.EDITING:
                break;
            case GameModeEnum.PLAYTESTING:
                PlayingUpdate();
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    void PlayingUpdate() {
        switch (this.play) {
            case PlayStateEnum.INITIALIZATION:
                break;
            case PlayStateEnum.DIALOGUE:
                break;
            case PlayStateEnum.PLAYING:
                DoAllEntityFrames();
                break;
            case PlayStateEnum.LOST:
                break;
            case PlayStateEnum.WON:
                break;
            case PlayStateEnum.MENU:
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
    
    public void OnUpdateGameState(GameState aGameState) {

    }
    
    void DoAllEntityFrames() {
        foreach (KeyValuePair<int, EntityBase> kvp in GM.boardManager.entityBaseDict) {
            EntityBase currentEntity = kvp.Value;
            currentEntity.DoFrame();
        }
    }
    
}
