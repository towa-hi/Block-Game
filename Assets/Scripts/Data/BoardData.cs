using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

public class BoardData : SingletonScriptableObject<BoardData> {

    public GameGrid gameGrid;
    public List<EntityBase> entityList;

    public string title;
    public string creator;
    public int par;
    public Vector2Int size;

    public int attempts;

    void Init() {

    }

    // destroy all the gameobjects and start over with new levelData
    public void InitializeLevel(LevelData aLevelData) {
        foreach (EntityBase entityBase in this.entityList) {
            Destroy(entityBase.gameObject);
        }
        this.gameGrid = new GameGrid(aLevelData.levelSchema.size);
        this.entityList = new List<EntityBase>();

        this.title = aLevelData.levelSchema.title;
        this.creator = aLevelData.levelSchema.creator;
        this.par = aLevelData.levelSchema.par;
        this.size = aLevelData.levelSchema.size;

        this.attempts = aLevelData.attempts;
    }


}
