
using System;
using System.Collections.Generic;
using UnityEngine;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.Serialization;
using JetBrains.Annotations;
using Sirenix.Utilities;


public struct BoardState {
    public bool isInitialized;
    public ImmutableDictionary<int, EntityState> entityDict;
    public string title;
    public string creator;
    public int par;
    public Vector2Int size;
    public int attempts;
    [SerializeField] int currentId;
    [SerializeField] Dictionary<int, EntityState> serializedEntityDict;
    ImmutableArray<BoardCell> boardCellIArray;

    #region Init

    public static BoardState GenerateBlankBoard() {
        BoardState newBoard = new BoardState {
            isInitialized = true,
            entityDict = ImmutableDictionary.Create<int, EntityState>(),
            title = "UninitializedBoard",
            creator = Config.USERNAME,
            par = 5,
            size = Constants.MAXBOARDSIZE,
            attempts = 0,
        };
        newBoard = InitBoardCellIArray(newBoard);
        return newBoard;
    }
    // only call this right before serialization
    public static BoardState PackBoardState(BoardState aBoardState) {
        Debug.Log("Packing BoardState");
        aBoardState.serializedEntityDict = aBoardState.entityDict.ToDictionary(p  => p.Key, p => p.Value);
        return aBoardState;
    }
    // only call this right after serialization
    public static BoardState UnpackBoardState(BoardState aBoardState) {
        Debug.Log("Unpacking BoardState");
        aBoardState.entityDict = aBoardState.serializedEntityDict.ToImmutableDictionary();
        aBoardState.serializedEntityDict.Clear();
        aBoardState = InitBoardCellIArray(aBoardState);
        return aBoardState;
    }

    #endregion

    #region Local Utility

    public EntityState GetEntityById(int id) {
        return this.entityDict[id];
    }

    public ImmutableArray<BoardCell> GetBoardCellIArray() {
        return this.boardCellIArray;
    }
    public BoardCell GetBoardCellAtPos(Vector2Int aPos) {
        if (!IsPosOnBoard(aPos)) {
            throw new ArgumentOutOfRangeException(nameof(aPos),"tried to get boardCell out of bounds");
        }
        return GetFromImmutableArray(aPos);
    }

    public int? GetEntityIdAtPos(Vector2Int aPos, bool aIsFront = true) {
        if (!IsPosOnBoard(aPos)) {
            throw new ArgumentOutOfRangeException(nameof(aPos),"tried to get boardCell out of bounds");
        }
        if (aIsFront) {
            return GetFromImmutableArray(aPos).frontEntityId;
        }
        else {
            return GetFromImmutableArray(aPos).backEntityId;
        }
    }

    public EntityState? GetEntityAtPos(Vector2Int aPos, bool aIsFront = true) {
        int? id = GetEntityIdAtPos(aPos, aIsFront);
        return id.HasValue ? (EntityState?) this.entityDict[id.Value] : null;
    }

    public bool IsRectInBoard(Vector2Int aOrigin, Vector2Int aSize) {
        return Util.IsRectInside(aOrigin, aSize, Vector2Int.zero, this.size);
    }

    public bool IsPosOnBoard(Vector2Int aPos) {
        return 0 <= aPos.x && aPos.x < this.size.x && 0 <= aPos.y && aPos.y < this.size.y;
    }

    public bool DoesFloorExist(Vector2Int aPos, int aId) {
        EntityState entityState = GetEntityById(aId);
        Vector2Int floorOrigin = aPos + Vector2Int.down;
        Vector2Int floorSize = new Vector2Int(entityState.size.x, 1);
        if (!IsRectInBoard(floorOrigin, floorSize)) {
            return false;
        }
        HashSet<BoardCell> floorSlice = GetBoardCellSlice(floorOrigin, floorSize).Values.ToHashSet();
        foreach (BoardCell floorCell in floorSlice) {
            if (floorCell.frontEntityId.HasValue) {

            }
            if (floorCell.frontEntityId.HasValue && floorCell.frontEntityId.Value != entityState.id) {
                return true;
            }
        }
        return false;
    }

    public bool IsRectEmpty(Vector2Int aOrigin, Vector2Int aSize, HashSet<int> aIgnoreSet = null, bool aIsFront = true) {
        // Debug.Log("IsRectEmpty aOrigin:" + aOrigin);
        foreach (var kvp in GetBoardCellSlice(aOrigin, aSize)) {
            Vector2Int key = kvp.Key;
            BoardCell boardCell = kvp.Value;
            Debug.Assert(key == boardCell.pos);
            // Debug.Log("IsRectEmpty examining boardCell at " + boardCell.pos + " with key: " + key);
            int? id = aIsFront ? boardCell.frontEntityId : boardCell.backEntityId;
            if (id.HasValue) {
                if (aIgnoreSet == null) {
                    // Debug.Log("IsRectEmpty returned false on pos: " + boardCell.pos + "because occupied by " + id.Value);
                    return false;
                }
                else if (!aIgnoreSet.Contains(id.Value)) {
                    return false;
                }
            }
        }
        return true;
    }

