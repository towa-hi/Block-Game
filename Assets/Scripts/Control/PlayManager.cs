﻿using System;
using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using UnityEngine.Rendering;

public delegate void OnUpdatePlayStateHandler(PlayState aPlayState);

[RequireComponent(typeof(BoardManager))]
public class PlayManager : SerializedMonoBehaviour {
    public Volume v;
    UnityEngine.Rendering.Universal.ColorAdjustments colorAdjustments;
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
                switch (this.currentState.timeMode) {
                    case TimeModeEnum.NORMAL:
                        DoAllEntityFrames();
                        break;
                    case TimeModeEnum.PAUSED:
                        break;
                    case TimeModeEnum.DOUBLE:
                        DoAllEntityFrames();
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
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
        // TODO: this sets timeMode and playMode to start the game
        initialPlayState = PlayState.SetTimeMode(initialPlayState, TimeModeEnum.NORMAL);
        initialPlayState = PlayState.SetPlayMode(initialPlayState, PlayModeEnum.PLAYING);
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
        UnityEngine.Rendering.VolumeProfile volumeProfile = this.v.profile;
        if(!volumeProfile) throw new System.NullReferenceException(nameof(UnityEngine.Rendering.VolumeProfile));
        if(!volumeProfile.TryGet(out this.colorAdjustments)) throw new System.NullReferenceException(nameof(this.colorAdjustments));
        switch (aTimeMode) {
            case TimeModeEnum.NORMAL:
                this.colorAdjustments.saturation.Override(0f);
                break;
            case TimeModeEnum.PAUSED:
                this.colorAdjustments.saturation.Override(-100f);
                break;
            case TimeModeEnum.DOUBLE:
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(aTimeMode), aTimeMode, null);
        }
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
        SetTimeMode(TimeModeEnum.NORMAL);
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

    public bool IsEntitySelectable(int aId, bool aIsUp) {
        EntityState entityState = GM.boardManager.GetEntityById(aId);
        if (entityState.data.isBoundary) {
            return false;
        }
        if (entityState.isFixed) {
            return false;
        }
        if (entityState.data.entityType == EntityTypeEnum.MOB) {
            return false;
        }
        if (!entityState.hasNodes) {
            return false;
        }
        // TODO: write this
        return true;
    }

    public void SelectEntity(int aId, bool aIsUp) {
        Debug.Assert(IsEntitySelectable(aId, aIsUp));
        EntityState rootEntity = GM.boardManager.GetEntityById(aId);
        Debug.Assert(rootEntity.hasNodes);
        SetSelectedEntityIdSet(GetSelectedEntityIdSet(rootEntity, aIsUp));
    }

    public HashSet<int> GetSelectedEntityIdSet(EntityState aRoot, bool aIsUp) {
        HashSet<EntityState> selectSet = new HashSet<EntityState>();
        HashSet<EntityState> mainTree = GetConnectedTree(aRoot, aIsUp);
        HashSet<EntityState> connectedTree = new HashSet<EntityState>();
        print(mainTree.Count);







        selectSet.UnionWith(mainTree);
        
        HashSet<int> selectIdSet = new HashSet<int>();
        foreach (EntityState selectedEntity in mainTree) {
            foreach (Vector2Int pos in Util.V2IInRect(selectedEntity.pos, selectedEntity.data.size)) {
                GM.boardManager.SetMarker(pos, Color.magenta, 2f);
            }
            selectIdSet.Add(selectedEntity.data.id);
        }
        return selectIdSet;
    }

    // ReSharper disable once ReturnTypeCanBeEnumerable.Local
    HashSet<EntityState> GetConnectedTree(EntityState aRoot, bool aIsUp, HashSet<EntityState> aConnectedTreeSet = null) {
        Debug.Assert(aRoot.hasNodes);
        MarkEntity(aRoot);
        if (aConnectedTreeSet == null) {
            aConnectedTreeSet = new HashSet<EntityState> {aRoot};
        }
        else {
            aConnectedTreeSet.Add(aRoot);
        }
        // HashSet<EntityState> connectedToRoot
        HashSet<EntityState> connectedToRoot = GetConnected(aRoot, aIsUp);
        foreach (EntityState connectedEntity in connectedToRoot) {
            if (!aConnectedTreeSet.Contains(connectedEntity) && IsEntitySelectable(connectedEntity.data.id, aIsUp)) {
                GetConnectedTree(connectedEntity, aIsUp, aConnectedTreeSet);
            }
        }
        return aConnectedTreeSet;
    }

    HashSet<EntityState> GetConnected(EntityState aRoot, bool aIsUp, HashSet<EntityState> aIgnoreSet = null) {
        Debug.Assert(aRoot.hasNodes);
        HashSet<EntityState> connectedEntitySet = new HashSet<EntityState>();
        // get the absolute pos of where to check above/below root
        foreach (Vector2Int absoluteNodePos in aRoot.GetAbsoluteNodePosSet(aIsUp)) {
            EntityState? maybeReciprocalEntity = GM.boardManager.GetReciprocalEntity(absoluteNodePos, aIsUp);
            if (maybeReciprocalEntity.HasValue) {
                if (aIgnoreSet != null) {
                    if (!aIgnoreSet.Contains(maybeReciprocalEntity.Value)) {
                        connectedEntitySet.Add(maybeReciprocalEntity.Value);
                        
                    }
                }
                else {
                    connectedEntitySet.Add(maybeReciprocalEntity.Value);
                }
            }
        }
        return connectedEntitySet;
    }

