using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using Schema;
using Sirenix.Utilities;
using UnityEngine;

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

    public Node? GetOppositeNode(Vector2Int aOffset, HashSet<int> aIgnoreList = null) {
        // Debug.Log("node for id: " + this.id + " with relative pos:" + this.relativePos + "GetOppositeNode at:" + (this.oppositeNodePos + aOffset));
        // EntityState? oppositeEntity = GM.boardManager.GetEntityAtPos(this.oppositeNodePos + aOffset);
        int? oppositeId = GM.boardManager.currentState.GetBoardCellAtPos(this.oppositeNodePos + aOffset).frontEntityId;
        if (oppositeId.HasValue) {
            EntityState oppositeEntity = GM.boardManager.GetEntityById(oppositeId.Value);
            if (oppositeEntity.hasNodes) {
                // Debug.Log("found node");
                if (aIgnoreList != null) {
                    if (!aIgnoreList.Contains(oppositeId.Value)) {
                        // Debug.Log("returning node with id: " + oppositeId.Value);
                        return oppositeEntity.GetNodeByAbsPos(!this.isUp, this.oppositeNodePos + aOffset);
                    }
                    else {
                        // Debug.Log("node is part of ignore list with id: " + oppositeId.Value);
                    }
                }
                else {
                    return oppositeEntity.GetNodeByAbsPos(!this.isUp, this.oppositeNodePos + aOffset);
                }
            }

        }
        // Debug.Log("did not find node");
        return null;
    }

    // public Node? GetOppositeNode(Vector2Int aOffset, HashSet<int> aIgnoreList = null) {
    //     // Debug.Log("node for id: " + this.id + " with relative pos:" + this.relativePos + "GetOppositeNode at:" + (this.oppositeNodePos + aOffset));
    //     // EntityState? oppositeEntity = GM.boardManager.GetEntityAtPos(this.oppositeNodePos + aOffset);
    //     int? oppositeId = GM.boardManager.boardCellDict[this.oppositeNodePos + aOffset].frontEntityId;
    //     if (oppositeId.HasValue) {
    //         EntityState oppositeEntity = GM.boardManager.GetEntityById(oppositeId.Value);
    //         if (oppositeEntity.hasNodes) {
    //             // Debug.Log("found node");
    //             if (aIgnoreList != null) {
    //                 if (!aIgnoreList.Contains(oppositeId.Value)) {
    //                     // Debug.Log("returning node with id: " + oppositeId.Value);
    //                     return oppositeEntity.GetNodeByAbsPos(!this.isUp, this.oppositeNodePos + aOffset);
    //                 }
    //                 else {
    //                     // Debug.Log("node is part of ignore list with id: " + oppositeId.Value);
    //                 }
    //             }
    //             else {
    //                 return oppositeEntity.GetNodeByAbsPos(!this.isUp, this.oppositeNodePos + aOffset);
    //             }
    //         }
    //
    //     }
    //     // Debug.Log("did not find node");
    //     return null;
    // }

    public Node? GetOppositeNode(Vector2Int aOffset, HashSet<EntityState> aIgnoreList) {
        int? oppositeId = GM.boardManager.currentState.GetBoardCellAtPos(this.oppositeNodePos + aOffset).frontEntityId;
        if (oppositeId.HasValue) {
            EntityState oppositeEntity = GM.boardManager.GetEntityById(oppositeId.Value);
            if (oppositeEntity.hasNodes) {
                // Debug.Log("found node");
                if (aIgnoreList != null) {
                    if (!aIgnoreList.Contains(oppositeEntity)) {
                        // Debug.Log("returning node with id: " + oppositeEntity.Value.id);
                        return oppositeEntity.GetNodeByAbsPos(!this.isUp, this.oppositeNodePos + aOffset);
                    }
                    else {
                        // Debug.Log("node is part of ignore list with id: " + oppositeEntity.Value.id);
                    }
                }
                else {
                    return oppositeEntity.GetNodeByAbsPos(!this.isUp, this.oppositeNodePos + aOffset);
                }
            }
        }
        // Debug.Log("did not find node");
        return null;
    }
}
//     public Node? GetOppositeNode(Vector2Int aOffset, HashSet<EntityState> aIgnoreList) {
//         int? oppositeId = GM.boardManager.boardCellDict[this.oppositeNodePos + aOffset].frontEntityId;
//         if (oppositeId.HasValue) {
//             EntityState oppositeEntity = GM.boardManager.GetEntityById(oppositeId.Value);
//             if (oppositeEntity.hasNodes) {
//                 // Debug.Log("found node");
//                 if (aIgnoreList != null) {
//                     if (!aIgnoreList.Contains(oppositeEntity)) {
//                         // Debug.Log("returning node with id: " + oppositeEntity.Value.id);
//                         return oppositeEntity.GetNodeByAbsPos(!this.isUp, this.oppositeNodePos + aOffset);
//                     }
//                     else {
//                         // Debug.Log("node is part of ignore list with id: " + oppositeEntity.Value.id);
//                     }
//                 }
//                 else {
//                     return oppositeEntity.GetNodeByAbsPos(!this.isUp, this.oppositeNodePos + aOffset);
//                 }
//             }
//         }
//         // Debug.Log("did not find node");
//         return null;
//     }
// }

