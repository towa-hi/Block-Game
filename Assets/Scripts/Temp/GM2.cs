using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

public class GM2 : SerializedMonoBehaviour {

    public static InputManager2 inputManager2;
    public static BoardManager2 boardManager2;

    public GameObject coreGameObject;

    void Awake() {
        GM2.inputManager2 = this.coreGameObject.GetComponent<InputManager2>();
        GM2.boardManager2 = this.coreGameObject.GetComponent<BoardManager2>();
        GM2.boardManager2.Init();
    }
}
