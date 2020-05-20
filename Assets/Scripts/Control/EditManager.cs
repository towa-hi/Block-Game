using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using Sirenix.OdinInspector;

[RequireComponent(typeof(BoardManager))]
public class EditManager : SerializedMonoBehaviour {

    [Header("Set In Editor")]
    public EditPanelBase editPanelBase;
    public FilePickerBase filePickerBase;
    public StateMachine stateMachine = new StateMachine();

    public void Init() {
        this.stateMachine.ChangeState(new EditTabPickerModeMoveState());
        SetEditMode(EditModeEnum.PICKER);
        this.editPanelBase.SetOptionsModeTitleField(GM.boardData.title);
        SetEditModeEntity(null);
    }

    void Update() {
        this.stateMachine.Update();
        if (this.stateMachine.GetState() is EditTabPickerModePlaceState) {
            if (GM.inputManager.rightMouseState == MouseStateEnum.CLICKED) {
                GM.editManager.stateMachine.ChangeState(new EditTabPickerModeMoveState());
            }
        }
        
    }

    public void SetEditMode(EditModeEnum aEditMode) {
        switch (aEditMode) {
            case EditModeEnum.PICKER:
                this.stateMachine.ChangeState(new EditTabPickerModeMoveState());
                break;
            case EditModeEnum.EDIT:
                this.stateMachine.ChangeState(new EditTabEditModeState());
                break;
            case EditModeEnum.OPTIONS:
                this.stateMachine.ChangeState(new EditTabOptionsModeState());
                this.editPanelBase.SetOptionsModeTitleField(GM.boardData.title);
                break;
        }
    }

    public void SetEditModeEntity(EntityData aEntityData) {
        if (aEntityData != null) {
            print("set edit entity to " + aEntityData.name);
        }
        this.editPanelBase.SetEditModeEntity(aEntityData);
    }

    // picker mode

    public void OnPickerModeItemClick(EntitySchema aEntitySchema) {
        this.stateMachine.ChangeState(new EditTabPickerModePlaceState(aEntitySchema));
    }

    // edit mode

    public void OnEditModeColorPickerClick(Color aColor) {
        if (this.stateMachine.GetState() is EditTabEditModeState) {
            EditTabEditModeState state = this.stateMachine.GetState() as EditTabEditModeState;
            state.ChangeEntityColor(aColor);
        }
    }
    public void OnEditModeFixedToggle(bool aIsFixed) {
        if (this.stateMachine.GetState() is EditTabEditModeState) {
            EditTabEditModeState state = this.stateMachine.GetState() as EditTabEditModeState;
            state.ChangeEntityFixed(aIsFixed);
        }
        
    }

    public void OnEditModeNodeToggle(NodeToggleStruct aNodeToggleStruct) {
        if (this.stateMachine.GetState() is EditTabEditModeState) {
            EditTabEditModeState state = this.stateMachine.GetState() as EditTabEditModeState;
            state.ChangeEntityNodes(aNodeToggleStruct);
        }
    }
    
    public void OnEditModeExtraButtonClick() {
        print("extra button clicked");
    }

    public void OnEditModeFlipButtonClick() {
        if (this.stateMachine.GetState() is EditTabEditModeState) {
            EditTabEditModeState state = this.stateMachine.GetState() as EditTabEditModeState;
            state.FlipEntity();
        }
    }

    public void OnEditModeDeleteButtonClick() {
        print("EditManager - on delete button clicked");
        if (this.stateMachine.GetState() is EditTabEditModeState) {
            EditTabEditModeState state = this.stateMachine.GetState() as EditTabEditModeState;
            state.DeleteEntity();
        }
    }

    // options mode

    public void OnOptionsModeTitleChange(string aTitle) {
        print("new title:" + aTitle);
        GM.boardData.title = aTitle;
    }

    public void OnOptionsModeParIntPickerChange(int aPar) {
        print("new par: " + aPar);
        GM.boardData.par = aPar;
    }

    public void OnOptionsModeLoadButtonClick() {
        print("load button clicked");
        // SaveLoad.LoadBoard();
        GM.I.ToggleFullPauseGame(true);
        this.filePickerBase.gameObject.SetActive(true);
    }

