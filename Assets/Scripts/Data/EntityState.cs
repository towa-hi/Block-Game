using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.Utilities;
using UnityEngine;

public struct EntityImmutableData {
    public int id;
    public bool isFront;
    public string name;
    public Vector2Int size;
    public EntityTypeEnum entityType;
    public bool isBoundary;
    public string prefabPath;
}

public struct MobData {
    public MoveTypeEnum movementType;
    public bool canHop;
    public float moveSpeed;
    public bool canKillOnTouch;
    public int touchPower;
    public bool canFall;
    public bool canKillOnFall;
    public int fallPower;
    public bool canPush;
    public bool canBePushed;
    public bool canBeLifted;
}

public readonly struct Node {
    public readonly bool isUp;
    public readonly int id;
    public readonly Vector2Int relativePos;
    public EntityState entityState {
        get {
            return GM.boardManager.GetEntityById(this.id);
        }
    }
    public Vector2Int absolutePos {
        get {
            return this.entityState.pos + this.relativePos;
        }
    }
    public Vector2Int oppositeNodePos {
        get {
            return this.absolutePos + Util.UpOrDown(this.isUp);
        }
    }
    public Node? oppositeNode {
        get {
            EntityState? oppositeEntity = GM.boardManager.GetEntityAtPos(this.oppositeNodePos);
            if (oppositeEntity.HasValue && oppositeEntity.Value.hasNodes) {
                return oppositeEntity.Value.GetNodeByAbsPos(!this.isUp, this.oppositeNodePos);
            }
            return null;
        }
    }

    public Node(bool aIsUp, int aId, Vector2Int aRelativePos) {
        this.relativePos = aRelativePos;
        this.id = aId;
        this.isUp = aIsUp;
    }
    
    public Node? GetOppositeNode(Vector2Int aOffset, HashSet<EntityState> aIgnoreList = null) {
        Debug.Log("node for id: " + this.id + " with relative pos:" + this.relativePos + "GetOppositeNode at:" + (this.oppositeNodePos + aOffset));
        EntityState? oppositeEntity = GM.boardManager.GetEntityAtPos(this.oppositeNodePos + aOffset);
        if (oppositeEntity.HasValue && oppositeEntity.Value.hasNodes) {
            Debug.Log("found node");
            if (aIgnoreList != null) {
                if (!aIgnoreList.Contains(oppositeEntity.Value)) {
                    Debug.Log("returning node with id: " + oppositeEntity.Value.data.id);
                    return oppositeEntity.Value.GetNodeByAbsPos(!this.isUp, this.oppositeNodePos + aOffset);
                }
                else {
                    Debug.Log("node is part of ignore list with id: " + oppositeEntity.Value.data.id);
                }
            }
            else {
                return oppositeEntity.Value.GetNodeByAbsPos(!this.isUp, this.oppositeNodePos + aOffset);
            }
        }
        Debug.Log("did not find node");
        return null;
    }
}

public struct EntityState {
    public bool isInitialized;
    public EntityImmutableData data;
    public MobData? mobData;
    
    public Vector2Int pos;
    public Vector2Int facing;
    public Color defaultColor;
    public bool isFixed;
    public TeamEnum team;
    public bool hasNodes;
    
    public HashSet<Vector2Int> upNodes;
    public HashSet<Vector2Int> downNodes;
    // TODO: convert all this shit to use node structs
    public HashSet<Node> nodeSet;
    
    public int touchDefense;
    public int fallDefense;

    public EntityBase entityBase {
        get {
            return GM.boardManager.GetEntityBaseById(this.data.id);
        }
    }

    public static EntityState CreateEntityState(EntitySchema aEntitySchema, Vector2Int aPos, Vector2Int aFacing, Color aDefaultColor, bool aIsFixed = false, bool aIsBoundary = false) {
        EntityState newEntityState = new EntityState();
        // set immutable values
        newEntityState.data = new EntityImmutableData {
            id = Constants.PLACEHOLDERINT,
            isFront = aEntitySchema.isFront,
            name = aEntitySchema.name,
            size = aEntitySchema.size,
            entityType = aEntitySchema.entityType,
            isBoundary = aIsBoundary,
            prefabPath = aEntitySchema.prefabPath,
        };
        if (aEntitySchema.entityType == EntityTypeEnum.MOB) {
            newEntityState.mobData = new MobData {
                movementType = aEntitySchema.movementType,
                canHop = aEntitySchema.canHop,
                moveSpeed = aEntitySchema.moveSpeed,
                canKillOnTouch = aEntitySchema.canKillOnTouch,
                touchPower = aEntitySchema.touchPower,
                canFall = aEntitySchema.canFall,
                canKillOnFall = aEntitySchema.canKillOnFall,
                fallPower = aEntitySchema.fallPower,
                canPush = aEntitySchema.canPush,
                canBePushed = aEntitySchema.canBePushed,
                canBeLifted = aEntitySchema.canBeLifted,
            };
        }
        else {
            newEntityState.mobData = null;
        }
        newEntityState.pos = aPos;
        newEntityState.facing = aFacing;
        newEntityState.defaultColor = aDefaultColor;
        newEntityState.isFixed = aIsFixed;
        newEntityState.team = aEntitySchema.defaultTeam;
        newEntityState.hasNodes = aEntitySchema.hasNodes;
        
        newEntityState.nodeSet = new HashSet<Node>();
        newEntityState.upNodes = new HashSet<Vector2Int>();
        newEntityState.downNodes = new HashSet<Vector2Int>();
        newEntityState.touchDefense = aEntitySchema.touchDefense;
        newEntityState.fallDefense = aEntitySchema.fallDefense;

        return newEntityState;
    }

