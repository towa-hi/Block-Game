using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

public class CursorBase : GUIBase {
    [SerializeField]
    Vector3 zOffset {
        get {
            if (this.isFront) {
                return new Vector3(0, 0, -1.01f);
            } else {
                return new Vector3(0, 0, 0.99f);
            }
        }
    }
    [SerializeField]
    Vector2Int pos;
    [SerializeField]
    Vector2Int size;
    [SerializeField]
    CursorModeEnum cursorMode;
    [SerializeField]
    bool isFront;
    [SerializeField]
    Object selectedSchema;
    [SerializeField]
    Vector2Int heldDataPos;
    [SerializeField]
    Vector2Int heldDataSize;

    public SpriteRenderer myRenderer;

    public void PlayInit() {
        this.cursorMode = CursorModeEnum.SELECTING;
        this.isFront = true;
    }

    public void SetHolding(EntityData aEntityData) {
        this.heldDataPos = aEntityData.pos;
        this.heldDataSize = aEntityData.size;
        this.cursorMode = CursorModeEnum.HOLDING;
    }

    public void SetSelecting() {
        this.cursorMode = CursorModeEnum.SELECTING;
    }
    
    void Awake() {
        this.cursorMode = CursorModeEnum.POINTING;
    }

    public override void OnUpdateState() {
        EditorState currentState = GM.editManager2.GetState();
        this.isFront = currentState.isFront;
        this.selectedSchema = currentState.selectedSchema;
        this.cursorMode = ChooseMode(currentState);
    }

    public CursorModeEnum ChooseMode(EditorState aEditorState) {
        switch (aEditorState.activeTab) {
            case (EditTabEnum.PICKER):
                if (aEditorState.selectedSchema != null) {
                    return CursorModeEnum.PLACING;
                } else if (aEditorState.selectedBgData != null) {
                    this.heldDataPos = aEditorState.selectedBgData.pos;
                    this.heldDataSize = aEditorState.selectedBgData.size;
                    return CursorModeEnum.HOLDING;
                } else if (aEditorState.selectedEntityData != null) {
                    this.heldDataPos = aEditorState.selectedEntityData.pos;
                    this.heldDataSize = aEditorState.selectedEntityData.size;
                    return CursorModeEnum.HOLDING;
                } else {
                    return CursorModeEnum.SELECTING;
                }
            case (EditTabEnum.EDIT):
                    return CursorModeEnum.SELECTING;
            case (EditTabEnum.OPTIONS):
                return CursorModeEnum.POINTING;
        }
        throw new System.Exception("Unrecognized EditTabEnum");
    }

    public void Update() {
        this.myRenderer.enabled = !GM.inputManager.isCursorOverUI;
        switch (this.cursorMode) {
            case CursorModeEnum.POINTING:
                CursorModePointingUpdate();
                break;
            case CursorModeEnum.PLACING:
                CursorModePlacing();
                break;
            case CursorModeEnum.SELECTING:
                CursorModeSelectingUpdate();
                break;
            case CursorModeEnum.HOLDING:
                CursorModeHoldingUpdate();
                break;
            case CursorModeEnum.OFF:
                break;
        }
    }
    // TODO: have the cursor linger on the selected thing in edit mode
    
    void CursorModePointingUpdate() {
        SetPos(GM.inputManager.mousePosV2);
        SetSize(new Vector2Int(1, 1));
        SetColor(Color.white);
    }

    void CursorModePlacing() {
        if (this.selectedSchema is EntitySchema) {
            EntitySchema selectedEntitySchema = this.selectedSchema as EntitySchema;
            SetPos(GM.inputManager.mousePosV2);
            SetSize(selectedEntitySchema.size);
        } else if (this.selectedSchema is BgSchema) {
            BgSchema selectedBgSchema = this.selectedSchema as BgSchema;
            SetPos(GM.inputManager.mousePosV2);
            SetSize(selectedBgSchema.size);
        }
    }

    void CursorModeSelectingUpdate() {
        if (this.isFront) {
            EntityData maybeAEntity = GM.boardData.GetEntityDataAtPos(GM.inputManager.mousePosV2);
            if (maybeAEntity != null) {
                SetPos(maybeAEntity.pos);
                SetSize(maybeAEntity.size);
            } else {
                SetPos(GM.inputManager.mousePosV2);
                SetSize(new Vector2Int(1, 1));
            }
        } else {
            BgData maybeABg = GM.boardData.backgroundData.GetBgDataAtPos(GM.inputManager.mousePosV2);
            if (maybeABg != null) {
                SetPos(maybeABg.pos);
                SetSize(maybeABg.size);
            } else {
                SetPos(GM.inputManager.mousePosV2);
                SetSize(new Vector2Int(1, 1));
            }
        }
    }

    void CursorModeHoldingUpdate() {
        SetPos(GM.inputManager.mousePosV2);
        SetSize(this.heldDataSize);
    }

    void SetPos(Vector2Int aPos) {
        if (this.pos != aPos) {
            this.pos = aPos;
            this.transform.position = Util.V2IToV3(aPos) + zOffset;
        }
    }

    void SetSize(Vector2Int aSize) {
        if (this.size != aSize) {
            this.size = aSize;
            this.myRenderer.size = new Vector2(aSize.x, aSize.y * Constants.BLOCKHEIGHT);
        }
    }

    void SetColor(Color aColor) {
        this.myRenderer.color = aColor;
    }
    
}
