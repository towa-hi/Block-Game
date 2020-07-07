using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
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
    public readonly int id;
    public readonly bool hasUp;
    public readonly bool hasDown;
    public readonly Vector2Int relativePos;

    public Vector2Int absolutePos {
        get {
            return GM.boardManager.GetEntityById(this.id).pos + this.relativePos;
        }
    }

    public Node(bool aHasUp, bool aHasDown, int aId, Vector2Int aRelativePos) {
        this.relativePos = aRelativePos;
        this.id = aId;
        this.hasUp = aHasUp;
        this.hasDown = aHasDown;
    }

    public static Node UpdateNodeUp(Node aNode, bool aHasUp) {
        Node newNode = new Node(aHasUp, aNode.hasDown, aNode.id, aNode.relativePos);
        return newNode;
    }

    public static Node UpdateNodeDown(Node aNode, bool aHasDown) {
        Node newNode = new Node(aNode.hasUp, aHasDown, aNode.id, aNode.relativePos);
        return newNode;
    }

    public bool HasDirection(bool aIsUp) {
        if (aIsUp) {
            return this.hasUp;
        }
        else {
            return this.hasDown;
        }
    }

    public (Node?, Node?) GetAllOppositeNodes(Vector2Int aOffset, HashSet<int> aIgnoreSet = null) {
        Node? upNode = null;
        Node? downNode = null;
        if (this.hasUp) {
            upNode = GetOppositeNode(true, aOffset, aIgnoreSet);
        }
        if (this.hasDown) {
            downNode = GetOppositeNode(false, aOffset, aIgnoreSet);
        }
        return (upNode, downNode);
    }

    public Node? GetOppositeNode(bool aIsUp, Vector2Int aOffset, HashSet<int> aIgnoreSet = null) {
        // Debug.Log("node for id: " + this.id + " with relative pos:" + this.relativePos + "GetOppositeNode at:" + (this.oppositeNodePos + aOffset));
        // EntityState? oppositeEntity = GM.boardManager.GetEntityAtPos(this.oppositeNodePos + aOffset);
        Vector2Int oppositeAbsPos = aIsUp ? this.absolutePos + aOffset + Vector2Int.up: this.absolutePos + aOffset + Vector2Int.down;
        int? oppositeId = GM.boardManager.currentState.GetBoardCellAtPos(oppositeAbsPos).frontEntityId;
        if (oppositeId.HasValue) {
            if (aIgnoreSet != null) {
                bool ignoreThis = false;
                foreach (int ignoredId in aIgnoreSet) {
                    if (ignoredId == oppositeId) {
                        ignoreThis = true;
                        break;
                    }
                }
                if (!ignoreThis) {
                    Node? oppositeNode = GM.boardManager.GetEntityById(oppositeId.Value).GetNodeByAbsPos(oppositeAbsPos);
                    if (oppositeNode?.HasDirection(!aIsUp) == true) {
                        return oppositeNode;
                    }
                }
            }
            else {
                Node? oppositeNode = GM.boardManager.GetEntityById(oppositeId.Value).GetNodeByAbsPos(oppositeAbsPos);
                if (oppositeNode?.HasDirection(!aIsUp) == true) {
                    return oppositeNode;
                }
            }
        }
        // Debug.Log("did not find node");
        return null;
    }
}


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
    public bool isSuspended;

    public ImmutableArray<Node> nodeIArray;
    public Node[] serializedNodeArray;
    public int touchDefense;
    public int fallDefense;

    public EntityBase entityBase {
        get {
            return GM.boardManager.GetEntityBaseById(this.id);
        }
    }

    public void PackEntityState() {
        this.serializedNodeArray = this.nodeIArray.ToArray();
    }

    public void UnpackEntityState() {
        this.nodeIArray = ImmutableArray.Create(this.serializedNodeArray);
        this.serializedNodeArray = null;
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
        newEntityState.isSuspended = false;
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

        this.nodeIArray = GenerateDefaultNodeSet();
        // if (this.entityType == EntityTypeEnum.BLOCK) {
        //     this.nodeSet = GenerateDefaultNodeSet();
        // }
    }

    ImmutableArray<Node> GenerateDefaultNodeSet() {
        // var newNodeSet = new HashSet<Node>();
        Node[] nodeArray = new Node[this.size.x * this.size.y];
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
        if (this.entityType == EntityTypeEnum.BLOCK) {
            for (int x = 0; x < this.size.x; x++) {
                Vector2Int topPos = new Vector2Int(x, this.size.y - 1);
                Vector2Int botPos = new Vector2Int(x, 0);
                if (topPos == botPos) {
                    nodeArray[Util.GetFlatIndexFromPos(topPos, this.size)] = new Node(hasUpNodes, hasDownNodes, this.id, topPos);
                }
                else {
                    if (hasUpNodes) {
                        nodeArray[Util.GetFlatIndexFromPos(topPos, this.size)] = new Node(true, false, this.id, topPos);
                    }
                    if (hasDownNodes) {
                        nodeArray[Util.GetFlatIndexFromPos(botPos, this.size)] = new Node(false, true, this.id, botPos);
                    }
                }
            }
        }

        return ImmutableArray.Create(nodeArray);
    }

    public static EntityState SetPos(EntityState aEntityState, Vector2Int aPos) {
        Debug.Assert(!aEntityState.isSuspended);
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

    public Node? GetNodeByAbsPos(Vector2Int aAbsPos) {
        foreach (Node node in this.nodeIArray) {
            if (node.absolutePos == aAbsPos) {
                return node;
            }
        }
        return null;
    }

    public List<Node> GetNodes(bool aIsUp) {
        List<Node> foundNodes = new List<Node>();
        foreach (Node node in this.nodeIArray) {
            if (node.HasDirection(aIsUp)) {
                foundNodes.Add(node);
            }
        }
        return foundNodes;
    }

    public ImmutableArray<Node> GetNodes() {
        return this.nodeIArray;
    }

    // public HashSet<Node> GetNodes(bool aIsUp) {
    //     HashSet<Node> filteredNodeSet = new HashSet<Node>();
    //     foreach (Node currentNode in this.nodeSet) {
    //         if (currentNode.isUp == aIsUp) {
    //             filteredNodeSet.Add(currentNode);
    //         }
    //     }
    //     return filteredNodeSet;
    // }
    //
    // public HashSet<Node> GetNodes() {
    //     return this.nodeSet;
    // }
}


