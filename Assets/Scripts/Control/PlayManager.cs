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

    public void KillEntities(HashSet<EntityData> aEntitiesToKill) {
        foreach (EntityData entityToKill in aEntitiesToKill) {
            BeginEntityDeath(entityToKill);
        }
    }

    public bool EntityFanCheck(EntityData aEntityData) {
        for (int x = aEntityData.pos.x; x < aEntityData.size.x; x++) {
            for (int y = aEntityData.pos.y; y >= 0; y++) {
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
    public HashSet<EntityData> BumpCheck(Vector2Int aPos, EntityData aEntityData) {
        bool isBlocked = false;
        HashSet<EntityData> maybeEntitiesToKill = new HashSet<EntityData>();
        if (GM.boardData.IsRectInBoard(aPos, aEntityData.size)) {
            HashSet<EntityData> entitySet = GM.boardData.SetOfEntitiesInRect(aPos, aEntityData.size);
            // foreach touched entity in the place i'm about to move to
            foreach (EntityData touchedEntityData in entitySet) {
                // if touched entity not me
                if (touchedEntityData != aEntityData) {
                    // if i can kill touched entity
                    if (aEntityData.touchAttack > touchedEntityData.touchDefense) {
                        maybeEntitiesToKill.Add(touchedEntityData);
                    } else {
                        // i'm blocked
                        isBlocked = true;
                    }
                }
            }
            if (isBlocked != true) {
                // check if ground exists after all this while ignoring everything im about to kill
                for (int x = aPos.x; x < aPos.x + aEntityData.size.x; x++) {
                    Vector2Int currentPos = new Vector2Int(x, aPos.y - 1);
                    Util.DebugAreaPulse(currentPos, new Vector2Int(1,1), Color.cyan);
                    EntityData currentEntity = GM.boardData.GetEntityDataAtPos(currentPos);
                    // if a entity exists in floor location
                    if (currentEntity != null) {
                        // if entity is not self
                        if (currentEntity != aEntityData) {
                            // if entity is not about to be killed
                            if (!maybeEntitiesToKill.Contains(currentEntity)) {
                                return maybeEntitiesToKill;
                            }
                        }
                    }
                }
            }
        }
        return null;
    }
}
