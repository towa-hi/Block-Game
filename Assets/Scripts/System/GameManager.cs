using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using Sirenix.OdinInspector;

public class GameManager : Singleton<GameManager> {
    public GameObject pausePanel;
    public bool isMenu;
    public PlayerInput playerInput;
    public GameModeEnum gameState;
    public LevelData levelToLoad;

    private void Awake() {
        this.isMenu = false;
    }

    public void NewBoard(LevelData aLevelData) {
        this.levelToLoad = aLevelData;
        SceneManager.LoadScene("EditorScene");
    }

    public void OnEscapeMenu(InputAction.CallbackContext context) {
        if (context.phase == InputActionPhase.Performed) {
            FullPauseToggle();
        }
    }

    public void FullPauseToggle() {
        if (isMenu) {
            DeactivateMenu();
        } else {
            ActivateMenu();
        }
    }

    public void ActivateMenu() {
        // this.playerInput.SwitchCurrentActionMap("Menu");
        this.isMenu = true;
        AudioListener.pause = true;
        Time.timeScale = 0;
        this.pausePanel.SetActive(true);
    }

    public void DeactivateMenu() {
        // this.playerInput.SwitchCurrentActionMap("Board");
        this.isMenu = false;
        AudioListener.pause = false;
        Time.timeScale = 1;
        this.pausePanel.SetActive(false);
    }
}