    public void OnOptionsModeSaveButtonClick() {
        print("save button clicked");
        if (SaveLoad.IsValidSave(GM.boardData)) {
            SaveLoad.SaveBoard(GM.boardData);
        } else {
            print("invalid save");
        }
        
    }

    public void OnOptionsModePlaytestButtonClick() {
        print("playtest button clicked");
        if (SaveLoad.IsValidSave(GM.boardData)) {
            GM.I.SetGameMode(GameModeEnum.PLAYTESTING);
            SaveLoad.SaveBoard(GM.boardData, true);
        } else {
            print("invalid save");
        }
    }

    public void LoadLevelFromFilePicker(string aFilename) {
        GM.I.LoadBoard(SaveLoad.LoadBoard(aFilename));
        EndFilePicker();
    }

    public void EndFilePicker() {
        this.filePickerBase.gameObject.SetActive(false);
        GM.I.ToggleFullPauseGame(false);

    }
}

#region editStates

public class EditTabPickerModeMoveState : GameState {
    [SerializeField]
    EntityData entityData;
    [SerializeField]
    Vector2Int movePos;
    [SerializeField]
    Vector2Int clickPosOffset;
    [SerializeField]
    bool isMoveValid;
    
    public EditTabPickerModeMoveState() {
    }

    public void Enter() {
        // Debug.Log("EditTabPickerModeState - entering"); 
        GM.cursorBase.SetVisible(true);      
    }

    public void Update() {
        // Debug.Log("EditTabPickerModeState - updating");
        CursorUpdate();
        switch (GM.inputManager.mouseState) {
            case MouseStateEnum.CLICKED:
                EntityData hoveredEntity = GM.boardData.GetEntityDataAtPos(GM.inputManager.mousePosV2);
                if (hoveredEntity != null) {
                    if (hoveredEntity.IsMovableInPickerMode()) {
                        this.entityData = hoveredEntity;
                        this.clickPosOffset = GM.inputManager.mousePosV2 - this.entityData.pos;
                    }
                }
                break;
            case MouseStateEnum.HELD:
                if (this.entityData != null) {
                    this.isMoveValid = GM.boardData.IsRectEmpty(this.movePos, this.entityData.size, this.entityData);
                    this.movePos = GM.inputManager.mousePosV2 - this.clickPosOffset;
                    this.entityData.entityBase.SetViewPosition(this.movePos);
                }
                break;
            case MouseStateEnum.RELEASED:
                if (this.entityData != null) {
                    if (this.isMoveValid) {
                        GM.boardData.MoveEntity(this.movePos, this.entityData);
                    }
                    this.entityData.entityBase.ResetViewPosition();
                    this.entityData = null;
                }
                break;
        }
    }

    void CursorUpdate() {
        if (this.entityData != null) {
            GM.cursorBase.SetSize(this.entityData.size);
            GM.cursorBase.SetPos(this.movePos);
            if (isMoveValid) {
                GM.cursorBase.SetColor(Color.green);
            } else {
                GM.cursorBase.SetColor(Color.red);
            }
        } else {
            GM.cursorBase.SetColor(Color.white);
            EntityData maybeAEntity = GM.boardData.GetEntityDataAtPos(GM.inputManager.mousePosV2);
            if (maybeAEntity != null && !maybeAEntity.isBoundary) {
                GM.cursorBase.SetAsEntity(maybeAEntity);
            } else {
                GM.cursorBase.ResetCursorOnMousePos();
            }
        }
    }

    public void Exit() {
        GM.cursorBase.SetVisible(false);
        // Debug.Log("EditTabPickerModeState - exiting");
    }
}

public class EditTabPickerModePlaceState : GameState {
    [SerializeField]
    EntitySchema entitySchema;
    [SerializeField]
    bool isPlacementValid;

    public EditTabPickerModePlaceState(EntitySchema aEntitySchema) {
        this.entitySchema = aEntitySchema;
    }

    public void Enter() {
        // Debug.Log("EditTabPickerModePlaceState - entering");
    }

