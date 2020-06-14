using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

public class CameraTarget : SerializedMonoBehaviour {
    Vector3 centerOfScreen = new Vector3(20, 15, 0);
    public GameObject cursor;
    public float offsetMultiply;
    public float tMultiply;
    // Start is called before the first frame update
    void Update() {
        //GM.boardManager.IsPosInBoard(GM.inputManager.mousePosV2)
        if (true) {
            Vector3 offset = GM.inputManager.mousePos - this.centerOfScreen;
            offset.z = 0;
            offset *= this.offsetMultiply;
            Vector3 desiredPos = this.centerOfScreen + new Vector3(5,0,0) + offset;
            float t = tMultiply* Time.deltaTime;
            Vector3 newPos = Vector3.Lerp(this.transform.position, desiredPos, t);
            this.transform.position = newPos;
        }

    }
}
