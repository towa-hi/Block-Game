using System;
using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;
using Sirenix.OdinInspector;
using Sirenix.Utilities;

public delegate void OnUpdatePlayStateHandler(PlayState aPlayState);

[RequireComponent(typeof(BoardManager))]
public class PlayManager : SerializedMonoBehaviour {

    [SerializeField] HashSet<int> entityIdsToKillThisFrame;
    [SerializeField] PlayState playState;
    public PlayState currentState {
        get {
            return this.playState;
        }
    }
    public event OnUpdatePlayStateHandler OnUpdatePlayState;
    [SerializeField] StateMachine inputStateMachine = new StateMachine();
    
    #region Lifecycle

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
        this.inputStateMachine.Update();
        switch (this.currentState.playMode) {
            case PlayModeEnum.INITIALIZATION:
                break;
            case PlayModeEnum.DIALOGUE:
                break;
            case PlayModeEnum.PLAYING:
                DoAllEntityFrames();
                break;
            case PlayModeEnum.LOST:
                break;
            case PlayModeEnum.WON:
                break;
            case PlayModeEnum.MENU:
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

    void Init() {
        print("PlayManager - Init");
        this.inputStateMachine.ChangeState(new PlayingState());
        InitializePlayState();
    }
    
    #endregion
    
    #region Listeners

    public void OnUpdateGameState(GameState aGameState) {
        switch (aGameState.gameMode) {
            case GameModeEnum.PLAYING:
                GM.instance.playPanel.SetActive(true);
                Init();
                break;
            case GameModeEnum.EDITING:
                GM.instance.playPanel.SetActive(false);
                break;
            case GameModeEnum.PLAYTESTING:
                GM.instance.playPanel.SetActive(true);
                Init();
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    #endregion

    #region PlayState
    
    void UpdatePlayState(PlayState aPlayState) {
        this.playState = aPlayState;
        switch (GM.instance.currentState.gameMode) {
            case GameModeEnum.PLAYING:
                if (Config.PRINTLISTENERUPDATES) {
                    print("PlayManager - Updating PlayState for " + this.OnUpdatePlayState?.GetInvocationList().Length + " delegates");
                }
                this.OnUpdatePlayState?.Invoke(this.currentState);
                break;
            case GameModeEnum.EDITING:
                break;
            case GameModeEnum.PLAYTESTING:
                if (Config.PRINTLISTENERUPDATES) {
                    print("PlayManager - Updating PlayState for " + this.OnUpdatePlayState?.GetInvocationList().Length + " delegates");
                }
                this.OnUpdatePlayState?.Invoke(this.currentState);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    void InitializePlayState() {
        PlayState initialPlayState = PlayState.CreatePlayState();
        UpdatePlayState(initialPlayState);
    }
    
    public void SetHeldEntity(int? aHeldEntityId) {
        PlayState newPlayState = PlayState.SetHeldEntity(this.currentState, aHeldEntityId);
        UpdatePlayState(newPlayState);
    }

    public void SetSelectedEntityIdSet([CanBeNull] HashSet<int> aSelectedEntityIdSet) {
        PlayState newPlayState = PlayState.SetSelectedEntityIdSet(this.currentState, aSelectedEntityIdSet);
        UpdatePlayState(newPlayState);
    }

    public void SetPlayMode(PlayModeEnum aPlayMode) {
        PlayState newPlayState = PlayState.SetPlayMode(this.currentState, aPlayMode);
        UpdatePlayState(newPlayState);
    }

    public void SetTimeMode(TimeModeEnum aTimeMode) {
        PlayState newPlayState = PlayState.SetTimeMode(this.currentState, aTimeMode);
        UpdatePlayState(newPlayState);
    }

    public void IncrementMoves() {
        PlayState newPlayState = PlayState.SetMoves(this.currentState, this.currentState.moves + 1);
        UpdatePlayState(newPlayState);
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

    #region StateMachineStates

    class PlayingState : StateMachineState {

        public void Enter() {
            
        }

        public void Update() {
            
        }

        public void Exit() {
            
        }
    }

    #endregion
}
