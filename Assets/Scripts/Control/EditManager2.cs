using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
[RequireComponent(typeof(BoardManager2))]
public class EditManager2 : SerializedMonoBehaviour {
    public BoardManager2 boardManager;

    public BoardData boardData;

    public EditModeEnum editMode;
    
    public EntityData editModeClickedEntity;

    public EntitySchema previewSchema;

    public PreviewCubeBase previewCubeBase;

    public EditPanelBase editPanelBase;

    void Awake() {
        SetEditMode(EditModeEnum.PICKER);
        this.editModeClickedEntity = null;
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
        if (this.previewSchema != null) {
            PickerPlaceUpdate();
        } else {
            PickerMoveUpdate();
        }
    }

    void PickerPlaceUpdate() {
        switch (InputManager.I.mouseState) {
            case MouseStateEnum.CLICKED:
                // if (IsPlacementAtMousePosValid()) {
                //     PlacePreview();
                //     this.previewCubeBase.SetActive(false);
                //     this.previewSchema = null;
                // }
                break;
            case MouseStateEnum.HELD:
                break;
            case MouseStateEnum.RELEASED:
                break;
            case MouseStateEnum.DEFAULT:
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

    void PickerMoveUpdate() {

    }

    void EditModeUpdate() {
        switch (InputManager.I.mouseState) {
            case MouseStateEnum.CLICKED:
                this.editModeClickedEntity = GM.boardData.GetEntityDataAtPos(InputManager.I.mousePosV2);
                if (this.editModeClickedEntity != null) {
                    this.previewCubeBase.SetColor(Color.white);
                    this.previewCubeBase.SetSize(this.editModeClickedEntity.size);
                    this.previewCubeBase.SetPos(this.editModeClickedEntity.pos);
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
        this.previewSchema = null;
    }


    // picker mode

    public void OnPickerModeItemClick(EntitySchema aEntitySchema) {
        print(aEntitySchema.name);
    }

    // edit mode

    public void OnEditModeColorPickerClick(Color aColor) {
        print(aColor);

    }
    public void OnEditModeFixedToggle(bool aIsFixed) {
        print("fixed:" + aIsFixed);
    }
    
    public void OnEditModeExtraButtonClick() {
        print("extra button clicked");
    }

    public void OnEditModeFlipButtonClick() {
        print("flip button clicked");
    }

    public void OnEditModeDeleteButtonClick() {
        boardManager.DestroyEntity(this.editModeClickedEntity);
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
    }

    public void OnOptionsModeSaveButtonClick() {
        print("save button clicked");
    }

    public void OnOptionsModePlaytestButtonClick() {
        print("playtest button clicked");
    }

}
