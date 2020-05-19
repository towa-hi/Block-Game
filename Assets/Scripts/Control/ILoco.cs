using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
// TODO: change how bumpcheck and apply bumpcheck works so pushing is done before evaluating ground
// so junkbot will push stuff when theres a 1 tile gap
// TODO: entities that can push wont push if they have to hop first. this is intentional for now
public class ILoco : IComponent {
    [SerializeField]
    StateMachine stateMachine;
    bool doNext;
    [Header("Set In Editor")]
    public bool canKillOnTouch;
    public int touchDamage;
    [Space]
    public bool canKillOnFall;
    public int fallDamage;
    [Space]
    public float turningMoveSpeed;
    [Space]
    public bool canWalk;
    public float walkingMoveSpeed;
    [Space]
    public bool canHop;
    public float hoppingMoveSpeed;
    public AnimationCurve hoppingCurve;
    [Space]
    public bool canPush;
    [Space]
    public bool canBeLifted;

    public override void Init() {
        this.stateMachine = new StateMachine();
        this.stateMachine.ChangeState(new ILocoWaitingState(this));
    }

    public override void DoFrame() {
        if (this.doNext) {
            this.stateMachine.ChangeState(ChooseNextState());
        }
        this.stateMachine.Update();
    }

    // this answers the question: what do i have to kill/push before I can move
    // to this position? if null, it means this move is blocked by something that i cant kill or push
    // also checks if position has a ground after everything has been killed/pushed
    BumpCheckResults BumpCheck(Vector2Int aDirection) {
        // print("doing bumpcheck for " + this.entityData.name + " at direction" + aDirection);
        HashSet<EntityData> entitiesToKill = new HashSet<EntityData>();
        HashSet<EntityData> entitiesToPush = new HashSet<EntityData>();
        Vector2Int newPos = this.entityData.pos + aDirection;
        foreach (EntityData touchedEntity in GM.boardData.GetEntitiesInRect(newPos, this.entityData.size, this.entityData)) {
            // if same team, is blocked
            if (touchedEntity.team == this.entityData.team) {
                // print("is blocked because same team");
                return null;
            }
            IPushable touchedIPushable = touchedEntity.entityBase.GetCachedIComponent<IPushable>() as IPushable;
            // if can kill else if can push
            if (this.touchDamage > touchedEntity.touchDefense) {
                entitiesToKill.Add(touchedEntity);
            } else if (this.canPush && touchedIPushable != null) {
                if (touchedIPushable.CanBePushed(aDirection, this.entityData)) {
                    entitiesToPush.Add(touchedEntity);
                } else {
                    // print("is blocked cuz pushable cant be pushed");
                    return null;
                }
            } else {
                // print("is blocked because entity cant be pushed or killed");
                return null;;
            }
        }
        // make a set of entities to ignore when doing a ground check
        HashSet<EntityData> ignoreSet = new HashSet<EntityData>();
        ignoreSet.UnionWith(entitiesToKill);
        ignoreSet.UnionWith(entitiesToPush);
        ignoreSet.Add(this.entityData);
        if (GM.boardData.IsRectEmpty(this.entityData.pos + aDirection + Vector2Int.down, this.entityData.size, ignoreSet)) {
            // print("no ground under entity")
            return null;
        } else {
            return new BumpCheckResults(aDirection, entitiesToKill, entitiesToPush);;
        }
    }

