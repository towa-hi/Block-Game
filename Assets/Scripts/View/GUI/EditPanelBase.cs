using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

public class EditPanelBase : SerializedMonoBehaviour {
    public void OnPickerTabClick() {
        print("picker tab clicked");
    }

    public void OnOptionsTabClick() {
        print("options tab clicked");
    }
}
