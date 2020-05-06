using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

[CreateAssetMenu(fileName = "Resources/ScriptableObjects/Levels", menuName = "ScriptableObjects/EntitySchema", order = 1)]
public class EntitySchema : SerializedScriptableObject {
    // set by editor
    public Vector2Int size;
    public EntityTypeEnum type;
    public List<Component> components = new List<Component>();
    public GameObject entityObject;
}
