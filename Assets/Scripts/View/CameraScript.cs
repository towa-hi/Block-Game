using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

public class CameraScript : SerializedMonoBehaviour {
    public GameObject emptyObject;
    void Update() {
        this.transform.LookAt(this.emptyObject.transform);
    }
}
