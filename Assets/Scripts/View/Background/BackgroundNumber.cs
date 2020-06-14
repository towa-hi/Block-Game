using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

public class BackgroundNumber : SerializedMonoBehaviour {
    int onesNumber = 0;
    int tensNumber = 0;
    GameObject onesNumberObject;
    GameObject tensNumberObject;
    public bool isPar;
    [SerializeField] int currentNumber;
    public Dictionary<int, GameObject> numberDict;
    Vector3 onesNumberPosition = new Vector3(2.75f, 0, 0.25f);
    Vector3 tensNumberPosition = new Vector3(0.75f, 0, 0.25f);

    public void Awake() {
        SetNumber(this.currentNumber);
    }
    void OnEnable() {
        GM.instance.OnUpdateGameState += OnUpdateGameState;
        GM.boardManager.OnUpdateBoardState += OnUpdateBoardState;
        GM.playManager.OnUpdatePlayState += OnUpdatePlayState;
    }

    void OnDisable() {
        GM.instance.OnUpdateGameState -= OnUpdateGameState;
        GM.boardManager.OnUpdateBoardState -= OnUpdateBoardState;
        GM.playManager.OnUpdatePlayState -= OnUpdatePlayState;
    }

    void OnUpdateGameState(GameState aGameState) {
        switch (aGameState.gameMode) {
            case GameModeEnum.PLAYING:
                break;
            case GameModeEnum.EDITING:
                SetNumber(0);
                break;
            case GameModeEnum.PLAYTESTING:
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    void OnUpdateBoardState(BoardState aBoardState) {
        if (this.isPar) {
            SetNumber(aBoardState.par);
        }
    }

    void OnUpdatePlayState(PlayState aPlayState) {
        if (!this.isPar) {
            SetNumber(aPlayState.moves);
        }
    }

    public void SetNumber(int aNumber) {
        if (this.onesNumberObject != null) {
            Destroy(this.onesNumberObject);
        }
        if (this.tensNumberObject != null) {
            Destroy(this.tensNumberObject);
        }
        if (0 <= aNumber && aNumber <= 99) {
            this.currentNumber = aNumber;
            this.onesNumber = aNumber % 10;
            this.tensNumber = aNumber / 10 % 10;
            var position = this.transform.position;
            this.onesNumberObject = Instantiate(this.numberDict[this.onesNumber], this.onesNumberPosition + position, Quaternion.identity, this.transform);
            this.onesNumberObject.transform.localPosition = this.onesNumberPosition;
            this.tensNumberObject = Instantiate(this.numberDict[this.tensNumber], this.tensNumberPosition + position, Quaternion.identity, this.transform);
            this.tensNumberObject.transform.localPosition = this.tensNumberPosition;
        }
        else {
            throw new ArgumentOutOfRangeException();
        }
    }
}
// movesNumbers position should be 54.66, 97.3333, -1.75