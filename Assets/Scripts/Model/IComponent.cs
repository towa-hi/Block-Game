﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

// abstract class for all components related to entities
public abstract class IComponent : SerializedMonoBehaviour {
    public EntityBase entityBase;

    public abstract void Init();

    public abstract void DoFrame();
}
