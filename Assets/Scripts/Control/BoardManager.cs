using System;
using System.Collections.Generic;
using System.IO;
using Schema;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using UnityEngine;

public delegate void OnUpdateBoardStateHandler(BoardState aBoardState);

public class BoardManager : SerializedMonoBehaviour {
    [SerializeField] BoardState boardState;
    public BoardState currentState {
        get { return this.boardState; }
    }
    public event OnUpdateBoardStateHandler OnUpdateBoardState;
    public Dictionary<int, EntityBase> entityBaseDict;

    #region Initialization

    // called by GM start
    public void InitializeStartingBoard() {
        InitBoard();
        AddBoundaryEntities();
    }
    
    // called by editManager or when coming back from playtest 
    public void LoadBoardStateFromFile(string aFilename = "PlayTestTemp.json") {
        print("attempting to load" + aFilename);
        BoardState loadedBoardState = GM.LoadBoardStateJson(aFilename);
        print(aFilename + "contains entity count of " + loadedBoardState.entityDict.Count);
        InitBoard(loadedBoardState);
        Debug.Log("loaded " + aFilename + " successfully");
    }

    void InitBoard(BoardState? aBoardState = null) {
        if (this.entityBaseDict != null) {
            foreach (EntityBase entityBase in this.entityBaseDict.Values) {
                Destroy(entityBase.gameObject);
            }
        }
        this.entityBaseDict = new Dictionary<int, EntityBase>();
        BoardState newBoardState = aBoardState ?? BoardState.GenerateBlankBoard();
        UpdateBoardState(newBoardState);
        foreach (EntityState entityState in newBoardState.entityDict.Values) {
            CreateEntityBase(entityState);
        }
    }

    void AddBoundaryEntities() {
        EntitySchema tallBoy = Resources.Load<EntitySchema>("ScriptableObjects/Entities/Blocks/1x11 block");
        EntitySchema longBoy = Resources.Load<EntitySchema>("ScriptableObjects/Entities/Blocks/20x1 block");
        AddEntityFromSchema(longBoy, new Vector2Int(0, 0), Constants.DEFAULTFACING, Constants.DEFAULTCOLOR, true, true);
        AddEntityFromSchema(longBoy, new Vector2Int(20, 0), Constants.DEFAULTFACING, Constants.DEFAULTCOLOR, true, true);

        AddEntityFromSchema(longBoy, new Vector2Int(0, 23), Constants.DEFAULTFACING, Constants.DEFAULTCOLOR, true, true);
        AddEntityFromSchema(longBoy, new Vector2Int(20, 23), Constants.DEFAULTFACING, Constants.DEFAULTCOLOR, true, true);

        AddEntityFromSchema(tallBoy, new Vector2Int(0, 1), Constants.DEFAULTFACING, Constants.DEFAULTCOLOR, true, true);
        AddEntityFromSchema(tallBoy, new Vector2Int(0, 12), Constants.DEFAULTFACING, Constants.DEFAULTCOLOR, true, true);

        AddEntityFromSchema(tallBoy, new Vector2Int(39, 1), Constants.DEFAULTFACING, Constants.DEFAULTCOLOR, true, true);
        AddEntityFromSchema(tallBoy, new Vector2Int(39, 12), Constants.DEFAULTFACING, Constants.DEFAULTCOLOR, true, true);
    }
    
    #endregion

    #region Listeners

    // special function called by GM.OnUpdateGameState delegate
    public void OnUpdateGameState(GameState aGameState) {
        
    }

    #endregion
    
    #region BoardState

    public void UpdateBoardState(BoardState aBoardState, HashSet<int> aEntitiesToUpdate = null) {
        if (Config.PRINTLISTENERUPDATES) {
            print("BoardManager - Updating BoardState for " + OnUpdateBoardState?.GetInvocationList().Length + " delegates");
        }
        // this is the only place where boardState gets set
        this.boardState = aBoardState;
        if (aEntitiesToUpdate != null) {
            foreach (int id in aEntitiesToUpdate) {
                GetEntityBaseById(id).OnUpdateBoardState(this.currentState);
            }
        }
        else {
            OnUpdateBoardState?.Invoke(this.currentState);
        }
    }

    void UpdateEntityAndBoardState(EntityState aEntityState, HashSet<int> aEntitiesToUpdate = null) {
        BoardState newBoardState = BoardState.UpdateEntity(this.currentState, aEntityState);
        UpdateBoardState(newBoardState, aEntitiesToUpdate);
    }

    public void SaveBoardState(bool aIsPlaytestTemp) {
        print("SaveBoardState");
        GM.SaveBoardStateJson(this.currentState, aIsPlaytestTemp);
    }

