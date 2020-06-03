using System;
using System.Collections;
using System.Collections.Generic;
using System.Timers;
using UnityEngine;

using Sirenix.OdinInspector;
using UnityEditor;

public class DebugPanel : SerializedMonoBehaviour {
    public void EditorStressTest() {
        BoardManager boardManager = GM.boardManager;
        EntitySchema smallBoy =
            AssetDatabase.LoadAssetAtPath<EntitySchema>(
                "Assets/Resources/ScriptableObjects/Entities/Blocks/1x1 block.asset");
        for (int x = 1; x < boardManager.currentState.size.x - 1; x++) {
            for (int y = 1; y < boardManager.currentState.size.y - 1; y++) {
                boardManager.AddEntity(smallBoy, new Vector2Int(x, y), Constants.DEFAULTFACING, Constants.DEFAULTCOLOR);
                
            }
        }
    }

}
