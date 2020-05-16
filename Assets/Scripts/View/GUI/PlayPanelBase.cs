using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Sirenix.OdinInspector;

public class PlayPanelBase : SerializedMonoBehaviour {
    [SerializeField]
    bool isPlaytest;
    public Text titleText;
    public GameObject playtestPanel;

    void Start() {
        SetTitle();
    }

    void SetPlaytest(bool aIsPlaytest) {
        this.isPlaytest = aIsPlaytest;
        this.playtestPanel.SetActive(aIsPlaytest);
    }

    public void SetTitle() {
        this.titleText.text = GM.boardData.title;
    }

    
}
