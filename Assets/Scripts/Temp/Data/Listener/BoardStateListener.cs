using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

public abstract class BoardStateListener : SerializedMonoBehaviour {
    public virtual void OnEnable() {
        GM.boardManager.OnUpdateBoardState += OnUpdateBoardState;
        OnUpdateBoardState(GM.boardManager.boardState);
    }

    public virtual void OnDisable() {
        GM.boardManager.OnUpdateBoardState -= OnUpdateBoardState;
    }

    public abstract void OnUpdateBoardState(BoardState aBoardState);
}
