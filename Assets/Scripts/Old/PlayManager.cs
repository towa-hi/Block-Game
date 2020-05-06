﻿// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;
// using System.Linq;
// using Sirenix.OdinInspector;

// // manages gameplay by taking InputManager state and editing BoardManager state
// // activating this makes the game playable
// public class PlayManager : Singleton<PlayManager> {
//     // set on awake
//     public HashSet<EntityBase> selectedSet;
//     public EntityBase clickedEntity;
//     public SelectionStateEnum selectionState;
//     public TimeStateEnum timeState;

//     void Awake() {
//         this.selectedSet = new HashSet<EntityBase>();
//         this.clickedEntity = null;
//         this.selectionState = SelectionStateEnum.UNSELECTED;
//         this.timeState = TimeStateEnum.NORMAL;
        
//     }

//     void Update() {
//         switch (InputManager.Instance.mouseState) {
//             case MouseStateEnum.DEFAULT:
//                 break;
//             case MouseStateEnum.CLICKED:
//                 this.clickedEntity = BoardManager.Instance.GetHoveredEntity();
//                 break;
//             case MouseStateEnum.HELD:
//                 switch (this.selectionState) {
//                     case SelectionStateEnum.UNSELECTED:
//                         OnEntityHeld();
//                         break;
//                     case SelectionStateEnum.SELECTED:
//                         break;
//                 }
//                 break;
//             case MouseStateEnum.RELEASED:
//                 switch (this.selectionState) {
//                     case SelectionStateEnum.UNSELECTED:
//                         break;
//                     case SelectionStateEnum.SELECTED:
//                         foreach (EntityBase entityBase in this.selectedSet) {
//                             // register new position for each 
//                         }
//                         break;
//                     case SelectionStateEnum.INVALID:
//                         foreach (EntityBase entityBase in this.selectedSet) {
//                             // smooth move back to original pos
//                         }
//                         break;
//                 }
//                 this.clickedEntity = null;
//                 this.selectedSet.Clear();
//                 this.selectionState = SelectionStateEnum.UNSELECTED;
//                 break;
//         }

//         switch (this.timeState) {
//             case TimeStateEnum.NORMAL:
//                 foreach (EntityBase entity in BoardManager.Instance.entityList) {
//                     foreach (IComponent component in entity.iComponentSet) {
//                         component.DoFrame();
//                     }
//                 }
//                 break;
//             case TimeStateEnum.PAUSED:
//                 break;
//         }
//     }
    
//     void PauseTime() {
//         this.timeState = TimeStateEnum.PAUSED;
//     }
    
//     void ResumeTime() {
//         this.timeState = TimeStateEnum.NORMAL;
//     }

//     void OnEntityHeld() {
//          if (this.clickedEntity != null && !this.clickedEntity.isFixed) {
//             // check if clickedEntity has INodal
//             if (this.clickedEntity.GetCachedIComponent<INodal>() != null) {
//                 // TODO: make this not retarded
//                 bool isAttemptingToSelect = false;
//                 // default value of this doesnt matter
//                 bool isDraggedUp = true;
//                 // TODO: needed to display debug info
//                 Color highlightColor = Color.white;
//                 if (InputManager.Instance.dragOffset.y > Constants.DRAGTHRESHOLD) {
//                     print("dragging up");
//                     isAttemptingToSelect = true;
//                     isDraggedUp = true;
//                     highlightColor = Color.red;
//                 } else if (InputManager.Instance.dragOffset.y < Constants.DRAGTHRESHOLD * -1) {
//                     print("dragging down");
//                     isAttemptingToSelect = true;
//                     isDraggedUp = false;
//                     highlightColor = Color.blue;
//                 }
//                 if (isAttemptingToSelect) {
//                     // print("starting selection");
//                     if (BoardManager.Instance.IsEntitySelectable(this.clickedEntity, isDraggedUp)) {
//                         // TODO: set selectedSet here
//                         this.selectedSet = BoardManager.Instance.GetSelectSet(this.clickedEntity, isDraggedUp);
//                         foreach(EntityBase entityBase in this.selectedSet) {
//                             entityBase.entityView.TempHighlight(highlightColor);
//                         }
//                         this.selectionState = SelectionStateEnum.SELECTED;
//                     } else {
//                         this.selectionState = SelectionStateEnum.INVALID;
//                     }
                    
//                 }
//             } else {
//                 print("entity doesn't have an INodal");
//             }
//         }
//     }
// }