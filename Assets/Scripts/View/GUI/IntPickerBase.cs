using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Sirenix.OdinInspector;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;

public class IntPickerBase : SerializedMonoBehaviour {
    public int defaultInt;
    int currentInt;

    public Text intPickerValueText;

    void Awake() {
        SetCurrentInt(defaultInt);
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
    }

    public void SetCurrentInt(int aInt) {
        this.currentInt = aInt;
        this.intPickerValueText.text = this.currentInt.ToString();
    }

    public int GetCurrentInt() {
        return this.currentInt;
    }
}
