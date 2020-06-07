using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

[RequireComponent(typeof(BoardManager))]
public class PlayManager : SerializedMonoBehaviour {
    public GUIPlayPanel playPanel;
    public TimeStateEnum time;
    public PlayStateEnum play;
    [SerializeField] HashSet<int> entityIdsToKillThisFrame;

    void OnEnable() {
        this.play = PlayStateEnum.PLAYING;
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
        switch (aGameState.gameMode) {
            case GameModeEnum.PLAYING:
                this.playPanel.gameObject.SetActive(true);
                break;
            case GameModeEnum.EDITING:
                this.playPanel.gameObject.SetActive(false);
                break;
            case GameModeEnum.PLAYTESTING:
                this.playPanel.gameObject.SetActive(true);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
    
    void DoAllEntityFrames() {
        this.entityIdsToKillThisFrame = new HashSet<int>();
        foreach (EntityBase currentEntity in GM.boardManager.entityBaseDict.Values) {
            currentEntity.DoFrame();
        }
        
        foreach (int id in this.entityIdsToKillThisFrame) {
            Debug.Assert(!GM.boardManager.currentState.entityDict.ContainsKey(id));
            Debug.Assert(GM.boardManager.entityBaseDict.ContainsKey(id));
            GM.boardManager.RemoveEntityBase(id);
            print(id + " DoAllEntityFrames - entityBase removed from game");
        }
    }
    
    // called when DyingState starts
    public void StartEntityForDeath(int aId) {
        GM.boardManager.RemoveEntity(aId);
        print(aId + " StartEntityForDeath - removed from board");
    }
    
    // called when DyingState finishes
    public void FinishEntityDeath(int aId) {
        this.entityIdsToKillThisFrame.Add(aId);
        print(aId + " FinishEntityDeath - entity marked for removal next frame");
    }
    
    public void OnBackToEditorButtonClick() {
        GM.boardManager.LoadBoardStateFromFile();
        GM.instance.SetGameMode(GameModeEnum.EDITING);
    }
}
