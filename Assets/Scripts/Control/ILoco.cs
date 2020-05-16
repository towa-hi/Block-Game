﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

public class ILoco : IComponent {
    [SerializeField]
    bool doNext;
    [SerializeField]
    StateMachine stateMachine;
    HashSet<EntityData> entitiesToKill;
    [Header("Set In Editor")]
    public bool canKillOnTouch;
    public int touchDamage;
    public bool canKillOnFall;
    public int fallDamage;
    public bool canHop;
    public bool canBeLifted;
    public float walkingMoveSpeed;
    public float turningMoveSpeed;
    public float hoppingMoveSpeed;
    public AnimationCurve hoppingCurve;

    public override void Init() {
        this.stateMachine = new StateMachine();
        this.stateMachine.ChangeState(new ILocoWaitingState(this));
        this.entitiesToKill = new HashSet<EntityData>();
    }

    public override void DoFrame() {
        if (this.doNext) {
            this.stateMachine.ChangeState(ChooseNextState());
        }
        this.stateMachine.Update();
    }

    public GameState ChooseNextState() {
         if (this.canBeLifted) {
                if (GM.playManager.EntityFanCheck(this.entityData)) {
                    // TODO finish this
                    print("should be lifted");
                }
            }
            // if floating
        if (GM.boardData.IsEntityFloating(this.entityData)) {
            // fall down
            return new ILocoFallingState(this, this.entityData.pos + Vector2Int.down);
        } else {
            Vector2Int facingPos = this.entityData.pos + this.entityData.facing;
            Vector2Int facingUpPos = facingPos + Vector2Int.up;
            Vector2Int facingDownPos = facingPos + Vector2Int.down;
            HashSet<EntityData> facingBumpCheck = GM.playManager.BumpCheck(facingPos, this.entityData);
            HashSet<EntityData> facingUpBumpCheck = GM.playManager.BumpCheck(facingUpPos, this.entityData);
            HashSet<EntityData> facingDownBumpCheck = GM.playManager.BumpCheck(facingDownPos, this.entityData);
            if (facingBumpCheck != null) {
                // print("walking side");
                GM.playManager.KillEntities(facingBumpCheck);
                return new ILocoWalkingState(this, facingPos);
            } else if (this.canHop) {
                    if (facingUpBumpCheck != null) {
                        // print("hopping up");
                        GM.playManager.KillEntities(facingUpBumpCheck);
                        return new ILocoHoppingState(this, facingUpPos);
                    } else if (facingDownBumpCheck != null) {
                        // print("hopping down");
                        GM.playManager.KillEntities(facingDownBumpCheck);
                        return new ILocoHoppingState(this, facingDownPos);
                    } else {
                        // print("turning");
                        return new ILocoTurningState(this);
                    }
            } else {
                // print("turning");
                return new ILocoTurningState(this);
            }
        }
    }

    public void DoNext(bool aDoNext) {
        this.doNext = aDoNext;
    }

    public bool IsEntityPosGroundedAndValid(Vector2Int aPos, EntityData aEntityData) {
        if (GM.boardData.IsEntityPosValid(aPos, aEntityData)) {
            // Debug.Log("entity pos is valid" + aPos);
            if (!GM.boardData.IsRectEmpty(aPos + Vector2Int.down, aEntityData.size, aEntityData)) {
                // Debug.Log("entity pos is grounded returning true" + aPos);
                return true;
            } else {
                // Debug.Log("entity pos is not grounded" + aPos);
            }
        } else {
            // Debug.Log("entity pos is invalid" + aPos);
        }
        return false;
    }
    class ILocoFloatingState : GameState {
        ILoco iLoco;
        EntityBase entityBase;
        EntityData entityData;

        public ILocoFloatingState(ILoco aIloco) {
            this.iLoco = aIloco;
            this.entityBase = aIloco.entityBase;
            this.entityData = aIloco.entityData;
        }

        public void Enter() {
            // print("ILocoRisingState - entered");
            this.iLoco.DoNext(false);
        }

        public void Update() {
            this.iLoco.DoNext(true);
        }

        public void Exit() {
            this.entityBase.ResetViewPosition();
        }
    }

    void OnDrawGizmos() {
        ILocoState currentState = this.stateMachine.GetState() as ILocoState;
        if ( currentState != null && currentState.destination != null) {
            Vector3 zOffset = new Vector3(0, 0, -1.01f);
            Gizmos.color = new Color(0, 1, 0, 0.2f);
            Vector3 origin = this.entityBase.transform.position;
            Vector3 destination = Util.V2IOffsetV3(currentState.destination, this.entityData.size);
            Gizmos.DrawLine(origin, destination);
            Gizmos.DrawCube(destination, this.entityView.GetComponent<Renderer>().bounds.size);
        }
    }

    #region StateClasses

    // base class for ILoco states that contains data needed for movement stuff
    abstract class ILocoState : GameState {
        protected ILoco iLoco;
        protected EntityBase entityBase;
        protected EntityData entityData;
        [SerializeField]
        public Vector2Int destination;
        protected Vector3 startPosition;
        protected Vector3 endPosition;
        [SerializeField]
        protected float t;

        abstract public void Enter();
        abstract public void Update();
        abstract public void Exit();
    }

    // state causes object to rise up one tile when raised by a fan
    class ILocoRisingState : ILocoState {

        public ILocoRisingState(ILoco aIloco, Vector2Int aDestination) {
            this.iLoco = aIloco;
            this.entityBase = aIloco.entityBase;
            this.entityData = aIloco.entityData;
            this.destination = aDestination;
            this.t = 0f;
        }