    //TODO: make this work with fans
    GameState ChooseNextState() {
         if (this.canBeLifted) {
                // if (GM.playManager.EntityFanCheck(this.entityData)) {
                //     // TODO finish this
                //     print("should be lifted");
                // }
            }
        // if floating
        if (GM.boardData.IsEntityPosFloating(this.entityData.pos, this.entityData)) {
            // fall down
            return new ILocoFallingState(this);
        }
        // if can walk
        if (this.canWalk) {
            BumpCheckResults facingBumpCheck = BumpCheck(this.entityData.facing);
            // if can walk in facing direction
            if (facingBumpCheck != null) {
                DoBumpCheckResults(facingBumpCheck);
                if (facingBumpCheck.shouldPush) {
                    return new ILocoPushingState(this, this.entityData.facing);
                } else {
                    return new ILocoWalkingState(this, this.entityData.facing);
                }
            // else if can hop
            } else if (this.canHop) {
                    BumpCheckResults facingUpBumpCheck = BumpCheck(this.entityData.facing + Vector2Int.up);
                    BumpCheckResults facingDownBumpCheck = BumpCheck(this.entityData.facing + Vector2Int.down);
                    // if can hop up
                    if (facingUpBumpCheck != null) {
                        DoBumpCheckResults(facingUpBumpCheck);
                        return new ILocoHoppingState(this, this.entityData.facing + Vector2Int.up);
                    // if can hop down
                    } else if (facingDownBumpCheck != null) {
                        DoBumpCheckResults(facingDownBumpCheck);
                        return new ILocoHoppingState(this, this.entityData.facing + Vector2Int.down);
                    // turn around
                    } else {
                        return new ILocoTurningState(this);
                    }
            } else {
                // print("turning");
                return new ILocoTurningState(this);
            }
        } else {
            return new ILocoWaitingState(this);
        }
    }

    void DoNext(bool aDoNext) {
        this.doNext = aDoNext;
    }

