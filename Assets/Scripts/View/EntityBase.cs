using System;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using Sirenix.Utilities;

public class EntityBase : MonoBehaviour {
    public int id;
    public bool isTempPos;
    public GameObject model;
    Renderer modelRenderer;
    HashSet<Renderer> childRenderers;
    EntityState oldEntityState;
    bool needsFirstUpdate;
    bool isDying;
    bool isPlayer;
    [SerializeField] StateMachine stateMachine;
    [SerializeField] bool needsNewState;

    Shader originalShader;

    [ShowInInspector] EntityState entityState {
        get {
            // TODO: make this not throw errors on inspection with Application.isPlaying
            return GM.boardManager.GetEntityById(this.id);
        }
    }

    public bool isMarked;
    #region Lifecycle

    void Awake() {
        this.model = this.transform.GetChild(0).gameObject;
        this.modelRenderer = this.model.GetComponent<Renderer>();
        this.childRenderers = this.model.GetComponentsInChildren<Renderer>().ToHashSet();
        this.stateMachine = new StateMachine();
        this.needsFirstUpdate = true;
        this.needsNewState = true;
        this.isDying = false;
        this.isMarked = false;
        this.originalShader = this.modelRenderer.material.shader;
    }

    void OnEnable() {
        GM.boardManager.OnUpdateBoardState += OnUpdateBoardState;
        if (GM.boardManager.currentState.isInitialized) {
            OnUpdateBoardState(GM.boardManager.currentState);
        }
    }

    void OnDisable() {
        GM.boardManager.OnUpdateBoardState -= OnUpdateBoardState;
    }

    void Update() {
        if (this.isMarked) {
            this.markerT += Time.deltaTime / 2.0f;
            if (this.markerT > 1) {
                this.isMarked = false;
                this.markerT = 0f;
            }
        }
    }
    public void Init(EntityState aEntityState) {
        this.id = aEntityState.data.id;
        this.transform.position = Util.V2IOffsetV3(aEntityState.pos, aEntityState.data.size, aEntityState.data.isFront);
        this.name = aEntityState.data.name;
        if (aEntityState.hasNodes) {
            foreach (Node upNode in aEntityState.GetNodes(true)) {
                Vector3 currentPosition = Util.V2IOffsetV3(upNode.absolutePos, new Vector2Int(1, 1));
                float studX = currentPosition.x;
                float studY = currentPosition.y + (Constants.BLOCKHEIGHT / 2);
                GameObject stud = Instantiate(GM.instance.studPrefab, new Vector3(studX, studY, 0), Quaternion.identity);
                stud.transform.SetParent(this.model.transform, true);
                Renderer studRenderer = stud.GetComponent<Renderer>();
                studRenderer.material.color = aEntityState.defaultColor;
                this.childRenderers.Add(studRenderer);
            }
        }
        SetColor(aEntityState.defaultColor);
        this.oldEntityState = aEntityState;
        if (aEntityState.team == TeamEnum.PLAYER) {
            this.isPlayer = true;
        }
        // if (!aEntityState.data.isFront) {
        //     GM.boardManager.OnUpdateBoardState -= OnUpdateBoardState;
        //     GM.boardManager.OnUpdateBoardStateBG += OnUpdateBoardState;
        // }
    }
    
    public void DoFrame() {
        if (this.needsNewState) {
            this.needsNewState = false;
            EntityBaseStateMachineState effectState = ApplyEffects();
            if (effectState != null) {
                this.stateMachine.ChangeState(effectState);
            }
            else {
                EntityBaseStateResults newStateResults = ChooseNextState();
                EntityBaseStateMachineState finalState = ProcessStateResults(newStateResults);
                this.stateMachine.ChangeState(finalState);
            }
        }
        this.stateMachine.Update();
    }

    #endregion
    
    #region Listeners

