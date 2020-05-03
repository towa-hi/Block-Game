using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

// manages loading levels and game state
// is a dependency for a lot of components
// anything that actually changes game state beyond entity scope goes in here
public class BoardManager : Singleton<BoardManager> {
    // set by InitializeLevel
    public GameGrid levelGrid;
    public GridViewBase gridView;
    public List<EntityBase> entityList;
    // set by editor
    public EntityBase entityMaster;
    // TODO: set by editor until menus are done
    public LevelData levelData;

    void Start() {
        InitializeLevel(this.levelData);
    }

    void InitializeLevel(LevelData aLevelData) {
        this.levelGrid = new GameGrid(aLevelData.levelSchema.size);
        this.gridView.Init(this.levelGrid);
        foreach (EntityData entityData in aLevelData.levelSchema.entityList) {
            this.entityList.Add(CreateEntity(entityData));
        }
    }

    public EntityBase CreateEntity(EntityData aEntityData) {
        GameObject newEntityPrefab = Instantiate(aEntityData.entitySchema.entityObject, Util.V2IOffsetV3(aEntityData.pos, aEntityData.entitySchema.size), Quaternion.identity, this.transform);
        EntityBase newEntityBase = newEntityPrefab.GetComponent<EntityBase>();
        newEntityBase.Init(aEntityData);
        this.levelGrid.RegisterEntity(newEntityBase);
        return newEntityBase;
    }

    EntityData CreateEntityData(EntitySchema aEntitySchema, Vector2Int aPos, Vector2Int aFacing, Color aColor, bool aIsFixed = false) {
        return new EntityData(aEntitySchema, aPos, aFacing, aColor, aIsFixed);
    }

    public EntityBase GetHoveredEntity() {
        return levelGrid.GetEntityAtPos(InputManager.Instance.mousePosV2);
    }

    // check if this entity can be selected and isnt blocked by fixed entities somewhere
    public bool IsEntitySelectable(EntityBase aEntityBase, bool aIsDraggingUp) {
        // check if connected tree has any fixed entities
        if (aEntityBase.GetCachedIComponent<INodal>() != null) {
            foreach (EntityBase connectedEntity in GetConnectedTree(aEntityBase, aIsDraggingUp)) {
                if (connectedEntity.isFixed) {
                    return false;
                }
            }
        }
        return true;
    }

    public void MoveEntity(Vector2Int aPos, EntityBase aEntityBase) {
        print("BoardManager moving entity");
        this.levelGrid.MoveEntity(aPos, aEntityBase);
    }

    public HashSet<EntityBase> GetSelectSet(EntityBase aRoot, bool aIsUp) {
        print("Getting select set");
        HashSet<EntityBase> selectSet = new HashSet<EntityBase>();
        HashSet<EntityBase> mainTree = GetConnectedTree(aRoot, aIsUp);
        selectSet.UnionWith(mainTree);
        print("returned hashset" + selectSet.Count);
        return selectSet;
    }

    
    // get a set of entities that are connected to the root entity by up/down nodes
    public HashSet<EntityBase> GetConnected(EntityBase aRoot, bool aIsUp) {
        // check if root entity has INodal
        INodal rootINodal = aRoot.GetCachedIComponent<INodal>() as INodal;
        if (rootINodal != null) {
            HashSet<EntityBase> connectedEntitySet = new HashSet<EntityBase>();
            // get the absolute positions of where to check above/below root
            foreach (Vector2Int absoluteNodePos in rootINodal.GetAbsoluteNodePosSet(aIsUp)) {
                // get position to check by adding V2I(1, 0) or V2I(-1, 0) to absolute pos 
                Vector2Int checkPos = absoluteNodePos + Util.UpOrDown(aIsUp);
                EntityBase maybeAEntity = this.levelGrid.GetEntityAtPos(checkPos);
                // if entity exists at this pos
                if (maybeAEntity != null) {
                    // if entity has a INodal
                    INodal maybeAEntityINodal = maybeAEntity.GetCachedIComponent<INodal>() as INodal;
                    if (maybeAEntityINodal != null) {
                        // if entity's INodal component has the opposite direction
                        if (maybeAEntityINodal.HasNodeOnThisAbsolutePosition(checkPos, !aIsUp)) {
                            // a connection exists so add this entity to the connectedSet
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

    // returns a set of all the entities recursively connected to the root entity
    // inclodes root entity and any connected fixed entities but nothing past that
    public HashSet<EntityBase> GetConnectedTree(EntityBase aRoot, bool aIsUp, HashSet<EntityBase> aConnectedTreeSet = null) {
        // check if root entity has INodal
        INodal rootINodal = aRoot.GetCachedIComponent<INodal>() as INodal;
        if (rootINodal != null) {
            // if this is the root of the tree
            if (aConnectedTreeSet == null) {
                // add the root to the set
                aConnectedTreeSet = new HashSet<EntityBase>();
                aConnectedTreeSet.Add(aRoot);
            }
            HashSet<EntityBase> connectedToRoot = GetConnected(aRoot, aIsUp);
            // add every connected entity to the set and then traverse up the tree
            foreach (EntityBase entityConnectedToRoot in connectedToRoot) {
                aConnectedTreeSet.Add(entityConnectedToRoot);
                // if connected entity isn't fixed, traverse
                if (!entityConnectedToRoot.isFixed) {
                    GetConnectedTree(entityConnectedToRoot, aIsUp, aConnectedTreeSet);
                }
                
            }
            return aConnectedTreeSet;
        } else {
            throw new System.Exception("root entity doesn't have an INodal");
        }
    }
    
    
}
