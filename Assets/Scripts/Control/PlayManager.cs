using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

// public struct PlayState {
//     public int par;
//     public EntityData selectedEntityData;

//     public void Init() {
//         this.par = GM.boardData.par;
//         this.selectedEntityData = null;
//     }
// }

public class PlayManager : SerializedMonoBehaviour {
    public TimeStateEnum timeState;
    public SelectionStateEnum selectionState;

    public bool isPlaytest;

    public HashSet<EntityData> selectedEntitySet;
    public HashSet<EntityData> destroyOnNextFrame;
    public PlayPanelBase playPanelBase;

    public bool selectionPrimed;
    public EntityData clickedEntityData;
    public bool selectionDragIsUp;
    // public Vector2Int dragOffsetV2;
    public Vector2Int offsetV2;

    public void Init() {
        this.selectionState = SelectionStateEnum.UNSELECTED;
        this.timeState = TimeStateEnum.NORMAL;
        this.selectedEntitySet = new HashSet<EntityData>();
        this.destroyOnNextFrame = new HashSet<EntityData>();
        GM.cursorBase.PlayInit();
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
                EntityData maybeAEntity = GM.boardData.GetEntityDataAtPos(GM.inputManager.mousePosV2);
                if (maybeAEntity != null) {
                    this.selectionPrimed = true;
                    this.clickedEntityData = maybeAEntity;
                    
                }
                break;
            case MouseStateEnum.HELD:
                this.offsetV2 = GM.inputManager.mousePosV2 - GM.inputManager.clickedPosV2;
                if (this.selectionPrimed) {
                    if (GM.inputManager.dragOffset.y > Constants.DRAGTHRESHOLD) {
                        bool isDragUp = true;
                        if (IsEntitySelectable(this.clickedEntityData, isDragUp)) {
                            SelectEntity(this.clickedEntityData, isDragUp);
                            this.selectionPrimed = false;
                        }
                    } else if (GM.inputManager.dragOffset.y < Constants.DRAGTHRESHOLD * -1) {
                        bool isDragUp = false;
                        if (IsEntitySelectable(this.clickedEntityData, isDragUp)) {
                            SelectEntity(this.clickedEntityData, isDragUp);
                            this.selectionPrimed = false;
                        }
                    }
                }
                if (this.selectedEntitySet.Count > 0) {

                    foreach (EntityData entityData in this.selectedEntitySet) {
                        entityData.entityBase.SetViewPosition(entityData.pos + this.offsetV2);
                    }
                }
                break;
            case MouseStateEnum.RELEASED:
                this.selectionPrimed = false;
                if (this.selectedEntitySet.Count > 0) {
                    if (CanPlace(new Vector2Int(999, 999))) {
                        // place blocks
                    } else {
                        foreach (EntityData entityData in this.selectedEntitySet) {
                            entityData.entityBase.ResetViewPosition();
                        }
                    }
                }
                this.clickedEntityData = null;
                GM.cursorBase.SetSelecting();
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

    // TODO: write this stuff
    public bool CanPlace(Vector2Int aOffset) {
        return false;
    }
    public void BeginEntityDeath(EntityData aEntityData, DeathType aDeathType) {
        GM.boardData.BanishEntity(aEntityData);
        aEntityData.entityBase.Die(aDeathType);
    }    

    public void FinishEntityDeath(EntityData aEntityData) {
        this.destroyOnNextFrame.Add(aEntityData);
        if (aEntityData == GM.boardData.playerEntityData) {
            LoseBoard();
        }
    }

    public bool IsEntitySelectable(EntityData aEntityData, bool aIsUp) {
        if (aEntityData.isFixed || aEntityData.isBoundary) {
            return false;
        }
        INodal entityINodal = aEntityData.entityBase.GetCachedIComponent<INodal>() as INodal;
        if (entityINodal != null) {
            foreach (EntityData connectedEntity in GetConnectedTree(aEntityData, aIsUp)) {
                if (connectedEntity.isFixed) {
                    print("entity not selected cuz tree blocked in aIsUp: " + aIsUp + " by entity " + connectedEntity.name);
                    return false;
                }
            }
            return true;
        } else {
            print("entity not selected cuz does not have inodal");
            return false;
        }
    }
    public void SelectEntity(EntityData aEntityData, bool aIsUp) {
        print("entity selected");
        this.selectedEntitySet = GetSelectSet(aEntityData, aIsUp);
        foreach (EntityData entity in this.selectedEntitySet) {
            Util.DebugAreaPulse(entity.pos, entity.size, Color.red);
        }
        GM.cursorBase.SetHolding(aEntityData);
    }

    public HashSet<EntityData> GetSelectSet(EntityData aRoot, bool aIsUp) {
        HashSet<EntityData> selectSet = new HashSet<EntityData>();
        HashSet<EntityData> mainTree = GetConnectedTree(aRoot, aIsUp);
        selectSet.UnionWith(mainTree);
        return selectSet;
    }
    public void LoseBoard() {
        print("GAME OVER YEAH");
        // SetTimeState(TimeStateEnum.PAUSED);
    }

    public void WinBoard() {
        print("YOU WIN");
        // SetTimeState(TimeStateEnum.PAUSED);
    }

    public void SetTimeState(TimeStateEnum aTime) {
        this.timeState = aTime;
        switch (aTime) {
            case TimeStateEnum.PAUSED:
                Time.timeScale = 0;
                break;
            case TimeStateEnum.NORMAL:
                Time.timeScale = 1;
                break;
            case TimeStateEnum.DOUBLE:
                Time.timeScale = 2;
                break;
        }
    }

    public HashSet<EntityData> GetConnected(EntityData aRoot, bool aIsUp) {
        INodal rootINodal = aRoot.entityBase.GetCachedIComponent<INodal>() as INodal;
        if (rootINodal != null) {
            HashSet<EntityData> connectedEntitySet = new HashSet<EntityData>();
            // get the absolute pos of where to check above/below root
            foreach (Vector2Int absoluteNodePos in rootINodal.GetAbsoluteNodePosSet(aIsUp)) {
                Vector2Int currentPos = absoluteNodePos + Util.UpOrDown(aIsUp);
                EntityData maybeAEntity = GM.boardData.GetEntityDataAtPos(currentPos);
                // if entity exists at this pos
                if (maybeAEntity != null) {
                    // if entity has INodal
                    INodal maybeINodal = maybeAEntity.entityBase.GetCachedIComponent<INodal>() as INodal;
                    if (maybeINodal != null) {
                        if (maybeINodal.HasNodeOnAbsolutePosition(currentPos, !aIsUp)) {
                            connectedEntitySet.Add(maybeAEntity);
                        }
                    }
                }
            }
            return connectedEntitySet;
        } else {
            throw new System.Exception("root entity doesn't have an INodal");
        }
    }

    public HashSet<EntityData> GetConnectedTree(EntityData aRoot, bool aIsUp, HashSet<EntityData> aConnectedTreeSet = null) {
        INodal rootINodal = aRoot.entityBase.GetCachedIComponent<INodal>() as INodal;
        if (rootINodal != null) {
            // if this is the root of the tree
            if (aConnectedTreeSet == null) {
                aConnectedTreeSet = new HashSet<EntityData>();
                aConnectedTreeSet.Add(aRoot);
            }
            HashSet<EntityData> connectedToRoot = GetConnected(aRoot, aIsUp);
            // add every connected entity to set and then traverse up the tree
            foreach (EntityData entityConnectedToRoot in connectedToRoot) {
                aConnectedTreeSet.Add(entityConnectedToRoot);
                // if connected entity isn't fixed then traverse
                if (!entityConnectedToRoot.isFixed) {
                    GetConnectedTree(entityConnectedToRoot, aIsUp, aConnectedTreeSet);
                }
            }
            return aConnectedTreeSet;
        } else {
            throw new System.Exception("root entity doesn't have an INodal");
        }
    }
    // public bool EntityFanCheck(EntityData aEntityData) {
    //     for (int x = aEntityData.pos.x; x < aEntityData.pos.x + aEntityData.size.x; x++) {
    //         for (int y = aEntityData.pos.y; y >= 0; y--) {
    //             Vector2Int currentPos = new Vector2Int(x, y);
    //             EntityData maybeAEntity = GM.boardData.GetEntityDataAtPos(currentPos);
    //             if (maybeAEntity != null) {
    //                 IFan maybeAIFan = maybeAEntity.entityBase.GetCachedIComponent<IFan>() as IFan;
    //                 if (maybeAIFan != null) {
    //                     if (maybeAIFan.isOn) {
    //                         if (GM.boardData.EmptyStraightBetween(new Vector2Int(x, aEntityData.pos.y), currentPos)) {
    //                             return true;
    //                         }
    //                     }
    //                 }
    //             }
    //         }
    //     }
    //     return false;
    // }

    
}
