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

    public void Init() {
        LoadBoardData();
        this.gridViewBase.Init();
    }

    public void LoadBoardData() {
        foreach (EntityData entityData in BoardData.entityDataSet) {
            CreateEntityFromData(entityData);
        }
    }
    public void CreateEntityFromData(EntityData aEntityData) {
        GameObject newEntityPrefab = Instantiate(aEntityData.entitySchema.entityObject, this.transform);
        EntityBase newEntityBase = newEntityPrefab.GetComponent<EntityBase>();
        newEntityBase.Init(aEntityData);
        this.entityBaseList.Add(newEntityBase);
        BoardData.RegisterEntityData(aEntityData);
    }

    public void DestroyEntity(EntityData aEntityData) {
        BoardData.UnRegisterEntityData(aEntityData);
        this.entityBaseList.Remove(aEntityData.entityBase);
        Destroy(aEntityData.entityBase.gameObject);
        print("BoardManager2 - DestroyEntity: " + aEntityData.name);
    }

    public void MoveEntity(Vector2Int aPos, EntityData aEntityData) {
        // set model
        BoardData.MoveEntity(aPos, aEntityData);
        // set view
        aEntityData.entityBase.SetViewPosition(aPos);
    }
    
}
