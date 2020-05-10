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
    [Header("Picker Mode")]
    [SerializeField] EntitySchema pickerModePlaceSchema;
    [SerializeField] EntityData pickerModeLastPlacedEntityData;
    
    public bool pickerModePlaceIsValid;
    [Header("Edit Mode")]
    public EntityData editModeClickedEntity;

    [Header("Options Mode")]
    [SerializeField] string newTitle;
    [SerializeField] int newPar;
    [Header("Set In Editor")]
    public PreviewCubeBase previewCubeBase;
    public PreviewStudioBase previewStudioBase;
    public EditPanelBase editPanelBase;

    void Awake() {
        Init();
    }

    public void Init() {
        SetEditMode(EditModeEnum.PICKER);
        this.boardManager = GM.boardManager;
        this.pickerModePlaceSchema = null;
        this.pickerModeLastPlacedEntityData = null;
        this.editModeClickedEntity = null;
        this.pickerModeLastPlacedEntityData = null;
        this.newTitle = GM.boardData.title;
        this.newPar = GM.boardData.par;
        this.editPanelBase.SetOptionsModeTitleField(this.newTitle);
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
        this.previewCubeBase.SetPos(InputManager.I.mousePosV2);
        this.pickerModePlaceIsValid = GM.boardData.IsRectEmpty(this.previewCubeBase.pos, this.previewCubeBase.size);
        switch (InputManager.I.mouseState) {
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
                // if (this.previewCubeBase.pos != InputManager.Instance.mousePosV2) {
                //     this.previewCubeBase.SetPos(InputManager.Instance.mousePosV2);
                //     this.isPlacementAtMousePosValid = IsPlacementAtMousePosValid();
                //     if (this.isPlacementAtMousePosValid) {
                //         this.previewCubeBase.SetColor(Color.green);
                //     } else {
                //         this.previewCubeBase.SetColor(Color.red);
                //     }
                // }
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

    }

    void EditModeUpdate() {
        switch (InputManager.I.mouseState) {
            case MouseStateEnum.CLICKED:
                this.editModeClickedEntity = GM.boardData.GetEntityDataAtPos(InputManager.I.mousePosV2);
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
                print("edit mdoe right click");
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
        this.newTitle = aTitle;
    }

    public void OnOptionsModeParIntPickerChange(int aPar) {
        print("new par: " + aPar);
        this.newPar = aPar;
    }

    public void OnOptionsModeLoadButtonClick() {
        print("load button clicked");
        SaveLoad.LoadBoard();
    }

    public void OnOptionsModeSaveButtonClick() {
        print("save button clicked");
        SaveLoad.SaveBoard(GM.boardData);
    }

    public void OnOptionsModePlaytestButtonClick() {
        print("playtest button clicked");
    }

}
