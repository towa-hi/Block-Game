using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

public abstract class GUIBase : SerializedMonoBehaviour {
    public virtual void OnEnable() {
        GM.editManager2.OnUpdateState += OnUpdateState;
        OnUpdateState();
        // TODO: figure out why state doesnt update until next frame
    }

    public virtual void OnDisable() {
        GM.editManager2.OnUpdateState -= OnUpdateState;
    }

    public abstract void OnUpdateState();
}