    public Dictionary<Vector2Int, BoardCell> GetBoardCellSlice(Vector2Int aOrigin, Vector2Int aSize) {
        // Debug.Log("GetBoardCellSlice aOrigin: " + aOrigin + " aSize: " + aSize);
        if (!IsRectInBoard(aOrigin, aSize)) {
            throw new ArgumentOutOfRangeException("GetBoardGridSlice - rect not in board" + aOrigin + aSize);
        }
        Dictionary<Vector2Int, BoardCell> sliceDict = new Dictionary<Vector2Int, BoardCell>();
        foreach (Vector2Int currentPos in Util.V2IArrayInRect(aOrigin, aSize)) {
            // sliceDict[currentPos] = this.boardCellArray[currentPos.x, currentPos.y];
            BoardCell boardCell = GetFromImmutableArray(currentPos);
            Debug.Assert(currentPos == boardCell.pos);
            // Debug.Log("GetBoardCellSlice currentPos: " + currentPos + " pos: " + boardCell.pos + " front: " + boardCell.frontEntityId + " back : " + boardCell.backEntityId);
            sliceDict[currentPos] = boardCell;
        }
        // Debug.Log("GetBoardCellSlice returned slice " + sliceDict.Count);
        return sliceDict;
    }

    #endregion

    #region BoardCell

    public static int GetIndexFromPos(int aX, int aY, Vector2Int aSize) {
        return aY * aSize.x + aX;
    }

    public static int GetIndexFromPos(Vector2Int aPos, Vector2Int aSize) {
        return aPos.y * aSize.x + aPos.x;
    }

    static BoardState InitBoardCellIArray(BoardState aBoardState) {
        BoardCell[] array = new BoardCell[aBoardState.size.x * aBoardState.size.y];
        for (int y = 0; y < aBoardState.size.y; y++) {
            for (int x = 0; x < aBoardState.size.x; x++) {
                int index = GetIndexFromPos(x, y, aBoardState.size);
                // Debug.Log("InitBoardCellArray inserting new Vector2Int" + new Vector2Int(x, y) + " into " + GetIndexFromPos(x, y, aSize));
                array[index] = new BoardCell(new Vector2Int(x, y));
            }
        }
        foreach (EntityState entityState in aBoardState.entityDict.Values) {
            foreach (Vector2Int posInEntity in Util.V2IArrayInRect(entityState.pos, entityState.size)) {
                int index = GetIndexFromPos(posInEntity, aBoardState.size);
                BoardCell boardCellWithEntity = array[index];
                if (entityState.isFront) {
                    boardCellWithEntity.frontEntityId = entityState.id;
                }
                else {
                    boardCellWithEntity.backEntityId = entityState.id;
                }
                array[index] = boardCellWithEntity;
            }
        }
        aBoardState.boardCellIArray = ImmutableArray.Create(array);
        return aBoardState;
    }

    void UpdateBoardCellArray() {
        ImmutableArray<BoardCell>.Builder builder = this.boardCellIArray.ToBuilder();
        // Debug.Log("UpdateBoardCellArray builder (before): " + builder.Count);
        for (int x = 0; x < this.size.x; x++) {
            for (int y = 0; y < this.size.y; y++) {
                BoardCell boardCellBeingCleared = this.boardCellIArray[GetIndexFromPos(x, y, this.size)];
                boardCellBeingCleared.frontEntityId = null;
                boardCellBeingCleared.backEntityId = null;
                int index = GetIndexFromPos(x, y, this.size);
                // Debug.Log("UpdateBoardCellArray inserting at index " + index + " boardCell " + boardCellBeingCleared.pos);
                builder.RemoveAt(index);
                builder.Insert(index, boardCellBeingCleared);
                // Debug.Log("builder count: " + builder.Count);
            }
        }
        // for every entity set occupied cells to that entities id
        foreach (EntityState currentEntity in this.entityDict.Values) {
            foreach (Vector2Int currentPos in Util.V2IArrayInRect(currentEntity.pos, currentEntity.size)) {
                // SetBoardCell(currentPos, currentEntity.id, currentEntity.isFront);
                int index = GetIndexFromPos(currentPos, this.size);
                BoardCell boardCellWithEntity = new BoardCell(currentPos);
                if (currentEntity.isFront) {
                    boardCellWithEntity.frontEntityId = currentEntity.id;
                }
                else {
                    boardCellWithEntity.backEntityId = currentEntity.id;
                }
                // Debug.Log("UpdateBoardCellArray inserted currentEntity: " + currentEntity.id + " into pos: " + currentPos + " with boardCell pos: " + boardCellWithEntity.pos);
                // Debug.Log("UpdateBoardCellArray insert index:" + GetIndexFromPos(currentPos, this.size));
                builder.RemoveAt(index);
                builder.Insert(index, boardCellWithEntity);
            }
        }
        // Debug.Log("UpdateBoardCellArray builder (after): " + builder.Count);
        this.boardCellIArray = builder.MoveToImmutable();
    }

