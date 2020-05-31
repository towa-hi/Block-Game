using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Sirenix.OdinInspector;

public struct BoardState {
    public Dictionary<int, EntityState> entityDict;
    public string title;
    public string creator;
    public int par;
    public Vector2Int size;
    public int attempts;
    public int currentId;

    public static BoardState GenerateBlankBoard() {
        BoardState newBoard = new BoardState();
        newBoard.entityDict = new Dictionary<int, EntityState>();
        newBoard.title = "Uninitialized Board";
        newBoard.creator = Config.USERNAME;
        newBoard.par = 5;
        newBoard.size = new Vector2Int(40, 24);
        newBoard.attempts = 0;
        return newBoard;
    }

    public bool CanMoveEntity(EntityState aEntityState, Vector2Int aPos) {
        return true;
    }

    public static Tuple<BoardState, EntityState> AddEntity(BoardState aBoardState, EntityState aEntityState) {
        int id = aBoardState.currentId;
        // id always set here
        aEntityState.id = id;
        // never change currentId outside of here
        aBoardState.currentId += 1;
        aBoardState.entityDict[id] = aEntityState;
        return new Tuple<BoardState, EntityState>(aBoardState, aEntityState);
    }
    
    public static BoardState RemoveEntity(BoardState aBoardState, int aId) {
        aBoardState.entityDict.Remove(aId);

        return aBoardState;

    }
    
    public static BoardState SetTitle(BoardState aBoardState, string aTitle) {
        aBoardState.title = aTitle;
        return aBoardState;
    }

    public static BoardState UpdateEntity(BoardState aBoardState, EntityState aEntityState) {
        aBoardState.entityDict[aEntityState.id] = aEntityState;
        return aBoardState;
    }
    // public static BoardState SetPar(BoardState aBoardState, int aPar) {

    // }
}


