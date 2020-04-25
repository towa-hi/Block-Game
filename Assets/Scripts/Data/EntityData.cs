using System.Collections;
using System.Collections.Generic;
using UnityEngine;
// represents the initial state of the entity
public class EntityData {
    // set from levelSchema editor or ingame editor
    public Vector2Int pos;
    public Vector2Int facing;
    public EntitySchema entitySchema;
    public Color color;
    public bool isFixed;
}
