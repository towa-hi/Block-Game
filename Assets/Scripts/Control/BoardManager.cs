using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

public class BoardManager : SerializedMonoBehaviour {
    public List<EntityBase> entityBaseList;
    public EntityData heldEntity;
    public GridViewBase gridViewBase;
    void Awake() {
    }

    public void Init(BoardData aBoardData) {
        LoadBoardData(aBoardData);
        this.gridViewBase.Init(aBoardData);
    }

    public void LoadBoardData(BoardData aBoardData) {
        foreach (EntityData entityData in aBoardData.entityDataSet) {
            CreateEntityFromData(entityData);
        }
    }
    public void CreateEntityFromData(EntityData aEntityData) {
        GameObject newEntityPrefab = Instantiate(aEntityData.entitySchema.entityObject, this.transform);
        EntityBase newEntityBase = newEntityPrefab.GetComponent<EntityBase>();
        newEntityBase.Init(aEntityData);
        this.entityBaseList.Add(newEntityBase);
    }

    public void DestroyEntity(EntityData aEntityData) {
        GM.boardData.UnRegisterEntityData(aEntityData);
        this.entityBaseList.Remove(aEntityData.entityBase);
        Destroy(aEntityData.entityBase.gameObject);
        print("BoardManager2 - DestroyEntity: " + aEntityData.name);
    }

    public void MoveEntity(Vector2Int aPos, EntityData aEntityData) {
        // set model
        GM.boardData.MoveEntity(aPos, aEntityData);
        // set view
        aEntityData.entityBase.SetViewPosition(aPos);
    }
    
}
