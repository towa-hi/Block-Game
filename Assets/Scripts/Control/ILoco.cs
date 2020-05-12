using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

public class ILoco : IComponent {
    [SerializeField]
    Vector2Int destination;
    [SerializeField]
    bool doNext;
    [SerializeField]
    StateMachine stateMachine;
    [Header("Set In Editor")]
    public float walkingMoveSpeed;
    public float turningMoveSpeed;
    public float hoppingMoveSpeed;
    public AnimationCurve hoppingCurve;


    public override void Init() {
        this.stateMachine = new StateMachine();
        this.destination = Vector2Int.zero;
        this.stateMachine.ChangeState(new ILocoWaitingState(this));

    }

    public override void DoFrame() {
        if (this.doNext) {
            // if floating
            if (GM.boardData.IsEntityFloating(this.entityData)) {
                this.destination = this.entityData.pos + Vector2Int.down;
                DoNext(false);
                // fall down
                this.stateMachine.ChangeState(new ILocoFallingState(this));
            } else {
                Vector2Int facingPos = this.entityData.pos + this.entityData.facing;
                Vector2Int facingUpPos = facingPos + Vector2Int.up;
                Vector2Int facingDownPos = facingPos + Vector2Int.down;
                if (GM.boardData.IsEntityPosGroundedAndValid(facingUpPos, this.entityData)) {
                    print("hopping up");
                    this.destination = facingUpPos;
                    this.stateMachine.ChangeState(new ILocoHoppingState(this));
                } else if (GM.boardData.IsEntityPosGroundedAndValid(facingPos, this.entityData)) {
                    print("walking side");
                    this.destination = facingPos;
                    this.stateMachine.ChangeState(new ILocoWalkingState(this));
                } else if (GM.boardData.IsEntityPosGroundedAndValid(facingDownPos, this.entityData)) {
                    print("hopping down");
                    this.destination = facingDownPos;
                    this.stateMachine.ChangeState(new ILocoHoppingState(this));
                } else {
                    this.destination = Vector2Int.zero;
                    this.stateMachine.ChangeState(new ILocoTurningState(this));
                }

            }
            
        }
        this.stateMachine.Update();
    }

    public void DoNext(bool aDoNext) {
        this.doNext = aDoNext;
    }

    class ILocoWalkingState : GameState {
        ILoco iLoco;
        EntityBase entityBase;
        EntityData entityData;
        Vector3 startPosition;
        Vector3 endPosition;
        float t;

        public ILocoWalkingState(ILoco aILoco) {
            this.iLoco = aILoco;
            this.entityBase = aILoco.entityBase;
            this.entityData = aILoco.entityData;
            this.t = 0f;
        }

        public void Enter() {
            print("ILocoWalkingState - entered");
            this.iLoco.DoNext(false);
            this.startPosition = this.entityBase.transform.position;
            GM.boardData.MoveEntity(this.iLoco.destination, this.entityData);
            this.endPosition = Util.V2IOffsetV3(this.iLoco.destination, this.entityData.size);
        }

        public void Update() {
            if (this.t < 1) {
                this.t += Time.deltaTime / this.iLoco.walkingMoveSpeed;
                this.entityBase.transform.position = Vector3.Lerp(this.startPosition, this.endPosition, this.t);
            } else {
                this.iLoco.DoNext(true);
            }
        }

        public void Exit() {
            this.entityBase.ResetViewPosition();
        }
    }

    class ILocoTurningState : GameState {
        ILoco iLoco;
        EntityBase entityBase;
        EntityData entityData;
        Quaternion startRotation;
        Quaternion endRotation;
        float t;

        public ILocoTurningState(ILoco aILoco) {
            this.iLoco = aILoco;
            this.entityBase = aILoco.entityBase;
            this.entityData = aILoco.entityData;
            this.t = 0f;
        }

        public void Enter() {
            print("ILocoTurningState - entered");
            this.iLoco.DoNext(false);
            this.entityData.FlipEntity();
            this.startRotation = this.entityBase.transform.rotation;
            this.endRotation = Quaternion.AngleAxis(180, Vector3.up) * this.startRotation;
        }

