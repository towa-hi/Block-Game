using System.Collections;
using System.Collections.Generic;
using UnityEngine;
// represents the initial state of the entity

// TODO: there's something weird about playerEntityData
public class EntityData {
    public EntityBase entityBase {
        get {
            foreach (EntityBase entityBase in GM.boardManager.entityBaseList) {
                if (entityBase.entityData == this) {
                    return entityBase;
                }
            }
            return null;
        }
    }
    public EntityView entityView {
        get {
            foreach (EntityBase entityBase in GM.boardManager.entityBaseList) {
                if (entityBase.entityData == this) {
                    return entityBase.entityView;
                }
            }
            return null;
        }
    }
    // componentsAreInitialized starts as false and is set to true after EntityBase is done initializing
    // used to prevent components from resetting when initialized
    public bool componentsAreInitialized;
    public string name;
    public Vector2Int pos;
    public Vector2Int facing;
    public Vector2Int size;
    public EntityTypeEnum type;
    public EntityPrefabEnum prefab;
    public Color defaultColor;
    public bool isFixed;
    public bool isBoundary;
    public TeamEnum team;
    // INodal data
    public HashSet<Vector2Int> upNodes;
    public HashSet<Vector2Int> downNodes;

    public bool isDying;
    // public int touchAttack;
    // public int fallAttack;
    public int touchDefense;
    public int fallDefense;
    StateMachine state;

    // use when creating from a schema
    public EntityData(EntitySchema aEntitySchema, Vector2Int aPos, Vector2Int aFacing, Color aDefaultColor, bool aIsFixed = false, bool aIsBoundary = false) {
        this.componentsAreInitialized = false;
        this.pos = aPos;
        this.facing = aFacing;
        this.size = aEntitySchema.size;
        this.type = aEntitySchema.type;
        this.prefab = aEntitySchema.prefab;
        this.defaultColor = aDefaultColor;
        this.isFixed = aIsFixed;
        this.isBoundary = aIsBoundary;
        this.name = GenerateName();
        this.upNodes = new HashSet<Vector2Int>();
        this.downNodes = new HashSet<Vector2Int>();
        this.isDying = false;
        this.touchDefense = aEntitySchema.touchDefense;
        this.fallDefense = aEntitySchema.fallDefense;
        this.team = aEntitySchema.defaultTeam;
    }


    public List<Vector2Int> GetOccupiedPos() {
        return Util.V2IInRect(this.pos, this.size);
    }

    string GenerateName() {
        string nameString = this.type.ToString() + " " + this.size;
        if (this.isBoundary) {
            nameString += " (boundary)";
        }
        nameString += GetHashCode();
        return nameString;
    }

    public bool IsMovableInPickerMode() {
        if (!this.isBoundary) {
            return true;
        } else {
            return false;
        }
    }
    
    public void Die() {
        Debug.Log("I'm dying");
        this.isDying = true;
    }

    public void SetPos(Vector2Int aPos) {
        this.pos = aPos;
    }

    public void SetDefaultColor(Color aDefaultColor) {
        this.defaultColor = aDefaultColor;
        this.entityView.SetColor(aDefaultColor);
    }

    public void SetFacing(Vector2Int aFacing) {
        this.facing = aFacing;
    }

    public void FlipEntity() {
        if (this.facing == Vector2Int.right) {
            this.facing = Vector2Int.left;
        } else {
            this.facing = Vector2Int.right;
        }
    }
}
