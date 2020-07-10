using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

public class StateMachine {
    StateMachineState state;

    public void ChangeState(StateMachineState aState) {
        this.state?.Exit();
        this.state = aState;
        this.state.Enter();
    }

    public void Update() {
        this.state?.Update();
    }

    public StateMachineState GetState() {
        return this.state;
    }
}
