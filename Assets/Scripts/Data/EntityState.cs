using System.Collections.Generic;
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
    public bool canKillOnFall;
    public int fallPower;
    public bool canPush;
    public bool canBePushed;
    public bool canBeLifted;
}

public struct EntityState {
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
            name = GenerateName(),
            size = aEntitySchema.size,
            entityType = aEntitySchema.entityType,
            isBoundary = aIsBoundary,
            prefabPath = aEntitySchema.prefabPath,
        };
        if (aEntitySchema.entityType == EntityTypeEnum.MOB) {
            newEntityState.mobData = new MobData() {
                movementType = aEntitySchema.movementType,
                canHop = aEntitySchema.canHop,
                moveSpeed = aEntitySchema.moveSpeed,
                canKillOnTouch = aEntitySchema.canKillOnTouch,
                touchPower = aEntitySchema.touchPower,
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
        // generate some nodes
        if (aEntitySchema.hasNodes) {
            HashSet<Vector2Int> newUpNodes = new HashSet<Vector2Int>();
            HashSet<Vector2Int> newDownNodes = new HashSet<Vector2Int>();
            bool hasUpNodes = true;
            bool hasDownNodes = true;
            if (aIsBoundary) {
                if (aPos.y + aEntitySchema.size.y == GM.boardManager.currentState.size.y) {
                    hasUpNodes = false;
                }

                if (aPos.y == 0) {
                    hasDownNodes = false;
                }
            }

            for (int x = 0; x < aEntitySchema.size.x; x++) {
                if (hasUpNodes) {
                    Vector2Int topPos = new Vector2Int(x, aEntitySchema.size.y - 1);
                    newUpNodes.Add(topPos);
                }

                if (hasDownNodes) {
                    Vector2Int botPos = new Vector2Int(x, 0);
                    newDownNodes.Add(botPos);
                }
            }

            newEntityState.upNodes = newUpNodes;
            newEntityState.downNodes = newDownNodes;
        }
        else {
            newEntityState.upNodes = new HashSet<Vector2Int>();
            newEntityState.downNodes = new HashSet<Vector2Int>();
        }
        newEntityState.touchDefense = aEntitySchema.touchDefense;
        newEntityState.fallDefense = aEntitySchema.fallDefense;

        return newEntityState;

        string GenerateName() {
            string nameString = aEntitySchema.name + " ";
            nameString += newEntityState.GetHashCode();
            if (aIsBoundary) {
                nameString += " (boundary) ";
            }
            return nameString;
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
}


