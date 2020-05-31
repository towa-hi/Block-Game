using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

public delegate void OnUpdateEditorStateHandler(EditorState aEditorState);

[RequireComponent(typeof(BoardManager))]
public class EditManager : SerializedMonoBehaviour {
    [SerializeField]
    EditorState currentState;
    public EditorState editorState {
        get {
            return this.currentState;
        }
    }
    public event OnUpdateEditorStateHandler OnUpdateEditorState;
    [SerializeField]
    StateMachine inputStateMachine = new StateMachine();
    
    void Awake() {
        UpdateEditorState(EditorState.CreateEditorState());
    }

    public void UpdateEditorState(EditorState aEditorState) {
        print("EditManager - Updating EditorState for " + this.OnUpdateEditorState?.GetInvocationList().Length + " delegates");
        this.currentState = aEditorState;
        this.OnUpdateEditorState?.Invoke(this.editorState);
    }
    
    // special function called by GM.OnUpdateGameState delegate
    public void OnUpdateGameState(GameState aGameState) {
        if (aGameState.gameMode == GameModeEnum.EDITING) {
            // turn self on
        }
    }

    public void AddSchema(Vector2Int aPos, Object aSchema) {
        if (aSchema is EntitySchema) {
            EntitySchema entitySchema = aSchema as EntitySchema;
            if (GM.boardManager.CanEditorPlaceSchema(aPos, entitySchema)) {
                GM.boardManager.AddEntity(entitySchema, aPos, Constants.DEFAULTFACING, Constants.DEFAULTCOLOR, false, false);
            }
        }
    }

    public void SetActiveTab(EditTabEnum aEditTabEnum) {
        print("SetActiveTab - changing tab");
        EditorState newEditorState = EditorState.SetActiveTab(this.currentState, aEditTabEnum);
        UpdateEditorState(newEditorState);
    }
    
    public void SetLayer(bool aIsFront) {
        EditorState newEditorState = EditorState.SetIsFront(this.currentState, aIsFront);
        UpdateEditorState(newEditorState);
    }

    // public void Init() {
    //     this.inputStateMachine = new StateMachine();
    //     this.currentState = new EditorState();
    //     this.currentState.Init();
    //     this.inputStateMachine.ChangeState(GetEditorGameState(GetState()));
    // }

    // void Update() {
    //     if (GetState().activeTab != this.activeTab) {
    //         this.inputStateMachine.ChangeState(GetEditorGameState(GetState()));
    //         this.activeTab = GetState().activeTab;
    //     }
    //     this.inputStateMachine.Update();
    // }

    // public EditorState GetState() {
    //     return currentState;
    // }

    // public void UpdateState(EditorState aEditorState) {
    //     this.currentState = aEditorState;
    //     // GM.boardData.par = aEditorState.par;
    //     // GM.boardData.title = aEditorState.title;
    //     OnUpdateState?.Invoke();
    // }

    // public void TryPlaceSchema(Vector2Int aPos, Object aSchema) {
    //     // Debug.Assert((aSchema is EntitySchema || aSchema is BgSchema));
    //     // if (aSchema is EntitySchema) {
    //     //     EntitySchema entitySchema = aSchema as EntitySchema;
    //     //     if (GM.boardData.CanEditorPlaceEntitySchema(aPos, entitySchema)) {
    //     //         EntityData newEntityData = new EntityData(entitySchema, aPos, Constants.DEFAULTFACING, Constants.DEFAULTCOLOR);
    //     //         GM.boardManager.CreateEntityFromData(newEntityData);
    //     //     }
    //     // } else if (aSchema is BgSchema) {
    //     //     BgSchema bgSchema = aSchema as BgSchema;
    //     //     if (GM.boardData.backgroundData.CanEditorPlaceBgSchema(aPos, bgSchema)) {
    //     //         BgData newBgData = new BgData(bgSchema, aPos, Constants.DEFAULTCOLOR);
    //     //         GM.boardManager.CreateBgFromData(newBgData);
    //     //     }
    //     // }
    // }

    
    // public void TryMoveEntity(Vector2Int aPos, EntityData aEntityData) {
    //     // if (CanMoveEntityInEditor(aEntityData)) {
    //     //     if (GM.boardData.IsRectEmpty(aPos, aEntityData.size, aEntityData)) {
    //     //         GM.boardManager.MoveEntityAndView(aPos, aEntityData);
    //     //     }
    //     // }
    // }

