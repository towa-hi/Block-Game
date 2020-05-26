using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.UI;
using System.Linq;
using Sirenix.OdinInspector;

public struct EditorState {
    public bool isFront;
    public int par;
    public string title;
    public EditTabEnum activeTab;
    public List<EntitySchema> frontContentList;
    public List<BgSchema> backContentList;
    public Object selectedSchema;
    public EntityData selectedEntityData;
    public BgData selectedBgData;
    public bool isPlacing {
        get {
            return (this.selectedEntityData != null || this.selectedBgData != null);
        }
    }

    public void Init() {
        this.isFront = true;
        this.frontContentList = Resources.LoadAll("ScriptableObjects/Entities", typeof(EntitySchema)).Cast<EntitySchema>().ToList();
        this.frontContentList.OrderBy(entitySchema => entitySchema.name.ToLower());
        this.backContentList = Resources.LoadAll("ScriptableObjects/Bg", typeof(BgSchema)).Cast<BgSchema>().ToList();
        this.backContentList.OrderBy(bgSchema => bgSchema.name.ToLower());
        this.selectedSchema = null;
        this.selectedEntityData = null;
        this.selectedBgData = null;
        this.par = GM.boardData.par;
        this.title = GM.boardData.title;
    }

    public static EditorState SetCurrentSchema(EditorState aState, Object aCurrentSchema) {
        // assert that if isFront, aCurrentSchema is a EntitySchema otherwise if !isFront, aCurrentSchema is a BgSchema
        Debug.Assert((aState.isFront && aCurrentSchema is EntitySchema) || (!aState.isFront && aCurrentSchema is BgSchema));
        aState.selectedSchema = aCurrentSchema;
        return aState;
    }

    public static EditorState ClearSchema(EditorState aState) {
        aState.selectedSchema = null;
        return aState;
    }

    public static EditorState SetSelectionToMousePos(EditorState aState) {
        if (aState.isFront) {
            aState.selectedEntityData = GM.boardData.GetEntityDataAtPos(GM.inputManager.mousePosV2);
        } else {
            aState.selectedBgData = GM.boardData.backgroundData.GetBgDataAtPos(GM.inputManager.mousePosV2);
        }
        return aState;
    }

    public static EditorState ClearSelection(EditorState aState) {
        aState.selectedEntityData = null;
        aState.selectedBgData = null;
        return aState;
    }

    public static EditorState SetIsFront(EditorState aState, bool aIsFront) {
        aState.isFront = aIsFront;
        aState.selectedSchema = null;
        aState.selectedEntityData = null;
        aState.selectedBgData = null;
        // aState.cursorMode = CursorModeEnum.SELECTING;
        return aState;
    }

    public static EditorState SetPar(EditorState aState, int aPar) {
        if (0 < aPar && aPar <= Constants.MAXPAR) {
            aState.par = aPar;
        } else {
            Debug.Log("SetPar failed to set par");
        }
        return aState;
    }

    public static EditorState SetLevelTitle(EditorState aState, string aTitle) {
        // TODO: better input validation here
        if (0 < aTitle.Length && aTitle.Length <= 32) {
            aState.title = aTitle;
        }
        return aState;
    }

    public static EditorState SetActiveTab(EditorState aState, EditTabEnum aEditTab) {
        aState.activeTab = aEditTab;
        aState.selectedSchema = null;
        aState.selectedEntityData = null;
        aState.selectedBgData = null;
        return aState;
    }

    public static EditorState SetCursorSelect(EditorState aState, CursorModeEnum aCursorMode) {
        // aState.cursorMode = aCursorMode;
        return aState;
    }
}

[RequireComponent(typeof(BoardManager))]
public class EditManager2 : SerializedMonoBehaviour {
    [SerializeField]
    EditorState currentState;
    public delegate void OnUpdateStateHandler();
    public event OnUpdateStateHandler OnUpdateState;
    EditTabEnum activeTab;
    [SerializeField]
    StateMachine inputStateMachine = new StateMachine();
    
    public void Init() {
        this.inputStateMachine = new StateMachine();
        this.currentState = new EditorState();
        this.currentState.Init();
        this.inputStateMachine.ChangeState(GetEditorGameState(GetState()));
    }



    void Update() {
        if (GetState().activeTab != this.activeTab) {
            this.inputStateMachine.ChangeState(GetEditorGameState(GetState()));
            this.activeTab = GetState().activeTab;
        }
        this.inputStateMachine.Update();
    }

    public EditorState GetState() {
        return currentState;
    }

    public void UpdateState(EditorState aEditorState) {
        this.currentState = aEditorState;
        GM.boardData.par = aEditorState.par;
        GM.boardData.title = aEditorState.title;
        OnUpdateState?.Invoke();
    }

