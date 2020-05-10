using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

// abstract class for all components related to entities

public abstract class IComponent : SerializedMonoBehaviour {
    public EntityData entityData {
        get {
            return this.entityBase.entityData;
        }
    }
    public EntityBase entityBase {
        get {
            return this.gameObject.GetComponent(typeof(EntityBase)) as EntityBase;
        }
    }
    public EntityView entityView {
        get {
            return this.entityBase.entityView;
        }
    }


    public abstract void Init();

    public abstract void DoFrame();

}