    // called only by BoardState.AddEntity
    public void Init(int aId) {
        Debug.Assert(!this.isInitialized);
        this.isInitialized = true;
        this.data.id = aId;
        this.data.name = GenerateName(this.data.name, this.data.isBoundary);
        
        string GenerateName(string aEntitySchemaName, bool aIsBoundary) {
            string nameString = aEntitySchemaName + " ";
            if (aIsBoundary) {
                nameString += "(boundary) ";
            }
            nameString += "id: " + aId;
            return nameString;
        }
        
        var newNodeSet = new HashSet<Node>();
        if (this.hasNodes) {
            HashSet<Vector2Int> newUpNodes = new HashSet<Vector2Int>();
            HashSet<Vector2Int> newDownNodes = new HashSet<Vector2Int>();
            bool hasUpNodes = true;
            bool hasDownNodes = true;
            if (this.data.isBoundary) {
                if (this.pos.y + this.data.size.y == GM.boardManager.currentState.size.y) {
                    hasUpNodes = false;
                }
                if (this.pos.y == 0) {
                    hasDownNodes = false;
                }
            }
            for (int x = 0; x < this.data.size.x; x++) {
                if (hasUpNodes) {
                    Vector2Int topPos = new Vector2Int(x, this.data.size.y - 1);
                    newUpNodes.Add(topPos);
                    newNodeSet.Add(new Node(true, this.data.id, topPos));
                }

                if (hasDownNodes) {
                    Vector2Int botPos = new Vector2Int(x, 0);
                    newDownNodes.Add(botPos);
                    newNodeSet.Add(new Node(false, this.data.id, botPos));
                }
            }
            this.nodeSet = newNodeSet;
            this.upNodes = newUpNodes;
            this.downNodes = newDownNodes;
        }
    }
    
    public static EntityState SetPos(EntityState aEntityState, Vector2Int aPos) {
        aEntityState.pos = aPos;
        return aEntityState;
    }

    public static EntityState SetFacing(EntityState aEntityState, Vector2Int aFacing) {
        if (Util.IsDirection(aFacing)) {
            aEntityState.facing = aFacing;
        }
        return aEntityState;
    }

    public static EntityState SetDefaultColor(EntityState aEntityState, Color aColor) {
        aEntityState.defaultColor = aColor;
        return aEntityState;
    }

    public static EntityState SetIsFixed(EntityState aEntityState, bool aIsFixed) {
        aEntityState.isFixed = aIsFixed;
        return aEntityState;
    }

    public static EntityState SetTeam(EntityState aEntityState, TeamEnum aTeam) {
        aEntityState.team = aTeam;
        return aEntityState;
    }

    public static EntityState SetNodes(EntityState aEntityState, HashSet<Vector2Int> aUpNodes, HashSet<Vector2Int> aDownNodes) {
        Debug.Assert(aUpNodes != null);
        Debug.Assert(aDownNodes != null);
        aEntityState.upNodes = aUpNodes;
        aEntityState.downNodes = aDownNodes;
        return aEntityState;
    }

    public static EntityState SetTouchDefense(EntityState aEntityState, int aTouchDefense) {
        if (0 <= aTouchDefense && aTouchDefense <= 999) {
            aEntityState.touchDefense = aTouchDefense;
            return aEntityState;
        } else {
            throw new System.Exception("tried to set invalid touchDefense");
        }
    }

    public static EntityState SetFallDefense(EntityState aEntityState, int aFallDefense) {
        if (0 <= aFallDefense && aFallDefense <= 999) {
            aEntityState.fallDefense = aFallDefense;
            return aEntityState;
        } else {
            throw new System.Exception("tried to set invalid fallDefense");
        }
    }

    public HashSet<Vector2Int> GetRelativeNodePosSet(bool aIsUp) {
        Debug.Assert(this.hasNodes);
        return aIsUp ? this.upNodes : this.downNodes;
    }
    
    public HashSet<Vector2Int> GetAbsoluteNodePosSet(bool aIsUp) {
        Debug.Assert(this.hasNodes);
        HashSet<Vector2Int> absoluteNodePosSet = new HashSet<Vector2Int>();
        HashSet<Vector2Int> relativeNodePosSet = GetRelativeNodePosSet(aIsUp);
        foreach (Vector2Int relativeNodePos in relativeNodePosSet) {
            Vector2Int absoluteNodePos = this.pos + relativeNodePos;
            absoluteNodePosSet.Add(absoluteNodePos);
        }
        return absoluteNodePosSet;
    }

    public Node GetNodeByAbsPos(bool aIsUp, Vector2Int aAbsPos) {
        foreach (Node node in this.nodeSet) {
            if (node.isUp == aIsUp && node.absolutePos == aAbsPos) {
                return node;
            }
        }
        throw new Exception("invalid absPos");
    }

    public HashSet<Node> GetNodes(bool aIsUp) {
        return this.nodeSet.Where(node => node.isUp == aIsUp).ToHashSet();
    }
}


