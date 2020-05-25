using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using Sirenix.OdinInspector;


// TODO: make background editing less jank
// [RequireComponent(typeof(BoardManager))]
// public class EditManager : SerializedMonoBehaviour {
//     public bool isFront;
    
//     [Header("Set In Editor")]
//     public EditPanelBase editPanelBase;
//     public FilePickerBase filePickerBase;
//     public StateMachine stateMachine = new StateMachine();

//     public void Init() {
//         this.stateMachine.ChangeState(new EditorPickerModeMoveState());
//         SetEditMode(EditModeEnum.PICKER);
//         this.editPanelBase.SetOptionsModeTitleField(GM.boardData.title);
//         SetEditModeEntity(null);
//     }

//     void Update() {
//         this.stateMachine.Update();
//         if (this.stateMachine.GetState() is EditorPickerModePlaceState) {
//             if (GM.inputManager.rightMouseState == MouseStateEnum.CLICKED) {
//                 GM.editManager.stateMachine.ChangeState(new EditorPickerModeMoveState());
//             }
//         }
//         if (this.stateMachine.GetState() is EditTabBgPickerModePlaceState) {
//             if (GM.inputManager.rightMouseState == MouseStateEnum.CLICKED) {
//                 GM.editManager.stateMachine.ChangeState(new EditTabBgPickerModeMoveState());
//             }
//         }
//     }

//     public void SetEditMode(EditModeEnum aEditMode) {
//         switch (aEditMode) {
//             case EditModeEnum.PICKER:
//                 this.stateMachine.ChangeState(new EditorPickerModeMoveState());
//                 break;
//             case EditModeEnum.EDIT:
//                 this.stateMachine.ChangeState(new EditorEditModeState());
//                 break;
//             case EditModeEnum.OPTIONS:
//                 this.stateMachine.ChangeState(new EditorOptionsModeState());
//                 this.editPanelBase.SetOptionsModeTitleField(GM.boardData.title);
//                 break;
//             case EditModeEnum.BGPICKER:
//                 this.stateMachine.ChangeState(new EditTabBgPickerModeMoveState());
//                 break;
//             case EditModeEnum.BGEDIT:
//                 print("bgEdit clicked");
//                 break;
//         }
//     }

//     public void SetEditModeEntity(EntityData aEntityData) {
//         if (aEntityData != null) {
//             print("set edit entity to " + aEntityData.name);
//         }
//         this.editPanelBase.SetEditModeEntity(aEntityData);
//     }

//     // picker mode

//     public void OnPickerModeItemClick(EntitySchema aEntitySchema) {
//         this.stateMachine.ChangeState(new EditorPickerModePlaceState(aEntitySchema));
//     }

//     public void OnBgPickerModeItemClick(BgSchema aBgSchema) {
//         this.stateMachine.ChangeState(new EditTabBgPickerModePlaceState(aBgSchema));
//     }
//     // edit mode

//     public void OnEditModeColorPickerClick(Color aColor) {
//         if (this.stateMachine.GetState() is EditorEditModeState) {
//             EditorEditModeState state = this.stateMachine.GetState() as EditorEditModeState;
//             state.ChangeEntityColor(aColor);
//         }
//     }
//     public void OnEditModeFixedToggle(bool aIsFixed) {
//         if (this.stateMachine.GetState() is EditorEditModeState) {
//             EditorEditModeState state = this.stateMachine.GetState() as EditorEditModeState;
//             state.ChangeEntityFixed(aIsFixed);
//         }
//     }

//     public void OnEditModeNodeToggle(NodeToggleStruct aNodeToggleStruct) {
//         if (this.stateMachine.GetState() is EditorEditModeState) {
//             EditorEditModeState state = this.stateMachine.GetState() as EditorEditModeState;
//             state.ChangeEntityNodes(aNodeToggleStruct);
//         }
//     }
    
//     public void OnEditModeExtraButtonClick() {
//         print("extra button clicked");
//     }

//     public void OnEditModeFlipButtonClick() {
//         if (this.stateMachine.GetState() is EditorEditModeState) {
//             EditorEditModeState state = this.stateMachine.GetState() as EditorEditModeState;
//             state.FlipEntity();
//         }
//     }

//     public void OnEditModeDeleteButtonClick() {
//         print("EditManager - on delete button clicked");
//         if (this.stateMachine.GetState() is EditorEditModeState) {
//             EditorEditModeState state = this.stateMachine.GetState() as EditorEditModeState;
//             state.DeleteEntity();
//         }
//     }

//     public void OnLayerPickerClick(bool aIsFront) {
//         // change layer
//         this.isFront = aIsFront;
//         print("layer changed. isfront = " + aIsFront);
//     }
//     // options mode

