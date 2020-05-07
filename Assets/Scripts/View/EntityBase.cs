using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

// all entities have this as the base component. holds important gameplay data and entity state
// but doesn't hold graphics data or graphics state
[SelectionBase]
public class EntityBase : SerializedMonoBehaviour {
    // set in init
    // public Vector2Int pos;
    // public Vector2Int facing;
    // public Vector2Int size;
    // public EntityTypeEnum type;
    // public bool isFixed;
    // public bool isBoundary;
    private HashSet<IComponent> iComponentSet;
    public EntityView entityView;
    public EntityData entityData;
    
    // we want to initialize entityBase, all the iComponents and entityView with entityData
    public void Init(EntityData aEntityData) {
        this.entityData = aEntityData;
        this.entityData.RegisterEntityBase(this);

        this.name = this.entityData.name;
        this.iComponentSet = new HashSet<IComponent>();
        this.entityView = this.transform.GetChild(0).GetComponent<EntityView>();
        this.entityView.Init(this.entityData);
        ResetViewPosition();
        foreach (IComponent iComponent in GetComponents(typeof(IComponent))) {
            iComponentSet.Add(iComponent);
            iComponent.Init(this.entityData);
        }
    }

    public IComponent GetCachedIComponent<T>() {
        foreach (IComponent iComponent in this.iComponentSet) {
            if (iComponent.GetType() == typeof(T)) {
                return iComponent;
            }
        }
        return null;
    }

    // public EntityData CreateEntityData() {
    //     return new EntityData(this.initialEntityData.entitySchema, this.pos, this.facing, this.entityView.defaultColor, this.isFixed, this.isBoundary);
    // }

    public void SetViewPosition(Vector2Int aPos) {
        this.transform.position = Util.V2IOffsetV3(aPos, this.entityData.size);
    }

    public void ResetViewPosition() {
        this.transform.position = Util.V2IOffsetV3(this.entityData.pos, this.entityData.size);
    }
}