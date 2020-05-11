using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

public class StateMachine {
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
            this.state.Execute();
        }
    }
}
