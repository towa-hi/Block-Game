using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

public class PlayManager : SerializedMonoBehaviour {
    public TimeStateEnum timeState;
    public SelectionStateEnum selectionState;

    public HashSet<EntityData> selectedEntitySet;
    public EntityData clickedEntityData;
    
    public HashSet<EntityData> destroyOnNextFrame;

    public void Init() {
        this.selectionState = SelectionStateEnum.UNSELECTED;
        this.timeState = TimeStateEnum.NORMAL;
        this.selectedEntitySet = new HashSet<EntityData>();
        this.destroyOnNextFrame = new HashSet<EntityData>();
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
                    entityData.entityBase.DoFrame();
                }
                break;
            case TimeStateEnum.PAUSED:
                break;
        }

        foreach (EntityData deadEntity in this.destroyOnNextFrame) {
            GM.boardManager.DestroyEntity(deadEntity);
        }
        this.destroyOnNextFrame.Clear();
    }

    public void BeginEntityDeath(EntityData aEntityData) {
        GM.boardData.BanishEntity(aEntityData);
        aEntityData.Die();
    }    

    public void FinishEntityDeath(EntityData aEntityData) {
        this.destroyOnNextFrame.Add(aEntityData);
    }


}
