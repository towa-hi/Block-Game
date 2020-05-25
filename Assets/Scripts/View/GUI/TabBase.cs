using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Sirenix.OdinInspector;

public class TabBase : SerializedMonoBehaviour {
    public HashSet<Button> buttons;

    public void DisableOtherTabs(Button aGameObject) {
        foreach (Button button in this.buttons) {
            button.interactable = true;
        }
        aGameObject.interactable = false;
    }
}
