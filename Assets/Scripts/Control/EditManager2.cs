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
        if (0 < aTitle.Length || aTitle.Length <= 32) {
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

    // Start is called before the first frame update
    public StateMachine stateMachine = new StateMachine();
    
    public void Init() {
        this.stateMachine = new StateMachine();
        this.stateMachine.ChangeState(GetEditorGameState(this.currentState));
        this.currentState = new EditorState();
        this.currentState.Init();
    }



    void Update() {
        this.stateMachine.Update();
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

    public void SetLevelTitle(string aTitle) {
        if (0 < aTitle.Length || aTitle.Length < 32) {
            GM.boardData.title = aTitle;

        }
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
                    break;
                case MouseStateEnum.RELEASED:
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