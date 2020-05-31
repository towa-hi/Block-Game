// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;
// using UnityEngine.UI;
// using Sirenix.OdinInspector;

// public class EditorTabButton : GUIBase {

//     public EditTabEnum editTabEnum;
//     Button button;

//     public override void OnEnable() {
//         this.button = GetComponent<Button>();
//         base.OnEnable();
//     }

//     public void OnClick() {
//         EditorState newState = EditorState.SetActiveTab(GM.editManager2.GetState(), this.editTabEnum);
//         GM.editManager2.UpdateState(newState);
//     }

//     // updates the view
//     public override void OnUpdateState() {
//         this.button.interactable = (this.editTabEnum != GM.editManager2.GetState().activeTab);
//     }


// }
