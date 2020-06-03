using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Sirenix.OdinInspector;
using System.IO;

[CreateAssetMenu(fileName = "Resources/ScriptableObjects/Levels", menuName = "ScriptableObjects/EntitySchema", order = 1)]
public class EntitySchema : SerializedScriptableObject {
    // set by editor
    public string prefabPath;
    public bool isFront;
    public Vector2Int size;
    public EntityTypeEnum entityType;
    public int touchDefense;
    public int fallDefense;
    public TeamEnum defaultTeam;
    public bool hasNodes;

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
