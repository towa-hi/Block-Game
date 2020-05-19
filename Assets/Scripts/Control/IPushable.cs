using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

public class IPushable : IComponent {
    [SerializeField]
    StateMachine stateMachine;
    bool doNext;
    EntityData pushedBy;
    [Header("Set In Editor")]
    public bool canKillOnFall;

    public override void Init() {
        this.stateMachine = new StateMachine();
        this.stateMachine.ChangeState(new IPushableWaitingState(this));
    }

    public override void DoFrame() {
        if (this.doNext) {
            // if floating
            if (GM.boardData.IsRectEmpty(this.entityData.pos + Vector2Int.down, this.entityData.size, this.entityData)) {
                this.stateMachine.ChangeState(new IPushableFallingState(this));
            } else {
                this.stateMachine.ChangeState(new IPushableWaitingState(this));
            }
        }
        this.stateMachine.Update();
    }

    public bool CanBePushed(Vector2Int aDirection, EntityData aPusher) {
        // if not already in pushed state
        if (!(this.stateMachine.GetState() is IPushablePushedState)) {
            if (this.pushedBy == null || this.pushedBy == aPusher) {
                print("CanBePushed direction" + aDirection);
                if (GM.boardData.IsRectEmpty(this.entityData.pos + aDirection, this.entityData.size, this.entityData)) {
                    return true;
                }
            }
        }
        return false;
    }

    public void Push(Vector2Int aDirection, EntityData aPushedBy) {
        this.pushedBy = aPushedBy;
        print("being pushed");
        this.stateMachine.ChangeState(new IPushablePushedState(this, aDirection));
    }

    void DoNext(bool aDoNext) {
        this.doNext = aDoNext;
    }

    #region StateClasses

    class IPushableWaitingState : GameState {
        IPushable iPushable;

        public IPushableWaitingState(IPushable aIPushable) {
            this.iPushable = aIPushable;
        }

        public void Enter() {
            this.iPushable.DoNext(false);
        }

        public void Update() {
            this.iPushable.DoNext(true);
        }

        public void Exit() {
            // print("ILocoWaitingState - exited");
        }
    }

    class IPushableFallingState : GameState {
        IPushable iPushable;
        EntityData entityData;
        EntityBase entityBase;
        public Vector2Int direction;
        public Vector2Int destination;
        public Vector3 startPosition;
        public Vector3 endPosition;
        public float t;

        public IPushableFallingState(IPushable aIPushable) {
            this.iPushable = aIPushable;
            this.direction = Vector2Int.down;
            this.entityData = aIPushable.entityData;
            this.entityBase = aIPushable.entityBase;
            this.destination = aIPushable.entityData.pos + Vector2Int.down;
            this.t = 0f;
        }

        public void Enter() {
            this.iPushable.DoNext(false);
            this.iPushable.DoNext(false);
            GM.boardData.MoveEntity(this.destination, this.entityData);
            this.startPosition = this.iPushable.transform.position;
            this.endPosition = Util.V2IOffsetV3(this.entityData.pos, this.entityData.size);
        }

        public void Update() {
            if (this.t < 1) {
                this.t += Time.deltaTime / Constants.GRAVITY;
                this.entityBase.transform.position = Vector3.Lerp(this.startPosition, this.endPosition, this.t);
            } else {
                this.iPushable.DoNext(true);
            }
        }

        public void Exit() {
            // print("ILocoWaitingState - exited");
        }
    }

    class IPushablePushedState : GameState {
        IPushable iPushable;
        EntityData entityData;
        EntityBase entityBase;
        public Vector2Int direction;
        public Vector2Int destination;
        public Vector3 startPosition;
        public Vector3 endPosition;
        public float t;

        public IPushablePushedState(IPushable aIPushable, Vector2Int aDirection) {
            Debug.Assert(Util.IsDirection(aDirection));
            this.iPushable = aIPushable;
            this.direction = aDirection;
            this.entityData = aIPushable.entityData;
            this.entityBase = aIPushable.entityBase;
            this.destination = this.entityData.pos + this.direction;
            this.t = 0f;
        }

        public void Enter() {
            // print("IPushablePushedState - entered");
            this.iPushable.DoNext(false);
            GM.boardData.MoveEntity(this.destination, this.entityData);
            this.startPosition = this.iPushable.transform.position;
            this.endPosition = Util.V2IOffsetV3(this.entityData.pos, this.entityData.size);
        }

        public void Update() {
            if (this.t < 1) {
                // stuff always gets pushed twice as fast as the velocity of the pusher
                this.t += Time.deltaTime / (Constants.PUSHSPEED / 2f);
                this.entityBase.transform.position = Vector3.Lerp(this.startPosition, this.endPosition, this.t);
            } else {
                this.iPushable.DoNext(true);
            }
            
        }

        public void Exit() {
            this.iPushable.pushedBy = null;
            // print("IPushablePushedState - exited");
        }
    }

    #endregion
}
