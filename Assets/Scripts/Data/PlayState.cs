using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using UnityEngine;
using Sirenix.OdinInspector;

public struct PlayState {
    public bool isInitialized;
    public int? heldEntityId;
    [CanBeNull] public HashSet<int> selectedEntityIdSet;
    public PlayModeEnum playMode;
    public TimeModeEnum timeMode;
    public int moves;
    
    public static PlayState CreatePlayState() {
        PlayState newPlayState = new PlayState {
            isInitialized = true,
            heldEntityId = Constants.PLACEHOLDERINT,
            selectedEntityIdSet = null,
            playMode = PlayModeEnum.INITIALIZATION,
            timeMode = TimeModeEnum.PAUSED,
            moves = 0,
        };
        return newPlayState;
    }

    public static PlayState SetHeldEntity(PlayState aPlayState, int? aHeldEntityId) {
        aPlayState.heldEntityId = aHeldEntityId;
        return aPlayState;
    }

    public static PlayState SetSelectedEntityIdSet(PlayState aPlayState, [CanBeNull] HashSet<int> aSelectedEntityIdSet) {
        aPlayState.selectedEntityIdSet = aSelectedEntityIdSet;
        return aPlayState;
    }

    public static PlayState SetPlayMode(PlayState aPlayState, PlayModeEnum aPlayMode) {
        aPlayState.playMode = aPlayMode;
        return aPlayState;
    }

    public static PlayState SetTimeMode(PlayState aPlayState, TimeModeEnum aTimeMode) {
        aPlayState.timeMode = aTimeMode;
        return aPlayState;
    }

    public static PlayState SetMoves(PlayState aPlayState, int aMoves) {
        aPlayState.moves = aMoves;
        return aPlayState;
    }
    
}