//     public void OnOptionsModeTitleChange(string aTitle) {
//         print("new title:" + aTitle);
//         GM.boardData.title = aTitle;
//     }

//     public void OnOptionsModeParIntPickerChange(int aPar) {
//         print("new par: " + aPar);
//         GM.boardData.par = aPar;
//     }

//     public void OnOptionsModeLoadButtonClick() {
//         print("load button clicked");
//         // SaveLoad.LoadBoard();
//         GM.I.ToggleFullPauseGame(true);
//         this.filePickerBase.gameObject.SetActive(true);
//     }

//     public void OnOptionsModeSaveButtonClick() {
//         print("save button clicked");
//         if (SaveLoad.IsValidSave(GM.boardData)) {
//             SaveLoad.SaveBoard(GM.boardData);
//         } else {
//             print("invalid save");
//         }
        
//     }

//     public void OnOptionsModePlaytestButtonClick() {
//         print("playtest button clicked");
//         if (SaveLoad.IsValidSave(GM.boardData)) {
//             GM.I.SetGameMode(GameModeEnum.PLAYTESTING);
//             SaveLoad.SaveBoard(GM.boardData, true);
//         } else {
//             print("invalid save");
//         }
//     }

//     public void LoadLevelFromFilePicker(string aFilename) {
//         GM.I.LoadBoard(SaveLoad.LoadBoard(aFilename));
//         EndFilePicker();
//     }

//     public void EndFilePicker() {
//         this.filePickerBase.gameObject.SetActive(false);
//         GM.I.ToggleFullPauseGame(false);

//     }
// }

// #region editStates

// public class EditorPickerModeMoveState : GameState {
//     [SerializeField]
//     EntityData entityData;
//     [SerializeField]
//     Vector2Int movePos;
//     [SerializeField]
//     Vector2Int clickPosOffset;
//     [SerializeField]
//     bool isMoveValid;
//     public bool isFront;

//     public EditorPickerModeMoveState() {
//     }

//     public void Enter() {
//         // Debug.Log("EditTabPickerModeState - entering"); 
//         GM.cursorBase.SetVisible(true);      
//     }

//     public void Update() {
//         // Debug.Log("EditTabPickerModeState - updating");
//         CursorUpdate();
//         switch (GM.inputManager.mouseState) {
//             case MouseStateEnum.CLICKED:
//                 EntityData hoveredEntity = GM.boardData.GetEntityDataAtPos(GM.inputManager.mousePosV2);
//                 if (hoveredEntity != null) {
//                     if (hoveredEntity.IsMovableInPickerMode()) {
//                         this.entityData = hoveredEntity;
//                         this.clickPosOffset = GM.inputManager.mousePosV2 - this.entityData.pos;
//                     }
//                 }
//                 break;
//             case MouseStateEnum.HELD:
//                 if (this.entityData != null) {
//                     this.isMoveValid = GM.boardData.IsRectEmpty(this.movePos, this.entityData.size, this.entityData);
//                     this.movePos = GM.inputManager.mousePosV2 - this.clickPosOffset;
//                     this.entityData.entityBase.SetViewPosition(this.movePos);
//                 }
//                 break;
//             case MouseStateEnum.RELEASED:
//                 if (this.entityData != null) {
//                     if (this.isMoveValid) {
//                         GM.boardData.MoveEntity(this.movePos, this.entityData);
//                     }
//                     this.entityData.entityBase.ResetViewPosition();
//                     this.entityData = null;
//                 }
//                 break;
//         }
//     }

//     void CursorUpdate() {
//         if (this.entityData != null) {
//             GM.cursorBase.SetSize(this.entityData.size);
//             GM.cursorBase.SetPos(this.movePos);
//             if (isMoveValid) {
//                 GM.cursorBase.SetColor(Color.green);
//             } else {
//                 GM.cursorBase.SetColor(Color.red);
//             }
//         } else {
//             GM.cursorBase.SetColor(Color.white);
//             EntityData maybeAEntity = GM.boardData.GetEntityDataAtPos(GM.inputManager.mousePosV2);
//             if (maybeAEntity != null && !maybeAEntity.isBoundary) {
//                 GM.cursorBase.SetAsEntity(maybeAEntity);
//             } else {
//                 GM.cursorBase.ResetCursorOnMousePos();
//             }
//         }
//     }

//     public void Exit() {
//         GM.cursorBase.SetVisible(false);
//         // Debug.Log("EditTabPickerModeState - exiting");
//     }
// }

// public class EditorPickerModePlaceState : GameState {
//     [SerializeField]
//     EntitySchema entitySchema;
//     [SerializeField]
//     bool isPlacementValid;
//     public bool isFront;

