
using System;
using System.Collections.Generic;
using UnityEngine;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.Serialization;
using JetBrains.Annotations;

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
            size = new Vector2Int(40, 24),
            attempts = 0,
        };
        return newBoard;
    }

    public static BoardState GetClone(BoardState aBoardState) {

        return aBoardState;
    }

    public bool CanMoveEntity(EntityState aEntityState, Vector2Int aPos) {
        return true;
    }

    public static Tuple<BoardState, EntityState> AddEntity(BoardState aBoardState, EntityState aEntityState) {
        int id = aBoardState.currentId;
        // id always set here
        aEntityState.Init(id);
        // NEVER CHANGE CURRENTID OUTSIDE OF THIS FUNCTION!!!
        aBoardState.currentId += 1;
        aBoardState.entityDict = aBoardState.entityDict.SetItem(id, aEntityState);
        // aBoardState.entityDict[id] = aEntityState;


        return new Tuple<BoardState, EntityState>(aBoardState, aEntityState);
    }
    
    public static BoardState RemoveEntity(BoardState aBoardState, int aId) {
        aBoardState.entityDict = aBoardState.entityDict.Remove(aId);
        Debug.Log("removed Entity");
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
        aBoardState.entityDict = aBoardState.entityDict.SetItem(aEntityState.id, aEntityState);
        return aBoardState;
    }

    public static BoardState UpdateEntityBatch(BoardState aBoardState, Dictionary<int, EntityState> aEntityStateDict) {
        aBoardState.entityDict = aBoardState.entityDict.SetItems(aEntityStateDict);
        return aBoardState;
    }
}


