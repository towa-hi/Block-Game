using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

[RequireComponent(typeof(BoardManager))]
public class PlayManager : SerializedMonoBehaviour {

    void OnEnable() {
        
    }
    
    public void OnUpdateGameState(GameState aGameState) {
        if (aGameState.gameMode == GameModeEnum.PLAYING) {
            // turn self on
        }
    }
}