    void UpdateBoardCellArray(BoardState aOldBoardState, HashSet<int> aEntitiesToUpdate) {
        // clear every entity in partial updates occupied cells
        ImmutableArray<BoardCell>.Builder builder = this.boardCellIArray.ToBuilder();
        foreach (int id in aEntitiesToUpdate) {
            EntityState oldEntityState = aOldBoardState.entityDict[id];
            foreach (Vector2Int oldPos in Util.V2IArrayInRect(oldEntityState.pos, oldEntityState.size)) {
                int index = GetIndexFromPos(oldPos, aOldBoardState.size);
                BoardCell boardCellBeingCleared = aOldBoardState.boardCellIArray[index];
                if (oldEntityState.isFront) {
                    boardCellBeingCleared.frontEntityId = null;
                }
                else {
                    boardCellBeingCleared.backEntityId = null;
                }
                builder.RemoveAt(index);
                builder.Insert(index, boardCellBeingCleared);
                // SetBoardCell(oldPos, null, oldEntityState.isFront);
            }
        }
        // reset every entity in partial update to new locations provided by aNewBoardState
        foreach (int id in aEntitiesToUpdate) {
            EntityState newEntityState = this.entityDict[id];
            foreach (Vector2Int newPos in Util.V2IArrayInRect(newEntityState.pos, newEntityState.size)) {
                int index = GetIndexFromPos(newPos, this.size);
                BoardCell boardCellWithEntity = this.boardCellIArray[index];
                if (newEntityState.isFront) {
                    boardCellWithEntity.frontEntityId = newEntityState.id;
                }
                else {
                    boardCellWithEntity.backEntityId = newEntityState.id;
                }
                builder.RemoveAt(index);
                builder.Insert(index, boardCellWithEntity);
                // SetBoardCell(newPos, newEntityState.id, newEntityState.isFront);
            }
        }
        this.boardCellIArray = builder.MoveToImmutable();
    }

    BoardCell GetFromImmutableArray(Vector2Int aPos) {
        int index = GetIndexFromPos(aPos, this.size);
        return this.boardCellIArray[index];
    }



    #endregion

    #region Setters

    public static Tuple<BoardState, EntityState> AddEntity(BoardState aBoardState, EntityState aEntityState) {
        int id = aBoardState.currentId;
        // id always set here
        aEntityState.Init(id);
        // NEVER CHANGE CURRENTID OUTSIDE OF THIS FUNCTION!!!
        aBoardState.currentId += 1;
        aBoardState.entityDict = aBoardState.entityDict.SetItem(id, aEntityState);
        // aBoardState.entityDict[id] = aEntityState;
        aBoardState.UpdateBoardCellArray();
        return new Tuple<BoardState, EntityState>(aBoardState, aEntityState);
    }
    
    public static BoardState RemoveEntity(BoardState aBoardState, int aId) {
        aBoardState.entityDict = aBoardState.entityDict.Remove(aId);
        Debug.Log("removed Entity");
        aBoardState.UpdateBoardCellArray();
        return aBoardState;
    }
    
    public static BoardState SetTitle(BoardState aBoardState, string aTitle) {
        aBoardState.title = aTitle;
        return aBoardState;
    }

    public static BoardState SetPar(BoardState aBoardState, int aPar) {
        aBoardState.par = aPar;
        return aBoardState;
    }
    
    public static BoardState UpdateEntity(BoardState aBoardState, EntityState aEntityState) {
        BoardState oldBoardState = aBoardState;
        aBoardState.entityDict = aBoardState.entityDict.SetItem(aEntityState.id, aEntityState);
        aBoardState.UpdateBoardCellArray(oldBoardState, new HashSet<int>{aEntityState.id});
        return aBoardState;
    }

    public static BoardState UpdateEntityBatch(BoardState aBoardState, Dictionary<int, EntityState> aEntityStateDict) {
        BoardState oldBoardState = aBoardState;
        aBoardState.entityDict = aBoardState.entityDict.SetItems(aEntityStateDict);
        aBoardState.UpdateBoardCellArray(oldBoardState, aEntityStateDict.Keys.ToHashSet());
        return aBoardState;
    }

    #endregion
}


