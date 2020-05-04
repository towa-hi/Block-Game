using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Sirenix.OdinInspector;

public class GameManager : SerializedMonoBehaviour {
    public GameObject pausePanel;
    public bool isMenu;

    void Update() {

    }

    public void OnEscapeMenu(InputAction.CallbackContext context) {
        if (isMenu) {
            DeactivateMenu();
        } else {
            ActivateMenu();
        }
    }
    
    public void ActivateMenu() {
        this.isMenu = true;
        AudioListener.pause = true;
        Time.timeScale = 0;
        this.pausePanel.SetActive(true);
    }

    public void DeactivateMenu() {
        this.isMenu = false;
        AudioListener.pause = false;
        Time.timeScale = 1;
        this.pausePanel.SetActive(false);
    }
}
