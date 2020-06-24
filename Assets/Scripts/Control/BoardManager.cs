using System;
using System.Collections.Generic;
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
    public Dictionary<Vector2Int, BoardCell> boardCellDict;
    public event OnUpdateBoardStateHandler OnUpdateBoardState;
    // public event OnUpdateBoardStateHandler OnUpdateBoardStateBG;
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
        this.boardCellDict = new Dictionary<Vector2Int, BoardCell>();
        foreach (Vector2Int pos in Util.V2IInRect(Vector2Int.zero, Constants.MAXBOARDSIZE)) {
            BoardCell currentCell = new BoardCell(pos);
            this.boardCellDict[pos] = currentCell;
        }

        this.entityBaseDict = new Dictionary<int, EntityBase>();
        BoardState newBoardState = aBoardState ?? BoardState.GenerateBlankBoard();
        this.boardCellDict = new Dictionary<Vector2Int, BoardCell>();
        foreach (Vector2Int pos in Util.V2IInRect(Vector2Int.zero, newBoardState.size)) {
            this.boardCellDict[pos] = new BoardCell(pos);
        }
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

    void UpdateBoardState(BoardState aBoardState, HashSet<int> aEntitiesToUpdate = null) {
        if (Config.PRINTLISTENERUPDATES) {
            print("BoardManager - Updating BoardState for " + OnUpdateBoardState?.GetInvocationList().Length + " delegates");
        }
        if (aEntitiesToUpdate != null) {
            BoardState oldBoardState = this.boardState;
            foreach (int id in aEntitiesToUpdate) {
                EntityState oldEntityState = EntityState.GetClone(oldBoardState.entityDict[id]);
                HashSet<Vector2Int> oldPosSet = Util.V2IInRect(oldEntityState.pos, oldEntityState.data.size).ToHashSet();
                foreach (Vector2Int pos in oldPosSet) {
                    if (oldEntityState.data.isFront) {
                        this.boardCellDict[pos].frontEntityState = null;
                    }
                    else {
                        this.boardCellDict[pos].backEntityState = null;
                    }
                }
                EntityState newEntityState = aBoardState.entityDict[id];
                HashSet<Vector2Int> newPosSet = Util.V2IInRect(newEntityState.pos, newEntityState.data.size).ToHashSet();
                foreach (Vector2Int pos in newPosSet) {
                    if (newEntityState.data.isFront) {
                        this.boardCellDict[pos].frontEntityState = newEntityState;
                    }
                    else {
                        this.boardCellDict[pos].backEntityState = newEntityState;
                    }
                }
            }
            this.boardState = aBoardState;
            foreach (int id in aEntitiesToUpdate) {
                GetEntityBaseById(id).OnUpdateBoardState(this.currentState);
            }
        }
        else {
            this.boardState = aBoardState;
            SetBoardCellDict(aBoardState);
            OnUpdateBoardState?.Invoke(this.currentState);
        }
    }
    
    void UpdateEntityAndBoardState(EntityState aEntityState, HashSet<int> aEntitiesToUpdate = null) {
        BoardState newBoardState = BoardState.UpdateEntity(this.currentState, aEntityState);
        UpdateBoardState(newBoardState, aEntitiesToUpdate);
    }

    void SetBoardCellDict(BoardState aBoardState) {
        foreach (var currentCell in this.boardCellDict.Values) {
            currentCell.frontEntityState = null;
            currentCell.backEntityState = null;
        }

        foreach (EntityState currentEntity in aBoardState.entityDict.Values) {
            foreach (Vector2Int currentPos in Util.V2IInRect(currentEntity.pos, currentEntity.data.size)) {
                BoardCell currentCell = this.boardCellDict[currentPos];
                if (currentEntity.data.isFront) {
                    currentCell.frontEntityState = currentEntity;
                }
                else {
                    currentCell.backEntityState = currentEntity;
                }
            }
        }
    }
    
    public void SaveBoardState(bool aIsPlaytestTemp) {
        print("SaveBoardState");
        GM.SaveBoardStateJson(this.currentState, aIsPlaytestTemp);
    }

    public void SetTitle(string aTitle) {
        if (0 < aTitle.Length && aTitle.Length <= Constants.MAXTITLELENGTH) {
            // TODO: validate titles so they can be valid filenames here
            // char[] invalidFileNameChars = Path.GetInvalidFileNameChars();
            BoardState newBoardState = BoardState.SetTitle(this.currentState, aTitle);
            UpdateBoardState(newBoardState);
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
        if (!IsRectEmpty(aPos, aEntitySchema.size, null, aEntitySchema.isFront)) {
            throw new Exception("AddEntity - Position is invalid");
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
        if (this.entityBaseDict.ContainsKey(aEntityState.data.id)) {
            throw new Exception("TRIED TO OVERWRITE EXISTING ENTITYSTATE");
        }
        // set the new EntityPosition
        Vector3 newEntityPosition = Util.V2IOffsetV3(aEntityState.pos, aEntityState.data.size, aEntityState.data.isFront);
        // instantiate a new gameObject entityPrefab from the schemas prefabPath
        GameObject entityPrefab = Instantiate(GM.LoadEntityPrefabByFilename(aEntityState.data.prefabPath), newEntityPosition, Quaternion.identity,  this.transform); 
        // get the entityBase
        EntityBase entityBase = entityPrefab.GetComponent<EntityBase>();
        // add it to the entityBaseDict

        this.entityBaseDict[aEntityState.data.id] = entityBase;
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
        print("moving entity:" + aId);
        EntityState entityState = GetEntityById(aId);
        print("original pos:" + entityState.pos);
        HashSet<EntityState> ignoreSet = new HashSet<EntityState> {entityState};
        if (IsRectEmpty(aPos, entityState.data.size, ignoreSet, entityState.data.isFront)) {
            UpdateEntityAndBoardState(EntityState.SetPos(entityState, aPos), new HashSet<int>{aId});
            if (aMoveEntityBase) {
                entityState.entityBase.ResetView();
            }
        } else {
            throw new Exception("MoveEntity - invalid move id: " + aId + " to pos: " + aPos);
        }
    }

    public void MoveEntityBatch(HashSet<int> aEntityIdSet, Vector2Int aOffset, bool aMoveEntityBase = false) {
        BoardState newBoardState = BoardState.GetClone(this.currentState);
        foreach (int id in aEntityIdSet) {
            EntityState movingEntity = this.currentState.entityDict[id];
            EntityState movedEntity = EntityState.SetPos(movingEntity, movingEntity.pos + aOffset);
            newBoardState = BoardState.UpdateEntity(newBoardState, movedEntity);
        }
        UpdateBoardState(newBoardState, aEntityIdSet);
        if (aMoveEntityBase) {
            foreach (int id in aEntityIdSet) {
                GetEntityBaseById(id).ResetView();
            }
        }
    }

    public void SetEntityFacing(int aId, Vector2Int aFacing) {
        EntityState entityState = GetEntityById(aId);
        if (Util.IsDirection(aFacing)) {
            UpdateEntityAndBoardState(EntityState.SetFacing(entityState, aFacing), new HashSet<int>{aId});
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
        if (!IsPosInBoard(aPos)) {
            return null;
        }
        if (aIsFront) {
            EntityState? entityState = this.boardCellDict[aPos].frontEntityState;
            return entityState;
        }
        else {
            EntityState? entityState = this.boardCellDict[aPos].backEntityState;
            return entityState;
        }
    }
    
    public EntityState? GetEntityAtMousePos(bool aIsFront = true) {
        return GetEntityAtPos(GM.inputManager.mousePosV2, aIsFront);
    }

    public EntityState GetEntityById(int aId) {
        EntityState entityState = EntityState.GetClone(this.currentState.entityDict[aId]);
        return entityState;
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
        if (IsRectInBoard(aOrigin, aSize)) {
            Dictionary<Vector2Int, BoardCell> sliceDict = new Dictionary<Vector2Int, BoardCell>();
            foreach (Vector2Int currentPos in Util.V2IInRect(aOrigin, aSize)) {
                if (this.boardCellDict.ContainsKey(currentPos)) {
                    sliceDict[currentPos] = this.boardCellDict[currentPos];
                }
            }
            return sliceDict;
        }
        throw new ArgumentOutOfRangeException("GetBoardGridSlice - rect not in board" + aOrigin + aSize);
    }

    public bool IsRectEmpty(Vector2Int aOrigin, Vector2Int aSize, HashSet<EntityState> aIgnoreSet = null, bool aIsFront = true) {
        try {
            foreach (BoardCell boardCell in GetBoardGridSlice(aOrigin, aSize).Values) {
                EntityState? entityState = null;
                switch (aIsFront) {
                    case true when boardCell.frontEntityState.HasValue:
                        entityState = boardCell.frontEntityState;
                        break;
                    case false when boardCell.backEntityState.HasValue:
                        entityState = boardCell.backEntityState;
                        break;
                }
                if (entityState.HasValue) {
                    if (aIgnoreSet == null) {
                        return false;
                    }
                    if (!aIgnoreSet.Contains(entityState.Value)) {
                        return false;
                    }
                }
            }
            return true;
        }
        catch (ArgumentOutOfRangeException) {
            return false;
        }
    }

    #endregion
}