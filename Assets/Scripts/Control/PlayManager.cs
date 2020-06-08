using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using Sirenix.Utilities;

[RequireComponent(typeof(BoardManager))]
public class PlayManager : SerializedMonoBehaviour {
    public TimeStateEnum time;
    public PlayStateEnum play;
    [SerializeField] HashSet<int> entityIdsToKillThisFrame;
    #region Lifecycle

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
    
    #endregion
    
    #region Listeners

    public void OnUpdateGameState(GameState aGameState) {
        switch (aGameState.gameMode) {
            case GameModeEnum.PLAYING:
                GM.instance.playPanel.gameObject.SetActive(true);
                break;
            case GameModeEnum.EDITING:
                GM.instance.playPanel.gameObject.SetActive(false);
                break;
            case GameModeEnum.PLAYTESTING:
                GM.instance.playPanel.gameObject.SetActive(true);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    #endregion

    #region Entity

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

    #endregion
    
    #region External

    public void OnBackToEditorButtonClick() {
        GM.boardManager.LoadBoardStateFromFile();
        GM.instance.SetGameMode(GameModeEnum.EDITING);
    }

    #endregion
    
    #region Utility

    public static bool DoesFloorExist(Vector2Int aPos, int aId) {
        EntityState entityState = GM.boardManager.GetEntityById(aId);
        Vector2Int floorOrigin = aPos + Vector2Int.down;
        Vector2Int floorSize = new Vector2Int(entityState.data.size.x, 1);
        if (!GM.boardManager.IsRectInBoard(floorOrigin, floorSize)) {
            return false;
        }
        HashSet<BoardCell> floorSlice = GM.boardManager.GetBoardGridSlice(floorOrigin, floorSize).Values.ToHashSet();
        foreach (BoardCell floorCell in floorSlice) {
            if (floorCell.frontEntityState.HasValue) {
                EntityState floorEntity = floorCell.frontEntityState.Value;
                // if floorEntity is me, skip
                if (floorEntity.data.id == entityState.data.id) {
                    continue;
                }
                if (EntityFallOnEntityResult(entityState.data.id, floorEntity.data.id) == FightResultEnum.TIE) {
                    return true;
                }
            }
        }
        return false;
    }
    
    public static FightResultEnum DoesAttackerWinTouchFight(int aAttackerId, int aDefenderId) {
        EntityState attacker = GM.boardManager.GetEntityById(aAttackerId);
        EntityState defender = GM.boardManager.GetEntityById(aDefenderId);
        // if attacker and defender are on the same team
        if (attacker.team == defender.team) {
            return FightResultEnum.TIE;
        }
        // if attacker is a mob, can kill on touch, and has a higher power than the defenders defense
        if (attacker.mobData.HasValue && attacker.mobData.Value.canKillOnTouch && attacker.mobData.Value.touchPower >= defender.touchDefense) {
            return FightResultEnum.DEFENDERDIES;
        }
        // if defender is a mob, can kill on touch, and has a higher power than the attackers defense
        if (defender.mobData.HasValue && defender.mobData.Value.canKillOnTouch && defender.mobData.Value.touchPower >= attacker.touchDefense) {
            return FightResultEnum.ATTACKERDIES;
        }
        return FightResultEnum.TIE;
    }

    public static FightResultEnum DoesFallerWinFallFight(int aFallerId, int aDefenderId) {
        EntityState faller = GM.boardManager.GetEntityById(aFallerId);
        EntityState defender = GM.boardManager.GetEntityById(aDefenderId);
        // if faller is a mob and can fall
        if (faller.mobData?.canKillOnFall == true) {
            if (faller.mobData.Value.fallPower >= defender.fallDefense) {
                return FightResultEnum.DEFENDERDIES;
            }
        }
        return FightResultEnum.TIE;
    }
    
    public static FightResultEnum EntityFallOnEntityResult(int aId, int aOtherId) {
        // ReSharper disable once SwitchStatementHandlesSomeKnownEnumValuesWithDefault
        switch (DoesFallerWinFallFight(aId, aOtherId)) {
            case FightResultEnum.DEFENDERDIES:
                // other entity squished
                return FightResultEnum.DEFENDERDIES;
            case FightResultEnum.TIE:
                // cant kill with fall damage, checking if can kill by bump
                switch(DoesAttackerWinTouchFight(aId, aOtherId)) {
                    case FightResultEnum.DEFENDERDIES:
                        // other entity would be killed by touch
                        return FightResultEnum.DEFENDERDIES;
                    case FightResultEnum.ATTACKERDIES:
                        // this entity would be killed by other entity
                        return FightResultEnum.ATTACKERDIES;
                    case FightResultEnum.TIE:
                        // this entity and other entity cant fight
                        return FightResultEnum.TIE;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    #endregion

}