        public void Update() {
            if (this.t < 1) {
                this.t += Time.deltaTime / this.iLoco.turningMoveSpeed;
                this.entityBase.transform.rotation = Quaternion.Lerp(this.startRotation, this.endRotation, this.t);
            } else {
                this.iLoco.DoNext(true);
            }
        }

        public void Exit() {
            
        }
    }

    class ILocoFallingState : GameState {
        ILoco iLoco;
        EntityBase entityBase;
        EntityData entityData;
        Vector2Int destination;
        Vector3 startPosition;
        Vector3 endPosition;
        float t;

        public ILocoFallingState(ILoco aILoco) {
            print("ILocoFallingState - entered");
            this.iLoco = aILoco;
            this.entityBase = aILoco.entityBase;
            this.entityData = aILoco.entityData;
            this.t = 0f;
        }

        public void Enter() {
            this.iLoco.DoNext(false);
            this.startPosition = this.entityBase.transform.position;
            GM.boardData.MoveEntity(this.iLoco.destination, this.entityData);
            this.endPosition = Util.V2IOffsetV3(this.iLoco.destination, this.entityData.size);
        }

        public void Update() {
            if (this.t < 1) {
                this.t += Time.deltaTime / this.iLoco.walkingMoveSpeed;
                this.entityBase.transform.position = Vector3.Lerp(this.startPosition, this.endPosition, this.t);
            } else {
                this.iLoco.DoNext(true);
            }
        }

        public void Exit() {
            this.entityBase.ResetViewPosition();
        }
    }

    class ILocoHoppingState : GameState {
        ILoco iLoco;
        EntityBase entityBase;
        EntityData entityData;
        Vector3 startPosition;
        Vector3 endPosition;
        float t;

        public ILocoHoppingState(ILoco aILoco) {
            
            this.iLoco = aILoco;
            this.entityBase = aILoco.entityBase;
            this.entityData = aILoco.entityData;
            this.t = 0f;
        }

        public void Enter() {
            print("ILocoHoppingState - entered");
            this.iLoco.DoNext(false);
            this.startPosition = this.entityBase.transform.position;
            GM.boardData.MoveEntity(this.iLoco.destination, this.entityData);
            this.endPosition = Util.V2IOffsetV3(this.iLoco.destination, this.entityData.size);
        }

        public void Update() {
            if (this.t < 1) {
                this.t += Time.deltaTime / this.iLoco.hoppingMoveSpeed;
                Vector3 yOffset = new Vector3(0, this.iLoco.hoppingCurve.Evaluate(t), 0);
                // Vector3 yOffset = Vector3.zero;
                Vector3 newPosition = Vector3.Lerp(this.startPosition, this.endPosition, this.t);
                this.entityBase.transform.position = newPosition + yOffset;
            } else {
                this.iLoco.DoNext(true);
            }
        }

        public void Exit() {
            this.entityBase.ResetViewPosition();
        }
    } 
    class ILocoWaitingState : GameState {
        ILoco iLoco;
        EntityBase entityBase;
        EntityData entityData;

        public ILocoWaitingState(ILoco aILoco) {
            print("ILocoWaitingState - entered");
            this.iLoco = aILoco;
            this.entityBase = aILoco.entityBase;
            this.entityData = aILoco.entityData;
        }

        public void Enter() {
            this.iLoco.DoNext(false);

        }

        public void Update() {
            this.iLoco.DoNext(true);
        }

        public void Exit() {
            
        }
    }

    void OnDrawGizmos() {
        if (this.destination != Vector2Int.zero) {
            Vector3 zOffset = new Vector3(0, 0, -1.01f);
            Gizmos.color = new Color(0, 1, 0, 0.2f);
            Vector3 origin = this.entityBase.transform.position;
            Vector3 destination = Util.V2IOffsetV3(this.destination, this.entityData.size);
            Gizmos.DrawLine(origin, destination);
            Gizmos.DrawCube(destination, this.entityView.GetComponent<Renderer>().bounds.size);
        }
    }
}