//     public EditorPickerModePlaceState(EntitySchema aEntitySchema) {
//         this.entitySchema = aEntitySchema;
//     }

//     public void Enter() {
//         // Debug.Log("EditTabPickerModePlaceState - entering");
//         GM.cursorBase.SetVisible(true);
//         GM.cursorBase.SetSize(this.entitySchema.size);
//     }

//     public void Update() {
//         // Debug.Log("EditTabPickerModePlaceState - updating");
//         CursorUpdate();
//         this.isPlacementValid = GM.boardData.IsRectEmpty(GM.inputManager.mousePosV2, this.entitySchema.size);
//         if (GM.inputManager.mouseState == MouseStateEnum.CLICKED) {
//             if (this.isPlacementValid) {
//                 EntityData entityData = new EntityData(this.entitySchema, GM.inputManager.mousePosV2, Constants.DEFAULTFACING, Constants.DEFAULTCOLOR);
//                 GM.boardManager.CreateEntityFromData(entityData);
//             }
//         }
//     }

//     void CursorUpdate() {
//         GM.cursorBase.SetPos(GM.inputManager.mousePosV2);
//         if (this.isPlacementValid) {
//             GM.cursorBase.SetColor(Color.green);
//         } else {
//             GM.cursorBase.SetColor(Color.red);
//         }
//     }
    
//     public void Exit() {
//         GM.cursorBase.SetVisible(false);
//         // Debug.Log("EditTabPickerModePlaceState - exiting");
//     }
// }

// public class EditorEditModeState : GameState {
//     [SerializeField]
//     EntityData entityData;

//     public EditorEditModeState() {
//         Debug.Log("Instantiating EditTabEditModeState setting entityData to null");
//         this.entityData = null;
//     }

//     public void Enter() {
//         // Debug.Log("EditTabEditModeState - entering");
//         GM.cursorBase.SetColor(Color.white);
//         GM.cursorBase.SetVisible(true);
//         GM.editManager.SetEditModeEntity(null);
//     }

//     public void Update() {
//         UpdateCursor();
//         // Debug.Log("EditTabEditModeState - updating");
//         if (GM.inputManager.mouseState == MouseStateEnum.CLICKED) {
//             Debug.Log("setting entityData");
//             this.entityData = GM.boardData.GetEntityDataAtPos(GM.inputManager.mousePosV2);
//             GM.editManager.SetEditModeEntity(this.entityData);
//         } else if (GM.inputManager.rightMouseState == MouseStateEnum.CLICKED) {
//             this.entityData = null;
//             GM.editManager.SetEditModeEntity(null);
//         }
//     }

//     void UpdateCursor() {
//         if (this.entityData != null) {
//             GM.cursorBase.SetAsEntity(this.entityData);
//         } else {
//             GM.cursorBase.SetColor(Color.white);
//             EntityData maybeAEntity = GM.boardData.GetEntityDataAtPos(GM.inputManager.mousePosV2);
//             if (maybeAEntity != null) {
//                 GM.cursorBase.SetAsEntity(maybeAEntity);
//                 GM.cursorBase.SetColor(Color.green);
//             } else {
//                 GM.cursorBase.ResetCursorOnMousePos();
//             }
//         }
//     }


//     public void ChangeEntityNodes(NodeToggleStruct aNodeToggleStruct) {
//         INodal nodal = this.entityData.entityBase.GetCachedIComponent<INodal>() as INodal;
//         Vector2Int currentPos = aNodeToggleStruct.node;
//         if (aNodeToggleStruct.toggled) {
//             nodal.AddNode(aNodeToggleStruct.node, aNodeToggleStruct.upDown);
//         } else {
//             nodal.RemoveNode(aNodeToggleStruct.node, aNodeToggleStruct.upDown);
//         }
//         GM.editManager.SetEditModeEntity(this.entityData);
//     } 

//     public void ChangeEntityColor(Color aColor) {
//         this.entityData.SetDefaultColor(aColor);
//     }

//     public void ChangeEntityFixed(bool aIsFixed) {
//         this.entityData.isFixed = aIsFixed;
//     }

//     public void FlipEntity() {
//         this.entityData.FlipEntity();
//     }

//     public void DeleteEntity() {
//         GM.editManager.SetEditModeEntity(null);
//         GM.boardManager.DestroyEntity(this.entityData);
//         this.entityData = null;
//         Debug.Log("EditTabEditModeState - deleted entity");
//     }

//     public void Exit() {
//         GM.cursorBase.SetVisible(false);
//         // Debug.Log("EditTabEditModeState - exiting");
//     }
// }