    protected void OnUpdateBoardState(BoardState aBoardState) {
        // ignore when entity is dying
        if (this.isDying) {
            return;
        }
        // when id is -42069, this wont recieve any boardupdates because it hasn't been
        // assigned an ID yet by BoardManager.CreateView
        if (this.entityState.data.id == Constants.PLACEHOLDERINT) {
            return;
        }
        EntityState newEntityState = aBoardState.entityDict[this.id];
        // if first update
        if (this.needsFirstUpdate) {
            // this.oldEntityState = newEntityState;
            this.needsFirstUpdate = false;
        }
        if (!this.oldEntityState.defaultColor.Equals(newEntityState.defaultColor)) {
            SetColor(newEntityState.defaultColor);
        }
        // if any changes detected
        // if (!this.oldEntityState.Equals(newEntityState)) {
        //     if (!this.oldEntityState.defaultColor.Equals(newEntityState.defaultColor)) {
        //         SetColor(newEntityState.defaultColor);
        //     }
        //     this.oldEntityState = newEntityState;
        // }
    }

    #endregion

    #region State Hijacker

    public void Die(DeathTypeEnum aDeathType) {
        this.stateMachine.ChangeState(new DyingState(this.id));
        print( this.id + " Die - set state to dying");
    }
    
    public void Push(Vector2Int aDirection) {
        print("entity has been pushed in " + aDirection);
    }
    

    #endregion

    #region StateMachine

    protected void SetNeedsNewState() {
        this.needsNewState = true;
        // print("choosing state next frame");
    }

    EntityBaseStateMachineState ApplyEffects() {
        // apply win result

        if (this.isPlayer) {
            foreach (BoardCell boardCell in GM.boardManager.GetBoardGridSlice(this.entityState.pos, this.entityState.data.size).Values) {
                if (!boardCell.IsExit()) {
                    return null;
                }
            }
            return new ExitingState(this.id);
        }
        return null;
    }

    EntityBaseStateMachineState ProcessStateResults(EntityBaseStateResults aStateResults) {
        Debug.Assert(aStateResults.isStateValid);
        if (aStateResults.shouldEntityDie) {
            return new DyingState(this.id);
        }
        if (aStateResults.entityKillSet != null) {
            foreach (int killId in aStateResults.entityKillSet) {
                GM.boardManager.GetEntityBaseById(killId).Die(DeathTypeEnum.BUMP);
            }
        }
        if (aStateResults.entityPushSet != null) {
            foreach (int pushId in aStateResults.entityPushSet) {
                GM.boardManager.GetEntityBaseById(pushId).Push(aStateResults.stateMachineState.direction);
            }
        }
        return aStateResults.stateMachineState;
    }

