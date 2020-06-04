using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Sirenix.OdinInspector;
using System.IO;

[CreateAssetMenu(fileName = "Resources/ScriptableObjects/Levels", menuName = "ScriptableObjects/EntitySchema", order = 1)]
public class EntitySchema : SerializedScriptableObject {
    [Header("EntityImmutableData")]
    public string prefabPath;
    public bool isFront;
    public Vector2Int size;
    public EntityTypeEnum entityType;
    public TeamEnum defaultTeam;
    [Header("EntityState")]
    public int touchDefense;
    public int fallDefense;
    public bool hasNodes;
    [Header("MobData")] 
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
    
    [ContextMenu("BgFileNameToNameField")]
    public void BgFileNameToNameField() {
        string path = AssetDatabase.GetAssetPath(this.GetInstanceID());
        this.prefabPath = "BGs/" + Path.GetFileNameWithoutExtension(path);
    }
    
    [ContextMenu("BlockFileNameToNameField")]
    public void BlockFileNameToNameField() {
        string path = AssetDatabase.GetAssetPath(this.GetInstanceID());
        this.prefabPath = "Blocks/" + Path.GetFileNameWithoutExtension(path);
    }
}
