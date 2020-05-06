using System.Collections;
using System.Collections.Generic;
using UnityEngine;
// represents the initial state of the entity

public class EntityData {
    public EntityBase entityBase;
    public EntityView entityView;
    // set from levelSchema editor or ingame editor
    public EntitySchema entitySchema;
    public string name;
    public Vector2Int pos;
    public Vector2Int facing;
    public Vector2Int size;
    public EntityTypeEnum type;
    public Color defaultColor;
    public bool isFixed;
    public bool isBoundary;
    
    // created first as a normal object before it gets registered to an entityBase
    public EntityData(EntitySchema aEntitySchema, Vector2Int aPos, Vector2Int aFacing, Color aDefaultColor, bool aIsFixed = false, bool aIsBoundary = false) {
        this.entityBase = null;
        this.entityView = null;
        this.name = GenerateName();
        this.entitySchema = aEntitySchema;
        this.pos = aPos;
        this.facing = aFacing;
        this.size = aEntitySchema.size;
        this.type = aEntitySchema.type;
        this.defaultColor = aDefaultColor;
        this.isFixed = aIsFixed;
        this.isBoundary = aIsBoundary;
    }

    public void RegisterEntityBase(EntityBase aEntityBase) {
        this.entityBase = aEntityBase;
        this.entityView = this.entityBase.entityView;
    }
    public List<Vector2Int> GetOccupiedPos() {
        return Util.V2IInRect(this.pos, this.size);
    }

    public string GenerateName() {
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
    
    public void SetPos(Vector2Int aPos) {
        this.pos = aPos;
    }

    public void SetDefaultColor(Color aDefaultColor) {
        this.defaultColor = aDefaultColor;
        this.entityView.SetColor(aDefaultColor);
    }
}
