﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

public class PlayManager : SerializedMonoBehaviour {
    public TimeStateEnum timeState;
    public SelectionStateEnum selectionState;

    public HashSet<EntityData> selectedEntitySet;
    public EntityData clickedEntityData;
    
    public void Init() {
        this.selectionState = SelectionStateEnum.UNSELECTED;
        this.timeState = TimeStateEnum.NORMAL;
        this.selectedEntitySet = new HashSet<EntityData>();
    }

    void Update() {
        switch (GM.inputManager.mouseState) {
            case MouseStateEnum.DEFAULT:
                break;
            case MouseStateEnum.CLICKED:
                this.clickedEntityData = GM.boardData.GetEntityDataAtPos(GM.inputManager.mousePosV2);
                break;
            case MouseStateEnum.HELD:
                break;
            case MouseStateEnum.RELEASED:
                this.clickedEntityData = null;
                break;
        }

        switch (this.timeState) {
            case TimeStateEnum.NORMAL:
                foreach (EntityData entityData in GM.boardData.entityDataSet) {
                    entityData.DoFrame();
                }
                break;
            case TimeStateEnum.PAUSED:
                break;
        }
    }
}