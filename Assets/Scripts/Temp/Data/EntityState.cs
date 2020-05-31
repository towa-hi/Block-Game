using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Sirenix.OdinInspector;







public struct EntityState {
    public int id;
    // never change these
    public bool isFront;
    public string name;
    public Vector2Int size;
    public EntityTypeEnum entityType;
    public bool isBoundary;
    public string prefabPath;

    public Vector2Int pos;
    public Vector2Int facing;
    public Color defaultColor;
    public bool isFixed;
    public TeamEnum team;

    public HashSet<Vector2Int> upNodes;
    public HashSet<Vector2Int> downNodes;

    public int touchDefense;
    public int fallDefense;

    public bool CustomEquals(EntityState aOther) {        
        if (GetType() != aOther.GetType())
        {
            return false;
        }
        
        if (
            this.id == aOther.id &&
            this.name == aOther.name &&
            this.size == aOther.size &&
            this.entityType == aOther.entityType &&
            this.isBoundary == aOther.isBoundary &&
            this.prefabPath == aOther.prefabPath &&
            this.pos == aOther.pos &&
            this.facing == aOther.facing &&
            this.defaultColor == aOther.defaultColor &&
            this.isFixed == aOther.isFixed &&
            this.team == aOther.team &&
            this.upNodes == aOther.upNodes &&
            this.downNodes == aOther.downNodes &&
            this.touchDefense == aOther.touchDefense &&
            this.fallDefense == aOther.fallDefense
        ) {
            return true;
        } else {
            return false;
        }
    }

    public static EntityState CreateEntityState(EntitySchema aEntitySchema, Vector2Int aPos, Vector2Int aFacing, Color aDefaultColor, bool aIsFixed = false, bool aIsBoundary = false) {
        EntityState newEntityState = new EntityState();
        // set vals from EntitySchema
        newEntityState.prefabPath = aEntitySchema.prefabPath;
        newEntityState.isFront = aEntitySchema.isFront;
        newEntityState.size = aEntitySchema.size;
        newEntityState.entityType = aEntitySchema.entityType;
        newEntityState.touchDefense = aEntitySchema.touchDefense;
        newEntityState.fallDefense = aEntitySchema.fallDefense;
        newEntityState.team = aEntitySchema.defaultTeam;
        
        // set vals from params
        newEntityState.pos = aPos;
        newEntityState.facing = aFacing;
        newEntityState.defaultColor = aDefaultColor;
        newEntityState.isFixed = aIsFixed;
        newEntityState.isBoundary = aIsBoundary;
        // initialize some vals
        newEntityState.upNodes = new HashSet<Vector2Int>();
        newEntityState.downNodes = new HashSet<Vector2Int>();
        newEntityState.name = GenerateName();

        return newEntityState;

        string GenerateName() {
            string nameString = aEntitySchema.name + " ";
            nameString += newEntityState.GetHashCode();
            if (newEntityState.isBoundary) {
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


