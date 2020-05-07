using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

// abstract class for all components related to entities

public abstract class IComponent : SerializedMonoBehaviour {
    public EntityData entityData;
    public EntityBase entityBase;
    public EntityView entityView;


    public abstract void Init(EntityData aEntityData);

    public abstract void DoFrame();

}
