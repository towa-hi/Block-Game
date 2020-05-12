using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

public class BoardManager : SerializedMonoBehaviour {
    public List<EntityBase> entityBaseList;
    public GridViewBase gridViewBase;

    public void Init() {
        LoadBoardData(GM.boardData);
        this.gridViewBase.Init();
    }

    public void LoadBoardData(BoardData aBoardData) {
        foreach (EntityBase entityBase in this.entityBaseList) {
            Destroy(entityBase.gameObject);
        }
        this.entityBaseList = new List<EntityBase>();
        // print("initial entityDataList size" + aBoardData.entityDataSet.Count);
        foreach (EntityData entityData in aBoardData.entityDataSet) {
            // print("2 entityDataList size" + aBoardData.entityDataSet.Count);
            CreateEntityFromData(entityData);
            // print("5 entityDataList size" + aBoardData.entityDataSet.Count);
        }
    }

    public void CreateEntityFromData(EntityData aEntityData) {
        // instantiate EntityData's prefab by using GM to lookup which prefab to get
        GameObject entityPrefab = Instantiate(GM.EntityPrefabEnumToPrefab(aEntityData.prefab), this.transform);
        // get the EntityBase for this prefab
        EntityBase entityBase = entityPrefab.GetComponent<EntityBase>();
        this.entityBaseList.Add(entityBase);
        // print("3 entityDataList size" + GM.boardData.entityDataSet.Count);
        entityBase.Init(aEntityData);
        GM.boardData.RegisterEntityData(aEntityData);
        // print("4 entityDataList size" + GM.boardData.entityDataSet.Count);
    }

    public void DestroyEntity(EntityData aEntityData) {
        GM.boardData.UnRegisterEntityData(aEntityData);
        Destroy(aEntityData.entityBase.gameObject);
        this.entityBaseList.Remove(aEntityData.entityBase);
        print("BoardManager - DestroyEntity: " + aEntityData.name);
    }

    public void MoveEntity(Vector2Int aPos, EntityData aEntityData) {
        // set model
        GM.boardData.MoveEntity(aPos, aEntityData);
        // set view
        aEntityData.entityBase.SetViewPosition(aPos);
    }
    
}
