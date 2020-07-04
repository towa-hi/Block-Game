using System;
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
    public GUIPopup popup;

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
                    if (this.OnUpdatePlayState != null) {
                        print("PlayManager - Updating PlayState for " + this.OnUpdatePlayState?.GetInvocationList().Length + " delegates");
                    }
                }
                this.OnUpdatePlayState?.Invoke(this.currentState);
                break;
            case GameModeEnum.EDITING:
                break;
            case GameModeEnum.PLAYTESTING:
                if (Config.PRINTLISTENERUPDATES) {
                    if (this.OnUpdatePlayState != null) {
                        print("PlayManager - Updating PlayState for " + this.OnUpdatePlayState?.GetInvocationList().Length + " delegates");
                    }
                }
                this.OnUpdatePlayState?.Invoke(this.currentState);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    void InitializePlayState() {
        PlayState initialPlayState = PlayState.CreatePlayState();
        initialPlayState = PlayState.SetTimeMode(initialPlayState, TimeModeEnum.NORMAL);
        initialPlayState = PlayState.SetPlayMode(initialPlayState, PlayModeEnum.PLAYING);
        UpdatePlayState(initialPlayState);
    }

    public void SetSelectedEntityIdSet(int? aHeldEntityId, [CanBeNull] HashSet<int> aSelectedEntityIdSet) {
        PlayState newPlayState = PlayState.SetSelectedEntityIdSet(this.currentState, aSelectedEntityIdSet);
        newPlayState = PlayState.SetHeldEntity(newPlayState, aHeldEntityId);
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
    public void StartEntityDeath(int aId) {
        GM.boardManager.RemoveEntity(aId);
        print(aId + " StartEntityForDeath - removed from board");
    }
    
    // called when DyingState finishes
    public void FinishEntityDeath(int aId, bool aIsPlayer = false) {
        this.entityIdsToKillThisFrame.Add(aId);
        print(aId + " FinishEntityDeath - entity marked for removal next frame");
        if (aIsPlayer) {
            LoseBoard();
        }
    }

    #endregion
    
    #region External

    public void OnBackToEditorButtonClick() {
        SetTimeMode(TimeModeEnum.NORMAL);
        GM.boardManager.LoadBoardStateFromFile();
        GM.instance.SetGameMode(GameModeEnum.EDITING);
    }

    public void WinBoard() {
        SetTimeMode(TimeModeEnum.PAUSED);
        SetPlayMode(PlayModeEnum.WON);
        this.popup.Init(true, GM.boardManager.currentState.par, this.currentState.moves);
    }

    public void LoseBoard() {
        SetTimeMode(TimeModeEnum.PAUSED);
        SetPlayMode(PlayModeEnum.LOST);
        this.popup.Init(false, GM.boardManager.currentState.par, this.currentState.moves);
    }
    #endregion
    
    #region Utility

    public static bool CanEntityBePushed(int aId, Vector2Int aDirection) {
        EntityState entityToPush = GM.boardManager.GetEntityById(aId);
        if (entityToPush.mobData?.canPush != true) {
            return false;
        }
        if (GM.boardManager.IsRectEmpty(entityToPush.pos + aDirection, entityToPush.size, new HashSet<int> {entityToPush.id})) {
            return true;
        }
        else {
            return false;
        }
    }

    public static bool DoesFloorExist(Vector2Int aPos, int aId) {
        EntityState entityState = GM.boardManager.GetEntityById(aId);
        Vector2Int floorOrigin = aPos + Vector2Int.down;
        Vector2Int floorSize = new Vector2Int(entityState.size.x, 1);
        if (!GM.boardManager.IsRectInBoard(floorOrigin, floorSize)) {
            return false;
        }
        HashSet<BoardCell> floorSlice = GM.boardManager.GetBoardGridSlice(floorOrigin, floorSize).Values.ToHashSet();
        foreach (BoardCell floorCell in floorSlice) {
            if (floorCell.frontEntityId.HasValue) {
                // EntityState floorEntity = floorCell.frontEntityId.Value;
                // if floorEntity is me, skip
                if (floorCell.frontEntityId.Value == entityState.id) {
                    continue;
                }
                if (EntityFallOnEntityResult(entityState.id, floorCell.frontEntityId.Value) == FightResultEnum.TIE) {
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
    // TODO: optimize block selection to not do GetConnectedTree twice just to check
    // public bool TrySelectEntity(int aId, bool aIsUp) {
    //     HashSet<int> newSelectedEntityIdSet = GetSelectedEntityIdSet(aId, aIsUp);
    //
    // }

    public bool IsEntitySelectable(int aId, bool aIsUp) {
        if (!IsEntityMovable(aId)) {
            return false;
        }
        HashSet<int> connectedTree = GetConnectedTree(aId, aIsUp, new HashSet<int>());
        bool isEntityConnectedToFixed = false;
        foreach (int connectedId in connectedTree) {
            if (!IsEntityMovable(connectedId)) {
                isEntityConnectedToFixed = true;
            }
        }
        return !isEntityConnectedToFixed;
    }

    public bool IsEntityMovable(int aId) {
        EntityState entityState = GM.boardManager.GetEntityById(aId);
        if (entityState.isBoundary) {
            return false;
        }
        if (entityState.isFixed) {
            return false;
        }
        if (entityState.entityType == EntityTypeEnum.MOB) {
            return false;
        }
        // if (!entityState.hasNodes) {
        //     return false;
        // }
        return true;
    }

    public bool DoesEntitySupportAMob(int aId) {
        EntityState entityState = GM.boardManager.GetEntityById(aId);
        for (int x = entityState.pos.x; x < entityState.pos.x + entityState.size.x; x++) {
            Vector2Int currentPos = new Vector2Int(x, entityState.pos.y + entityState.size.y + 1);
            EntityState? maybeAEntity = GM.boardManager.GetEntityAtPos(currentPos);
            if (maybeAEntity.HasValue && maybeAEntity.Value.mobData?.canFall == true) {
                return true;
            }
        }
        return false;
    }
    
    public void SelectEntity(int aId, bool aIsUp) {
        SetSelectedEntityIdSet(aId, GetSelectedEntityIdSet(aId, aIsUp));
    }

    public HashSet<int> GetSelectedEntityIdSet(int aId, bool aIsUp) {
        // Debug.Assert(rootEntity.hasNodes);
        HashSet<int> mainTree = GetConnectedTree(aId, aIsUp);
        HashSet<int> selectSet = new HashSet<int>(mainTree);
        foreach (int currentId in mainTree) {
            // MarkEntity(currentEntity, Color.green);
            foreach (int hangerId in GetConnected(currentId, !aIsUp, mainTree)) {
                bool addHangerToConnectedTree = true;
                HashSet<int> hangerConnectedSet = GetAllConnected(hangerId, mainTree);
                foreach (int hangerConnectedId in hangerConnectedSet) {
                    if (addHangerToConnectedTree) {
                        if (!IsEntityMovable(hangerConnectedId)) {
                            addHangerToConnectedTree = false;
                        }
                    }
                }
                if (addHangerToConnectedTree) {
                    // foreach (int hangerConnectedId in hangerConnectedSet) {
                    //     if (!hangerConnected.entityBase.isMarked) {
                    //         MarkEntity(hangerConnected, IsEntityMovable(hangerConnected.id) ? Color.cyan : Color.red);
                    //     }
                    // }
                    selectSet.UnionWith(hangerConnectedSet);
                }
            }
        }
        return selectSet;
    }

    // ReSharper disable once ReturnTypeCanBeEnumerable.Local
    HashSet<int> GetConnectedTree(int aRootId, bool aIsUp, HashSet<int> aConnectedTreeSet = null) {
        // Debug.Assert(aRoot.hasNodes);
        if (aConnectedTreeSet == null) {
            aConnectedTreeSet = new HashSet<int> {aRootId};
        }
        else {
            aConnectedTreeSet.Add(aRootId);
        }
        
        if (IsEntityMovable(aRootId)) {
            foreach (int connectedId in GetConnected(aRootId, aIsUp)) {
                if (!aConnectedTreeSet.Contains(connectedId)) {
                    GetConnectedTree(connectedId, aIsUp, aConnectedTreeSet);
                }
            }
        }
        return aConnectedTreeSet;
    }

    HashSet<int> GetConnected(int aRootId, bool aIsUp, HashSet<int> aIgnoreSet = null) {
        // Debug.Assert(aRoot.hasNodes);
        HashSet<int> connectedEntitySet = new HashSet<int>();
        // get the absolute pos of where to check above/below root
        foreach (Node node in GM.boardManager.GetEntityById(aRootId).GetNodes(aIsUp)) {
            Node? oppositeNode = node.GetOppositeNode(aIsUp, Vector2Int.zero, aIgnoreSet);
            if (oppositeNode.HasValue) {
                connectedEntitySet.Add(oppositeNode.Value.id);
            }
        }
        return connectedEntitySet;
    }

    HashSet<int> GetConnected(int aRootId, HashSet<int> aIgnoreSet = null) {
        HashSet<int> connectedEntitySet = new HashSet<int>();
        // get the absolute pos of where to check above/below root
        foreach (Node node in GM.boardManager.GetEntityById(aRootId).GetNodes()) {
            (Node? upNode, Node? downNode) = node.GetAllOppositeNodes(Vector2Int.zero, aIgnoreSet);
            if (upNode.HasValue) {
                connectedEntitySet.Add(upNode.Value.id);
            }
            if (downNode.HasValue) {
                connectedEntitySet.Add(downNode.Value.id);
            }
        }
        return connectedEntitySet;
    }

    HashSet<int> GetAllConnected(int aRootId, HashSet<int> aIgnoreSet = null) {
        if (aIgnoreSet == null) {
            aIgnoreSet = new HashSet<int>();
        }
        HashSet<int> visitedSet = new HashSet<int>(aIgnoreSet);
        HashSet<int> allConnectedSet = new HashSet<int>();
        GetAllConnectedRecursive(aRootId);
        return allConnectedSet;

        void GetAllConnectedRecursive(int rRootId) {
            visitedSet.Add(rRootId);
            allConnectedSet.Add(rRootId);
            HashSet<int> connectedSet = GetConnected(rRootId, visitedSet);
            foreach (int connectedId in connectedSet) {
                if (!visitedSet.Contains(connectedId)) {
                    GetAllConnectedRecursive(connectedId);
                }
            }
        }
    }

    void MarkEntity(EntityState aEntityState, Color aColor) {
        // foreach (Vector2Int pos in Util.V2IInRect(aEntityState.pos, aEntityState.size)) {
        //     GM.debugDrawer.SetMarker(pos, aColor, 2f);
        // }
        aEntityState.entityBase.SetMarker(aColor, 2f);
    }

    public bool IsEntityConnectedToFixed(int aId, bool aIsUp, HashSet<EntityState> aIgnoreSet = null) {
        bool isEntityConnectedToFixed = false;
        HashSet<int> connectedTree = GetConnectedTree(aId, aIsUp, new HashSet<int>());
        foreach (int connectedId in connectedTree) {
            if (!IsEntityMovable(connectedId)) {
                isEntityConnectedToFixed = true;
            }
        }
        return isEntityConnectedToFixed;
    }
    
    public bool CanPlaceSelection(Vector2Int aOffset) {
        Debug.Assert(this.currentState.selectedEntityIdSet != null);
        bool touchingUp = false;
        bool touchingDown = false;
        HashSet<int> ignoreSet = this.currentState.selectedEntityIdSet;
        foreach (int selectedEntityId in ignoreSet) {
            EntityState selectedEntity = GM.boardManager.GetEntityById(selectedEntityId);
            // Debug.Assert(selectedEntity.hasNodes);
            if (!CanPlaceEntity(selectedEntityId, aOffset, ignoreSet)) {
                print("CanPlaceSelection - false because entity is blocked");
                return false;
            }
            foreach (Node node in selectedEntity.nodeIArray) {
                (Node? upNode, Node? downNode) = node.GetAllOppositeNodes(aOffset, ignoreSet);
                if (upNode.HasValue && !ignoreSet.Contains(upNode.Value.id)) {
                    touchingUp = true;
                }
                if (downNode.HasValue && !ignoreSet.Contains(downNode.Value.id)) {
                    touchingDown = true;
                }
                if (touchingUp && touchingDown) {
                    return false;
                }
                // EntityState? maybeAEntity = node.GetOppositeNode(aOffset, ignoreSet)?.entityState;
                // Node? oppositeNode = node.GetAllOppositeNodes(aOffset, ignoreSet);
                // if (oppositeNode.HasValue && !ignoreSet.Contains(oppositeNode.Value.id)) {
                //     if (node.isUp) {
                //         touchingUp = true;
                //     }
                //     else {
                //         touchingDown = true;
                //     }
                //     if (touchingUp && touchingDown) {
                //         return false;
                //     }
                // }
            }
        }
        if (touchingUp ^ touchingDown) {
            // print("CanPlaceSelection - true because touching one way");
            return true;
        }
        else {
            // print("CanPlaceSelection - false because touching no ways");
            return false;
        }
    }

    public bool CanPlaceEntity(int aId, Vector2Int aOffset, HashSet<int> aEntityIdIgnoreSet = null) {
        EntityState entityState = GM.boardManager.GetEntityById(aId);
        // Debug.Assert(entityState.hasNodes);
        HashSet<int> entityIdIgnoreSet = aEntityIdIgnoreSet ?? new HashSet<int> {aId};
        if (GM.boardManager.IsRectInBoard(entityState.pos + aOffset, entityState.size) && GM.boardManager.IsRectEmpty(entityState.pos + aOffset, entityState.size, entityIdIgnoreSet)) {
            return true;
        }
        else {
            return false;
        }
    }




























    
    #endregion

    #region StateMachineStates

    class PlayingState : StateMachineState {
        int? clickedEntityId;
        bool failedUpSelect;
        bool failedDownSelect;

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
                        this.clickedEntityId = maybeAEntity.Value.id;
                    }
                    break;
                case MouseStateEnum.HELD:
                    if (this.clickedEntityId.HasValue) {
                        if (GM.inputManager.dragOffset.y > Constants.DRAGTHRESHOLD) {
                            if (!this.failedUpSelect) {
                                // print("attempting up select");
                                if (GM.playManager.IsEntitySelectable(this.clickedEntityId.Value, true)) {
                                    GM.playManager.SelectEntity(this.clickedEntityId.Value, true);
                                    this.clickedEntityId = null;
                                    GM.playManager.SetTimeMode(TimeModeEnum.PAUSED);
                                }
                                else {
                                    this.failedUpSelect = true;
                                }
                            }
                        } else if (GM.inputManager.dragOffset.y < Constants.DRAGTHRESHOLD * -1) {
                            if (!this.failedDownSelect) {
                                // print("attempting down select");
                                if (GM.playManager.IsEntitySelectable(this.clickedEntityId.Value, false)) {
                                    GM.playManager.SelectEntity(this.clickedEntityId.Value, false);
                                    this.clickedEntityId = null;
                                    GM.playManager.SetTimeMode(TimeModeEnum.PAUSED);
                                }
                                else {
                                    this.failedDownSelect = true;
                                }
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
                            GM.playManager.IncrementMoves();
                        }
                        else {
                            foreach (int id in selectedEntityIdSet) {
                                GM.boardManager.GetEntityBaseById(id).ResetView();
                            }
                        }
                    }
                    GM.playManager.SetSelectedEntityIdSet(null, null);
                    GM.playManager.SetTimeMode(TimeModeEnum.NORMAL);
                    this.failedUpSelect = false;
                    this.failedDownSelect = false;
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
