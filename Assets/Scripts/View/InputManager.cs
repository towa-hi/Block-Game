using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;

// manages ingame input
public class InputManager : Singleton<InputManager> {
    private Controls controls;
    [Header("Mouse Pos")]
    [System.NonSerialized] public Vector3 mousePos;
    public Vector2Int mousePosV2;
    [System.NonSerialized] public Vector3 oldMousePos;
    public bool isCursorOverUI;
    public Vector2Int oldMousePosV2;
    public Vector3 clickedPos;
    public Vector2Int clickedPosV2;
    public Vector3 dragOffset;
    public Vector3 oldDragOffset;
    public MouseStateEnum mouseState;
    public MouseStateEnum rightMouseState;
    bool mouseIsHeldDownOneFrame;
    bool mouseIsReleasedOneFrame;
    
    void Awake() {
        this.controls = new Controls();
        this.mouseState = MouseStateEnum.DEFAULT;
        this.mouseIsHeldDownOneFrame = false;
        this.mouseIsReleasedOneFrame = false;
    }

    public void OnClickDown(InputAction.CallbackContext context) {
        switch (context.phase) {
            case InputActionPhase.Performed:
                if (!isCursorOverUI) {
                    this.mouseState = MouseStateEnum.CLICKED;
                }
                break;
            case InputActionPhase.Canceled:
                this.mouseState = MouseStateEnum.RELEASED;
                break;
        }
    }

    public void OnRightClickDown(InputAction.CallbackContext context) {
        switch (context.phase) {
            case InputActionPhase.Performed:
                this.rightMouseState = MouseStateEnum.CLICKED;
                break;
            case InputActionPhase.Canceled:
                this.rightMouseState = MouseStateEnum.DEFAULT;
                break;
        }
    }

    void Update() {
        this.isCursorOverUI = EventSystem.current.IsPointerOverGameObject();
        if (this.mouseIsHeldDownOneFrame) {
            this.mouseIsHeldDownOneFrame = false;
            this.mouseState = MouseStateEnum.HELD;
        }
        if (this.mouseIsReleasedOneFrame) {
            this.mouseIsReleasedOneFrame = false;
            this.mouseState = MouseStateEnum.DEFAULT;
        }
        // this Update() must run before anything else.
        this.oldMousePos = this.mousePos;
        this.oldMousePosV2 = this.mousePosV2; 
        this.mousePos = GetMousePos();
        this.mousePosV2 = Util.V3ToV2I(this.mousePos);

        switch (this.mouseState) {
            case MouseStateEnum.DEFAULT:
                break;
            case MouseStateEnum.CLICKED:
                // runs once for one frame before mouseState changes to HELD
                // print("GameManger - clicked: " + this.mousePos);
                this.clickedPos = this.mousePos;
                this.clickedPosV2 = Util.V3ToV2I(this.clickedPos);
                // OnClickDown();
                this.mouseIsHeldDownOneFrame = true;
                break;
            case MouseStateEnum.HELD:
                this.oldDragOffset = this.dragOffset;
                this.dragOffset = this.mousePos - this.clickedPos;
                break;
            case MouseStateEnum.RELEASED:
                // runs once for one frame before mouseState changes to DEFAULT
                this.dragOffset = Vector3.zero;
                this.oldDragOffset = Vector3.zero;
                this.clickedPos = Vector3.zero;
                this.mouseIsReleasedOneFrame = true;
                // print("GameManger - released: " + this.mousePos +  " offset: " + this.dragOffset);
                break;
        }
    }

    public Vector3 GetMousePos() {
        RaycastHit hit;
        Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
        if (Physics.Raycast(ray, out hit, Mathf.Infinity)) {
            return new Vector3(hit.point.x, hit.point.y, hit.point.z);
        } else {
            return this.mousePos;
        }
    }
}