    public void TryPlaceSchema(Vector2Int aPos, Object aSchema) {
        Debug.Assert((aSchema is EntitySchema || aSchema is BgSchema));
        if (aSchema is EntitySchema) {
            EntitySchema entitySchema = aSchema as EntitySchema;
            if (GM.boardData.CanEditorPlaceEntitySchema(aPos, entitySchema)) {
                EntityData newEntityData = new EntityData(entitySchema, aPos, Constants.DEFAULTFACING, Constants.DEFAULTCOLOR);
                GM.boardManager.CreateEntityFromData(newEntityData);
            }
        } else if (aSchema is BgSchema) {
            BgSchema bgSchema = aSchema as BgSchema;
            if (GM.boardData.backgroundData.CanEditorPlaceBgSchema(aPos, bgSchema)) {
                BgData newBgData = new BgData(bgSchema, aPos, Constants.DEFAULTCOLOR);
                GM.boardManager.CreateBgFromData(newBgData);
            }
        }
    }

    
    public void TryMoveEntity(Vector2Int aPos, EntityData aEntityData) {
        if (EditManager2.CanMoveEntityInEditor(aEntityData)) {
            if (GM.boardData.IsRectEmpty(aPos, aEntityData.size, aEntityData)) {
                GM.boardManager.MoveEntityAndView(aPos, aEntityData);
            }
        }
    }

    public void TryMoveBg(Vector2Int aPos, BgData aBgData) {
        if (GM.boardData.backgroundData.IsRectEmpty(aPos, aBgData.size, aBgData)) {
            GM.boardManager.MoveBgAndView(aPos, aBgData);
        }
    }

    static bool CanMoveEntityInEditor(EntityData aEntityData) {
        if (aEntityData.isBoundary) {
            return false;
        }
        return true;
    }

    static GameState GetEditorGameState(EditorState aEditorState) {
        switch (aEditorState.activeTab) {
            case EditTabEnum.PICKER:
                return new EditorPickerState();
            case EditTabEnum.EDIT:
                return new EditorEditState();
            case EditTabEnum.OPTIONS:
                return new EditorOptionsState();
        }
        throw new System.Exception("unrecognized EditTabEnum");
    }
}

public class EditorPickerState : GameState {
    // TODO: make clickposoffset work
    public EditorPickerState() {

    }

    public void Enter() {
        
    }

    public void Update() {
        EditorState currentState = GM.editManager2.GetState();
        if (currentState.selectedSchema != null) {
            switch (GM.inputManager.mouseState) {
                case MouseStateEnum.CLICKED:
                    GM.editManager2.TryPlaceSchema(GM.inputManager.mousePosV2, currentState.selectedSchema);
                    
                    break;
            }
            if (GM.inputManager.rightMouseState == MouseStateEnum.CLICKED) {
                // clear placement selection
                GM.editManager2.UpdateState(EditorState.ClearSchema(currentState));
            }
        } else {
            switch (GM.inputManager.mouseState) {
                case MouseStateEnum.CLICKED:
                    EditorState newClickedState = EditorState.SetSelectionToMousePos(currentState);
                    GM.editManager2.UpdateState(newClickedState);
                    break;
                case MouseStateEnum.HELD:
                    if (currentState.selectedEntityData != null) {
                        currentState.selectedEntityData.entityBase.SetViewPosition(GM.inputManager.mousePosV2);
                    } else if (currentState.selectedBgData != null) {
                        currentState.selectedBgData.bgBase.SetViewPosition(GM.inputManager.mousePosV2);
                    }
                    break;
                case MouseStateEnum.RELEASED:
                    if (currentState.selectedEntityData != null) {
                        GM.editManager2.TryMoveEntity(GM.inputManager.mousePosV2, currentState.selectedEntityData);
                        currentState.selectedEntityData.entityBase.ResetViewPosition();
                    } else if (currentState.selectedBgData != null) {
                        GM.editManager2.TryMoveBg(GM.inputManager.mousePosV2, currentState.selectedBgData);
                        currentState.selectedBgData.bgBase.ResetViewPosition();
                    }
                    EditorState newReleasedState = EditorState.ClearSelection(currentState);
                    GM.editManager2.UpdateState(newReleasedState);
                    break;
            }
        }
    }
    
    void CursorUpdate() {
        
    }
    
    public void Exit() {

    }
}

public class EditorEditState : GameState {

    public void Enter() {

    }

    public void Update() {
        EditorState currentState = GM.editManager2.GetState();
        switch (GM.inputManager.mouseState) {
            case MouseStateEnum.CLICKED:
                EditorState newClickedState = EditorState.SetSelectionToMousePos(currentState);
                GM.editManager2.UpdateState(newClickedState);
                break;
        }
        if (GM.inputManager.rightMouseState == MouseStateEnum.CLICKED) {
            EditorState newRightClickedState = EditorState.ClearSelection(currentState);
            GM.editManager2.UpdateState(newRightClickedState);
        }
    }

    public void Exit() {

    }
}

public class EditorOptionsState : GameState {

    public void Enter() {

    }

    public void Update() {

    }

    public void Exit() {

    }
}