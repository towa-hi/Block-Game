using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
// using SimpleFileBrowser;

public class EditManager : Singleton<EditManager> {


    public EditModeEnum editMode;

    // during PICKER mode
    public EntitySchema previewSchema;
    public bool isPlacementAtMousePosValid;
    public EntityBase pickerModeClickedEntity;
    public Vector2Int pickerModeNewPos;
    // during EDIT mode
    public EntityBase editModeClickedEntity;
    // during OPTIONS mode
    public GameObject filePicker;

    // set by editor
    public PreviewCubeBase previewCubeBase;
    public EditModePanelBase editModePanelBase;
    public GameObject filePickerMaster;
    public GameObject canvas;
    // test
    public LevelSchema newLevelSchema;

    void Awake() {
        this.editMode = EditModeEnum.PICKER;
        this.previewSchema = null;
        // InitFileBrowser();
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

    void EditModeUpdate() {
        switch (InputManager.Instance.mouseState) {
            case MouseStateEnum.CLICKED:
                this.editModeClickedEntity = BoardManager.Instance.GetHoveredEntity();
                if (this.editModeClickedEntity != null) {
                    this.previewCubeBase.SetColor(Color.white);
                    this.previewCubeBase.SetSize(this.editModeClickedEntity.size);
                    this.previewCubeBase.SetPos(this.editModeClickedEntity.pos);
                    this.previewCubeBase.SetActive(true);
                } else {
                    this.previewCubeBase.SetActive(false);
                }
                this.editModePanelBase.SetEntity(this.editModeClickedEntity);
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

    public void PickerPlaceUpdate() {
        switch (InputManager.Instance.mouseState) {
            case MouseStateEnum.CLICKED:
                if (IsPlacementAtMousePosValid()) {
                    PlacePreview();
                    this.previewCubeBase.SetActive(false);
                    this.previewSchema = null;
                }
                break;
            case MouseStateEnum.HELD:
                break;
            case MouseStateEnum.RELEASED:
                break;
            case MouseStateEnum.DEFAULT:
                if (this.previewCubeBase.pos != InputManager.Instance.mousePosV2) {
                    this.previewCubeBase.SetPos(InputManager.Instance.mousePosV2);
                    this.isPlacementAtMousePosValid = IsPlacementAtMousePosValid();
                    if (this.isPlacementAtMousePosValid) {
                        this.previewCubeBase.SetColor(Color.green);
                    } else {
                        this.previewCubeBase.SetColor(Color.red);
                    }
                }
                break;
        }
    }

    public void PickerMoveUpdate() {
        switch (InputManager.Instance.mouseState) {
            case MouseStateEnum.CLICKED:
                if (IsEntityMovableInPickerMode(BoardManager.Instance.GetHoveredEntity())) {
                    this.pickerModeClickedEntity = BoardManager.Instance.GetHoveredEntity();
                    this.pickerModeClickedEntity.entityView.SetGhost(true);
                }
                break;
            case MouseStateEnum.HELD:
                if (this.pickerModeClickedEntity != null) {
                    pickerModeNewPos = Util.V3ToV2I(InputManager.Instance.dragOffset) + this.pickerModeClickedEntity.pos;
                    this.pickerModeClickedEntity.transform.position = Util.V2IOffsetV3(pickerModeNewPos, this.pickerModeClickedEntity.size);
                }
                break;
            case MouseStateEnum.RELEASED:
                if (this.pickerModeClickedEntity != null) {
                    if (IsMovementToPosValid(pickerModeNewPos, this.pickerModeClickedEntity)) {
                        BoardManager.Instance.MoveEntity(pickerModeNewPos, this.pickerModeClickedEntity);
                    } else {
                        this.pickerModeClickedEntity.transform.position = Util.V2IOffsetV3(this.pickerModeClickedEntity.pos, this.pickerModeClickedEntity.size);
                    }
                    this.pickerModeClickedEntity.entityView.SetGhost(false);
                    this.pickerModeClickedEntity = null;
                }
                break;
            case MouseStateEnum.DEFAULT:
                break;
        }
    }

    public void OnEditModeFixedToggleClick(bool aIsOn) {
        this.editModeClickedEntity.isFixed = aIsOn;
    }

    public void OnEditModeColorPicker(Color aColor) {
        this.editModeClickedEntity.entityView.SetColor(aColor, true);
    }

    public bool IsEntityMovableInPickerMode(EntityBase aEntityBase) {
        if (aEntityBase != null && !aEntityBase.isBoundary) {
            return true;
        } else {
            return false;
        }
    }

    public void OnPickerModeItemClick(EntitySchema aEntitySchema) {
        this.previewSchema = aEntitySchema;
        this.previewCubeBase.SetSize(this.previewSchema.size);
        this.previewCubeBase.SetPos(InputManager.Instance.mousePosV2);
        this.previewCubeBase.SetActive(true);
    }

    bool IsPlacementAtMousePosValid() {
        if (BoardManager.Instance.levelGrid.IsRectInGrid(InputManager.Instance.mousePosV2, this.previewSchema.size)) {
            if (!BoardManager.Instance.levelGrid.HasEntitiesBetweenPos(InputManager.Instance.mousePosV2, this.previewSchema.size)) {
                return true;
            }
        }
        return false;
    }

    bool IsMovementToPosValid(Vector2Int aPos, EntityBase aEntityBase) {
        if (BoardManager.Instance.levelGrid.IsRectInGrid(aPos, aEntityBase.size)) {
            if (!BoardManager.Instance.levelGrid.HasEntitiesBetweenPos(aPos, aEntityBase.size, aEntityBase)) {
                return true;
            }
        }
        return false;
    }

    public void PlacePreview() {
        EntityData entityData = new EntityData(this.previewSchema, InputManager.Instance.mousePosV2, Constants.DEFAULTFACING, Constants.DEFAULTCOLOR);
        BoardManager.Instance.CreateEntity(entityData);
    }

    
    public void SetEditMode(EditModeEnum aEditMode) {
        this.editMode = aEditMode;
        this.previewCubeBase.SetActive(false);
        ResetEditMode();
        ResetPickerMode();
    }

    void ResetPickerMode() {
        this.previewSchema = null;
        this.pickerModeClickedEntity = null;
    }

    void ResetEditMode() {
        this.editModeClickedEntity = null;
        this.editModePanelBase.SetEntity(null);
    }

    void ResetOptionsMode() {

    }

    public void OnEditModeDeleteButtonClick() {
        BoardManager.Instance.DeleteEntity(this.editModeClickedEntity);
        this.editModeClickedEntity = null;
        this.editModePanelBase.SetEntity(this.editModeClickedEntity);
        this.previewCubeBase.SetActive(false);
    }

    public void SaveLevel(string aTitle, int aPar) {
        LevelData oldLevelData = BoardManager.Instance.levelData;
        List<EntityBase> entityList = BoardManager.Instance.entityList;
        List<EntityData> entityDataList = new List<EntityData>();
        foreach (EntityBase entityBase in entityList) {
            entityDataList.Add(entityBase.CreateEntityData());
        }
        LevelSchema newSchema = LevelSchema.CreateInstance<LevelSchema>();
        newSchema.Init(aTitle, Config.USERNAME, aPar, oldLevelData.levelSchema.size, entityDataList);
        this.newLevelSchema = newSchema;
        LevelSaveLoad.SaveLevel(newSchema);
    }



    public void StartFilePicker() {
        
        if (this.filePicker == null) {
            GameManager.Instance.FullPauseToggle();
            this.filePicker = Instantiate(this.filePickerMaster, this.canvas.transform);
        }
        
    }

    public void EndFilePicker() {
        if (this.filePicker != null) {
            GameManager.Instance.FullPauseToggle();
            Object.Destroy(this.filePicker);
        }
        
    }

    public void LoadLevelFromFilePicker(string aFileName) {
        print(aFileName);
        EndFilePicker();
        LevelSchema newLevelSchema = LevelSaveLoad.LoadLevel(aFileName);
        LevelData newLevelData = new LevelData(newLevelSchema, 0);
        BoardManager.Instance.InitializeLevel(newLevelData);
    }

    // public void InitFileBrowser() {
    //     FileBrowser.SetFilters(true, new FileBrowser.Filter( "JSON", ".json"));
    //     FileBrowser.SetDefaultFilter(".json");
    //     FileBrowser.AddQuickLink( "LevelJSON", Config.PATHTOLEVELJSON, null);
    // }

    // public void LoadLevelFileBrowser() {
    //     GameManager.Instance.FullPauseToggle();
    //     // StartCoroutine(ShowLoadDialogCoroutine());

    // }

    // IEnumerator ShowLoadDialogCoroutine() {
    //     yield return FileBrowser.WaitForLoadDialog(false, Config.PATHTOLEVELJSON, "Load File", "Load");
    //     print(FileBrowser.Success + " " + FileBrowser.Result);
    //     if (FileBrowser.Success) {
    //         string loadedLevelJson = FileBrowserHelpers.ReadTextFromFile(FileBrowser.Result);
    //         print(loadedLevelJson);
    //     }
    //     GameManager.Instance.FullPauseToggle();
    // }
}