        public override void Enter() {
            // print("ILocoRisingState - entered");
            this.iLoco.DoNext(false);
            this.startPosition = this.entityBase.transform.position;
            GM.boardData.MoveEntity(this.destination, this.entityData);
            this.endPosition = Util.V2IOffsetV3(this.destination, this.entityData.size);
        }

        public override void Update() {
            if (this.t < 1) {
                this.t += Time.deltaTime / Constants.GRAVITY;
                this.entityBase.transform.position = Vector3.Lerp(this.startPosition, this.endPosition, this.t);
            } else {
                this.iLoco.DoNext(true);
            }
        }

        public override void Exit() {
             // print("ILocoRisingState - exited");
            this.entityBase.ResetViewPosition();
        }
    }

    class ILocoWalkingState : ILocoState {

        public ILocoWalkingState(ILoco aILoco, Vector2Int aDestination) {
            this.iLoco = aILoco;
            this.entityBase = aILoco.entityBase;
            this.entityData = aILoco.entityData;
            this.destination = aDestination;
            this.t = 0f;
        }

        public override void Enter() {
            // print("ILocoWalkingState - entered");
            this.iLoco.DoNext(false);
            this.startPosition = this.entityBase.transform.position;
            GM.boardData.MoveEntity(this.destination, this.entityData);
            this.endPosition = Util.V2IOffsetV3(this.destination, this.entityData.size);
        }

        public override void Update() {
            if (this.t < 1) {
                this.t += Time.deltaTime / this.iLoco.walkingMoveSpeed;
                this.entityBase.transform.position = Vector3.Lerp(this.startPosition, this.endPosition, this.t);
            } else {
                this.iLoco.DoNext(true);
            }
        }

        public override void Exit() {
            // print("ILocoWalkingState - exited");
            this.entityBase.ResetViewPosition();
        }
    }

    class ILocoTurningState : ILocoState {
        Quaternion startRotation;
        Quaternion endRotation;

        public ILocoTurningState(ILoco aILoco) {
            this.iLoco = aILoco;
            this.entityBase = aILoco.entityBase;
            this.entityData = aILoco.entityData;
            this.destination = this.entityData.pos;
            this.t = 0f;
        }

        public override void Enter() {
            // print("ILocoTurningState - entered");
            this.iLoco.DoNext(false);
            this.entityData.FlipEntity();
            this.startRotation = this.entityBase.transform.rotation;
            this.endRotation = Quaternion.AngleAxis(180, Vector3.up) * this.startRotation;
        }

        public override void Update() {
            if (this.t < 1) {
                this.t += Time.deltaTime / this.iLoco.turningMoveSpeed;
                this.entityBase.transform.rotation = Quaternion.Lerp(this.startRotation, this.endRotation, this.t);
            } else {
                this.iLoco.DoNext(true);
            }
        }

        public override void Exit() {
            // print("ILocoTurningState - exited");
            this.entityBase.transform.rotation = this.endRotation;
        }
    }

    class ILocoFallingState : ILocoState {

        public ILocoFallingState(ILoco aILoco, Vector2Int aDestination) {
            this.iLoco = aILoco;
            this.entityBase = aILoco.entityBase;
            this.entityData = aILoco.entityData;
            this.destination = aDestination;
            this.t = 0f;
        }

        public override void Enter() {
            // print("ILocoFallingState - entered");
            this.iLoco.DoNext(false);
            this.startPosition = this.entityBase.transform.position;
            GM.boardData.MoveEntity(this.destination, this.entityData);
            this.endPosition = Util.V2IOffsetV3(this.destination, this.entityData.size);
        }

        public override void Update() {
            if (this.t < 1) {
                this.t += Time.deltaTime / this.iLoco.walkingMoveSpeed;
                this.entityBase.transform.position = Vector3.Lerp(this.startPosition, this.endPosition, this.t);
            } else {
                this.iLoco.DoNext(true);
            }
        }

        public override void Exit() {
            this.entityBase.ResetViewPosition();
            // print("ILocoFallingState - exited");
        }
    }

    class ILocoHoppingState : ILocoState {

        public ILocoHoppingState(ILoco aILoco, Vector2Int aDestination) {
            
            this.iLoco = aILoco;
            this.entityBase = aILoco.entityBase;
            this.entityData = aILoco.entityData;
            this.destination = aDestination;
            this.t = 0f;
        }

        public override void Enter() {
            // print("ILocoHoppingState - entered");
            this.iLoco.DoNext(false);
            this.startPosition = this.entityBase.transform.position;
            GM.boardData.MoveEntity(this.destination, this.entityData);
            this.endPosition = Util.V2IOffsetV3(this.destination, this.entityData.size);
        }

        public override void Update() {
            if (this.t < 1) {
                this.t += Time.deltaTime / this.iLoco.hoppingMoveSpeed;
                Vector3 yOffset = new Vector3(0, this.iLoco.hoppingCurve.Evaluate(t), 0);
                Vector3 newPosition = Vector3.Lerp(this.startPosition, this.endPosition, this.t);
                this.entityBase.transform.position = newPosition + yOffset;
            } else {
                this.iLoco.DoNext(true);
            }
        }

        public override void Exit() {
            // print("ILocoHoppingState - exited");
            this.entityBase.ResetViewPosition();
        }
    } 

    class ILocoWaitingState : ILocoState {

        public ILocoWaitingState(ILoco aILoco) {
            this.iLoco = aILoco;
            this.entityBase = aILoco.entityBase;
            this.entityData = aILoco.entityData;
            this.destination = this.entityData.pos;
        }

        public override void Enter() {
            // print("ILocoWaitingState - entered");
            this.iLoco.DoNext(false);

        }

        public override void Update() {
            this.iLoco.DoNext(true);
        }

        public override void Exit() {
            // print("ILocoWaitingState - exited");
        }
    }

    #endregion
}
