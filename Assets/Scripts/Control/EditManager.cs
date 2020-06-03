using System;
using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using UnityEditor;

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
    EditorState oldState;
    
    void Awake() {
        UpdateEditorState(EditorState.CreateEditorState());
        // initialize input state machine
        this.inputStateMachine.ChangeState(new EditorPickerModeInputState());
    }
    
    void Update() {
        this.inputStateMachine.Update();
    }
    
    public void UpdateEditorState(EditorState aEditorState) {
        print("EditManager - Updating EditorState for " + this.OnUpdateEditorState?.GetInvocationList().Length + " delegates");
        this.oldState = this.editorState;
        this.editorState = aEditorState;
        this.OnUpdateEditorState?.Invoke(this.currentState);
    }
    
    // special function called by GM.OnUpdateGameState delegate
    public void OnUpdateGameState(GameState aGameState) {
        if (aGameState.gameMode == GameModeEnum.EDITING) {
            // turn self on
        }
    }
    
    public void AddSchema(Vector2Int aPos, EntitySchema aEntitySchema) {
        if (GM.boardManager.CanEditorPlaceSchema(aPos, aEntitySchema)) {
            GM.boardManager.AddEntity(aEntitySchema, aPos, Constants.DEFAULTFACING, Constants.DEFAULTCOLOR);
        }
    }

    public void SetActiveTab(EditTabEnum aEditTabEnum) {
        print("SetActiveTab - changing tab");
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

    public void PlaceSelectedSchema(Vector2Int aPos, EntitySchema aEntitySchema) {
        if (GM.boardManager.IsRectEmpty(aPos, aEntitySchema.size)) {
            GM.boardManager.AddEntity(aEntitySchema, aPos, Constants.DEFAULTFACING, Constants.DEFAULTCOLOR);
        }
    }

    public void SetSelectedEntity(int aId) {
        EditorState newEditorState = EditorState.SetSelectedEntityId(this.currentState, true, aId);
        UpdateEditorState(newEditorState);
    }

    public void ResetSelectedEntity() {
        EditorState newEditorState = EditorState.SetSelectedEntityId(this.currentState, false);
        UpdateEditorState(newEditorState);
    }
    
    public EntityState GetSelectedEntity() {
        return GM.boardManager.GetEntityById(this.currentState.selectedEntityId);
    }

    public bool CanMoveTo(Vector2Int aPos, EntityState aEntityState) {
        if (!aEntityState.isBoundary) {
            if (GM.boardManager.IsRectEmpty(aPos, aEntityState.size, new HashSet<EntityState> {aEntityState})) {

                return true;
            }
        }
        return false;
    }
    
    class EditorPickerModeInputState : StateMachineState {
        EntityBase selectedEntityBase;
        
        public void Enter() {
            GM.editManager.ResetSelectedEntity();
            Debug.Assert(GM.editManager.currentState.activeTab == EditTabEnum.PICKER);
        }

        public void Update() {
            if (GM.editManager.currentState.selectedSchema != null) {
                EntitySchema selectedSchema = GM.editManager.currentState.selectedSchema;
                if (GM.inputManager.mouseState == MouseStateEnum.CLICKED) {
                    if (GM.boardManager.CanEditorPlaceSchema(GM.inputManager.mousePosV2, selectedSchema)) {
                        GM.editManager.PlaceSelectedSchema(GM.inputManager.mousePosV2, selectedSchema);
                    }
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
                        EntityState? clickedEntity = GM.boardManager.GetEntityAtMousePos();
                        // if clickedEntityExists and is not a boundary
                        if (clickedEntity?.isBoundary == false) {
                            // select this entity in state and store the entityBase locally
                            EntityState selectedEntity = clickedEntity.Value;
                            GM.editManager.SetSelectedEntity(selectedEntity.id);
                            this.selectedEntityBase = selectedEntity.entityBase;
                        }
                        else {
                            GM.editManager.ResetSelectedEntity();
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
                                GM.boardManager.MoveEntity(newPos, selectedEntity);
                            }
                            else {
                                selectedEntity.entityBase.ResetTempView();
                            }
                        }
                        GM.editManager.ResetSelectedEntity();
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
            GM.editManager.ResetSelectedEntity();
            Debug.Assert(GM.editManager.currentState.activeTab == EditTabEnum.EDIT);
        }

        public void Update() {
            if (GM.inputManager.mouseState == MouseStateEnum.CLICKED) {
                EntityState? maybeAEntity = GM.boardManager.GetEntityAtMousePos();
                if (maybeAEntity.HasValue) {
                    int id = maybeAEntity.Value.id;
                    GM.editManager.SetSelectedEntity(id);
                }
                else {
                    GM.editManager.ResetSelectedEntity();
                }
            }

            if (GM.inputManager.rightMouseState == MouseStateEnum.CLICKED) {
                GM.editManager.ResetSelectedEntity();
            }
        }

        public void Exit() {
        }
    }

    class EditorOptionsModeInputState : StateMachineState {
        
        public void Enter() {
            GM.editManager.ResetSelectedEntity();
            Debug.Assert(GM.editManager.currentState.activeTab == EditTabEnum.OPTIONS);
        }

        public void Update() {
        }

        public void Exit() {
        }
    }
}