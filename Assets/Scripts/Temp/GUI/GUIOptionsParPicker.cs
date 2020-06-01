using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using UnityEngine.UI;

public class GUIOptionsParPicker : BoardStateListener {
    public Button leftButton;
    public Button rightButton;
    public Text text;

    void Awake() {
        this.leftButton.onClick.AddListener(() => GM.editManager.SetPar(false));
        this.rightButton.onClick.AddListener(() => GM.editManager.SetPar(true));
    }
    
    protected override void OnUpdateBoardState(BoardState aBoardState) {
        this.text.text = aBoardState.par.ToString();
        this.leftButton.interactable = (aBoardState.par != 1);
        this.rightButton.interactable = (aBoardState.par != Constants.MAXPAR);
    }
}
