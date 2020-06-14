using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using Random = UnityEngine.Random;

public class Platform : SerializedMonoBehaviour {
    public GameObject model;
    bool isPaused;
    public bool isUp;
    public Vector3 bottomPosition = Vector3.zero;
    public Vector3 topPosition = new Vector3(0, 28f, 0);
    public float moveSpeed;

    [SerializeField] float t = 1.01f;
    [SerializeField] float waitT = 0f;
    [SerializeField] float waitDuration = 4f;
    void OnEnable() {
        GM.playManager.OnUpdatePlayState += OnUpdatePlayState;
    }

    void OnDisable() {
        GM.playManager.OnUpdatePlayState -= OnUpdatePlayState;
    }

    void DoAnimation() {
        this.t = 0;
    }

    void Update() {
        if (!this.isPaused) {
            if (this.t < 1) {
                this.t += Time.deltaTime / this.moveSpeed;
                if (this.isUp) {
                    this.model.transform.localPosition = Vector3.Lerp(this.bottomPosition, this.topPosition, this.t);
                }
                else {
                    this.model.transform.localPosition = Vector3.Lerp(this.topPosition, this.bottomPosition, this.t);
                }
            }
            else {
                if (this.waitT < 1) {
                    this.waitT += Time.deltaTime / this.waitDuration;
                }
                else {
                    this.model.transform.localPosition = this.bottomPosition;
                    this.waitDuration = Random.Range(5f, 10f);
                    this.t = 0f;
                    this.waitT = 0f;
                }
            }
        }
    }
    void OnUpdatePlayState(PlayState aPlayState) {
        switch (aPlayState.timeMode) {
            case TimeModeEnum.NORMAL:
                this.isPaused = false;
                break;
            case TimeModeEnum.PAUSED:
                this.isPaused = true;
                break;
            case TimeModeEnum.DOUBLE:
                this.isPaused = false;
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
}
