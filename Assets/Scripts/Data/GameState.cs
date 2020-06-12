using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

public struct GameState {
    public GameModeEnum gameMode;
    public bool isFullPaused;

    public static GameState SetGameMode(GameState aGameState, GameModeEnum aGameMode) {
        aGameState.gameMode = aGameMode;
        return aGameState;
    }

    public static GameState SetIsFullPaused(GameState aGameState, bool aIsFullPaused) {
        aGameState.isFullPaused = aIsFullPaused;
        return aGameState;
    }

    public static GameState CreateGameState(GameModeEnum aGameMode) {
        GameState gameState = new GameState {
            gameMode = aGameMode,
            isFullPaused = false,
        };
        return gameState;
    }
}
