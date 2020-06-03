using System;
using UnityEngine;

public class Cursor : EditorStateListener {
    [SerializeField] Vector2Int pos;
    [SerializeField] Vector2Int size;
    [SerializeField] CursorModeEnum cursorMode;
    [SerializeField] bool isFront;
    [SerializeField] EntitySchema selectedSchema;
    [SerializeField] Vector2Int heldPos;
    [SerializeField] Vector2Int heldSize;
    Vector3 zOffset {
        get {
            if (this.isFront) {
                return new Vector3(0, 0, -1.01f);
            }
            else {
                return new Vector3(0, 0, 0.99f);
            }
        }
    }

    SpriteRenderer myRenderer;
    void Awake() {
        this.myRenderer = GetComponent<SpriteRenderer>();
    }

    public override void OnEnable() {
        this.cursorMode = CursorModeEnum.POINTING;
        base.OnEnable();
    }
    public void Update() {
        this.myRenderer.enabled = !GM.inputManager.isCursorOverUI;
        switch (this.cursorMode) {
            case CursorModeEnum.POINTING:
                CursorModePointingUpdate();
                break;
            case CursorModeEnum.PLACING:
                CursorModePlacingUpdate();
                break;
            case CursorModeEnum.SELECTING:
                CursorModeSelectingUpdate();
                break;
            case CursorModeEnum.HOLDING:
                CursorModeHoldingUpdate();
                break;
            case CursorModeEnum.GAME:
                CursorModeGameUpdate();
                break;
            case CursorModeEnum.OFF:
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
    
    protected override void OnUpdateEditorState(EditorState aNewEditorState) {
        this.isFront = aNewEditorState.isFront;
        this.selectedSchema = aNewEditorState.selectedSchema;
        this.cursorMode = ChooseMode(aNewEditorState);
    }

    public void OnUpdateGameState(GameState aNewGameState) {
        
    }

    public CursorModeEnum ChooseMode(EditorState aEditorState) {
        switch (aEditorState.activeTab) {
            case EditTabEnum.PICKER:
                if (aEditorState.selectedSchema != null) {
                    return CursorModeEnum.PLACING;
                }
                else if (GM.editManager.currentState.hasSelectedEntity) {
                    EntityState selectedEntity = GM.editManager.GetSelectedEntity();
                    this.heldPos = selectedEntity.pos;
                    this.heldSize = selectedEntity.data.size;
                    return CursorModeEnum.HOLDING;
                }
                else {
                    return CursorModeEnum.SELECTING;
                }
            case EditTabEnum.EDIT:
                return CursorModeEnum.SELECTING;
            case EditTabEnum.OPTIONS:
                return CursorModeEnum.POINTING;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }



     void CursorModePointingUpdate() {
         SetPos(GM.inputManager.mousePosV2);
         SetSize(new Vector2Int(1, 1));
         SetColor(Color.white);
     }

     void CursorModePlacingUpdate() {
         EntitySchema selectedEntitySchema = this.selectedSchema;
         SetPos(GM.inputManager.mousePosV2);
         SetSize(selectedEntitySchema.size);
     }

     void CursorModeSelectingUpdate() {
         EntityState? maybeAEntity = GM.boardManager.GetEntityAtMousePos();

         if (maybeAEntity.HasValue) {
             SetPos(maybeAEntity.Value.pos);
             SetSize(maybeAEntity.Value.data.size);
         }
         else {
             SetPos(GM.inputManager.mousePosV2);
             SetSize(new Vector2Int(1, 1));
         }
     }
    
     void CursorModeHoldingUpdate() {
         SetPos(this.heldPos + GM.inputManager.dragOffsetV2);
         SetSize(this.heldSize);
     }

     void CursorModeGameUpdate() {
         // placeholder
     }
     
     void SetPos(Vector2Int aPos) {
         if (this.pos != aPos) {
             this.pos = aPos;
             this.transform.position = Util.V2IToV3(aPos) + this.zOffset;
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
