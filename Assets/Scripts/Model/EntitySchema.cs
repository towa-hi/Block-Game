using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

[CreateAssetMenu(fileName = "Resources/ScriptableObjects/Levels", menuName = "ScriptableObjects/EntitySchema", order = 1)]
public class EntitySchema : SerializedScriptableObject {
    // set by editor
    public Vector2Int size;
    public EntityTypeEnum type;
    public EntityPrefabEnum prefab;
    public int touchDefense;
    public int fallDefense;
    public TeamEnum defaultTeam;
}
