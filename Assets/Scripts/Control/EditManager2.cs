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
    public EditTabEnum activeTab;
    public List<EntitySchema> frontContentList;
    public List<BgSchema> backContentList;
    public Object currentSchema;
    public Object selectedObject;
    
    public void Init() {
        this.isFront = true;
        this.frontContentList = Resources.LoadAll("ScriptableObjects/Entities", typeof(EntitySchema)).Cast<EntitySchema>().ToList();
        this.frontContentList.OrderBy(entitySchema => entitySchema.name.ToLower());
        this.backContentList = Resources.LoadAll("ScriptableObjects/Bg", typeof(BgSchema)).Cast<BgSchema>().ToList();
        this.backContentList.OrderBy(bgSchema => bgSchema.name.ToLower());
        this.par = GM.boardData.par;
        
    }

    public static EditorState SetCurrentSchema(EditorState aState, Object aCurrentSchema) {
        // assert that if isFront, aCurrentSchema is a EntitySchema otherwise if !isFront, aCurrentSchema is a BgSchema
        Debug.Assert((aState.isFront && aCurrentSchema is EntitySchema) || (!aState.isFront && aCurrentSchema is BgSchema));
        aState.currentSchema = aCurrentSchema;
        return aState;
    }

    public static EditorState SetIsFront(EditorState aState, bool aIsFront) {
        aState.isFront = aIsFront;
        return aState;
    }

    public static EditorState SetPar(EditorState aState, int aPar) {
        if (0 < aPar && aPar < Constants.MAXPAR) {
            aState.par = aPar;
        } else {
            Debug.Log("SetPar failed to set par");
        }
        aState.UpdateBoard();
        return aState;
    }

    public static EditorState SetActiveTabIndex(EditorState aState, EditTabEnum aEditTab) {
        aState.activeTab = aEditTab;
        return aState;
    }

    void UpdateBoard() {
        GM.boardData.par = this.par;
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
        this.stateMachine.ChangeState(new EditorPickerState());
        this.currentState = new EditorState();
        this.currentState.Init();
        
    }

    void Update() {

    }

    public EditorState GetState() {
        return currentState;
    }

    public void UpdateState(EditorState aEditorState) {
        this.currentState = aEditorState;
        OnUpdateState?.Invoke();
    }

}

public class EditorPickerState : GameState {
    public bool isFront;
    public bool isPlacementValid;

    // if moving
    public EntityData selectedEntityData;
    // if not moving
    public EntitySchema selectedEntitySchema;

    public void Enter() {
        GM.cursorBase.SetVisible(true);
    }

    public void Update() {

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