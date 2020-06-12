using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using UnityEngine.UI;

public class GUIPopup : SerializedMonoBehaviour {
    public Button backToEditorButton;
    public Text titleText;
    public Text infoText;

    public void Init(bool didWin, int par, int moves) {
        this.gameObject.SetActive(true);
        this.titleText.text = didWin ? "You Win!" : "You Died!";
        this.infoText.text = "Your moves: " + moves + "\n " +
                             "Level par: " + par;
        if (moves <= par && didWin) {
            this.infoText.text += "\n You beat par! Great job!";
        }
        this.backToEditorButton.onClick.AddListener(OnBackButtonClicked);
    }

    public void OnBackButtonClicked() {
        this.gameObject.SetActive(false);
        GM.playManager.OnBackToEditorButtonClick();
    }
}