    (HashSet<EntityState>, HashSet<EntityState>) GetConnectedBothSides(EntityState aRoot, HashSet<EntityState> aIgnoreSet = null) {
        HashSet<EntityState> upSet = GetConnected(aRoot, true, aIgnoreSet);
        HashSet<EntityState> downSet = GetConnected(aRoot, false, aIgnoreSet);
        return (upSet, downSet);
    }

    void MarkEntity(EntityState aEntityState) {
        foreach (Vector2Int pos in Util.V2IInRect(aEntityState.pos, aEntityState.data.size)) {
            GM.boardManager.SetMarker(pos, Color.magenta, 2f);
        }
    }
    
    public bool CanPlaceSelection(Vector2Int aOffset) {
        Debug.Assert(this.currentState.selectedEntityIdSet != null);
        HashSet<EntityState> ignoreEntityStateSet = GM.boardManager.ConvertIdSetToEntityStateSet(this.currentState.selectedEntityIdSet);
        foreach (int id in this.currentState.selectedEntityIdSet) {
            if (!CanPlaceEntity(id, aOffset, ignoreEntityStateSet)) {
                return false;
            }
        }
        return true;
    }

    public bool CanPlaceEntity(int aId, Vector2Int aOffset, HashSet<EntityState> aEntityIdIgnoreSet = null) {
        // TODO: make this think about studs
        EntityState entityState = GM.boardManager.GetEntityById(aId);
        if (aEntityIdIgnoreSet == null) {
            return GM.boardManager.IsRectEmpty(entityState.pos + aOffset, entityState.data.size, new HashSet<EntityState> {entityState});
        }
        else {
            return GM.boardManager.IsRectEmpty(entityState.pos + aOffset, entityState.data.size, aEntityIdIgnoreSet);
        }
    }

    
    #endregion

    #region StateMachineStates

    class PlayingState : StateMachineState {
        int? clickedEntityId;
        public void Enter() {
            this.clickedEntityId = null;
        }

        public void Update() {
            switch (GM.inputManager.mouseState) {
                case MouseStateEnum.DEFAULT:
                    break;
                case MouseStateEnum.CLICKED:
                    EntityState? maybeAEntity = GM.boardManager.GetEntityAtMousePos();
                    if (maybeAEntity.HasValue) {
                        this.clickedEntityId = maybeAEntity.Value.data.id;
                    }
                    break;
                case MouseStateEnum.HELD:
                    if (this.clickedEntityId.HasValue) {
                        if (GM.inputManager.dragOffset.y > Constants.DRAGTHRESHOLD) {
                            if (GM.playManager.IsEntitySelectable(this.clickedEntityId.Value, true)) {
                                GM.playManager.SelectEntity(this.clickedEntityId.Value, true);
                                this.clickedEntityId = null;
                                GM.playManager.SetTimeMode(TimeModeEnum.PAUSED);
                            }
                        } else if (GM.inputManager.dragOffset.y < Constants.DRAGTHRESHOLD * -1) {
                            if (GM.playManager.IsEntitySelectable(this.clickedEntityId.Value, false)) {
                                GM.playManager.SelectEntity(this.clickedEntityId.Value, false);
                                this.clickedEntityId = null;
                                GM.playManager.SetTimeMode(TimeModeEnum.PAUSED);
                            }
                        }
                    }
                    if (GM.playManager.currentState.selectedEntityIdSet != null) {
                        foreach (int id in GM.playManager.currentState.selectedEntityIdSet) {
                            EntityState draggedEntity = GM.boardManager.GetEntityById(id);
                            draggedEntity.entityBase.SetTempViewPosition(draggedEntity.pos + GM.inputManager.dragOffsetV2);
                        }
                    }
                    break;
                case MouseStateEnum.RELEASED:
                    this.clickedEntityId = null;
                    HashSet<int> selectedEntityIdSet = GM.playManager.currentState.selectedEntityIdSet;
                    if (selectedEntityIdSet != null) {
                        // TODO: this might not work because dragOffsetV2 could be null
                        if (GM.playManager.CanPlaceSelection(GM.inputManager.dragOffsetV2)) {
                            GM.boardManager.MoveEntityBatch(selectedEntityIdSet, GM.inputManager.dragOffsetV2, true);
                        }
                        else {
                            foreach (int id in selectedEntityIdSet) {
                                GM.boardManager.GetEntityBaseById(id).ResetView();
                            }
                        }
                    }
                    GM.playManager.SetSelectedEntityIdSet(null);
                    GM.playManager.SetTimeMode(TimeModeEnum.NORMAL);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public void Exit() {
            
        }
    }

    #endregion
}
