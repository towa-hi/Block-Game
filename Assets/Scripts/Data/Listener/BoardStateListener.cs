using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

public abstract class BoardStateListener : SerializedMonoBehaviour {
    public virtual void OnEnable() {
        GM.boardManager.OnUpdateBoardState += OnUpdateBoardState;
        if (GM.boardManager.currentState.isInitialized) {
            OnUpdateBoardState(GM.boardManager.currentState);
        }
        else {
            print("BoardState was not initialized");
        }
    }

    public virtual void OnDisable() {
        GM.boardManager.OnUpdateBoardState -= OnUpdateBoardState;
    }

    protected abstract void OnUpdateBoardState(BoardState aBoardState);
}
