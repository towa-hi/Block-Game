using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

public readonly struct BoardStateImmutable {
    public readonly bool isInitialized;
    public readonly Dictionary<int, EntityState> entityDict;
    public readonly string title;
    public readonly string creator;
    public readonly int par;
    public readonly Vector2Int size;
    public readonly int attempts;
    [SerializeField] readonly int currentId;

    public static BoardStateImmutable GenerateBlankBoard() {
        BoardStateImmutable newBoard = new BoardStateImmutable();
        return newBoard;
    }

    public BoardStateImmutable(bool args = false) {
        this.isInitialized = true;
        this.entityDict = new Dictionary<int, EntityState>();
        this.title = "Uninitialized Board";
        this.creator = Config.USERNAME;
        this.par = 5;
        this.size = Constants.MAXBOARDSIZE;
        this.attempts = 0;
        this.currentId = 0;
    }

    public BoardStateImmutable(BoardStateImmutable aBoardState, Dictionary<int, EntityState> aEntityDict) {
        this.isInitialized = aBoardState.isInitialized;
        this.entityDict = aEntityDict;
        this.title = aBoardState.title;
        this.creator = aBoardState.creator;
        this.par = aBoardState.par;
        this.size = aBoardState.size;
        this.attempts = aBoardState.attempts;
        this.currentId = aBoardState.currentId;
    }

    public static Tuple<BoardStateImmutable, EntityState> AddEntity(BoardStateImmutable aBoardState, EntityState aEntityState) {
        int id = aBoardState.currentId;
        aEntityState.Init(id);
        Dictionary<int, EntityState> newEntityDict = new Dictionary<int, EntityState>(aBoardState.entityDict);
        newEntityDict[id] = aEntityState;
        return new Tuple<BoardStateImmutable, EntityState>(new BoardStateImmutable(aBoardState, newEntityDict), aEntityState);
    }
}
