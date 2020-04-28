using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

public class EditManager : Singleton<EditManager> {
    public EntitySchema previewSchema;
    public bool isPlacementAtMousePosValid;
    public EditModeEnum editMode;
    public PlacementStateEnum placementState;
    // set by editor
    public PreviewCubeBase previewCubeBase;
    public InfoPanelBase infoPanelBase;
    public EntityBase hoveredEntity;
    void Awake() {
        this.editMode = EditModeEnum.EDITENTITY;
        this.placementState = PlacementStateEnum.DEFAULT;
        this.previewSchema = null;
    }

    void Update() {
        switch (this.editMode) {
            case EditModeEnum.PLACEENTITY:
                PlaceEntityUpdate();
                break;
            case EditModeEnum.EDITENTITY:
                EntityBase newHoveredEntity = BoardManager.Instance.GetHoveredEntity();
                if (this.hoveredEntity != newHoveredEntity) {
                    this.hoveredEntity = newHoveredEntity;
                    infoPanelBase.SetEntity(this.hoveredEntity);
                }
                break;
            case EditModeEnum.DELETEENTITY:
                break;
        }

    
    }


    void PlaceEntityUpdate() {
        switch (this.placementState) {
            case PlacementStateEnum.DEFAULT:
                break;
            case PlacementStateEnum.CLICKED:
            print("CLICKED");
                this.previewCubeBase.SetSize(this.previewSchema.size);
                this.previewCubeBase.SetPos(InputManager.Instance.mousePosV2);
                this.previewCubeBase.SetActive(true);
                this.placementState = PlacementStateEnum.HELD;
                break;
            case PlacementStateEnum.HELD:
                this.previewCubeBase.SetPos(InputManager.Instance.mousePosV2);
                this.isPlacementAtMousePosValid = IsPlacementAtMousePosValid();
                if (isPlacementAtMousePosValid) {
                    this.previewCubeBase.SetColor(Color.green);
                    if (InputManager.Instance.mouseState == MouseStateEnum.CLICKED) {
                        PlacePreview();
                        this.placementState = PlacementStateEnum.UNCLICKED;
                    }
                } else {
                    this.previewCubeBase.SetColor(Color.red);
                }
                
                break;
            case PlacementStateEnum.UNCLICKED:
                this.previewCubeBase.SetActive(false);
                this.previewSchema = null;
                this.placementState = PlacementStateEnum.DEFAULT;
                break;
        }
    }


    public void OnPickerItemSelect(EntitySchema aEntitySchema) {
        this.placementState = PlacementStateEnum.CLICKED;
        this.previewSchema = aEntitySchema;
    }


    public void MovePreviewCubeToMousePos() {
        this.previewCubeBase.transform.position = Util.V2IOffsetV3(InputManager.Instance.mousePosV2, this.previewSchema.size);
    }

    bool IsPlacementAtMousePosValid() {
        if (BoardManager.Instance.levelGrid.IsRectInGrid(InputManager.Instance.mousePosV2, this.previewSchema.size)) {
            if (!BoardManager.Instance.levelGrid.HasEntitiesBetweenPos(InputManager.Instance.mousePosV2, this.previewSchema.size)) {
                return true;
            }
        }
        return false;
    }

    public void PlacePreview() {
        EntityData entityData = new EntityData(this.previewSchema, InputManager.Instance.mousePosV2, Constants.DEFAULTFACING, Constants.DEFAULTCOLOR);
        BoardManager.Instance.CreateEntity(entityData);
    }


    // public void SetPreviewCubeColor(Color aColor) {
    //     this.previewCube.GetComponent<Renderer>().material
    // }
    // public void OnPreviewClick() {
    //     // if valid location
    //     print("onpreviewclicked");
    //     if (this.isPlacementValid) {
    //         EntityData entityData = new EntityData(this.selectedSchema, Util.V3ToV2I(InputManager.Instance.mousePos), Constants.DEFAULTFACING, Constants.DEFAULTCOLOR);
    //         BoardManager.Instance.CreateEntity(entityData);
    //         this.selectedSchema = null;
    //         this.previewCube.SetActive(false);
    //     }
    // }
}
