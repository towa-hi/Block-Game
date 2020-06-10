using System;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

public delegate void OnUpdateEditorStateHandler(EditorState aEditorState);

[RequireComponent(typeof(BoardManager))]
public class EditManager : SerializedMonoBehaviour {
    [SerializeField] EditorState editorState;
    public EditorState currentState {
        get {
            return this.editorState;
        }
    }
    public event OnUpdateEditorStateHandler OnUpdateEditorState;
    [SerializeField] StateMachine inputStateMachine = new StateMachine();

    #region Lifecycle

    void Awake() {
        UpdateEditorState(EditorState.CreateEditorState());
        // initialize input state machine
        this.inputStateMachine.ChangeState(new EditorPickerModeInputState());
    }
    
    void Update() {
        if (!GM.instance.currentState.isFullPaused && GM.instance.currentState.gameMode == GameModeEnum.EDITING) {
            this.inputStateMachine.Update();
        }
    }

    #endregion
    
    #region Listeners

    // special function called by GM.OnUpdateGameState delegate
    public void OnUpdateGameState(GameState aGameState) {
        GM.instance.editorPanel.SetActive(aGameState.gameMode == GameModeEnum.EDITING);
    }

    #endregion

    #region EditorState

    void UpdateEditorState(EditorState aEditorState) {
        this.editorState = aEditorState;
        switch (GM.instance.currentState.gameMode) {
            case GameModeEnum.PLAYING:
                break;
            case GameModeEnum.EDITING:
                if (Config.PRINTLISTENERUPDATES) {
                    if (this.OnUpdateEditorState != null) {
                        print("EditManager - Updating EditorState for " + this.OnUpdateEditorState?.GetInvocationList().Length + " delegates");
                    }
                }
                this.editorState = aEditorState;
                this.OnUpdateEditorState?.Invoke(this.currentState);
                break;
            case GameModeEnum.PLAYTESTING:
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    #endregion

    #region Protected

    protected void AddSchema(Vector2Int aPos, EntitySchema aEntitySchema) {
        if (GM.boardManager.CanEditorPlaceSchema(aPos, aEntitySchema)) {
            GM.boardManager.AddEntityFromSchema(aEntitySchema, aPos, Constants.DEFAULTFACING, Constants.DEFAULTCOLOR);
        }
    }
    
    protected void PlaceSelectedSchema(Vector2Int aPos, EntitySchema aEntitySchema) {
        if (GM.boardManager.IsRectEmpty(aPos, aEntitySchema.size, null, aEntitySchema.isFront)) {
            GM.boardManager.AddEntityFromSchema(aEntitySchema, aPos, Constants.DEFAULTFACING, Constants.DEFAULTCOLOR);
        }
    }

    protected void SetSelectedEntity(int aId) {
        EditorState newEditorState = EditorState.SetSelectedEntityId(this.currentState, aId);
        UpdateEditorState(newEditorState);
    }

    protected void ClearSelectedEntity() {
        EditorState newEditorState = EditorState.ClearSelectedEntityId(this.currentState);
        UpdateEditorState(newEditorState);
    }
    
    protected bool CanMoveTo(Vector2Int aPos, EntityState aEntityState) {
        if (!aEntityState.data.isBoundary) {
            if (GM.boardManager.IsRectEmpty(aPos, aEntityState.data.size, new HashSet<EntityState> {aEntityState}, aEntityState.data.isFront)) {
                return true;
            }
        }
        return false;
    }
    
    #endregion

    #region External

    public void SetActiveTab(EditTabEnum aEditTabEnum) {
        EditorState newEditorState = EditorState.SetActiveTab(this.currentState, aEditTabEnum);
        UpdateEditorState(newEditorState);
        switch (aEditTabEnum) {
            case EditTabEnum.PICKER:
                this.inputStateMachine.ChangeState(new EditorPickerModeInputState());
                break;
            case EditTabEnum.EDIT:
                this.inputStateMachine.ChangeState(new EditorEditModeInputState());
                break;
            case EditTabEnum.OPTIONS:
                this.inputStateMachine.ChangeState(new EditorOptionsModeInputState());
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(aEditTabEnum), aEditTabEnum, null);
        }
    }
    
    public void SetLayer(bool aIsFront) {
        EditorState newEditorState = EditorState.SetIsFront(this.currentState, aIsFront);
        UpdateEditorState(newEditorState);
    }

    public void SetPar(bool aIsUp) {
        int par = GM.boardManager.currentState.par;
        if (aIsUp) {
            par += 1;
        } else {
            par -= 1;
        }
        GM.boardManager.SetPar(par);
    }
    
    public void SetSelectedSchema(EntitySchema aEntitySchema) {
        EditorState newEditorState = EditorState.SetSelectedSchema(this.currentState, aEntitySchema);
        UpdateEditorState(newEditorState);
    }

    public void ClearSelectedSchema() {
        EditorState newEditorState = EditorState.ClearSelectedSchema(this.currentState);
        UpdateEditorState(newEditorState);
    }
    
    public EntityState GetSelectedEntity() {
        if (this.currentState.selectedEntityId != Constants.PLACEHOLDERINT) {
            return GM.boardManager.GetEntityById(this.currentState.selectedEntityId);
        }
        else {
            throw new Exception("GetSelectedEntity - no entity selected");
        }
    }

    public void OnIsFixedToggleValueChanged(bool aIsFixed) {
        if (this.currentState.hasSelectedEntity) {
            GM.boardManager.SetEntityIsFixed(this.currentState.selectedEntityId, aIsFixed);
        }
    }

    public void OnColorSubmit(Color aColor) {
        if (this.currentState.hasSelectedEntity) {
            GM.boardManager.SetEntityDefaultColor(this.currentState.selectedEntityId, aColor);
        }
    }
    
    public void OnDeleteButtonClicked() {
        if (this.currentState.hasSelectedEntity) {
            GM.boardManager.RemoveEntity(this.currentState.selectedEntityId, true);
            ClearSelectedEntity();
        }
    }

    public void OnFlipButtonClicked() {
        if (this.currentState.hasSelectedEntity) {
            EntityState selectedEntity = GetSelectedEntity();
            if (Util.IsDirection(selectedEntity.facing)) {
                if (selectedEntity.facing == Vector2Int.left) {
                    GM.boardManager.SetEntityFacing(this.currentState.selectedEntityId, Vector2Int.right);
                }
                else if (selectedEntity.facing == Vector2Int.right) {
                    GM.boardManager.SetEntityFacing(this.currentState.selectedEntityId, Vector2Int.left);
                }
                else {
                    throw new Exception("OnFlipButtonClicked - selectedEntityFacing isn't left or right");
                }
            }
        }
    }

    public void OnSaveButtonClicked() {
        GM.boardManager.SaveBoardState(false);
    }

    public void OnLoadButtonClicked() {
        GM.instance.SetFilePickerActive(true);
    }
    
    public void OnPlaytestButtonClicked() {
        GM.boardManager.SaveBoardState(true);
        GM.instance.SetGameMode(GameModeEnum.PLAYTESTING);
    }

    public void OnFilePickerLoadButtonClicked(string aFilename) {
        GM.boardManager.LoadBoardStateFromFile(aFilename);
    }

    #endregion

    #region StateMachineStates

    class EditorPickerModeInputState : StateMachineState {
        EntityBase selectedEntityBase;
        
        public void Enter() {
            Debug.Assert(GM.editManager.currentState.activeTab == EditTabEnum.PICKER);
        }

        public void Update() {
            if (GM.editManager.currentState.selectedSchema != null) {
                EntitySchema selectedSchema = GM.editManager.currentState.selectedSchema;
                if (GM.inputManager.mouseState == MouseStateEnum.CLICKED) {
                    GM.editManager.AddSchema(GM.inputManager.mousePosV2, selectedSchema);
                }
                if (GM.inputManager.rightMouseState == MouseStateEnum.CLICKED) {
                    GM.editManager.ClearSelectedSchema();
                }
            }
            else {
                switch (GM.inputManager.mouseState) {
                    case MouseStateEnum.DEFAULT:
                        break;
                    case MouseStateEnum.CLICKED:
                        EntityState? clickedEntity = GM.boardManager.GetEntityAtMousePos(GM.editManager.currentState.isFront);
                        // if clickedEntityExists and is not a boundary
                        if (clickedEntity?.data.isBoundary == false) {
                            // select this entity in state and store the entityBase locally
                            EntityState selectedEntity = clickedEntity.Value;
                            GM.editManager.SetSelectedEntity(selectedEntity.data.id);
                            this.selectedEntityBase = selectedEntity.entityBase;
                        }
                        else {
                            GM.editManager.ClearSelectedEntity();
                        }
                        break;
                    case MouseStateEnum.HELD:
                        if (GM.editManager.currentState.hasSelectedEntity) {
                            Vector2Int newPos = GM.editManager.GetSelectedEntity().pos + GM.inputManager.dragOffsetV2;
                            this.selectedEntityBase.SetTempViewPosition(newPos);
                        }
                        break;
                    case MouseStateEnum.RELEASED:
                        if (GM.editManager.currentState.hasSelectedEntity) {
                            EntityState selectedEntity = GM.editManager.GetSelectedEntity();
                            Vector2Int newPos = selectedEntity.pos + GM.inputManager.dragOffsetV2;
                            // if entity can move
                            if (GM.editManager.CanMoveTo(newPos, selectedEntity)) {
                                // move this entity
                                GM.boardManager.MoveEntity(selectedEntity.data.id, newPos, true);
                            }
                            else {
                                selectedEntity.entityBase.ResetView();
                            }
                        }
                        GM.editManager.ClearSelectedEntity();
                        this.selectedEntityBase = null;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        public void Exit() {
        }
    }

    class EditorEditModeInputState : StateMachineState {

        public void Enter() {
            Debug.Assert(GM.editManager.currentState.activeTab == EditTabEnum.EDIT);
        }

        public void Update() {
            if (GM.inputManager.mouseState == MouseStateEnum.CLICKED) {
                EntityState? maybeAEntity = GM.boardManager.GetEntityAtMousePos(GM.editManager.currentState.isFront);
                if (maybeAEntity.HasValue) {
                    int id = maybeAEntity.Value.data.id;
                    GM.editManager.SetSelectedEntity(id);
                }
                else {
                    GM.editManager.ClearSelectedEntity();
                }
            }

            if (GM.inputManager.rightMouseState == MouseStateEnum.CLICKED) {
                GM.editManager.ClearSelectedEntity();
            }
        }

        public void Exit() {
        }
    }

    class EditorOptionsModeInputState : StateMachineState {
        
        public void Enter() {
            Debug.Assert(GM.editManager.currentState.activeTab == EditTabEnum.OPTIONS);
        }

        public void Update() {
        }

        public void Exit() {
        }
    }

    #endregion
}