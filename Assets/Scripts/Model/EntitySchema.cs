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
    public Vector2Int size;
    public EntityTypeEnum type;
    // public EntityPrefabEnum prefab;
    public int touchDefense;
    public int fallDefense;
    public TeamEnum defaultTeam;

    [ContextMenu("FileNameToNameField")]
    public void FileNameToNameField() {
        this.prefabPath = AssetDatabase.GetAssetPath(this.GetInstanceID());
    }
    [ContextMenu("BlockFileNameToNameField")]
    public void BlockFileNameToNameField() {
        string path = AssetDatabase.GetAssetPath(this.GetInstanceID());
        this.prefabPath = "Blocks/" + Path.GetFileNameWithoutExtension(path);
    }
}