    public void Update() {
        // Debug.Log("EditTabPickerModePlaceState - updating");
        CursorUpdate();
        this.isPlacementValid = GM.boardData.IsRectEmpty(GM.inputManager.mousePosV2, this.entitySchema.size);
        if (GM.inputManager.mouseState == MouseStateEnum.CLICKED) {
            if (this.isPlacementValid) {
                EntityData entityData = new EntityData(this.entitySchema, GM.inputManager.mousePosV2, Constants.DEFAULTFACING, Constants.DEFAULTCOLOR);
                GM.boardManager.CreateEntityFromData(entityData);
            }
        }
    }

    void CursorUpdate() {
        GM.cursorBase.SetVisible(true);
        GM.cursorBase.SetSize(this.entitySchema.size);
        GM.cursorBase.SetPos(GM.inputManager.mousePosV2);
        if (this.isPlacementValid) {
            GM.cursorBase.SetColor(Color.green);
        } else {
            GM.cursorBase.SetColor(Color.red);
        }
    }
    
    public void Exit() {
        GM.cursorBase.SetVisible(false);
        // Debug.Log("EditTabPickerModePlaceState - exiting");
    }
}

public class EditTabEditModeState : GameState {
    [SerializeField]
    EntityData entityData;

    public EditTabEditModeState() {
        Debug.Log("Instantiating EditTabEditModeState setting entityData to null");
        this.entityData = null;
    }

    public void Enter() {
        // Debug.Log("EditTabEditModeState - entering");
        GM.cursorBase.SetColor(Color.white);
        GM.cursorBase.SetVisible(true);
        GM.editManager.SetEditModeEntity(null);
    }

    public void Update() {
        UpdateCursor();
        // Debug.Log("EditTabEditModeState - updating");
        if (GM.inputManager.mouseState == MouseStateEnum.CLICKED) {
            Debug.Log("setting entityData");
            this.entityData = GM.boardData.GetEntityDataAtPos(GM.inputManager.mousePosV2);
            GM.editManager.SetEditModeEntity(this.entityData);
        } else if (GM.inputManager.rightMouseState == MouseStateEnum.CLICKED) {
            this.entityData = null;
            GM.editManager.SetEditModeEntity(null);
        }
    }

    void UpdateCursor() {
        if (this.entityData != null) {
            GM.cursorBase.SetAsEntity(this.entityData);
        } else {
            GM.cursorBase.SetColor(Color.white);
            EntityData maybeAEntity = GM.boardData.GetEntityDataAtPos(GM.inputManager.mousePosV2);
            if (maybeAEntity != null) {
                GM.cursorBase.SetAsEntity(maybeAEntity);
                GM.cursorBase.SetColor(Color.green);
            } else {
                GM.cursorBase.ResetCursorOnMousePos();
            }
        }
    }


    public void ChangeEntityNodes(NodeToggleStruct aNodeToggleStruct) {
        INodal nodal = this.entityData.entityBase.GetCachedIComponent<INodal>() as INodal;
        Vector2Int currentPos = aNodeToggleStruct.node;
        if (aNodeToggleStruct.toggled) {
            nodal.AddNode(aNodeToggleStruct.node, aNodeToggleStruct.upDown);
        } else {
            nodal.RemoveNode(aNodeToggleStruct.node, aNodeToggleStruct.upDown);
        }
        GM.editManager.SetEditModeEntity(this.entityData);
    } 

    public void ChangeEntityColor(Color aColor) {
        this.entityData.SetDefaultColor(aColor);
    }

    public void ChangeEntityFixed(bool aIsFixed) {
        this.entityData.isFixed = aIsFixed;
    }

    public void FlipEntity() {
        this.entityData.FlipEntity();
    }

    public void DeleteEntity() {
        GM.editManager.SetEditModeEntity(null);
        GM.boardManager.DestroyEntity(this.entityData);
        this.entityData = null;
        Debug.Log("EditTabEditModeState - deleted entity");
    }

    public void Exit() {
        GM.cursorBase.SetVisible(false);
        // Debug.Log("EditTabEditModeState - exiting");
    }
}

public class EditTabOptionsModeState : GameState {

    public EditTabOptionsModeState() {
    }

    public void Enter() {
        // Debug.Log("EditTabOptionsModeState - entering");
    }

    public void Update() {
        // Debug.Log("EditTabOptionsModeState - updating");
    }

    public void Exit() {
        // Debug.Log("EditTabOptionsModeState - exiting");
    }
}

#endregion