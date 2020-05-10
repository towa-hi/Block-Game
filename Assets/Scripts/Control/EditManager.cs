using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using Sirenix.OdinInspector;

[RequireComponent(typeof(BoardManager))]
public class EditManager : SerializedMonoBehaviour {
    BoardManager boardManager;
    

    public EditModeEnum editMode;
    public Vector2Int clickPosOffset;
    [Header("Picker Mode")]
    [SerializeField] EntitySchema pickerModePlaceSchema;
    [SerializeField] EntityData pickerModeLastPlacedEntityData;
    public EntityData pickerModeMoveEntity;
    public Vector2Int pickerModeMovePos;

    public bool pickerModePlaceIsValid;
    [Header("Edit Mode")]
    public EntityData editModeClickedEntity;

    [Header("Set In Editor")]
    public PreviewCubeBase previewCubeBase;
    public PreviewStudioBase previewStudioBase;
    public EditPanelBase editPanelBase;
    public FilePickerBase filePickerBase;
    public CursorBase cursorBase;

    public void Init() {
        SetEditMode(EditModeEnum.PICKER);
        this.boardManager = GM.boardManager;
        this.pickerModePlaceSchema = null;
        this.pickerModeLastPlacedEntityData = null;
        this.editModeClickedEntity = null;
        this.pickerModeLastPlacedEntityData = null;
        this.editPanelBase.SetOptionsModeTitleField(GM.boardData.title);
        this.previewStudioBase.Init();
        
    }

    void Update() {
        switch (this.editMode) {
            case EditModeEnum.PICKER:
                PickerModeUpdate();
                break;
            case EditModeEnum.EDIT:
                EditModeUpdate();
                break;
            case EditModeEnum.OPTIONS:
                break;
        }
    }

    void PickerModeUpdate() {
        if (this.pickerModePlaceSchema != null) {
            PickerModePlaceUpdate();
        } else {
            PickerModeMoveUpdate();
        }
    }

    void PickerModePlaceUpdate() {
        this.previewCubeBase.SetPos(GM.inputManager.mousePosV2);
        this.pickerModePlaceIsValid = GM.boardData.IsRectEmpty(this.previewCubeBase.pos, this.previewCubeBase.size);
        switch (GM.inputManager.mouseState) {
            case MouseStateEnum.CLICKED:
                if (this.pickerModePlaceIsValid) {
                    PickerModePlaceOnClick();
                }
                break;
            case MouseStateEnum.HELD:
                break;
            case MouseStateEnum.RELEASED:
                break;
            case MouseStateEnum.DEFAULT:
                if (this.pickerModePlaceIsValid) {
                    this.previewCubeBase.SetColor(Color.green);
                } else {
                    this.previewCubeBase.SetColor(Color.red);
                }
                break;
        }
    }

    void PickerModePlaceOnClick() {
        EntityData newEntityData = new EntityData(this.pickerModePlaceSchema, this.previewCubeBase.pos, Vector2Int.right, Constants.DEFAULTCOLOR);
        if (GM.boardData.IsRectInBoard(this.previewCubeBase.pos, this.previewCubeBase.size)) {
            this.boardManager.CreateEntityFromData(newEntityData);
            this.pickerModeLastPlacedEntityData = newEntityData;
        }
    }

    void PickerModeMoveUpdate() {
        PickerModeMoveCursorUpdate();
        switch (GM.inputManager.mouseState) {
            case MouseStateEnum.CLICKED:
                EntityData hoveredEntity = GM.boardData.GetEntityDataAtPos(GM.inputManager.mousePosV2);
                if (hoveredEntity != null) {
                    if (hoveredEntity.IsMovableInPickerMode()) {
                        this.pickerModeMoveEntity = hoveredEntity;
                        this.pickerModeMoveEntity.entityView.SetGhost(true);
                        this.clickPosOffset = GM.inputManager.mousePosV2 - this.pickerModeMoveEntity.pos;
                        print(this.clickPosOffset);
                    }
                }
                break;
            case MouseStateEnum.HELD:
                if (this.pickerModeMoveEntity != null) {
                    this.pickerModeMovePos = GM.inputManager.mousePosV2 - this.clickPosOffset;
                    this.pickerModeMoveEntity.entityBase.SetViewPosition(this.pickerModeMovePos);
                }
                break;
            case MouseStateEnum.RELEASED:
                if (this.pickerModeMoveEntity != null) {
                    if (IsPickerModeMoveValid()) {
                        this.pickerModeMoveEntity.SetPos(this.pickerModeMovePos);
                    }
                    this.pickerModeMoveEntity.entityBase.ResetViewPosition();
                    this.pickerModeMoveEntity.entityView.SetGhost(false);
                    this.pickerModeMoveEntity = null;
                }
                break;
            case MouseStateEnum.DEFAULT:
                break;
        }
    }

    void PickerModeMoveCursorUpdate() {
        if (GM.boardData.IsPosInBoard(GM.inputManager.mousePosV2)) {
            this.cursorBase.gameObject.SetActive(true);
            // if holding a entity to move
            if (this.pickerModeMoveEntity != null) {
                // set cursor pos to entity size and pos to place where entity will be dropped
                this.cursorBase.SetSize(this.pickerModeMoveEntity.size);
                this.cursorBase.SetPos(this.pickerModeMovePos);
                // if that pos is invalid, cursor becomes red
                if (IsPickerModeMoveValid()) {
                    this.cursorBase.SetColor(Color.white);
                } else {
                    this.cursorBase.SetColor(Color.red);
                }
            } else {
                // set cursor back to white when no entities held
                this.cursorBase.SetColor(Color.white);
                // check for an entity at mousePos and set cursor as that entity if true else reset
                EntityData maybeAEntity = GM.boardData.GetEntityDataAtPos(GM.inputManager.mousePosV2);
                if (maybeAEntity != null && !maybeAEntity.isBoundary) {
                    this.cursorBase.SetAsEntity(maybeAEntity);
                } else {
                    this.cursorBase.ResetCursorOnMousePos();
                }
            }
        } else {
            this.cursorBase.gameObject.SetActive(false);
        }
    }

