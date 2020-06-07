using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using Sirenix.Utilities;

[RequireComponent(typeof(EntityBase))]
public class MobBase : SerializedMonoBehaviour {
    // EntityBase entityBase;
    // [SerializeField] StateMachine stateMachine;
    // [SerializeField] bool needsNewState;
    // [SerializeField] MobData mobData;
    // EntityState entityState {
    //     get {
    //         return GM.boardManager.GetEntityById(this.entityBase.data.id);
    //     }
    // }
    //
    // bool isDying;
    //
    // void Awake() {
    //     this.entityBase = GetComponent<EntityBase>();
    //     this.stateMachine = new StateMachine();
    //     this.needsNewState = true;
    //     this.isDying = false;
    // }
    //
    // public void Init(EntityState aEntityState) {
    //     if (aEntityState.mobData != null) {
    //         this.mobData = aEntityState.mobData.Value;
    //     }
    //     else {
    //         throw new Exception("malformed entityState! mob lacks mobData!");
    //     }
    // }
    //
    // public void DoFrame() {
    //     if (this.needsNewState) {
    //         StateMachineState newState = ChooseNextState();
    //         StateMachineState newStateWithDeathStateCheck = StateDeathCheck(newState);
    //         this.stateMachine.ChangeState(newState);
    //     }
    //     this.stateMachine.Update();
    // }
    //
    // public StateMachineState GetMobState() {
    //     return this.stateMachine.GetState();
    // }
    //
    // StateMachineState ChooseNextState() {
    //     if (this.isDying) {
    //         // this happens when i am already inside of a hurtbox
    //         return new DyingState();
    //     }
    //     switch (this.mobData.movementType) {
    //         case MoveTypeEnum.INANIMATE:
    //             return new WaitingState();
    //         case MoveTypeEnum.PATROL:
    //             return PatrolChooseNextState();
    //         case MoveTypeEnum.FLY:
    //             return FlyChooseNextState();
    //         case MoveTypeEnum.PATHPATROL:
    //             return PathfinderChooseNextState();
    //         case MoveTypeEnum.PATHFLY:
    //             return PathfinderChooseNextState();
    //         default:
    //             throw new ArgumentOutOfRangeException();
    //     }
    // }
    //
    // StateMachineState MoveTowardsFacingDirection() {
    //     Vector2Int facing = this.entityState.facing;
    //     Vector2Int facingUp = facing + Vector2Int.up;
    //     Vector2Int facingDown = facing + Vector2Int.down;
    //     if (PosHasFloorAndIsClear(facing)) {
    //         // walk in facing direction
    //         return new WalkingState(facing);
    //     } 
    //     else if (PosHasFloorAndIsClear(facingUp)) {
    //         // hop up
    //         return new HoppingState(facingUp);
    //     } 
    //     else if (PosHasFloorAndIsClear(facingDown)) {
    //         // hop down
    //         return new HoppingState(facingDown);
    //     }
    //     else {
    //         // this should never trigger
    //         return new TurningState();
    //     }
    // }
    //
    // StateMachineState PatrolChooseNextState() {
    //     // if floor under me
    //     if (ShouldIFall()) {
    //         // if i can move forwards, forwardsup or forwardsdown
    //         if (CanIMoveTowardsFacingDirection()) {
    //             // move forwards, forwardsup or forwardsdown
    //             return MoveTowardsFacingDirection();
    //         }
    //         else {
    //             // something is preventing me from moving forwards, forwardsup or forwardsdown
    //             // check if i can kill or push entities to clear space
    //             if (CanObstructionsBeMovedOrKilled()) {
    //                 // kill obstructions
    //                 KillObstructions();
    //                 PushObstructions();
    //                 return new PushingState();
    //             }
    //             else {
    //                 // turn around because blocked
    //                 return new TurningState();
    //             }
    //         }
    //     }
    //     else {
    //         // fall because there's no floor
    //         return new FallingState();
    //     }
    // }
    //
    // StateMachineState FlyChooseNextState() {
    //     return new WaitingState();
    // }
    //
    // StateMachineState PathfinderChooseNextState() {
    //     return new WaitingState();
    // }
    //
    // bool PosHasFloorAndIsClear(Vector2Int aPos) {
    //     // NOTE: MOBS IN THE PROCESS OF FALLING ARE NOT FLOORS
    //     return true;
    // }
    //
    //
    // // get these positions
    // // Y..
    // // Y..
    // // OXX
    // // +++
    // IEnumerable<BoardCell> GetFloorSlice(Vector2Int aOrigin) {
    //     Vector2Int floorOrigin = aOrigin + Vector2Int.down;
    //     Vector2Int floorSize = new Vector2Int(this.entityState.data.size.x, 1);
    //     return GM.boardManager.GetBoardGridSlice(floorOrigin, floorSize).Values.ToHashSet();
    // }
    //
    // // is this a entity i should treat like a floor and choose to step on?
    // bool CanStepOnEntity(EntityState aEntityState) {
    //     switch (aEntityState.data.entityType) {
    //         case EntityTypeEnum.BG:
    //             return false;
    //         case EntityTypeEnum.BLOCK:
    //             return true;
    //         case EntityTypeEnum.PUSHABLE:
    //             if (GM.boardManager.GetEntityBaseById(aEntityState.data.id).mobBase.GetMobState() is FallingState) {
    //                 return false;
    //             }
    //             else {
    //                 return true;
    //             }
    //             break;
    //         case EntityTypeEnum.MOB:
    //             return false;
    //         case EntityTypeEnum.SPECIALBLOCK:
    //             return true;
    //         default:
    //             throw new ArgumentOutOfRangeException();
    //     }
    // }
    //
    // bool ShouldIFall() {
    //     foreach (BoardCell boardCell in GetFloorSlice(this.entityState.pos)) {
    //         if (boardCell.frontEntityState.HasValue) {
    //             EntityState blockingEntityState = boardCell.frontEntityState.Value;
    //             if (blockingEntityState.data.entityType == EntityTypeEnum.MOB) {
    //                 return false;
    //             }
    //         }
    //     }
    //     return true;
    // }
    //
    // bool CanIMoveTowardsFacingDirection() {
    //     return true;
    // }
    //
    // bool CanObstructionsBeMovedOrKilled() {
    //     return true;
    // }
    //
    // bool EntityIsNotMe(EntityState aEntityState) {
    //     return aEntityState.data.id != this.entityState.data.id;
    // }
    //
    // bool CanKillEntity(EntityState aEntityState, DeathTypeEnum aDeathType) {
    //     switch (aDeathType) {
    //         case DeathTypeEnum.BUMP:
    //             return this.mobData.canKillOnTouch && this.mobData.touchPower > aEntityState.touchDefense;
    //         case DeathTypeEnum.BISECTED:
    //             return false;
    //         case DeathTypeEnum.FIRE:
    //             return false;
    //         case DeathTypeEnum.SQUISHED:
    //             return this.mobData.canKillOnFall && this.mobData.fallPower > aEntityState.fallDefense;
    //         default:
    //             throw new ArgumentOutOfRangeException(nameof(aDeathType), aDeathType, null);
    //     }
    // }
    //
    // void KillObstructions() {
    //     
    // }
    //
    // void PushObstructions() {
    //     
    // }
    //
    // StateMachineState StateDeathCheck(StateMachineState aState) {
    //     // if this new state leads to death i bumped into a mob that can kill
    //     // set isDying = true;
    //     // return deathState
    //     // else if i am going to walk into a hurtbox
    //     // set isDying = true but
    //     // return aState 
    // }
    //
    // class WalkingState : StateMachineState {
    //
    //     public WalkingState(Vector2Int aDirection) {
    //         
    //     }
    //     
    //     public void Enter() {
    //         // before moving, check if the blocking entity can kill on touch. if true, yeet self
    //         throw new NotImplementedException();
    //     }
    //
    //     public void Update() {
    //         throw new NotImplementedException();
    //     }
    //
    //     public void Exit() {
    //         throw new NotImplementedException();
    //     }
    // }
    //
    // class HoppingState : StateMachineState {
    //     
    //     public HoppingState(Vector2Int aDirection) {
    //         
    //     }
    //     
    //     public void Enter() {
    //         // before moving, check if the blocking entity can kill on touch. if true, yeet self
    //         throw new NotImplementedException();
    //     }
    //
    //     public void Update() {
    //         throw new NotImplementedException();
    //     }
    //
    //     public void Exit() {
    //         throw new NotImplementedException();
    //     }
    // }
    //
    // class TurningState : StateMachineState {
    //     public void Enter() {
    //         throw new NotImplementedException();
    //     }
    //
    //     public void Update() {
    //         throw new NotImplementedException();
    //     }
    //
    //     public void Exit() {
    //         throw new NotImplementedException();
    //     }
    // }
    //
    // class PushingState : StateMachineState {
    //
    //     public PushingState(Vector2Int aDirection) {
    //         
    //     }
    //     
    //     public void Enter() {
    //         throw new NotImplementedException();
    //     }
    //
    //     public void Update() {
    //         throw new NotImplementedException();
    //     }
    //
    //     public void Exit() {
    //         throw new NotImplementedException();
    //     }
    // }
    //
    // class FlyingState : StateMachineState {
    //     public void Enter() {
    //         throw new NotImplementedException();
    //     }
    //
    //     public void Update() {
    //         throw new NotImplementedException();
    //     }
    //
    //     public void Exit() {
    //         throw new NotImplementedException();
    //     }
    // }
    //
    // class WaitingState : StateMachineState {
    //     public void Enter() {
    //         
    //     }
    //
    //     public void Update() {
    //         
    //     }
    //
    //     public void Exit() {
    //         
    //     }
    // }
    //
    // class FallingState : StateMachineState {
    //     public void Enter() {
    //         // before moving, check if the blocking entity can kill on touch. if true, yeet self
    //         throw new NotImplementedException();
    //     }
    //
    //     public void Update() {
    //         throw new NotImplementedException();
    //     }
    //
    //     public void Exit() {
    //         throw new NotImplementedException();
    //     }
    // }
    //
    // class DyingState : StateMachineState {
    //     public void Enter() {
    //         throw new NotImplementedException();
    //     }
    //
    //     public void Update() {
    //         throw new NotImplementedException();
    //     }
    //
    //     public void Exit() {
    //         throw new NotImplementedException();
    //     }
    // }
}