    public bool SetTitle(string aTitle) {
        if (0 < aTitle.Length && aTitle.Length <= Constants.MAXTITLELENGTH) {
            // TODO: validate titles so they can be valid filenames here and make input less hardass
            if (aTitle.IndexOfAny(Path.GetInvalidFileNameChars()) == -1) {
                BoardState newBoardState = BoardState.SetTitle(this.currentState, aTitle);
                UpdateBoardState(newBoardState);
                return true;
            }
            else {
                return false;
            }
        }
        else {
            throw new Exception("SetTitle - invalid title");
        }
    }

    public void SetPar(int aPar) {
        if (0 < aPar && aPar <= Constants.MAXPAR) {
            BoardState newBoardState = BoardState.SetPar(this.currentState, aPar);
            UpdateBoardState(newBoardState);
        }
    }

    #endregion
    
    #region Entity

    public void AddEntityFromSchema(EntitySchema aEntitySchema, Vector2Int aPos, Vector2Int aFacing, Color aDefaultColor, bool aIsFixed = false, bool aIsBoundary = false) {
        // if the area isn't clear, throw an exception
        Debug.Log("AddEntityFromSchema aPos: " + aPos);
        if (!this.currentState.IsRectEmpty(aPos, aEntitySchema.size, null, aEntitySchema.isFront)) {
            throw new Exception("AddEntity - Position is invalid pos:" + aPos + " schema:" + aEntitySchema.name + " schema size:" + aEntitySchema.size);
        }
        // generate a fresh entityState without an ID
        EntityState newEntityStateWithoutId = EntityState.CreateEntityState(aEntitySchema, aPos, aFacing, aDefaultColor, aIsFixed, aIsBoundary);
        // add it to the board and get the new boardState and entityState with ID back
        (BoardState newBoard, EntityState newEntityStateWithId) = BoardState.AddEntity(this.currentState, newEntityStateWithoutId);
        // update the boardState
        UpdateBoardState(newBoard);
        CreateEntityBase(newEntityStateWithId);
    }
    
    void CreateEntityBase(EntityState aEntityState) {
        if (this.entityBaseDict.ContainsKey(aEntityState.id)) {
            throw new Exception("TRIED TO OVERWRITE EXISTING ENTITYSTATE");
        }
        // set the new EntityPosition
        Vector3 newEntityPosition = Util.V2IOffsetV3(aEntityState.pos, aEntityState.size, aEntityState.isFront);
        // instantiate a new gameObject entityPrefab from the schemas prefabPath
        GameObject entityPrefab = Instantiate(GM.LoadEntityPrefabByFilename(aEntityState.prefabPath), newEntityPosition, Quaternion.identity,  this.transform);
        // get the entityBase
        EntityBase entityBase = entityPrefab.GetComponent<EntityBase>();
        // add it to the entityBaseDict

        this.entityBaseDict[aEntityState.id] = entityBase;
        // initialize entityBase with the newest state
        entityBase.Init(aEntityState);
    }
    
    public void RemoveEntity(int aId, bool aRemoveEntityBase = false) {
        if (aRemoveEntityBase) {
            EntityBase entityBase = this.entityBaseDict[aId];
            this.entityBaseDict.Remove(aId);
            Destroy(entityBase.gameObject);
        }
        // remove entity from boardstate
        BoardState newBoard = BoardState.RemoveEntity(this.currentState, aId);
        // update the boardState
        UpdateBoardState(newBoard);
    }

    public void RemoveEntityBase(int aId) {
        EntityBase entityBase = this.entityBaseDict[aId];
        this.entityBaseDict.Remove(aId);
        Destroy(entityBase.gameObject);
    }
    
    public void MoveEntity(int aId, Vector2Int aPos, bool aMoveEntityBase = false) {
        EntityState entityState = GetEntityById(aId);
        HashSet<int> ignoreSet = new HashSet<int> {aId};
        if (IsRectEmpty(aPos, entityState.size, ignoreSet, entityState.isFront)) {
            UpdateEntityAndBoardState(EntityState.SetPos(entityState, aPos), new HashSet<int>{aId});
            if (aMoveEntityBase) {
                entityState.entityBase.ResetView();
            }
        } else {
            throw new Exception("MoveEntity - invalid move id: " + aId + " to pos: " + aPos);
        }
    }
    public void MoveEntityBatch(HashSet<int> aEntityIdSet, Vector2Int aOffset, bool aMoveEntityBase = false) {
        Dictionary<int, EntityState> entityStateDict = new Dictionary<int, EntityState>();
        foreach (int id in aEntityIdSet) {
            EntityState movingEntity = this.currentState.entityDict[id];
            EntityState movedEntity = EntityState.SetPos(movingEntity, movingEntity.pos + aOffset);
            entityStateDict[id] = movedEntity;
        }
        UpdateBoardState(BoardState.UpdateEntityBatch(this.currentState, entityStateDict));
        if (aMoveEntityBase) {
            foreach (int id in aEntityIdSet) {
                GetEntityBaseById(id).ResetView();
            }
        }
    }

