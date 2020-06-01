using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using UnityEngine.UI;

public class GUIOptionsTitleField : BoardStateListener {
    InputField input;

    void Awake() {
        this.input = GetComponent<InputField>();
    }

    protected override void OnUpdateBoardState(BoardState aNewBoardState) {
        this.input.text = aNewBoardState.title;
    }
}