    EntityBaseStateResults ChooseNextState() {
        // TODO: sometimes this makes a exception when the entity is dead but chooseNextState still happened
        switch (this.entityState.data.entityType) {
            case EntityTypeEnum.MOB:
                return MobChooseNextState();
            case EntityTypeEnum.SPECIALBLOCK:
                return SpecialBlockChooseNextState();
            case EntityTypeEnum.BG:
                return DumbChooseNextState();
            case EntityTypeEnum.BLOCK:
                return DumbChooseNextState();
            case EntityTypeEnum.PUSHABLE:
                return DumbChooseNextState();
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    EntityBaseStateResults MobChooseNextState() {
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

    EntityBaseStateResults InanimateChooseNextState() {
        // try falling
        if (this.entityState.mobData?.canFall == true) {
            FallingState fallingState = new FallingState(this.id);
            EntityBaseStateResults fallingStateResults = fallingState.GetStateResults();
            if (fallingStateResults.isStateValid) { return fallingStateResults; }
        }
        // wait
        WaitingState waitingState = new WaitingState();
        return waitingState.GetStateResults();
    }
    
    EntityBaseStateResults MobPatrolChooseNextState() {
        // try falling
        if (this.entityState.mobData?.canFall == true) {
            FallingState fallingState = new FallingState(this.id);
            EntityBaseStateResults fallingStateResults = fallingState.GetStateResults();
            if (fallingStateResults.isStateValid) { return fallingStateResults; }
        }
        // try walking in facing direction
        Vector2Int facing = this.entityState.facing;
        WalkingState walkingState = new WalkingState(facing, this.id);
        EntityBaseStateResults walkingStateResults = walkingState.GetStateResults();
        if (walkingStateResults.isStateValid) { return walkingStateResults; }

        if (this.entityState.mobData?.canHop == true) {
            // try hop up
            Vector2Int facingUp = this.entityState.facing + Vector2Int.up;
            HoppingState hoppingStateUp = new HoppingState(facingUp, this.id);
            EntityBaseStateResults hoppingStateUpResults = hoppingStateUp.GetStateResults();
            if (hoppingStateUpResults.isStateValid) { return hoppingStateUpResults; }
            // try hop down
            Vector2Int facingDown = this.entityState.facing + Vector2Int.down;
            HoppingState hoppingStateDown = new HoppingState(facingDown, this.id);
            EntityBaseStateResults hoppingStateDownResults = hoppingStateDown.GetStateResults();
            if (hoppingStateDownResults.isStateValid) { return hoppingStateDownResults; }
        }
        // turn around
        TurningState turningState = new TurningState(this.id);
        EntityBaseStateResults turningStateResults = turningState.GetStateResults();
        return turningStateResults;
    }
    
    EntityBaseStateResults MobFlyChooseNextState() {
        // TODO: write this
        WaitingState waitingState = new WaitingState();
        return waitingState.GetStateResults();
    }
    
    EntityBaseStateResults MobPathChooseNextState() {
        // TODO: write this
        WaitingState waitingState = new WaitingState();
        return waitingState.GetStateResults();
    }
    EntityBaseStateResults SpecialBlockChooseNextState() {
        // TODO: write this
        WaitingState waitingState = new WaitingState();
        return waitingState.GetStateResults();
    }
    
    EntityBaseStateResults DumbChooseNextState() {
        WaitingState waitingState = new WaitingState();
        return waitingState.GetStateResults();
    }

    #endregion
    
    #region View

    public void SetDithering(bool aIsDithering) {
        if (aIsDithering) {
            Shader ditheringShader = GM.instance.ditheringShader;
            foreach (Renderer childRenderer in this.childRenderers) {
                var material = childRenderer.material;
                material.shader = ditheringShader;
                material.SetFloat("_Opacity", 0.5f);
                material.SetFloat("_DitherSize", 2f);
                material.SetColor("_AlbedoColor", this.entityState.defaultColor);
                material.color = this.entityState.defaultColor;
            }
        }
        else {
            foreach (Renderer childRenderer in this.childRenderers) {
                var material = childRenderer.material;
                material.shader = this.originalShader;
                material.color = this.entityState.defaultColor;

            }
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
        if (!this.isTempPos) {
            SetDithering(true);
        }
        this.isTempPos = true;
    }

    public void ResetView() {
        EntityState currentState = GM.boardManager.GetEntityById(this.id);
        this.transform.position = Util.V2IOffsetV3(currentState.pos, this.entityState.data.size, this.entityState.data.isFront);
        if (this.isTempPos) {
            SetDithering(false);
        }
        this.isTempPos = false;

    }

    #endregion

    #region Utility

    // true if this entity can be pushed
    public bool PushableCheckIfPushable(Vector2Int aDirection) {
        Debug.Assert(Util.IsDirection(aDirection));
        // if this entity has mobData and canBePushed
        if (this.entityState.mobData?.canBePushed == true) {
            Vector2Int newPos = this.entityState.pos + aDirection;
            // for each cell in the new pos
            foreach (BoardCell boardCell in GM.boardManager.GetBoardGridSlice(newPos, this.entityState.data.size).Values) {
                // if entity exists and isn't me
                if (boardCell.frontEntityState.HasValue && this.id != boardCell.frontEntityState.Value.data.id) {
                    return false;
                }
            }
            // return true because new pos is empty
            return true;
        }
        else {
            // return false because not a mob or cant be pushed
            return false;
        }
    }

    #endregion

    float markerT = 0f;
    float markerDuration = 1f;
    Color markerColor = Color.white;
    public void SetMarker(Color aColor, float aDuration) {
        this.isMarked = true;
        this.markerDuration = aDuration;
        this.markerColor = aColor;

    }
    
    void OnDrawGizmos() {
        // TODO: figure out why this doesnt draw gizmos for id == 0 for some reason

        if (GM.boardManager != null && GM.boardManager.currentState.entityDict.ContainsKey(this.id)) {
            EntityState currentEntityState = this.entityState;
            if (this.isMarked) {
                this.markerT += Time.deltaTime / this.markerDuration;
            }
            if (this.markerT > 1) {
                this.isMarked = false;
                this.markerT = 0;
            }
            Gizmos.color = this.isMarked ? this.markerColor : Color.white;
            Vector2Int size = this.entityState.data.size;
            Vector3 position = Util.V2IOffsetV3(this.entityState.pos, size, currentEntityState.data.isFront);
            Vector3 sizeV3 = new Vector3(size.x, size.y * Constants.BLOCKHEIGHT, 2f);
            Gizmos.DrawWireCube(position, sizeV3);
            if (currentEntityState.hasNodes) {
                Vector3 zOffset = new Vector3(0, 0, -1.01f);
                foreach (Node node in currentEntityState.GetNodes()) {
                    Vector3 arrowOrigin = Util.V2IOffsetV3(node.absolutePos, new Vector2Int(1, 1)) + zOffset;
                    Gizmos.color = node.isUp ? Color.red : Color.blue;
                    Vector3 direction = node.isUp ? new Vector3(0, 0.5f, 0) : new Vector3(0, -0.5f, 0);
                    DrawArrow.I.ForGizmo(arrowOrigin, direction);
                }
            }
        }
    }

    #region StateMachineStates

    abstract class EntityBaseStateMachineState : StateMachineState {
        public Vector2Int direction;
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
        public abstract EntityBaseStateResults GetStateResults();
    }

    class PostBoxState : EntityBaseStateMachineState {
        public PostBoxState(int aId) {
            this.entityBase = GM.boardManager.GetEntityBaseById(aId);
            this.t = 0f;
        }

        public override void Enter() {
            this.entityBase.SetMarker(Color.yellow, Constants.SPAWNTIME);
        }

        public override void Update() {
            if (this.t < 1) {
                this.t += Time.deltaTime / Constants.SPAWNTIME;
                this.entityBase.transform.position = Vector3.Lerp(this.startPosition, this.endPosition, this.t);
            }
            else {
                GM.playManager.WinBoard();
                this.entityBase.SetNeedsNewState();
            }
        }

        public override void Exit() {
            throw new NotImplementedException();
        }

        public override EntityBaseStateResults GetStateResults() {
            throw new NotImplementedException();
        }
    }

    class ExitingState : EntityBaseStateMachineState {

        public ExitingState(int aId) {
            this.entityBase = GM.boardManager.GetEntityBaseById(aId);
            Vector3 zOffset = new Vector3(0, 0, 3.1f);
            this.startPos = this.entityBase.entityState.pos;
            this.startPosition = Util.V2IOffsetV3(this.startPos, this.entityBase.entityState.data.size);
            this.endPos = this.startPos;
            this.endPosition = Util.V2IOffsetV3(this.startPos, this.entityBase.entityState.data.size) + zOffset;
            this.t = 0f;
        }

        public override void Enter() {
            if (this.entityBase.entityState.facing == Vector2Int.left) {
                this.entityBase.transform.rotation = Quaternion.AngleAxis(90, Vector3.up) * this.entityBase.transform.rotation;
            }
            else if (this.entityBase.entityState.facing == Vector2Int.right) {
                this.entityBase.transform.rotation = Quaternion.AngleAxis(-90, Vector3.up) * this.entityBase.transform.rotation;
            }
        }

        public override void Update() {
            if (this.t < 1) {
                this.t += Time.deltaTime / Constants.SPAWNTIME;
                this.entityBase.transform.position = Vector3.Lerp(this.startPosition, this.endPosition, this.t);
            }
            else {
                GM.playManager.WinBoard();
                this.entityBase.SetNeedsNewState();
            }
        }

        public override void Exit() {
            this.entityBase.ResetView();
        }

        public override EntityBaseStateResults GetStateResults() {
            throw new NotImplementedException();
        }
    }

    // TODO: make a spawning thing later
    class SpawningState : EntityBaseStateMachineState {
        public SpawningState(int aId) {
            this.entityBase = GM.boardManager.GetEntityBaseById(aId);
            Vector3 zOffset = new Vector3(0, 0, 1f);
            this.startPos = this.entityBase.entityState.pos;
            this.startPosition = Util.V2IOffsetV3(this.startPos, this.entityBase.entityState.data.size) + zOffset;
            this.endPos = this.startPos;
            this.endPosition = Util.V2IOffsetV3(this.startPos, this.entityBase.entityState.data.size);
            this.t = 0f;
        }

        public override void Enter() {

        }

        public override void Update() {
            if (this.t < 1) {
                this.t += Time.deltaTime / Constants.SPAWNTIME;
                this.entityBase.transform.position = Vector3.Lerp(this.startPosition, this.endPosition, this.t);
            }
            else {
                this.entityBase.SetNeedsNewState();
            }
        }

        public override void Exit() {
            this.entityBase.ResetView();
        }

        public override EntityBaseStateResults GetStateResults() {
            throw new NotImplementedException();
        }
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

        public override EntityBaseStateResults GetStateResults() {
            bool shouldIDie = false;
            HashSet<int> entityKillSet = new HashSet<int>();
            
            EntityState currentState = this.entityBase.entityState;
            // if floor doesnt exist on new pos, return false and null
            if (!PlayManager.DoesFloorExist(this.endPos, this.entityBase.id)) {
                // return illegal state because no floor
                return new EntityBaseStateResults(this, false);
            }
            
            foreach (BoardCell boardCell in GM.boardManager.GetBoardGridSlice(this.endPos, currentState.data.size).Values) {
                if (!boardCell.frontEntityState.HasValue) {
                    continue;
                }
                int blockingEntityId = boardCell.frontEntityState.Value.data.id;
                // if the cell is occupied by me
                if (currentState.data.id == blockingEntityId) {
                    continue;
                }
                switch (PlayManager.DoesAttackerWinTouchFight(this.entityBase.id, blockingEntityId)) {
                    case FightResultEnum.DEFENDERDIES:
                        entityKillSet.Add(blockingEntityId);
                        break;
                    case FightResultEnum.ATTACKERDIES:
                        shouldIDie = true;
                        break;
                    case FightResultEnum.TIE:
                        // return illegal state because something is blocking me
                        return new EntityBaseStateResults(this, false);
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            // checked every cell in new pos, found none blocking and maybe added some to the kill set
            return new EntityBaseStateResults(this, true, shouldIDie, entityKillSet);
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

        public override EntityBaseStateResults GetStateResults() {
            bool shouldIDie = false;
            HashSet<int> entityKillSet = new HashSet<int>();
            
            EntityState currentState = this.entityBase.entityState;
            // if floor doesnt exist on new pos, return false and null
            if (!PlayManager.DoesFloorExist(this.endPos, this.entityBase.id)) {
                // return illegal state because no floor
                return new EntityBaseStateResults(this, false);
            }
            
            foreach (BoardCell boardCell in GM.boardManager.GetBoardGridSlice(this.endPos, currentState.data.size).Values) {
                if (!boardCell.frontEntityState.HasValue) {
                    continue;
                }
                int blockingEntityId = boardCell.frontEntityState.Value.data.id;
                // if the cell is occupied by me
                if (currentState.data.id == blockingEntityId) {
                    continue;
                }
                switch (PlayManager.DoesAttackerWinTouchFight(this.entityBase.id, blockingEntityId)) {
                    case FightResultEnum.DEFENDERDIES:
                        entityKillSet.Add(blockingEntityId);
                        break;
                    case FightResultEnum.ATTACKERDIES:
                        shouldIDie = true;
                        break;
                    case FightResultEnum.TIE:
                        // return illegal state because something is blocking me
                        return new EntityBaseStateResults(this, false);
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            // checked every cell in new pos, found none blocking and maybe added some to the kill set
            return new EntityBaseStateResults(this, true, shouldIDie, entityKillSet);
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

        public override EntityBaseStateResults GetStateResults() {
            // returns always true because you shoudl always be able to turn
            return new EntityBaseStateResults(this, true);
        }
    }
    
    class PushingState : EntityBaseStateMachineState {
    
        public PushingState(int aId, Vector2Int aDirection) {
            this.direction = aDirection;
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

        public override EntityBaseStateResults GetStateResults() {
            throw new NotImplementedException();
        }
    }

    class PushedState : EntityBaseStateMachineState {
        public PushedState(Vector2Int aDirection, int aId) {
            Debug.Assert(Util.IsDirection(aDirection));
            this.direction = aDirection;
            this.entityBase = GM.boardManager.GetEntityBaseById(aId);
            this.startPos = this.entityBase.entityState.pos;
            this.startPosition = Util.V2IOffsetV3(this.startPos, this.entityBase.entityState.data.size);
            this.endPos = this.entityBase.entityState.pos + this.direction;
            this.endPosition = Util.V2IOffsetV3(this.endPos, this.entityBase.entityState.data.size);
            this.moveSpeed = Constants.PUSHSPEED;
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

        public override EntityBaseStateResults GetStateResults() {
            throw new NotImplementedException();
        }
    }
    
    class FlyingState : EntityBaseStateMachineState {
        public override void Enter() {
        }
    
        public override void Update() {
        }
    
        public override void Exit() {
        }

        public override EntityBaseStateResults GetStateResults() {
            throw new NotImplementedException();
        }
    }
    
    class WaitingState : EntityBaseStateMachineState {
        public override void Enter() {
            
        }
    
        public override void Update() {
            
        }
    
        public override void Exit() {
            
        }

        public override EntityBaseStateResults GetStateResults() {
            return new EntityBaseStateResults(this, true);
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

        public override EntityBaseStateResults GetStateResults() {
            bool shouldIDie = false;
            EntityState currentState = this.entityBase.entityState;
            HashSet<int> entityKillSet = new HashSet<int>();
            // if no floor under me
            if (!PlayManager.DoesFloorExist(currentState.pos, currentState.data.id)) {
                // foreach boardCell in newpos
                foreach (BoardCell boardCell in GM.boardManager.GetBoardGridSlice(this.endPos, currentState.data.size).Values) {
                    if (boardCell.frontEntityState.HasValue) {
                        EntityState blockingEntity = boardCell.frontEntityState.Value;
                        // if blockingEntity is me, skip
                        if (blockingEntity.data.id == currentState.data.id) {
                            continue;
                        }
                        switch (PlayManager.EntityFallOnEntityResult(currentState.data.id, blockingEntity.data.id)) {
                            case FightResultEnum.DEFENDERDIES:
                                entityKillSet.Add(blockingEntity.data.id);
                                break;
                            case FightResultEnum.ATTACKERDIES:
                                shouldIDie = true;
                                break;
                            case FightResultEnum.TIE:
                                return new EntityBaseStateResults(this, false);
                            default:
                                throw new ArgumentOutOfRangeException();
                        }
                    }
                }
                return new EntityBaseStateResults(this, true, shouldIDie, entityKillSet);
            }
            else {
                // return invalid because floor already exists
                return new EntityBaseStateResults(this, false);
            }
        }
    }
    
    class DyingState : EntityBaseStateMachineState {
        readonly float timeToDie;
        bool isPlayer;

        public DyingState(int aId) {
            this.entityBase = GM.boardManager.GetEntityBaseById(aId);
            this.timeToDie = 0.5f;
            this.t = 0f;
            this.isPlayer = this.entityBase.entityState.team == TeamEnum.PLAYER;
        }
        public override void Enter() {
            this.entityBase.isDying = true;
            print("entered death state");
            GM.playManager.StartEntityForDeath(this.entityBase.id);
        }
    
        public override void Update() {
            if (this.t < 1) {
                this.t += Time.deltaTime / this.timeToDie;
                this.entityBase.transform.localScale = Vector3.Lerp(this.entityBase.transform.localScale, Vector3.zero, this.t);
            }
            else {
                print(this.entityBase.id + " DyingState - completed animation");
                GM.playManager.FinishEntityDeath(this.entityBase.id, this.isPlayer);
            }
        }
    
        public override void Exit() {
        }

        public override EntityBaseStateResults GetStateResults() {
            // always return valid 
            return new EntityBaseStateResults(this, true);
        }
    }
    
    class EntityBaseStateResults {
        public readonly EntityBaseStateMachineState stateMachineState;
        public readonly bool isStateValid;
        public readonly bool shouldEntityDie;
        public readonly IEnumerable<int> entityKillSet;
        public readonly IEnumerable<int> entityPushSet;

        public EntityBaseStateResults (EntityBaseStateMachineState aStateMachineState, bool aIsStateValid, bool aShouldIDie = false, IEnumerable<int> aEntityKillSet = null, IEnumerable<int> aEntityPushSet = null) {
            this.stateMachineState = aStateMachineState;
            this.isStateValid = aIsStateValid;
            this.shouldEntityDie = aShouldIDie;
            this.entityKillSet = aEntityKillSet ?? new HashSet<int>();
            this.entityPushSet = aEntityPushSet ?? new HashSet<int>();
        }
    }

    #endregion
}

