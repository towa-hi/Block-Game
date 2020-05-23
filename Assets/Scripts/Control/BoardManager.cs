using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

public class BoardManager : SerializedMonoBehaviour {
    public List<EntityBase> entityBaseList;
    public List<BgBase> bgBaseList;
    public GameObject backgroundContainer;

    public void Init() {
        LoadBoardData(GM.boardData);
    }

    public void LoadBoardData(BoardData aBoardData) {
        foreach (EntityBase entityBase in this.entityBaseList) {
            Destroy(entityBase.gameObject);
        }
        this.entityBaseList = new List<EntityBase>();
        foreach (EntityData entityData in aBoardData.entityDataSet) {
            CreateEntityFromData(entityData);
        }
        foreach (BgBase bgBase in this.bgBaseList) {
            Destroy(bgBase.gameObject);
        }
        this.bgBaseList = new List<BgBase>();
        foreach (BgData bgData in aBoardData.backgroundData.bgDataSet) {
            CreateBgFromData(bgData);
        }
    }

    public void CreateBgFromData(BgData aBgData) {
        GameObject bgPrefab = Instantiate(GM.LoadBgPrefabByFilename(aBgData.prefabPath), this.backgroundContainer.transform);
        BgBase bgBase = bgPrefab.GetComponent<BgBase>();
        this.bgBaseList.Add(bgBase);
        GM.boardData.backgroundData.RegisterBgData(aBgData);
    }

    public void DestroyBg(BgData aBgData) {
        GM.boardData.backgroundData.UnRegisterBgData(aBgData);
        Destroy(aBgData.bgBase.gameObject);
        this.bgBaseList.Remove(aBgData.bgBase);
        print("BoardManager - DestroyBg: + " + aBgData.name);
    }

    public void CreateEntityFromData(EntityData aEntityData) {
        if (aEntityData.type == EntityTypeEnum.PLAYER) {
            GM.boardData.SetPlayerEntity(aEntityData);
        }
        // instantiate EntityData's prefab by using GM to lookup which prefab to get
        GameObject entityPrefab = Instantiate(GM.LoadEntityPrefabByFilename(aEntityData.prefabPath), this.transform);
        // get the EntityBase for this prefab
        EntityBase entityBase = entityPrefab.GetComponent<EntityBase>();
        this.entityBaseList.Add(entityBase);
        // print("3 entityDataList size" + GM.boardData.entityDataSet.Count);
        entityBase.Init(aEntityData);
        GM.boardData.RegisterEntityData(aEntityData);
        
        // print("4 entityDataList size" + GM.boardData.entityDataSet.Count);
    }

    public void DestroyEntity(EntityData aEntityData) {
        if (aEntityData == GM.boardData.playerEntityData) {
            GM.boardData.SetPlayerEntity(null);
        }
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
