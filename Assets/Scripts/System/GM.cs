// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;
// using Sirenix.OdinInspector;
// // TODO: add custom Inodal support to specialblocks
// public class GM : Singleton<GM> {
//     public static BoardData boardData;
//     public static InputManager inputManager;
//     public static BoardManager boardManager;
//     // public static EditManager editManager;
//     public static EditManager2 editManager2;
//     public static PlayManager playManager;
//     public static GridViewBase gridViewBase;
//     public static CursorBase cursorBase;

//     //temp
//     public static BoardManager2 boardManager2;

//     public GameModeEnum gameMode;

//     [Header("Set In Editor")]
//     public GameObject boardManagerGameObject;
//     public GameObject editPanel;
//     public GameObject playPanel;
//     public GameObject pausePanel;
//     public GameObject blockPrefabMaster;
//     public GameObject playerPrefabMaster;
//     public GameObject shufflebotPrefabMaster;
//     public GameObject pushablePrefabMaster;
//     public GameObject testPrefabMaster;
//     public GameObject biggerTestPrefabMaster;
//     GameObject activePanel;

//     public bool isFullPaused;
//     private void Awake() {
//         // set as editing by default
//         // NewBoard();
        
//         // SetGameMode(GameModeEnum.EDITING);
//     }

//     public void NewBoard() {
//         GM.boardData = new BoardData();
//         GM.boardManager = this.boardManagerGameObject.GetComponent<BoardManager>();
//         // GM.editManager = this.boardManagerGameObject.GetComponent<EditManager>();
//         // temporary
//         GM.editManager2 = this.boardManagerGameObject.GetComponent<EditManager2>();

//         GM.inputManager = this.boardManagerGameObject.GetComponent<InputManager>();
//         GM.playManager = this.boardManagerGameObject.GetComponent<PlayManager>();
//         GM.gridViewBase = this.boardManagerGameObject.GetComponentInChildren<GridViewBase>();
//         GM.cursorBase = this.boardManagerGameObject.GetComponentInChildren<CursorBase>();
//         // temp
//         // GM.boardManager2 = this.boardManagerGameObject.GetComponent<BoardManager2>();
//         // GM.boardManager2.Init();


//         GM.boardManager.Init();
//         GM.editManager2.Init();
//         GM.playManager.Init();
//         GM.gridViewBase.Init();

//     }



//     public void LoadBoard(BoardData aBoardData) {
//         GM.boardData = aBoardData;
//         GM.boardManager.Init();
//         GM.editManager2.Init();
//         GM.gridViewBase.Init();
//     }

//     // sets game mode but with int because unity is too dumb to use enums in editor
//     public void SetGameModeWithInt(int gameModeEnumInt) {
//         SetGameMode((GameModeEnum)gameModeEnumInt);
//     }
    
//     public void SetGameMode(GameModeEnum aGameMode) {
//         if (this.gameMode == GameModeEnum.PLAYTESTING) {
//             LoadBoard(SaveLoad.LoadBoard("PlaytestTemp.board"));
//         }
//         this.gameMode = aGameMode;
//         switch (this.gameMode) {
//             case GameModeEnum.EDITING:
//                 Time.timeScale = 0;
//                 this.activePanel = this.editPanel;
//                 this.playPanel.SetActive(false);
//                 GM.editManager2.enabled = true;
//                 GM.playManager.enabled = false;
//                 GM.editManager2.Init();
//                 break;
//             case GameModeEnum.PLAYING:
//                 Time.timeScale = 1;
//                 this.activePanel = this.playPanel;
//                 this.editPanel.SetActive(false);
//                 GM.editManager2.enabled = false;
//                 GM.playManager.enabled = true;
//                 // GM.editManager.enabled = false;
//                 GM.playManager.SetPlaytest(false);
//                 GM.playManager.Init();
//                 break;
//             case GameModeEnum.PLAYTESTING:
//                 Time.timeScale = 1;
//                 this.activePanel = this.playPanel;
//                 this.editPanel.SetActive(false);
//                 GM.editManager2.enabled = false;
//                 GM.playManager.enabled = true;
//                 // GM.editManager.enabled = false;
//                 GM.playManager.SetPlaytest(true);
//                 GM.playManager.Init();
//                 break;
//         }
//         this.activePanel.SetActive(true);
//     }

//     public static GameObject LoadEntityPrefabByFilename(string aFilename) {
//         return Resources.Load("EntityPrefabs/" + aFilename) as GameObject;
//     }

//     public static GameObject LoadBgPrefabByFilename(string aFilename) {
//         return Resources.Load("BgPrefabs/" + aFilename) as GameObject;
//     }
    
//     public void ToggleFullPauseGame(bool aIsFullPaused) {
//         this.isFullPaused = aIsFullPaused;
//         this.pausePanel.SetActive(this.isFullPaused);
//         AudioListener.pause = this.isFullPaused;
//         this.activePanel.SetActive(!this.isFullPaused);
//         if (this.isFullPaused) {
//             Time.timeScale = 0;
//         } else {
//             Time.timeScale = 1;
//         }
//     }
// }
