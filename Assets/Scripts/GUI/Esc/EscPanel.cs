using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

public class EscPanel : SerializedMonoBehaviour {

    public void ExitToDesktop() {
        Application.Quit();
    }

    public void ToggleVisible(bool aIsVisible) {
        print(aIsVisible);
        this.gameObject.SetActive(aIsVisible);
    }
}