    public void SetEntityFacing(int aId, Vector2Int aFacing, bool aSetEntityBase = false) {
        EntityState entityState = GetEntityById(aId);
        if (Util.IsDirection(aFacing)) {
            UpdateEntityAndBoardState(EntityState.SetFacing(entityState, aFacing), new HashSet<int>{aId});
            if (aSetEntityBase) {
                entityState.entityBase.ResetView();
            }
        } else {
            throw new Exception("SetEntityFacing - invalid facing direction");
        }
    }

    public void SetEntityDefaultColor(int aId, Color aColor) {
        UpdateEntityAndBoardState(EntityState.SetDefaultColor(GetEntityById(aId), aColor), new HashSet<int>{aId});
    }

    public void SetEntityIsFixed(int aId, bool aIsFixed) {
        UpdateEntityAndBoardState(EntityState.SetIsFixed(GetEntityById(aId), aIsFixed), new HashSet<int>{aId});
    }

    public void SetEntityIsFixedBatch(IEnumerable<int> aIdSet, bool aIsFixed) {
        BoardState newBoardState = this.currentState;
        foreach (int id in aIdSet) {
            EntityState entityState = GetEntityById(id);
            entityState.isFixed = aIsFixed;
            newBoardState.entityDict = newBoardState.entityDict.SetItem(id, entityState);
            // newBoardState.entityDict[id] = entityState;
        }
        UpdateBoardState(newBoardState);
    }
    
    public void SetEntityTeam(int aId, TeamEnum aTeam) {
        UpdateEntityAndBoardState(EntityState.SetTeam(GetEntityById(aId), aTeam), new HashSet<int>{aId});
    }

    public void SetEntityTouchDefense(int aId, int aTouchDefense) {
        EntityState entityState = GetEntityById(aId);
        if (0 <= aTouchDefense && aTouchDefense <= 999) {
            UpdateEntityAndBoardState(EntityState.SetTouchDefense(entityState, aTouchDefense), new HashSet<int>{aId});
        }
        else {
            throw new Exception("SetEntityTouchDefense - invalid touchDefense");
        }
    }

    public void SetEntityFallDefense(int aId, int aFallDefense) {
        EntityState entityState = GetEntityById(aId);
        if (0 <= aFallDefense && aFallDefense <= 999) {
            UpdateEntityAndBoardState(EntityState.SetFallDefense(entityState, aFallDefense), new HashSet<int>{aId});
        }
        else {
            throw new Exception("SetEntityFallDefense - invalid touchDefense");
        }
    }

    #endregion

    #region Utility

    public EntityState? GetEntityAtPos(Vector2Int aPos, bool aIsFront = true) {
        return this.currentState.GetEntityAtPos(aPos, aIsFront);
    }

    public EntityState? GetEntityAtMousePos(bool aIsFront = true) {
        if (this.currentState.IsPosOnBoard(GM.inputManager.mousePosV2)) {
            return GetEntityAtPos(GM.inputManager.mousePosV2, aIsFront);
        }
        else {
            return null;
        }
    }

    public EntityState GetEntityById(int aId) {
        return this.currentState.GetEntityById(aId);
    }

    public EntityBase GetEntityBaseById(int aId) {
        return this.entityBaseDict[aId];
    }
    
    public bool CanEditorPlaceSchema(Vector2Int aPos, EntitySchema aEntitySchema) {
        // TODO: finish this
        if (IsRectEmpty(aPos, aEntitySchema.size, null, aEntitySchema.isFront)) {
            return true;
        }
        return false;
    }

    public bool IsPosInBoard(Vector2Int aPos) {
        return Util.IsInside(aPos, Vector2Int.zero, this.currentState.size);
    }

    public bool IsRectInBoard(Vector2Int aOrigin, Vector2Int aSize) {
        return Util.IsRectInside(aOrigin, aSize, Vector2Int.zero, this.currentState.size);
    }

    public Dictionary<Vector2Int, BoardCell> GetBoardGridSlice(Vector2Int aOrigin, Vector2Int aSize) {
        return this.currentState.GetBoardCellSlice(aOrigin, aSize);
    }

    public bool IsRectEmpty(Vector2Int aOrigin, Vector2Int aSize, HashSet<int> aIgnoreSet = null, bool aIsFront = true) {
        return this.currentState.IsRectEmpty(aOrigin, aSize, aIgnoreSet, aIsFront);
    }

    #endregion
}