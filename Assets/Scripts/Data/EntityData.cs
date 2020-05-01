using System.Collections;
using System.Collections.Generic;
using UnityEngine;
// represents the initial state of the entity
public class EntityData {
    // set from levelSchema editor or ingame editor
    public EntitySchema entitySchema;
    public Vector2Int pos;
    public Vector2Int facing;
    public Color color;
    public bool isFixed;
    public bool isBoundary;
    public EntityData( EntitySchema aEntitySchema, Vector2Int aPos, Vector2Int aFacing, Color aColor, bool aIsFixed = false, bool aIsBoundary = false) {
        this.entitySchema = aEntitySchema;
        this.pos = aPos;
        this.facing = aFacing;
        this.color = aColor;
        this.isFixed = aIsFixed;
        this.isBoundary = aIsBoundary;
    }
}
