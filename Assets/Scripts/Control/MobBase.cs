using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

[RequireComponent(typeof(EntityBase))]
public class MobBase : SerializedMonoBehaviour {
    EntityBase entityBase;
    [SerializeField] StateMachine stateMachine;
    [SerializeField] bool needsNewState;
    [SerializeField] MobData mobData;
    
    void Awake() {
        this.entityBase = GetComponent<EntityBase>();
        this.stateMachine = new StateMachine();
        this.needsNewState = true;
    }

    public void Init(EntityState aEntityState) {
        if (aEntityState.mobData != null) {
            this.mobData = aEntityState.mobData.Value;
        }
        else {
            throw new Exception("malformed entityState! mob lacks mobData!");
        }
    }
    
    public void DoFrame() {
        if (this.needsNewState) {
            this.stateMachine.ChangeState(ChooseNextState());
        }
        this.stateMachine.Update();
    }

    EntityState GetEntityState() {
        return new EntityState();
    }
    
    StateMachineState ChooseNextState() {
        switch (this.mobData.movementType) {
            case MoveTypeEnum.INANIMATE:
                return new WaitingState();
            case MoveTypeEnum.PATROL:
                return PatrolChooseNextState();
            case MoveTypeEnum.FLY:
                return FlyChooseNextState();
            case MoveTypeEnum.PATHPATROL:
                return PathfinderChooseNextState();
            case MoveTypeEnum.PATHFLY:
                return PathfinderChooseNextState();
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    StateMachineState MoveTowardsFacingDirection() {
        Vector2Int facing = GetEntityState().facing;
        Vector2Int facingUp = facing + Vector2Int.up;
        Vector2Int facingDown = facing + Vector2Int.down;
        if (PosHasFloorAndIsClear(facing)) {
            // walk in facing direction
            return new WalkingState();
        } 
        else if (PosHasFloorAndIsClear(facingUp)) {
            // hop up
            return new HoppingState();
        } 
        else if (PosHasFloorAndIsClear(facingDown)) {
            // hop down
            return new HoppingState();
        }
        else {
            // this should never trigger
            return new TurningState();
        }
    }
    
    StateMachineState PatrolChooseNextState() {
        // if floor under me
        if (HasFloorOrKillableEntityUnderMe()) {
            // if i can move forwards, forwardsup or forwardsdown
            if (CanIMoveTowardsFacingDirection()) {
                // move forwards, forwardsup or forwardsdown
                return MoveTowardsFacingDirection();
            }
            else {
                // something is preventing me from moving forwards, forwardsup or forwardsdown
                // check if i can kill or push entities to clear space
                if (CanObstructionsBeMovedOrKilled()) {
                    // kill obstructions
                    KillObstructions();
                    PushObstructions();
                    return new PushingState();
                }
                else {
                    // turn around because blocked
                    return new TurningState();
                }
            }
        }
        else {
            // fall because there's no floor
            return new FallingState();
        }
    }

    StateMachineState FlyChooseNextState() {
        return new WaitingState();
    }
    
    StateMachineState PathfinderChooseNextState() {
        return new WaitingState();
    }

    bool PosHasFloorAndIsClear(Vector2Int aPos) {
        // NOTE: MOBS IN THE PROCESS OF FALLING ARE NOT FLOORS
        return true;
    }
    
    bool HasFloorOrKillableEntityUnderMe() {
        return true;
    }

    bool HasFloorUnderPos(Vector2Int aPos) {
        return true;
    }

    bool CanIMoveTowardsFacingDirection() {
        return true;
    }

    bool CanObstructionsBeMovedOrKilled() {
        return true;
    }

    void KillObstructions() {
        
    }

    void PushObstructions() {
        
    }
    
    class WalkingState : StateMachineState {
        public void Enter() {
            throw new NotImplementedException();
        }

        public void Update() {
            throw new NotImplementedException();
        }

        public void Exit() {
            throw new NotImplementedException();
        }
    }

    class HoppingState : StateMachineState {
        public void Enter() {
            throw new NotImplementedException();
        }

        public void Update() {
            throw new NotImplementedException();
        }

        public void Exit() {
            throw new NotImplementedException();
        }
    }

    class TurningState : StateMachineState {
        public void Enter() {
            throw new NotImplementedException();
        }

        public void Update() {
            throw new NotImplementedException();
        }

        public void Exit() {
            throw new NotImplementedException();
        }
    }

    class PushingState : StateMachineState {
        public void Enter() {
            throw new NotImplementedException();
        }

        public void Update() {
            throw new NotImplementedException();
        }

        public void Exit() {
            throw new NotImplementedException();
        }
    }

    class FlyingState : StateMachineState {
        public void Enter() {
            throw new NotImplementedException();
        }

        public void Update() {
            throw new NotImplementedException();
        }

        public void Exit() {
            throw new NotImplementedException();
        }
    }
    
    class WaitingState : StateMachineState {
        public void Enter() {
            
        }

        public void Update() {
            
        }

        public void Exit() {
            
        }
    }

    class FallingState : StateMachineState {
        public void Enter() {
            throw new NotImplementedException();
        }

        public void Update() {
            throw new NotImplementedException();
        }

        public void Exit() {
            throw new NotImplementedException();
        }
    }
}
