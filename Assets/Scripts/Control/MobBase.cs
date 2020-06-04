using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

[RequireComponent(typeof(EntityBase))]
public class MobBase : SerializedMonoBehaviour {
    EntityBase entityBase;
    [SerializeField] StateMachine stateMachine;
    [SerializeField] bool needsNewState;
    void Awake() {
        this.entityBase = GetComponent<EntityBase>();
        this.stateMachine = new StateMachine();
        this.needsNewState = true;
    }

    public void DoFrame() {
        if (this.needsNewState) {
            this.stateMachine.ChangeState(ChooseNextState());
        }
        this.stateMachine.Update();
    }

    StateMachineState ChooseNextState() {
        return new WaitingState();
    }

    class WaitingState : StateMachineState {
        public void Enter() {
            
        }

        public void Update() {
            
        }

        public void Exit() {
            
        }
    }
}
