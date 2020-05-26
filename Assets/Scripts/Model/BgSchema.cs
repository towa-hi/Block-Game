using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Sirenix.OdinInspector;
using System.IO;

[CreateAssetMenu(fileName = "Resources/ScriptableObjects/Levels", menuName = "ScriptableObjects/BgSchema", order = 1)]
public class BgSchema : SerializedScriptableObject {
    public string prefabPath;
    public Vector2Int size;
    public bool isBlocking;

    [ContextMenu("BgFileNameToNameField")]
    public void BgFileNameToNameField() {
        string path = AssetDatabase.GetAssetPath(this.GetInstanceID());
        this.prefabPath = Path.GetFileNameWithoutExtension(path);
    }
}