    // public void TryMoveBg(Vector2Int aPos, BgData aBgData) {
    //     // if (GM.boardData.backgroundData.IsRectEmpty(aPos, aBgData.size, aBgData)) {
    //     //     GM.boardManager.MoveBgAndView(aPos, aBgData);
    //     // }
    // }

    // static bool CanMoveEntityInEditor(EntityData aEntityData) {
    //     if (aEntityData.isBoundary) {
    //         return false;
    //     }
    //     return true;
    // }

    // static GameState GetEditorGameState(EditorState aEditorState) {
    //     switch (aEditorState.activeTab) {
    //         case EditTabEnum.PICKER:
    //             return new EditorPickerState2();
    //         case EditTabEnum.EDIT:
    //             return new EditorEditState2();
    //         case EditTabEnum.OPTIONS:
    //             return new EditorOptionsState2();
    //     }
    //     throw new System.Exception("unrecognized EditTabEnum");
    // }
}

// public class EditorPickerState2 : GameState {
//     // TODO: make clickposoffset work
//     public EditorPickerState2() {

//     }

//     public void Enter() {
        
//     }

//     public void Update() {
//         // EditorState currentState = GM2.editManager3.GetState();
//         // if (currentState.selectedSchema != null) {
//         //     switch (GM2.inputManager2.mouseState) {
//         //         case MouseStateEnum.CLICKED:
//         //             GM2.editManager2.TryPlaceSchema(GM.inputManager.mousePosV2, currentState.selectedSchema);
                    
//         //             break;
//         //     }
//         //     if (GM.inputManager.rightMouseState == MouseStateEnum.CLICKED) {
//         //         // clear placement selection
//         //         GM.editManager2.UpdateState(EditorState.ClearSchema(currentState));
//         //     }
//         // } else {
//         //     switch (GM.inputManager.mouseState) {
//         //         case MouseStateEnum.CLICKED:
//         //             EditorState newClickedState = EditorState.SetSelectionToMousePos(currentState);
//         //             GM.editManager2.UpdateState(newClickedState);
//         //             break;
//         //         case MouseStateEnum.HELD:
//         //             if (currentState.selectedEntityData != null) {
//         //                 currentState.selectedEntityData.entityBase.SetViewPosition(currentState.selectedEntityData.pos + GM.inputManager.dragOffsetV2);
//         //             } else if (currentState.selectedBgData != null) {
//         //                 currentState.selectedBgData.bgBase.SetViewPosition(currentState.selectedBgData.pos + GM.inputManager.dragOffsetV2);
//         //             }
//         //             break;
//         //         case MouseStateEnum.RELEASED:
//         //             if (currentState.selectedEntityData != null) {
//         //                 GM.editManager2.TryMoveEntity(currentState.selectedEntityData.pos + GM.inputManager.dragOffsetV2, currentState.selectedEntityData);
//         //                 currentState.selectedEntityData.entityBase.ResetViewPosition();
//         //             } else if (currentState.selectedBgData != null) {
//         //                 GM.editManager2.TryMoveBg(currentState.selectedBgData.pos + GM.inputManager.dragOffsetV2, currentState.selectedBgData);
//         //                 currentState.selectedBgData.bgBase.ResetViewPosition();
//         //             }
//         //             EditorState newReleasedState = EditorState.ClearSelection(currentState);
//         //             GM.editManager2.UpdateState(newReleasedState);
//         //             break;
//         //     }
//         // }
//     }
    
//     void CursorUpdate() {
        
//     }
    
//     public void Exit() {

//     }
// }

// public class EditorEditState2 : GameState {

//     public void Enter() {

//     }

//     public void Update() {
//         // EditorState currentState = GM.editManager2.GetState();
//         // switch (GM.inputManager.mouseState) {
//         //     case MouseStateEnum.CLICKED:
//         //         EditorState newClickedState = EditorState.SetSelectionToMousePos(currentState);
//         //         GM.editManager2.UpdateState(newClickedState);
//         //         break;
//         // }
//         // if (GM.inputManager.rightMouseState == MouseStateEnum.CLICKED) {
//         //     EditorState newRightClickedState = EditorState.ClearSelection(currentState);
//         //     GM.editManager2.UpdateState(newRightClickedState);
//         // }
//     }

//     public void Exit() {

//     }
// }

// public class EditorOptionsState2 : GameState {

//     public void Enter() {

//     }

//     public void Update() {

//     }

//     public void Exit() {

//     }
// }