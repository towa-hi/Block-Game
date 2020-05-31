using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

public class StateMachine {
    [SerializeField]
    StateMachineState state;

    public void ChangeState(StateMachineState aState) {
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

    public StateMachineState GetState() {
        return this.state;
    }
}
