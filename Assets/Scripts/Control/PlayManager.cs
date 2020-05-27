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
    public GameObject debugLineMaster;

    public TimeStateEnum timeState;
    public SelectionStateEnum selectionState;
    public bool isPlaytest;

    public HashSet<EntityData> selectedEntitySet;
    public HashSet<EntityData> destroyOnNextFrame;
    public PlayPanelBase playPanelBase;

    public bool selectionPrimed;
    public EntityData clickedEntityData;
    public bool selectionDragIsUp;

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
                if (this.clickedEntityData != null) {
                    if (GM.inputManager.dragOffset.y > Constants.DRAGTHRESHOLD) {
                        bool isDragUp = true;
                        if (IsEntitySelectable(this.clickedEntityData, isDragUp)) {
                            SelectEntity(this.clickedEntityData, isDragUp);
                            this.clickedEntityData = null;
                        }
                    } else if (GM.inputManager.dragOffset.y < Constants.DRAGTHRESHOLD * -1) {
                        bool isDragUp = false;
                        if (IsEntitySelectable(this.clickedEntityData, isDragUp)) {
                            SelectEntity(this.clickedEntityData, isDragUp);
                            this.clickedEntityData = null;
                        }
                    }
                }
                if (this.selectedEntitySet.Count > 0) {
                    foreach (EntityData entityData in this.selectedEntitySet) {
                        entityData.entityBase.SetViewPosition(entityData.pos + GM.inputManager.dragOffsetV2);
                    }
                }
                break;
            case MouseStateEnum.RELEASED:
                this.clickedEntityData = null;
                if (this.selectedEntitySet.Count > 0) {
                    if (CanPlace(GM.inputManager.dragOffsetV2)) {
                        // place blocks
                        foreach (EntityData entityData in this.selectedEntitySet) {
                            GM.boardManager.MoveEntityAndView(entityData.pos + GM.inputManager.dragOffsetV2, entityData);
                        }
                    } else {
                        foreach (EntityData entityData in this.selectedEntitySet) {
                            entityData.entityBase.ResetViewPosition();
                        }
                    }
                }
                this.clickedEntityData = null;
                this.selectedEntitySet.Clear();
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

    public bool CanPlace(Vector2Int aOffset) {
        Debug.Assert(this.selectedEntitySet.Count > 0);
        foreach (EntityData entity in this.selectedEntitySet) {
            if (!GM.boardData.IsRectEmpty(entity.pos + aOffset, entity.size, this.selectedEntitySet)) {
                return false;
            }
        }
        return true;
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
                if (connectedEntity.isFixed || connectedEntity.isBoundary) {
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
        GM.cursorBase.SetHolding(aEntityData);
    }

    public HashSet<EntityData> GetSelectSet(EntityData aRoot, bool aIsUp) {
        HashSet<EntityData> selectSet = new HashSet<EntityData>();
        HashSet<EntityData> mainTree = GetConnectedTree(aRoot, aIsUp);
        HashSet<EntityData> connectedTree = new HashSet<EntityData>();
        foreach (EntityData entityData in mainTree) {
            Util.DebugAreaPulse(entityData.pos, entityData.size, Color.red);
            HashSet<EntityData> connected = GetConnected(entityData, !aIsUp, mainTree);
            foreach (EntityData hanger in connected) {
                if (!IsEntityConnectedToFixed(hanger, mainTree)) {
                    // Util.DebugAreaPulse(hanger.pos, hanger.size, Color.blue);
                    HashSet<EntityData> hangerTree = GetAllConnected(hanger, mainTree);
                    hangerTree.Add(hanger);
                    foreach(EntityData hangerConnected in hangerTree) {
                        Util.DebugAreaPulse(hangerConnected.pos, hangerConnected.size, Color.blue);
                    }
                    connectedTree.UnionWith(hangerTree);
                } else {
                    Util.DebugAreaPulse(hanger.pos, hanger.size, Color.yellow);
                }
            }
        }
        selectSet.UnionWith(mainTree);
        selectSet.UnionWith(connectedTree);
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

    public HashSet<EntityData> GetConnected(EntityData aRoot, bool aIsUp, HashSet<EntityData> aIgnoreSet = null) {
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
                            if (aIgnoreSet != null) {
                                if (!aIgnoreSet.Contains(maybeAEntity)) {
                                    connectedEntitySet.Add(maybeAEntity);
                                }
                            } else {
                                connectedEntitySet.Add(maybeAEntity);
                            }
                        }
                    }
                }
            }
            return connectedEntitySet;
        } else {
            throw new System.Exception("root entity doesn't have an INodal");
        }
    }

    public HashSet<EntityData> GetConnected(EntityData aRoot, HashSet<EntityData> aIgnoreSet = null) {
        HashSet<EntityData> connectedAboveSet = GetConnected(aRoot, true, aIgnoreSet);
        HashSet<EntityData> connectedBelowSet = GetConnected(aRoot, false, aIgnoreSet);        
        connectedAboveSet.UnionWith(connectedBelowSet);
        return connectedAboveSet;
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

    public HashSet<EntityData> GetAllConnected(EntityData aRoot, HashSet<EntityData> aIgnoreSet = null) {
        if (aIgnoreSet == null) {
            aIgnoreSet = new HashSet<EntityData>();
        }
        HashSet<EntityData> allConnectedSet = new HashSet<EntityData>();
        HashSet<EntityData> ignoreSetClone = new HashSet<EntityData>(aIgnoreSet);
        GetAllConnectedRecursive(aRoot, ignoreSetClone);
        return allConnectedSet;

        void GetAllConnectedRecursive(EntityData rRoot, HashSet<EntityData> rIgnoreSet) {
            rIgnoreSet.Add(rRoot);
            allConnectedSet.Add(rRoot);
            foreach (EntityData connected in GetConnected(rRoot, rIgnoreSet)) {
                if (!rIgnoreSet.Contains(connected)) {
                    GetAllConnectedRecursive(connected, rIgnoreSet);
                }
            }
        }
    }

    public bool IsEntityConnectedToFixed(EntityData aRoot, HashSet<EntityData> aIgnoreSet = null) {
        bool isEntityConnectedToFixed = false;
        if (aIgnoreSet == null) {
            aIgnoreSet = new HashSet<EntityData>();
        }
        HashSet<EntityData> ignoreSetClone = new HashSet<EntityData>(aIgnoreSet);
        IsEntityConnectedToFixedRecursive(aRoot, ignoreSetClone);
        return isEntityConnectedToFixed;

        void IsEntityConnectedToFixedRecursive(EntityData rRoot, HashSet<EntityData> rIgnoreSet) {
            rIgnoreSet.Add(rRoot);
            print("IsEntityConnectedToFixedRecursive - added " + rRoot.name + " to IgnoreSet with size of " + rIgnoreSet.Count);
            if (rRoot.isFixed) {
                isEntityConnectedToFixed = true;
                Util.DebugAreaPulse(rRoot.pos, rRoot.size, Color.red);
                print("IsEntityConnectedToFixedRecursive - returning because is connected to root");
                return;
            } else {
                foreach (EntityData connected in GetConnected(rRoot, rIgnoreSet)) {
                    if (!rIgnoreSet.Contains(connected)) {
                        DebugDrawArrow(rRoot, connected);
                        IsEntityConnectedToFixedRecursive(connected, rIgnoreSet);
                    }
                }
            }
        }
    }

    public void DebugDrawArrow(EntityData aOriginEntity, EntityData aEndEntity) {
        Vector3 zOffset = new Vector3(0, 0, -1.01f);
        Vector3 startPos = Util.V2IOffsetV3(aOriginEntity.pos, aOriginEntity.size) + zOffset;
        Vector3 endPos = Util.V2IOffsetV3(aEndEntity.pos, aEndEntity.size) + zOffset;
        GameObject lineObject = Instantiate(debugLineMaster, Vector3.zero, Quaternion.identity);
        LineRenderer lineRenderer = lineObject.GetComponent<LineRenderer>();
        lineRenderer.SetPosition(0, startPos);
        lineRenderer.SetPosition(1, endPos);
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
