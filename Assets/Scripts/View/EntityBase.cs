using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using Sirenix.Utilities;

public class EntityBase : BoardStateListener {
    public int id;
    [SerializeField] bool isTempPos;
    public GameObject model;
    // public EntityImmutableData data;
    Renderer modelRenderer;
    HashSet<Renderer> childRenderers;
    EntityState oldEntityState;
    bool needsFirstUpdate;
    bool isDying;
    [SerializeField] StateMachine stateMachine;
    [SerializeField] bool needsNewState;
    [ShowInInspector] EntityState entityState {
        get {
            return GM.boardManager.GetEntityById(this.id);
        }
    }

    void Awake() {
        this.model = this.transform.GetChild(0).gameObject;
        this.modelRenderer = this.model.GetComponent<Renderer>();
        this.childRenderers = this.model.GetComponentsInChildren<Renderer>().ToHashSet();
        this.stateMachine = new StateMachine();
        this.needsFirstUpdate = true;
        this.needsNewState = true;
        this.isDying = false;
    }

    public void DoFrame() {
        if (this.needsNewState) {
            this.needsNewState = false;
            EntityBaseStateMachineState newState = ChooseNextState();
            print(this.id + " chose state " + newState.GetType());
            EntityBaseStateMachineState newStateAfterDeathCheck = StateAfterDeathCheck(newState);
            this.stateMachine.ChangeState(newStateAfterDeathCheck);
        }
        this.stateMachine.Update();
    }

    EntityBaseStateMachineState StateAfterDeathCheck(EntityBaseStateMachineState aNewState) {
        // TODO: write this
        // if this new states position contains entities
        // do a comparison of entities power
        // if i win, kill entity
        // if entity wins, return a new death state instead of aNewState
        if (aNewState.GetBlockingEntityIdSet() == null) {
            return aNewState;
        }
        HashSet<int> blockingEntityIdSet = aNewState.GetBlockingEntityIdSet().ToHashSet();
        // if im a mob
        if (this.entityState.mobData.HasValue) {
            // for each mob blocking me
            foreach (int blockingEntityId in blockingEntityIdSet) {
                EntityState blockingEntityState = GM.boardManager.GetEntityById(blockingEntityId);
                Debug.Assert(blockingEntityState.mobData.HasValue);
                if (this.entityState.mobData.Value.touchPower >= blockingEntityState.mobData.Value.touchPower) {
                    print(this.id + " StateAfterDeathCheck - is killing " + blockingEntityId);
                    blockingEntityState.entityBase.Die(DeathTypeEnum.BUMP);
                }
                else if (this.entityState.mobData.Value.touchPower < blockingEntityState.mobData.Value.touchPower) {
                    print(this.id + " StateAfterDeathCheck - is being killed by " + blockingEntityId);
                    return new DyingState(this.id);
                }
            }
        }
        return aNewState;
    }

    public void Die(DeathTypeEnum aDeathType) {
        this.stateMachine.ChangeState(new DyingState(this.id));
        print( this.id + " Die - set state to dying");
    }
    
