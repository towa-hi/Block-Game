using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

// data that defines a level is an instance of this in the Data/Levels folder
[CreateAssetMenu(fileName = "Resources/ScriptableObjects/Levels", menuName = "ScriptableObjects/LevelSchema", order = 1)]
public class LevelSchema : SerializedScriptableObject {
    // set by editor or ingame editor
    public string title;
    public string creator;
    public int par;
    public Vector2Int size;
    public List<EntityData> entityList = new List<EntityData>();

    public void Init(string aTitle, string aCreator, int aPar, Vector2Int aSize, List<EntityData> aEntityList) {
        this.title = aTitle;
        this.creator = aCreator;
        this.par = aPar;
        this.size = aSize;
        this.entityList =aEntityList;
    }
}
