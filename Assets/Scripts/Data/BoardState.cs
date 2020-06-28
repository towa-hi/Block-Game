
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

    public BoardCell[,] boardCellArray;

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
        return aBoardState;
    }

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
        newBoard.boardCellArray = InitBoardCellArray(newBoard.size);
        return newBoard;
    }

    public BoardCell GetBoardCellAtPos(Vector2Int aPos) {
        if (!IsPosOnBoard(aPos)) {
            throw new ArgumentOutOfRangeException(nameof(aPos),"tried to get boardCell out of bounds");
        }
        return this.boardCellArray[aPos.x, aPos.y];
    }

    public int? GetEntityIdAtPos(Vector2Int aPos, bool aIsFront = true) {
        if (!IsPosOnBoard(aPos)) {
            throw new ArgumentOutOfRangeException(nameof(aPos),"tried to get boardCell out of bounds");
        }
        if (aIsFront) {
            return this.boardCellArray[aPos.x, aPos.y].frontEntityId;
        }
        else {
            return this.boardCellArray[aPos.x, aPos.y].backEntityId;
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

    void SetBoardCell(Vector2Int aPos, int? aId, bool aIsFront = true) {
        BoardCell boardCell = this.boardCellArray[aPos.x, aPos.y];
        if (aIsFront) {
            boardCell.frontEntityId = aId;
        }
        else {
            boardCell.backEntityId = aId;
        }
        this.boardCellArray[aPos.x, aPos.y] = boardCell;
    }

    void ClearBoardCell(Vector2Int aPos) {
        BoardCell boardCell = this.boardCellArray[aPos.x, aPos.y];
        boardCell.frontEntityId = null;
        boardCell.backEntityId = null;
        this.boardCellArray[aPos.x, aPos.y] = boardCell;
    }

    void UpdateBoardCellArray() {
        for (int x = 0; x < this.size.x; x++) {
            for (int y = 0; y < this.size.y; y++) {
                ClearBoardCell(new Vector2Int(x, y));
            }
        }
        // for every entity set occupied cells to that entities id
        foreach (EntityState currentEntity in this.entityDict.Values) {
            foreach (Vector2Int currentPos in Util.V2IInRect(currentEntity.pos, currentEntity.size)) {
                SetBoardCell(currentPos, currentEntity.id, currentEntity.isFront);
            }
        }
    }

    void UpdateBoardCellArray(BoardState aOldBoardState, HashSet<int> aEntitiesToUpdate) {
        // clear every entity in partial updates occupied cells
        foreach (int currentId in aEntitiesToUpdate) {
            EntityState oldEntityState = aOldBoardState.entityDict[currentId];
            foreach (Vector2Int oldPos in Util.V2IInRect(oldEntityState.pos, oldEntityState.size)) {
                SetBoardCell(oldPos, null, oldEntityState.isFront);
            }
        }
        // reset every entity in partial update to new locations provided by aNewBoardState
        foreach (int currentId in aEntitiesToUpdate) {
            EntityState newEntityState = this.entityDict[currentId];
            foreach (Vector2Int newPos in Util.V2IInRect(newEntityState.pos, newEntityState.size)) {
                SetBoardCell(newPos, newEntityState.id, newEntityState.isFront);
            }
        }

    }

    public Dictionary<Vector2Int, BoardCell> GetBoardCellSlice(Vector2Int aOrigin, Vector2Int aSize) {
        if (!IsRectInBoard(aOrigin, aSize)) {
            throw new ArgumentOutOfRangeException("GetBoardGridSlice - rect not in board" + aOrigin + aSize);
        }
        Dictionary<Vector2Int, BoardCell> sliceDict = new Dictionary<Vector2Int, BoardCell>();
        foreach (Vector2Int currentPos in Util.V2IInRect(aOrigin, aSize)) {
            sliceDict[currentPos] = this.boardCellArray[currentPos.x, currentPos.y];
        }
        return sliceDict;
    }

    public bool IsRectEmpty(Vector2Int aOrigin, Vector2Int aSize, HashSet<int> aIgnoreSet = null, bool aIsFront = true) {
        foreach (BoardCell boardCell in GetBoardCellSlice(aOrigin, aSize).Values) {
            int? id = aIsFront ? boardCell.frontEntityId : boardCell.backEntityId;
            if (id.HasValue) {
                if (aIgnoreSet == null) {
                    return false;
                }
                else if (!aIgnoreSet.Contains(id.Value)) {
                    return false;
                }
            }
        }
        return true;
    }
    static BoardCell[,] InitBoardCellArray(Vector2Int aSize) {
        BoardCell[,] newBoardCellArray = new BoardCell[aSize.x, aSize.y];
        for (int x = 0; x < aSize.x; x++) {
            for (int y = 0; y < aSize.y; y++) {
                newBoardCellArray[x, y] = new BoardCell(new Vector2Int(x, y));
            }
        }
        return newBoardCellArray;
    }

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
        // foreach (int id in aEntityStateDict.Keys) {
        //     Debug.Log("id:" + id + "old pos:" + oldBoardState.entityDict[id].pos);
        // }
        // foreach (int id in aEntityStateDict.Keys) {
        //     Debug.Log("id:" + id + "destination pos:" + aEntityStateDict[id].pos);
        // }
        aBoardState.entityDict = aBoardState.entityDict.SetItems(aEntityStateDict);
        aBoardState.UpdateBoardCellArray(oldBoardState, aEntityStateDict.Keys.ToHashSet());
        // foreach (int id in aEntityStateDict.Keys) {
        //     Debug.Log("id:" + id + "new pos:" + aBoardState.entityDict[id].pos);
        // }
        return aBoardState;
    }
}