    EntityBaseStateMachineState ChooseNextState() {
        switch (this.entityState.data.entityType) {
            case EntityTypeEnum.MOB:
                return MobChooseNextState();
            case EntityTypeEnum.SPECIALBLOCK:
                break;
            case EntityTypeEnum.BG:
                break;
            case EntityTypeEnum.BLOCK:
                break;
            case EntityTypeEnum.PUSHABLE:
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
        return new WaitingState();
    }

    EntityBaseStateMachineState MobChooseNextState() {
        switch (this.entityState.mobData?.movementType) {
            case MoveTypeEnum.INANIMATE:
                return InanimateChooseNextState();
            case MoveTypeEnum.PATROL:
                return MobPatrolChooseNextState();
            case MoveTypeEnum.FLY:
                return MobFlyChooseNextState();
            case MoveTypeEnum.PATHPATROL:
                return MobPathChooseNextState();
            case MoveTypeEnum.PATHFLY:
                return MobPathChooseNextState();
            case null:
                throw new Exception("MobChooseNextState - has no mobData!!");
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    EntityBaseStateMachineState InanimateChooseNextState() {
        if (!DoesFloorExist(this.entityState.pos)) {
            return new FallingState(this.id);
        }
        else {
            return new WaitingState();
        }
    }
    
    EntityBaseStateMachineState MobPatrolChooseNextState() {
        // if there is no floor under me
        if (!DoesFloorExist(this.entityState.pos)) {
            return new FallingState(this.id);
        }
        
        Vector2Int facing = this.entityState.facing;
        if (DoesFloorExist(this.entityState.pos + facing) && !IsMovementBlocked(this.entityState.pos + facing)) {
            return new WalkingState(facing, this.id);
        }

        if (this.entityState.mobData?.canHop == true) {
            Vector2Int facingUp = this.entityState.facing + Vector2Int.up;
            if (DoesFloorExist(this.entityState.pos + facingUp) && !IsMovementBlocked(this.entityState.pos + facingUp)) {
                return new HoppingState(facingUp, this.id);
            }

            Vector2Int facingDown = this.entityState.facing + Vector2Int.down;
            if (DoesFloorExist(this.entityState.pos + facingDown) && !IsMovementBlocked(this.entityState.pos + facingDown)) {
                return new HoppingState(facingDown, this.id);
            }
        }
        
        return new TurningState(this.id);
    }

    IEnumerable<BoardCell> GetFloorSlice(Vector2Int aOrigin) {
        Vector2Int floorOrigin = aOrigin + Vector2Int.down;
        Vector2Int floorSize = new Vector2Int(this.entityState.data.size.x, 1);
        return GM.boardManager.GetBoardGridSlice(floorOrigin, floorSize).Values;
    }
    // returns walking or hopping or turning
    
    bool DoesFloorExist(Vector2Int aPos) {
        // for each floor cell
        if (!GM.boardManager.IsPosInBoard(aPos + Vector2Int.down)) {
            // this is just a convinient LIE. if you try to floorcheck on y = 0 it will say there's no floor
            // a side effect of this is that if something is at 0, it will probably fall through the level
            // and break everything
            // print("FLOOR CHECKED A POS WHERE Y == 0");
            return false;
        }
        // ReSharper disable once LoopCanBeConvertedToQuery
        foreach (BoardCell boardCell in GetFloorSlice(aPos)) {
            // if floor cell has entity is not null and it isnt a mob
            if (boardCell.frontEntityState.HasValue && boardCell.frontEntityState?.data.entityType != EntityTypeEnum.MOB) {
                return true;
            }
        }
        return false;
    }
    
    bool IsMovementBlocked(Vector2Int aPos) {
        // for each cell in the new position
        foreach (BoardCell boardCell in GM.boardManager.GetBoardGridSlice(aPos, this.entityState.data.size).Values) {
            // if cell has entity 
            if (boardCell.frontEntityState.HasValue) {
                // if entity isn't me
                if (this.id != boardCell.frontEntityState.Value.data.id) {
                    // if entity isn't a mob
                    if (boardCell.frontEntityState?.data.entityType != EntityTypeEnum.MOB) {
                        return true;
                    }
                    // if entity is on my team
                    if (this.entityState.team == boardCell.frontEntityState?.team) {
                        return true;
                    }
                }
                
            }
        }
        return false;
    }
    
    EntityBaseStateMachineState MobFlyChooseNextState() {
        // TODO: write this
        return new WaitingState();
    }

    EntityBaseStateMachineState MobPathChooseNextState() {
        // TODO: write this
        return new WaitingState();
    }
    EntityBaseStateMachineState SpecialBlockChooseNextState() {
        // TODO: write this
        return new WaitingState();
    }

    EntityBaseStateMachineState DumbChooseNextState() {
        return new WaitingState();
    }

    public void SetNeedsNewState() {
        this.needsNewState = true;
        print("choosing state next frame");
    }
    
    public void Init(EntityState aEntityState) {
        // this.data = aEntityState.data;
        this.id = aEntityState.data.id;
        this.transform.position = Util.V2IOffsetV3(aEntityState.pos, aEntityState.data.size, aEntityState.data.isFront);
        this.name = aEntityState.data.name + " Id: " + this.id;
        if (aEntityState.hasNodes) {
            foreach (Vector2Int upNode in aEntityState.upNodes) {
                Vector2Int currentPos = aEntityState.pos + upNode;
                Vector3 currentPosition = Util.V2IOffsetV3(currentPos, new Vector2Int(1, 1));
                float studX = currentPosition.x;
                float studY = currentPosition.y + (Constants.BLOCKHEIGHT / 2);
                GameObject stud = Instantiate(GM.instance.studPrefab, new Vector3(studX, studY, 0), Quaternion.identity);
                stud.transform.SetParent(this.model.transform, true);
                Renderer studRenderer = stud.GetComponent<Renderer>();
                studRenderer.material.color = aEntityState.defaultColor;
                this.childRenderers.Add(studRenderer);
            }
        }
        if (aEntityState.mobData != null) {
            // this.mobBase.Init(aEntityState);
        }
        this.oldEntityState = aEntityState;
    }

    protected override void OnUpdateBoardState(BoardState aBoardState) {
        // when id is -42069, this wont recieve any boardupdates because it hasn't been
        // assigned an ID yet by BoardManager.CreateView
        if (this.isDying) {
            return;
        }
        if (this.entityState.data.id == Constants.PLACEHOLDERINT) {
            return;
        }
        EntityState newEntityState = aBoardState.entityDict[this.id];
        if (this.needsFirstUpdate) {
            this.oldEntityState = newEntityState;
            this.needsFirstUpdate = false;
        }
        if (!this.oldEntityState.Equals(newEntityState)) {
            // print(newEntityState.data.id + " - i should update");
            this.transform.position = Util.V2IOffsetV3(newEntityState.pos, newEntityState.data.size, this.entityState.data.isFront);
            SetColor(newEntityState.defaultColor);
            this.oldEntityState = newEntityState;
        }
        else {
            // print(this.data.id + " - i dont care");
        }
    }

    public void SetColor(Color aColor) {
        this.modelRenderer.material.color = aColor;
        foreach (Renderer childRenderer in this.childRenderers) {
            childRenderer.material.color = aColor;
        }
    }

    public void SetTempViewPosition(Vector2Int aPos) {
        this.transform.position = Util.V2IOffsetV3(aPos, this.entityState.data.size, this.entityState.data.isFront);
        this.isTempPos = true;
    }

    public void ResetView() {
        EntityState currentState = GM.boardManager.GetEntityById(this.id);
        this.transform.position = Util.V2IOffsetV3(currentState.pos, this.entityState.data.size, this.entityState.data.isFront);
        this.isTempPos = false;
    }
    
    void OnDrawGizmos() {
        if (GM.boardManager != null && GM.boardManager.currentState.entityDict.ContainsKey(this.id)) {
            Gizmos.color = Color.red;
            Vector2Int size = this.entityState.data.size;
            Vector3 position = Util.V2IOffsetV3(this.entityState.pos, size);
            Vector3 sizeV3 = new Vector3(size.x, size.y * Constants.BLOCKHEIGHT, 2f);
            Gizmos.DrawWireCube(position, sizeV3);
            if (this.entityState.hasNodes) {
                Vector3 zOffset = new Vector3(0, 0, -1.01f);
                Gizmos.color = Color.red;
                foreach (Vector2Int upNode in this.entityState.upNodes) {
                    Vector2Int currentPos = this.entityState.pos + upNode;
                    Vector3 arrowOrigin = Util.V2IOffsetV3(currentPos, new Vector2Int(1, 1)) + zOffset;
                    DrawArrow.I.ForGizmo(arrowOrigin, new Vector3(0, 0.5f, 0));
                }
        
                Gizmos.color = Color.blue;
                foreach (Vector2Int downNode in this.entityState.downNodes) {
                    Vector2Int currentPos = this.entityState.pos + downNode;
                    Vector3 arrowOrigin = Util.V2IOffsetV3(currentPos, new Vector2Int(1, 1)) + zOffset;
                    DrawArrow.I.ForGizmo(arrowOrigin, new Vector3(0, -0.5f, 0));
                }
            }
        }
    }

    abstract class EntityBaseStateMachineState : StateMachineState {
        protected Vector2Int direction;
        protected EntityBase entityBase;
        protected Vector2Int startPos;
        protected Vector3 startPosition;
        protected Vector2Int endPos;
        protected Vector3 endPosition;
        protected float moveSpeed;
        protected float t;

        public abstract void Enter();
        public abstract void Update();
        public abstract void Exit();
        public abstract IEnumerable<int> GetBlockingEntityIdSet();
    }
    
    class WalkingState : EntityBaseStateMachineState {

        public WalkingState(Vector2Int aDirection, int aId) {
            Debug.Assert(Util.IsDirection(aDirection));
            this.direction = aDirection;
            this.entityBase = GM.boardManager.GetEntityBaseById(aId);
            Debug.Assert(this.entityBase.entityState.mobData.HasValue);
            this.startPos = this.entityBase.entityState.pos;
            this.startPosition = Util.V2IOffsetV3(this.startPos, this.entityBase.entityState.data.size);
            this.endPos = this.entityBase.entityState.pos + this.direction;
            this.endPosition = Util.V2IOffsetV3(this.endPos, this.entityBase.entityState.data.size);
            this.moveSpeed = this.entityBase.entityState.mobData.Value.moveSpeed;
            this.t = 0f;
        }
        
        public override void Enter() {
            GM.boardManager.MoveEntity(this.entityBase.id, this.endPos);
        }
    
        public override void Update() {
            if (this.t < 1) {
                this.t += Time.deltaTime / this.moveSpeed;
                this.entityBase.transform.position = Vector3.Lerp(this.startPosition, this.endPosition, this.t);
            }
            else {
                this.entityBase.SetNeedsNewState();
            }
        }
    
        public override void Exit() {
            // maybe have ResetTempView here
            this.entityBase.ResetView();
        }

        public override IEnumerable<int> GetBlockingEntityIdSet() {
            Debug.Assert(this.endPos != null);
            Debug.Assert(this.entityBase != null);
            HashSet<int> blockingEntityIdSet = new HashSet<int>();
            foreach (BoardCell boardCell in GM.boardManager.GetBoardGridSlice(this.endPos, this.entityBase.entityState.data.size).Values) {
                if (boardCell.frontEntityState.HasValue) {
                    int blockingEntityId = boardCell.frontEntityState.Value.data.id;
                    if (this.entityBase.id != blockingEntityId) {
                        blockingEntityIdSet.Add(blockingEntityId);
                    }
                }
            }
            return blockingEntityIdSet;
        }
    }
    
    class HoppingState : EntityBaseStateMachineState {
        
        public HoppingState(Vector2Int aDirection, int aId) {
            Debug.Assert(Util.IsDirection(aDirection));
            this.direction = aDirection;
            this.entityBase = GM.boardManager.GetEntityBaseById(aId);
            Debug.Assert(this.entityBase.entityState.mobData.HasValue);
            this.startPos = this.entityBase.entityState.pos;
            this.startPosition = Util.V2IOffsetV3(this.startPos, this.entityBase.entityState.data.size);
            this.endPos = this.entityBase.entityState.pos + this.direction;
            this.endPosition = Util.V2IOffsetV3(this.endPos, this.entityBase.entityState.data.size);
            this.moveSpeed = this.entityBase.entityState.mobData.Value.moveSpeed;
            this.t = 0f;
        }
        
        public override void Enter() {
            GM.boardManager.MoveEntity(this.entityBase.id, this.endPos);
        }
    
        public override void Update() {
            if (this.t < 1) {
                this.t += Time.deltaTime / this.moveSpeed;
                this.entityBase.transform.position = Vector3.Lerp(this.startPosition, this.endPosition, this.t);
            }
            else {
                this.entityBase.SetNeedsNewState();
            }
        }
    
        public override void Exit() {
            this.entityBase.ResetView();
        }
        
        public override IEnumerable<int> GetBlockingEntityIdSet() {
            Debug.Assert(this.endPos != null);
            Debug.Assert(this.entityBase != null);
            HashSet<int> blockingEntityIdSet = new HashSet<int>();
            foreach (BoardCell boardCell in GM.boardManager.GetBoardGridSlice(this.endPos, this.entityBase.entityState.data.size).Values) {
                if (boardCell.frontEntityState.HasValue) {
                    int blockingEntityId = boardCell.frontEntityState.Value.data.id;
                    if (this.entityBase.id != blockingEntityId) {
                        blockingEntityIdSet.Add(blockingEntityId);
                    }
                }
            }
            return blockingEntityIdSet;
        }
    }
    
    class TurningState : EntityBaseStateMachineState {
        Quaternion startRotation;
        Quaternion endRotation;

        public TurningState(int aId) {
            this.entityBase = GM.boardManager.GetEntityBaseById(aId);
            Debug.Assert(this.entityBase.entityState.mobData.HasValue);
            Debug.Assert(Util.IsDirection(this.entityBase.entityState.facing));
        }
        
        public override void Enter() {
            this.startRotation = this.entityBase.transform.rotation;
            this.endRotation = Quaternion.AngleAxis(180, Vector3.up) * this.startRotation;
            if (this.entityBase.entityState.facing == Vector2Int.left) {
                GM.boardManager.SetEntityFacing(this.entityBase.id, Vector2Int.right);
            }
            else if (this.entityBase.entityState.facing == Vector2Int.right) {
                GM.boardManager.SetEntityFacing(this.entityBase.id, Vector2Int.left);
            }
            else {
                throw new Exception("TurningState - entity is not facing left or right");
            }
            Debug.Assert(this.entityBase.entityState.mobData.HasValue);
            this.moveSpeed = this.entityBase.entityState.mobData.Value.moveSpeed;
            this.t = 0f;
        }
    
        public override void Update() {
            if (this.t < 1) {
                this.t += Time.deltaTime / this.moveSpeed;
                this.entityBase.transform.rotation = Quaternion.Lerp(this.startRotation, this.endRotation, this.t);
            }
            else {
                this.entityBase.SetNeedsNewState();
            }
        }
    
        public override void Exit() {
            this.entityBase.ResetView();
        }
        
        public override IEnumerable<int> GetBlockingEntityIdSet() {
            return null;
        }
    }
    
    class PushingState : EntityBaseStateMachineState {
    
        public PushingState(int aId, Vector2Int aDirection) {
            this.direction = Vector2Int.down;
            this.entityBase = GM.boardManager.GetEntityBaseById(aId);
            Debug.Assert(this.entityBase.entityState.mobData.HasValue);
            this.startPos = this.entityBase.entityState.pos;
            this.startPosition = Util.V2IOffsetV3(this.startPos, this.entityBase.entityState.data.size);
            this.endPos = this.entityBase.entityState.pos + this.direction;
            this.endPosition = Util.V2IOffsetV3(this.endPos, this.entityBase.entityState.data.size);
            this.moveSpeed = Constants.GRAVITY;
            this.t = 0f;
        }
        
        public override void Enter() {
            GM.boardManager.MoveEntity(this.entityBase.id, this.endPos);
        }
    
        public override void Update() {
            if (this.t < 1) {
                this.t += Time.deltaTime / this.moveSpeed;
                this.entityBase.transform.position = Vector3.Lerp(this.startPosition, this.endPosition, this.t);
            }
            else {
                this.entityBase.SetNeedsNewState();
            }
        }
    
        public override void Exit() {
            this.entityBase.ResetView();
        }
        
        public override IEnumerable<int> GetBlockingEntityIdSet() {
            Debug.Assert(this.endPos != null);
            Debug.Assert(this.entityBase != null);
            HashSet<int> blockingEntityIdSet = new HashSet<int>();
            foreach (BoardCell boardCell in GM.boardManager.GetBoardGridSlice(this.endPos, this.entityBase.entityState.data.size).Values) {
                if (boardCell.frontEntityState.HasValue) {
                    int blockingEntityId = boardCell.frontEntityState.Value.data.id;
                    if (this.entityBase.id != blockingEntityId) {
                        blockingEntityIdSet.Add(blockingEntityId);
                    }
                }
            }
            return blockingEntityIdSet;
        }
    }
    
    class FlyingState : EntityBaseStateMachineState {
        public override void Enter() {
        }
    
        public override void Update() {
        }
    
        public override void Exit() {
        }
        
        public override IEnumerable<int> GetBlockingEntityIdSet() {
            return null;
        }
    }
    
    class WaitingState : EntityBaseStateMachineState {
        public override void Enter() {
            
        }
    
        public override void Update() {
            
        }
    
        public override void Exit() {
            
        }
        
        public override IEnumerable<int> GetBlockingEntityIdSet() {
            return null;
        }
    }
    
    class FallingState : EntityBaseStateMachineState {

        public FallingState(int aId) {
            this.direction = Vector2Int.down;
            this.entityBase = GM.boardManager.GetEntityBaseById(aId);
            this.startPos = this.entityBase.entityState.pos;
            this.startPosition = Util.V2IOffsetV3(this.startPos, this.entityBase.entityState.data.size);
            this.endPos = this.entityBase.entityState.pos + this.direction;
            this.endPosition = Util.V2IOffsetV3(this.endPos, this.entityBase.entityState.data.size);
            this.moveSpeed = Constants.GRAVITY;
            this.t = 0f;
        }
        
        public override void Enter() {
            GM.boardManager.MoveEntity(this.entityBase.id, this.endPos);
        }
    
        public override void Update() {
            if (this.t < 1) {
                this.t += Time.deltaTime / this.moveSpeed;
                this.entityBase.transform.position = Vector3.Lerp(this.startPosition, this.endPosition, this.t);
            }
            else {
                this.entityBase.SetNeedsNewState();
            }
        }
    
        public override void Exit() {
            this.entityBase.ResetView();
        }
        
        public override IEnumerable<int> GetBlockingEntityIdSet() {
            Debug.Assert(this.endPos != null);
            Debug.Assert(this.entityBase != null);
            HashSet<int> blockingEntityIdSet = new HashSet<int>();
            foreach (BoardCell boardCell in GM.boardManager.GetBoardGridSlice(this.endPos, this.entityBase.entityState.data.size).Values) {
                if (boardCell.frontEntityState.HasValue) {
                    int blockingEntityId = boardCell.frontEntityState.Value.data.id;
                    if (this.entityBase.id != blockingEntityId) {
                        blockingEntityIdSet.Add(blockingEntityId);
                    }
                }
            }
            return blockingEntityIdSet;
        }
    }
    
    class DyingState : EntityBaseStateMachineState {
        [SerializeField] readonly float timeToDie;
        
        public DyingState(int aId) {
            this.entityBase = GM.boardManager.GetEntityBaseById(aId);
            this.timeToDie = 0.5f;
            this.t = 0f;
        }
        public override void Enter() {
            this.entityBase.isDying = true;
            print("entered death state");
            GM.playManager.StartEntityForDeath(this.entityBase.id);
        }
    
        public override void Update() {
            if (this.t < 1) {
                this.t += Time.deltaTime / this.timeToDie;
            }
            else {
                print(this.entityBase.id + " DyingState - completed animation");
                GM.playManager.FinishEntityDeath(this.entityBase.id);
            }
        }
    
        public override void Exit() {
        }
        
        public override IEnumerable<int> GetBlockingEntityIdSet() {
            return null;
        }
    }
}
