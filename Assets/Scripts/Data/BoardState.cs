
using System;
using System.Collections.Generic;
using UnityEngine;

public struct BoardState {
    public bool isInitialized;
    public Dictionary<int, EntityState> entityDict;
    public string title;
    public string creator;
    public int par;
    public Vector2Int size;
    public int attempts;
    public int currentId;

    public static BoardState GenerateBlankBoard() {
        BoardState newBoard = new BoardState {
            isInitialized = true,
            entityDict = new Dictionary<int, EntityState>(),
            title = "Uninitialized Board",
            creator = Config.USERNAME,
            par = 5,
            size = new Vector2Int(40, 24),
            attempts = 0,
        };
        return newBoard;
    }

    public bool CanMoveEntity(EntityState aEntityState, Vector2Int aPos) {
        return true;
    }

    public static Tuple<BoardState, EntityState> AddEntity(BoardState aBoardState, EntityState aEntityState) {
        int id = aBoardState.currentId;
        // id always set here
        aEntityState.data.id = id;
        aEntityState.data.name = GenerateName(aEntityState.data.name);
        // NEVER CHANGE CURRENTID OUTSIDE OF THIS FUNCTION!!!
        aBoardState.currentId += 1;
        aBoardState.entityDict[id] = aEntityState;
        return new Tuple<BoardState, EntityState>(aBoardState, aEntityState);

        string GenerateName(string aEntitySchemaName) {
            string nameString = aEntitySchemaName + " ";
            if (aEntityState.data.isBoundary) {
                nameString += "(boundary) ";
            }
            nameString += "id: " + id;
            return nameString;
        }
    }
    
    public static BoardState RemoveEntity(BoardState aBoardState, int aId) {
        aBoardState.entityDict.Remove(aId);
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
        aBoardState.entityDict[aEntityState.data.id] = aEntityState;
        return aBoardState;
    }
    // public static BoardState SetPar(BoardState aBoardState, int aPar) {

    // }
}