public struct EntityState {
    public int id;
    public bool isFront;
    public string name;
    public Vector2Int size;
    public EntityTypeEnum entityType;
    public bool isBoundary;
    public string prefabPath;
    public bool isExit;

    public bool isInitialized;
    // public EntityImmutableData data;
    public MobData? mobData;
    
    public Vector2Int pos;
    public Vector2Int facing;
    public Color defaultColor;
    public bool isFixed;
    public TeamEnum team;
    public bool hasNodes {
        get {
            return this.nodeSet.Length > 0;
        }
    }

    // public HashSet<Node> nodeSet;
    public ImmutableArray<Node> nodeSet;
    public int touchDefense;
    public int fallDefense;

    public EntityBase entityBase {
        get {
            return GM.boardManager.GetEntityBaseById(this.id);
        }
    }

    public static EntityState GetClone(EntityState aEntityState) {
        EntityState newEntityState = aEntityState;
        return newEntityState;
    }

    public static EntityState CreateEntityState(EntitySchema aEntitySchema, Vector2Int aPos, Vector2Int aFacing, Color aDefaultColor, bool aIsFixed = false, bool aIsBoundary = false) {
        EntityState newEntityState = new EntityState();

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
        newEntityState.id = Constants.PLACEHOLDERINT;
        newEntityState.isFront = aEntitySchema.isFront;
        newEntityState.name = aEntitySchema.name;
        newEntityState.size = aEntitySchema.size;
        newEntityState.entityType = aEntitySchema.entityType;
        newEntityState.isBoundary = aIsBoundary;
        newEntityState.prefabPath = aEntitySchema.prefabPath;
        newEntityState.isExit = aEntitySchema.isExit;
        newEntityState.pos = aPos;
        newEntityState.facing = aFacing;
        newEntityState.defaultColor = aDefaultColor;
        newEntityState.isFixed = aIsFixed;
        newEntityState.team = aEntitySchema.defaultTeam;
        
        // newEntityState.nodeSet = new HashSet<Node>();
        newEntityState.touchDefense = aEntitySchema.touchDefense;
        newEntityState.fallDefense = aEntitySchema.fallDefense;

        return newEntityState;
    }

    // called only by BoardState.AddEntity
    public void Init(int aId) {
        Debug.Assert(!this.isInitialized);
        this.isInitialized = true;
        this.id = aId;
        this.name = GenerateName(this.name, this.isBoundary);
        
        string GenerateName(string aEntitySchemaName, bool aIsBoundary) {
            string nameString = aEntitySchemaName + " ";
            if (aIsBoundary) {
                nameString += "(boundary) ";
            }
            nameString += "id: " + aId;
            return nameString;
        }

        this.nodeSet = GenerateDefaultNodeSet();
        // if (this.entityType == EntityTypeEnum.BLOCK) {
        //     this.nodeSet = GenerateDefaultNodeSet();
        // }
    }

    ImmutableArray<Node> GenerateDefaultNodeSet() {
        // var newNodeSet = new HashSet<Node>();
        Node[] nodeArray = new Node[this.size.x * this.size.y];
        HashSet<Vector2Int> newUpNodes = new HashSet<Vector2Int>();
        HashSet<Vector2Int> newDownNodes = new HashSet<Vector2Int>();
        bool hasUpNodes = true;
        bool hasDownNodes = true;
        if (this.isBoundary) {
            if (this.pos.y + this.size.y == GM.boardManager.currentState.size.y) {
                hasUpNodes = false;
            }
            if (this.pos.y == 0) {
                hasDownNodes = false;
            }
        }
        for (int x = 0; x < this.size.x; x++) {
            if (hasUpNodes) {
                Vector2Int topPos = new Vector2Int(x, this.size.y - 1);
                newUpNodes.Add(topPos);
                nodeArray[Util.GetFlatIndexFromPos(x, this.size.y - 1)] = new Node()
                newNodeSet.Add(new Node(true, aEntityState.id, topPos));
            }

            if (hasDownNodes) {
                Vector2Int botPos = new Vector2Int(x, 0);
                newDownNodes.Add(botPos);
                newNodeSet.Add(new Node(false, aEntityState.id, botPos));
            }
        }
        return newNodeSet;
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
    
    public Node GetNodeByAbsPos(bool aIsUp, Vector2Int aAbsPos) {
        foreach (Node node in this.nodeSet) {
            if (node.isUp == aIsUp && node.relativePos + this.pos == aAbsPos) {
                return node;
            }
        }
        throw new Exception("invalid absPos");
    }

    public HashSet<Node> GetNodes(bool aIsUp) {
        HashSet<Node> filteredNodeSet = new HashSet<Node>();
        foreach (Node currentNode in this.nodeSet) {
            if (currentNode.isUp == aIsUp) {
                filteredNodeSet.Add(currentNode);
            }
        }
        return filteredNodeSet;
    }

    public HashSet<Node> GetNodes() {
        return this.nodeSet;
    }
}


