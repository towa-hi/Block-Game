using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Sirenix.OdinInspector;

[System.Serializable]
public class OnValueChanged : UnityEvent<int>{};

public class IntPickerBase : SerializedMonoBehaviour {
    public int defaultInt;
    int currentInt;

    // set in editor
    public OnValueChanged onValueChanged = new OnValueChanged();


    public Text intPickerValueText;

    void OnEnable() {
        this.defaultInt = GM.boardData.par;
        SetCurrentInt(this.defaultInt);
    }


    public void OnIntPickerButtonClicked(bool aIsPlus) {
        if (aIsPlus) {
            if (this.currentInt < 99) {
                SetCurrentInt(this.currentInt + 1);
            }
        } else {
            if (this.currentInt > 0) {
                SetCurrentInt(this.currentInt - 1);
            }
        }
        onValueChanged.Invoke(this.currentInt);
    }

    public void SetCurrentInt(int aInt) {
        this.currentInt = aInt;
        this.intPickerValueText.text = this.currentInt.ToString();
    }

    public int GetCurrentInt() {
        return this.currentInt;
    }



    // // new code
    // void OnEnable() {
    //     GM.editManager2.onUpdateState += OnUpdateState;
        
    //     OnUpdateState();
    // }

    // void OnDisable() {
    //     GM.editManager2.onUpdateState -= OnUpdateState;
    // }

    // // updates the view
    // public void OnUpdateState() {
    //     GM.EditManager2.editState.par // doesnt exist yet
    // }

    // // when clicked
    // public void OnIntPickerButtonClicked(bool aIsPlus) {
    //     EditorState newState = GM.editManager2.GetCurrentState();
    //     newState = EditorState.SetPar(newState, 1);
    //     GM.EditManager2.UpdateState(newState);
    // }

}