    void EditModeUpdate() {
        switch (GM.inputManager.mouseState) {
            case MouseStateEnum.CLICKED:
                this.editModeClickedEntity = GM.boardData.GetEntityDataAtPos(GM.inputManager.mousePosV2);
                if (this.editModeClickedEntity != null) {
                    this.previewCubeBase.SetColor(Color.white);
                    this.previewCubeBase.SetAsEntity(this.editModeClickedEntity);
                    this.previewCubeBase.SetActive(true);
                } else {
                    this.previewCubeBase.SetActive(false);
                }
                this.editPanelBase.SetEditModeEntity(this.editModeClickedEntity);
                break;
        }
    }

    void EditModeCursorUpdate() {

    }

    public void SetEditMode(EditModeEnum aEditMode) {
        this.editMode = aEditMode;
        this.pickerModePlaceSchema = null;
        this.previewCubeBase.SetActive(false);
        switch (aEditMode) {
            case EditModeEnum.PICKER:
                break;
            case EditModeEnum.EDIT:
                EditModeSetEntity(this.pickerModeLastPlacedEntityData);
                this.editModeClickedEntity = this.pickerModeLastPlacedEntityData;
                this.pickerModeLastPlacedEntityData = null;
                break;
            case EditModeEnum.OPTIONS:
                this.editPanelBase.SetOptionsModeTitleField(GM.boardData.title);
                break;
        }
    }

    public void OnRightClick(InputAction.CallbackContext context) {
        if (context.phase == InputActionPhase.Performed) {
            switch (this.editMode) {
                case EditModeEnum.PICKER:
                    if (this.pickerModePlaceSchema != null) {
                        PickerModePlaceReset();
                    }
                    break;
                case EditModeEnum.EDIT:
                print("edit mode right click");
                    if (this.editModeClickedEntity != null) {
                        EditModeReset();
                    }
                    break;
                case EditModeEnum.OPTIONS:
                    break;
            }
        }
    }

    void PickerModePlaceReset() {
        this.pickerModePlaceSchema = null;
        this.pickerModeLastPlacedEntityData = null;
        this.previewCubeBase.SetActive(false);
    }

    public void EditModeReset() {
        this.editModeClickedEntity = null;
        this.previewCubeBase.SetActive(false);
        EditModeSetEntity(null);
    }

    public void EditModeSetEntity(EntityData aEntityData) {
        this.previewCubeBase.SetActive(aEntityData != null);
        if (aEntityData != null) {
            this.previewCubeBase.SetColor(Color.white);
            this.previewCubeBase.SetAsEntity(aEntityData);
            
        }
        
        this.editPanelBase.SetEditModeEntity(aEntityData);
    }

    // picker mode

    public void OnPickerModeItemClick(EntitySchema aEntitySchema) {
        this.pickerModePlaceSchema = aEntitySchema;
        previewCubeBase.SetAsSchema(aEntitySchema);
    }

    public bool IsPickerModeMoveValid() {
        return GM.boardData.IsRectEmpty(this.pickerModeMovePos, this.pickerModeMoveEntity.size, this.pickerModeMoveEntity);
    }

    // edit mode

    public void OnEditModeColorPickerClick(Color aColor) {
        this.editModeClickedEntity.SetDefaultColor(aColor);
    }
    public void OnEditModeFixedToggle(bool aIsFixed) {
        this.editModeClickedEntity.isFixed = aIsFixed;
    }

    public void OnEditModeNodeToggle(NodeToggleStruct aNodeToggleStruct) {
        INodal nodal = this.editModeClickedEntity.entityBase.GetCachedIComponent<INodal>() as INodal;
        Vector2Int currentPos = aNodeToggleStruct.node;
        if (aNodeToggleStruct.toggled) {
            nodal.AddNode(aNodeToggleStruct.node, aNodeToggleStruct.upDown);
        } else {
            nodal.RemoveNode(aNodeToggleStruct.node, aNodeToggleStruct.upDown);
        }
        this.editPanelBase.SetEditModeEntity(this.editModeClickedEntity);
    }
    
    public void OnEditModeExtraButtonClick() {
        print("extra button clicked");
    }

    public void OnEditModeFlipButtonClick() {
        this.editModeClickedEntity.FlipEntity();
    }

    public void OnEditModeDeleteButtonClick() {
        EditModeSetEntity(null);
        this.boardManager.DestroyEntity(this.editModeClickedEntity);
        this.editModeClickedEntity = null;
        print("delete button clicked");
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
        SaveLoad.SaveBoard(GM.boardData);
    }

    public void OnOptionsModePlaytestButtonClick() {
        print("playtest button clicked");
    }

    public void LoadLevelFromFilePicker(string aFilename) {
        SaveLoad.LoadBoard(aFilename);
        EndFilePicker();
    }

    public void EndFilePicker() {
        this.filePickerBase.gameObject.SetActive(false);
        GM.I.ToggleFullPauseGame(false);

    }
}
