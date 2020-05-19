using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

public class PlayManager : SerializedMonoBehaviour {
    public TimeStateEnum timeState;
    public SelectionStateEnum selectionState;

    public bool isPlaytest;

    public HashSet<EntityData> selectedEntitySet;
    public EntityData clickedEntityData;
    public HashSet<EntityData> destroyOnNextFrame;
    public PlayPanelBase playPanelBase;

    public void Init() {
        this.selectionState = SelectionStateEnum.UNSELECTED;
        this.timeState = TimeStateEnum.NORMAL;
        this.selectedEntitySet = new HashSet<EntityData>();
        this.destroyOnNextFrame = new HashSet<EntityData>();
    }

    // called from GM
    public void SetPlaytest(bool aIsPlaytest) {
        this.isPlaytest = aIsPlaytest;
        this.playPanelBase.SetPlaytest(aIsPlaytest);
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

    public void BeginEntityDeath(EntityData aEntityData, DeathType aDeathType) {
        GM.boardData.BanishEntity(aEntityData);
        aEntityData.entityBase.Die(aDeathType);
    }    

    public void FinishEntityDeath(EntityData aEntityData) {
        this.destroyOnNextFrame.Add(aEntityData);
    }

    public bool EntityFanCheck(EntityData aEntityData) {
        for (int x = aEntityData.pos.x; x < aEntityData.pos.x + aEntityData.size.x; x++) {
            for (int y = aEntityData.pos.y; y >= 0; y--) {
                Vector2Int currentPos = new Vector2Int(x, y);
                EntityData maybeAEntity = GM.boardData.GetEntityDataAtPos(currentPos);
                if (maybeAEntity != null) {
                    IFan maybeAIFan = maybeAEntity.entityBase.GetCachedIComponent<IFan>() as IFan;
                    if (maybeAIFan != null) {
                        if (maybeAIFan.isOn) {
                            if (GM.boardData.EmptyStraightBetween(new Vector2Int(x, aEntityData.pos.y), currentPos)) {
                                return true;
                            }
                        }
                    }
                }
            }
        }
        return false;
    }

    
}