    // kills or pushes everything in BumpCheckResults so entity can move to a empty place
    void DoBumpCheckResults(BumpCheckResults aBumpCheck) {
        // if killables exist, yeet them
        foreach (EntityData entityToKill in aBumpCheck.entitiesToKill) {
            GM.playManager.BeginEntityDeath(entityToKill);
        }
        // if pushables exist
        if (this.canPush) {
            if (aBumpCheck.entitiesToPush.Count != 0) {
                foreach (EntityData entityToPush in aBumpCheck.entitiesToPush) {
                    IPushable entityIPushable = entityToPush.entityBase.GetCachedIComponent<IPushable>() as IPushable;
                    Debug.Assert(entityIPushable.CanBePushed(aBumpCheck.direction, this.entityData));
                    // set that ones state to  be pushed
                    entityIPushable.Push(aBumpCheck.direction, this.entityData);
                }
            }
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
        public Vector2Int direction;
        public Vector2Int destination;
        protected Vector3 startPosition;
        protected Vector3 endPosition;
        [SerializeField]
        protected float t;

        abstract public void Enter();
        abstract public void Update();
        abstract public void Exit();
    }

    // causes entity to rise up one tile when raised by a fan
    class ILocoRisingState : ILocoState {

        public ILocoRisingState(ILoco aILoco) {
            this.iLoco = aILoco;
            this.entityBase = aILoco.entityBase;
            this.entityData = aILoco.entityData;
            this.direction = Vector2Int.up;
            this.destination = aILoco.entityData.pos + Vector2Int.up;
            this.t = 0f;
        }

        public override void Enter() {
            // print("ILocoRisingState - entered");
            this.iLoco.DoNext(false);
            this.startPosition = this.entityBase.transform.position;
            // move the entity to new pos
            GM.boardData.MoveEntity(this.destination, this.entityData);
            this.endPosition = Util.V2IOffsetV3(this.destination, this.entityData.size);
            // TODO: do some kind of rising animation
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

    class ILocoFloatingState : ILocoState {

        public ILocoFloatingState(ILoco aILoco) {
            this.iLoco = aILoco;
            this.entityBase = aILoco.entityBase;
            this.entityData = aILoco.entityData;
            this.t = 0f;
        }

        public override void Enter() {
            // print("ILocoFloatingState - entered");
            this.iLoco.DoNext(false);
            // TODO: do some kind of floating animation
        }

        public override void Update() {
            this.iLoco.DoNext(true);
        }

        public override void Exit() {
             // print("ILocoFloatingState - exited");
            this.entityBase.ResetViewPosition();
        }
    }

    // makes entity move to destination
    class ILocoWalkingState : ILocoState {

        public ILocoWalkingState(ILoco aILoco, Vector2Int aDirection) {
            Debug.Assert(Util.IsDirection(aDirection));
            this.iLoco = aILoco;
            this.entityBase = aILoco.entityBase;
            this.entityData = aILoco.entityData;
            this.direction = aDirection;
            this.destination = aILoco.entityData.pos + aDirection;
            this.t = 0f;
        }

        public override void Enter() {
            // print("ILocoWalkingState - entered");
            this.iLoco.DoNext(false);
            this.startPosition = this.entityBase.transform.position;
            // move the entity to new pos
            GM.boardData.MoveEntity(this.destination, this.entityData);
            this.endPosition = Util.V2IOffsetV3(this.destination, this.entityData.size);
            // TODO: do some kind of walking animation
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

    // makes entity turn around 180 degrees
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
            // flip this entity
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

    // makes entity fall to destination
    class ILocoFallingState : ILocoState {

        public ILocoFallingState(ILoco aILoco) {
            this.iLoco = aILoco;
            this.entityBase = aILoco.entityBase;
            this.entityData = aILoco.entityData;
            this.direction = Vector2Int.down;
            this.destination = aILoco.entityData.pos + this.direction;

            this.t = 0f;
        }

        public override void Enter() {
            // print("ILocoFallingState - entered");
            this.iLoco.DoNext(false);
            this.startPosition = this.entityBase.transform.position;
            // move the entity to new pos
            GM.boardData.MoveEntity(this.destination, this.entityData);
            this.endPosition = Util.V2IOffsetV3(this.destination, this.entityData.size);
            // TODO: do some kind of falling animation
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
            this.entityBase.ResetViewPosition();
            // print("ILocoFallingState - exited");
        }
    }

    // makes entity hop to destination
    class ILocoHoppingState : ILocoState {

        public ILocoHoppingState(ILoco aILoco, Vector2Int aDirection) {
            Debug.Assert(Util.IsDirection(aDirection));
            this.iLoco = aILoco;
            this.entityBase = aILoco.entityBase;
            this.entityData = aILoco.entityData;
            this.direction = aDirection;
            this.destination = aILoco.entityData.pos + aDirection;
            this.t = 0f;
        }

        public override void Enter() {
            // print("ILocoHoppingState - entered");
            this.iLoco.DoNext(false);
            this.startPosition = this.entityBase.transform.position;
            // move the entity to new pos
            GM.boardData.MoveEntity(this.destination, this.entityData);
            this.endPosition = Util.V2IOffsetV3(this.destination, this.entityData.size);
            // TODO: do some kind of hopping animation
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

    /// does nothing for one frame
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
            // TODO: do some kind of idle animation
        }

        public override void Update() {
            this.iLoco.DoNext(true);
        }

        public override void Exit() {
            // print("ILocoWaitingState - exited");
        }
    }

    // pushing is done in DoBumpCheckResults. this just moves the entity
    // in the right direction but slowly and does the animation
    class ILocoPushingState : ILocoState {

        public ILocoPushingState(ILoco aILoco, Vector2Int aDirection) {
            Debug.Assert(Util.IsDirection(aDirection));
            this.iLoco = aILoco;
            this.entityBase = aILoco.entityBase;
            this.entityData = aILoco.entityData;
            this.destination = aILoco.entityData.pos + aDirection;
            this.direction = aDirection;
            this.t = 0f;
        }

        public override void Enter() {
            print("ILocoPushingState - entered");
            this.iLoco.DoNext(false);
            // set starting position
            this.startPosition = this.iLoco.transform.position;
            GM.boardData.MoveEntity(this.destination, this.entityData);
            // set ending opsition
            this.endPosition = Util.V2IOffsetV3(this.destination, this.entityData.size);
        }

        public override void Update() {
            if (this.t < 1) {
                this.t += Time.deltaTime / Constants.PUSHSPEED;
                this.entityBase.transform.position = Vector3.Lerp(this.startPosition, this.endPosition, this.t);
            } else {
                this.iLoco.DoNext(true);
            }
        }

        public override void Exit() {
            this.entityBase.ResetViewPosition();
            print("ILocoPushingState - exited");
        }
    }

    #endregion
}

// class for bump check results that holds two sets for entities
// that need to be killed or pushed before you can move to a location
public class BumpCheckResults {
    public HashSet<EntityData> entitiesToKill;
    public HashSet<EntityData> entitiesToPush;
    public Vector2Int direction;

    public bool shouldPush {
        get {return entitiesToPush.Count > 0;}
    }
    public BumpCheckResults(Vector2Int aDirection, HashSet<EntityData> aEntitiesToKill, HashSet<EntityData> aEntitiesToPush) {
        this.direction = aDirection;
        this.entitiesToKill = aEntitiesToKill;
        this.entitiesToPush = aEntitiesToPush;
    }
}