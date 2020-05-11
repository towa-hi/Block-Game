using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

public class StateMachine {
    [SerializeField]
    GameState state;

    public void ChangeState(GameState aState) {
        if (this.state != null) {
            this.state.Exit();
        }
        this.state = aState;
        this.state.Enter();
    }

    public void Update() {
        if (this.state != null) {
            this.state.Update();
        }
    }

    public GameState GetState() {
        return this.state;
    }
}