// public class EditorOptionsModeState : GameState {

//     public EditorOptionsModeState() {
//     }

//     public void Enter() {
//         // Debug.Log("EditTabOptionsModeState - entering");
//     }

//     public void Update() {
//         // Debug.Log("EditTabOptionsModeState - updating");
//     }

//     public void Exit() {
//         // Debug.Log("EditTabOptionsModeState - exiting");
//     }
// }

// public class EditTabBgPickerModePlaceState : GameState {
//     [SerializeField]
//     BgSchema bgSchema;
//     [SerializeField]
//     bool isPlacementValid;

//     public EditTabBgPickerModePlaceState(BgSchema aBgSchema) {
//         this.bgSchema = aBgSchema;
//     }

//     public void Enter() {
//         GM.cursorBase.SetVisible(true);
//         GM.cursorBase.SetSize(this.bgSchema.size);
//     }

//     public void Update() {
//         CursorUpdate();
//         this.isPlacementValid = GM.boardData.backgroundData.IsRectEmpty(GM.inputManager.mousePosV2, this.bgSchema.size);
//         if (GM.inputManager.mouseState == MouseStateEnum.CLICKED) {
//             if (this.isPlacementValid) {
//                 BgData bgData = new BgData(this.bgSchema, GM.inputManager.mousePosV2, Constants.DEFAULTCOLOR);
//                 GM.boardManager.CreateBgFromData(bgData);
//             }
//         }
//     }

//     void CursorUpdate() {
//         GM.cursorBase.SetPos(GM.inputManager.mousePosV2);
//         if (this.isPlacementValid) {
//             GM.cursorBase.SetColor(Color.green);
//         } else {
//             GM.cursorBase.SetColor(Color.red);
//         }
//     }

//     public void Exit() {

//     }

// }

// public class EditTabBgPickerModeMoveState : GameState {
//     [SerializeField]
//     BgData bgData;
//     [SerializeField]
//     Vector2Int movePos;
//     [SerializeField]
//     Vector2Int clickPosOffset;
//     [SerializeField]
//     bool isMoveValid;

//     public EditTabBgPickerModeMoveState() {
//         GM.cursorBase.SetVisible(true);
//     }
//     public void Enter() {

//     }

//     public void Update() {
//         CursorUpdate();
//         switch (GM.inputManager.mouseState) {
//             case MouseStateEnum.CLICKED:
//                 BgData hoveredBg = GM.boardData.backgroundData.GetBgDataAtPos(GM.inputManager.mousePosV2);
//                 if (hoveredBg != null) {
//                     this.bgData = hoveredBg;
//                     this.clickPosOffset = GM.inputManager.mousePosV2 - this.bgData.pos;
//                 }
//                 break;
//             case MouseStateEnum.HELD:
//                 if (this.bgData != null) {
//                     this.isMoveValid = GM.boardData.backgroundData.IsRectEmpty(this.movePos, this.bgData.size, this.bgData);
//                     this.movePos = GM.inputManager.mousePosV2 - this.clickPosOffset;
//                     this.bgData.bgBase.SetViewPosition(this.movePos);
//                 }
//                 break;
//             case MouseStateEnum.RELEASED:
//                 if (this.bgData != null) {
//                     if (this.isMoveValid) {
//                         GM.boardData.backgroundData.MoveBg(this.movePos, this.bgData);
//                     }
//                     this.bgData.bgBase.ResetViewPosition();
//                     this.bgData = null;
//                 }
//                 break;
//         }
//     }

//     void CursorUpdate() {
//         if (this.bgData != null) {
//             GM.cursorBase.SetSize(this.bgData.size);
//             GM.cursorBase.SetPos(this.movePos);
//             if (isMoveValid) {
//                 GM.cursorBase.SetColor(Color.green);
//             } else {
//                 GM.cursorBase.SetColor(Color.red);
//             }
//         } else {
//             GM.cursorBase.SetColor(Color.white);
//             BgData maybeABg = GM.boardData.backgroundData.GetBgDataAtPos(GM.inputManager.mousePosV2);
//             if (maybeABg != null) {
//                 GM.cursorBase.SetPos(maybeABg.pos);
//                 GM.cursorBase.SetColor(Color.green);
//                 GM.cursorBase.SetSize(maybeABg.size);

                
//             } else {
//                 GM.cursorBase.ResetCursorOnMousePos();
//             }
//         }
//     }

//     public void Exit() {
//         GM.cursorBase.SetVisible(false);
//     }

    
// }

// public class EditTabBgEditModeState : GameState {
//     public void Enter() {

//     }

//     public void Update() {

//     }

//     public void Exit() {

//     }
// }
// #endregion