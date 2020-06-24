using System;
using Schema;
using Sirenix.OdinInspector;
using UnityEngine;
// TODO: decouple cursor from editor state
public class Cursor : SerializedMonoBehaviour {
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

    void OnEnable() {
        this.cursorMode = CursorModeEnum.POINTING;
        GM.editManager.OnUpdateEditorState += OnUpdateEditorState;
        GM.playManager.OnUpdatePlayState += OnUpdatePlayState;
    }

    void OnDisable() {
        GM.editManager.OnUpdateEditorState -= OnUpdateEditorState;
        GM.playManager.OnUpdatePlayState -= OnUpdatePlayState;
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
    
    void OnUpdateEditorState(EditorState aNewEditorState) {
        switch (GM.instance.currentState.gameMode) {
            case GameModeEnum.PLAYING:
                throw new Exception("received OnUpdateEditorState but GameState.gameMode = PLAYING");
            case GameModeEnum.EDITING:
                this.isFront = aNewEditorState.isFront;
                this.selectedSchema = aNewEditorState.selectedSchema;
                this.cursorMode = EditModeChooseMode(aNewEditorState);
                break;
            case GameModeEnum.PLAYTESTING:
                throw new Exception("received OnUpdateEditorState updated but GameState.gameMode = PLAYING");
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    void OnUpdatePlayState(PlayState aNewPlayState) {
        switch (GM.instance.currentState.gameMode) {
            case GameModeEnum.PLAYING:
                print("cursor OnUpdatePlayState");
                this.isFront = true;
                this.selectedSchema = null;
                this.cursorMode = PlayModeChooseMode(aNewPlayState);
                break;
            case GameModeEnum.EDITING:
                throw new Exception("received OnUpdatePlayState but GameState.gameMode = PLAYING");
            case GameModeEnum.PLAYTESTING:
                this.isFront = true;
                this.selectedSchema = null;
                this.cursorMode = PlayModeChooseMode(aNewPlayState);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
    
    public void OnUpdateGameState(GameState aNewGameState) {
        switch (aNewGameState.gameMode) {
            case GameModeEnum.PLAYING:
                this.cursorMode = PlayModeChooseMode(GM.playManager.currentState);
                break;
            case GameModeEnum.EDITING:
                break;
            case GameModeEnum.PLAYTESTING:
                this.cursorMode = PlayModeChooseMode(GM.playManager.currentState);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    public CursorModeEnum PlayModeChooseMode(PlayState aPlayState) {
        if (aPlayState.heldEntityId.HasValue && aPlayState.heldEntityId != -42069) {
            EntityState heldEntity = GM.boardManager.GetEntityById(aPlayState.heldEntityId.Value);
            this.heldSize = heldEntity.size;
            this.heldPos = heldEntity.pos;
            return CursorModeEnum.HOLDING;
        }
        return CursorModeEnum.POINTING;
    }
    
    public CursorModeEnum EditModeChooseMode(EditorState aEditorState) {
        switch (aEditorState.activeTab) {
            case EditTabEnum.PICKER:
                if (aEditorState.selectedSchema != null) {
                    return CursorModeEnum.PLACING;
                }
                else if (GM.editManager.currentState.hasSelectedEntity) {
                    EntityState selectedEntity = GM.editManager.GetSelectedEntity();
                    this.heldPos = selectedEntity.pos;
                    this.heldSize = selectedEntity.size;
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
         EntityState? maybeAEntity = GM.boardManager.GetEntityAtMousePos(this.isFront);

         if (maybeAEntity.HasValue) {
             SetPos(maybeAEntity.Value.pos);
             SetSize(maybeAEntity.Value.size);
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
