using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using UnityEngine;
using Sirenix.OdinInspector;
// ReSharper disable InconsistentNaming

public class EntityBrain {
    public StateMachine stateMachine;
    public EntityBase entityBase;
    readonly int id;
    public bool needsNewAction;
    EntityState entityState {
        get {
            return GM.boardManager.GetEntityById(this.entityBase.id);
        }
    }

    public EntityBrain(EntityBase aEntityBase) {
        this.entityBase = aEntityBase;
        this.id = aEntityBase.id;
        this.stateMachine = new StateMachine();
        this.stateMachine.ChangeState(new WaitAction(this.id));
        this.needsNewAction = true;
    }

    public void DoFrame() {
        if (this.needsNewAction) {
            // Debug.Log(this.id + ": brain choosing next state...");
            if (this.entityState.isSuspended) {
                Debug.Log(this.id + " tried to DoFrame but is suspended");
                return;
            }
            this.needsNewAction = false;
            ActionResult newActionResult = ChooseNextAction();
            ChangeAction(newActionResult);
        }
        this.stateMachine.Update();
    }
    // TODO: hook this up

    public void ChangeAction(ActionResult aActionResult) {
        Debug.Assert(aActionResult.entityAction.id == this.id);
        // Debug.Log(this.id + " ChangeActionExternally requiredActionResult queue size: " + aActionResult.requiredActionResultTree.Count);
        foreach (ActionResult requiredActionResult in aActionResult.requiredActionResultTree) {
            EntityBrain otherEntityBrain = requiredActionResult.entityAction.entityBrain;
            otherEntityBrain.ChangeAction(requiredActionResult);
        }
        this.needsNewAction = false;
        Debug.Log(this.id + " ChangeAction performing " + aActionResult.entityAction.GetType());
        this.stateMachine.ChangeState(aActionResult.entityAction);
    }

    // public void ChangeAction(EntityAction aEntityAction) {
    //     Debug.Log(aEntityAction.id);
    //     Debug.Log(this.id);
    //     Debug.Assert(aEntityAction.id == this.id);
    //     // Debug.Log(this.id + " ChangeAction occured");
    //     this.needsNewAction = false;
    //     this.stateMachine.ChangeState(aEntityAction);
    //
    // }

    // void ExecuteActionResult(ActionResult aActionResult) {
    //     Debug.Assert(aActionResult.boardState.HasValue);
    //     Debug.Log(this.id + " ExecuteActionResult queue size: " + aActionResult.requiredActionResultQueue.Count);
    //     foreach (ActionResult requiredActionResult in aActionResult.requiredActionResultQueue) {
    //         Debug.Log(this.id + " ExecuteActionResult executing " + requiredActionResult.entityAction.GetType() + " for " + requiredActionResult.entityAction.id);
    //         EntityBrain otherEntityBrain = GM.boardManager.GetEntityBaseById(aActionResult.entityAction.id).entityBrain;
    //         otherEntityBrain.ChangeActionExternally(requiredActionResult);
    //     }
    //
    //     Debug.Log(this.id + " ExecuteActionResult executing final action of " + aActionResult.entityAction.GetType());
    //     Debug.Log(this.id + " ExecuteActionResult currentPos = " + GM.boardManager.GetEntityById(this.id).pos);
    //     ChangeAction(aActionResult.entityAction);
    // }

    ActionResult ChooseNextAction() {
        EntityState currentState = GM.boardManager.GetEntityById(this.id);
        WaitAction waitAction = new WaitAction(this.id);
        switch (currentState.entityType) {
            case EntityTypeEnum.BG:
                return waitAction.GetActionResult(GM.boardManager.currentState);
            case EntityTypeEnum.BLOCK:
                return waitAction.GetActionResult(GM.boardManager.currentState);
            case EntityTypeEnum.PUSHABLE:
                return waitAction.GetActionResult(GM.boardManager.currentState);
            case EntityTypeEnum.MOB:
                return MobChooseNextAction();
            case EntityTypeEnum.SPECIALBLOCK:
                return waitAction.GetActionResult(GM.boardManager.currentState);
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    ActionResult MobChooseNextAction() {
        switch (this.entityState.mobData?.movementType) {
            case MoveTypeEnum.INANIMATE:
                WaitAction waitAction = new WaitAction(this.id);
                return waitAction.GetActionResult(GM.boardManager.currentState);
            case MoveTypeEnum.PATROL:
                MoveAction moveForwardAction = new MoveAction(this.id, this.entityState.facing);
                // Debug.Log(this.id + " MobChooseNextAction on pos " + this.entityState.pos);
                ActionResult moveForwardResult = moveForwardAction.GetActionResult(GM.boardManager.currentState);
                if (moveForwardResult.boardState.HasValue) {
                    return moveForwardResult;
                }
                if (this.entityState.mobData?.canHop == true) {
                    MoveAction moveForwardUpAction = new MoveAction(this.id, this.entityState.facing + Vector2Int.up);
                    ActionResult moveForwardUpResult = moveForwardUpAction.GetActionResult(GM.boardManager.currentState);
                    if (moveForwardUpResult.boardState.HasValue) {
                        return moveForwardUpResult;
                    }
                    MoveAction moveForwardDownAction = new MoveAction(this.id, this.entityState.facing + Vector2Int.down);
                    ActionResult moveForwardDownResult = moveForwardDownAction.GetActionResult(GM.boardManager.currentState);
                    if (moveForwardDownResult.boardState.HasValue) {
                        return moveForwardDownResult;
                    }
                }
                TurnAction turnAction = new TurnAction(this.id);
                ActionResult turnResult = turnAction.GetActionResult(GM.boardManager.currentState);
                Debug.Assert(turnResult.boardState.HasValue);
                return turnResult;
            case MoveTypeEnum.FLY:
                throw new NotImplementedException();
            case MoveTypeEnum.PATHPATROL:
                throw new NotImplementedException();
            case MoveTypeEnum.PATHFLY:
                throw new NotImplementedException();
            case null:
                throw new NotImplementedException();
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    // static void ExecuteAction(EntityAction aEntityAction) {
    //     GM.boardManager.UpdateBoardState(aEntityAction);
    // }
}

// this enum is specifically to hard code priority lists
public enum EntityActionEnum {
    MoveAction,
    FallAction,
    DieAction,
    PushAction,
}

public class WaitAction : EntityAction {

    public WaitAction(int aId) {
        this.priorityList = new List<EntityActionEnum>();
        this.id = aId;
        this.direction = Vector2Int.zero;
        this.entityBase = GM.boardManager.GetEntityBaseById(aId);
        this.entityBrain = this.entityBase.entityBrain;
    }

    public override void Enter() {

    }

    public override void Update() {
    }

    public override void Exit() {
    }

    public override ActionResult GetActionResult(BoardState aBoardState) {
        return new ActionResult(this, ApplyAction(aBoardState));
    }

    public override BoardState ApplyAction(BoardState aBoardState) {
        return aBoardState;
    }
}

public class DieAction : EntityAction {
    public int attackerId;
    float timeToDie;
    Vector3 startScale;
    Vector3 endScale;

    public DieAction(int aId, int aAttackerId) {
        this.priorityList = new List<EntityActionEnum>();
        this.id = aId;
        this.direction = Vector2Int.zero;
        this.entityBase = GM.boardManager.GetEntityBaseById(aId);
        this.entityBrain = this.entityBase.entityBrain;
        this.attackerId = aAttackerId;
    }

    public override void Enter() {
        Debug.Log(this.id + " DieAction entered");
        BoardState nextBoardState = ApplyAction(GM.boardManager.currentState);
        GM.boardManager.UpdateBoardState(nextBoardState);
        // TODO: remove this hard coded float
        this.timeToDie = 1f;
        this.startScale = this.entityBase.transform.localScale;
        this.endScale = Vector3.zero;
    }

    public override void Update() {
        if (this.t < 1) {
            this.t += Time.deltaTime / this.timeToDie;
            this.entityBase.transform.localScale = Vector3.Lerp(this.startScale, this.endScale, this.t);
        }
        else {
            Debug.Log(this.id + " DieAction done");
            GM.playManager.FlagEntityForDeath(this.id);
        }
    }

    public override void Exit() {
        Debug.Log(this.id + " DieAction exited");
    }

    public override ActionResult GetActionResult(BoardState aBoardState) {
        Debug.Log(this.id + " DieAction.GetActionResult");
        BoardState simBoardState = aBoardState;
        EntityState simEntityState = simBoardState.GetEntityById(this.id);
        EntityState simAttackerEntityState = simBoardState.GetEntityById(this.attackerId);
        if (simAttackerEntityState.team != TeamEnum.NEUTRAL && simAttackerEntityState.team == simEntityState.team) {
            Debug.Log(this.id + " DieAction.GetActionResult is friendly fire and not NEUTRAL");
            return new ActionResult(this, null);
        }
        if (simAttackerEntityState.mobData == null) {
            Debug.Log(this.id + " DieAction.GetActionResult attacker " + this.attackerId + " is not a mob");
            return new ActionResult(this, null);
        }
        if (simAttackerEntityState.mobData?.canKillOnTouch == true) {
            if (simAttackerEntityState.mobData.Value.touchPower > simEntityState.touchDefense) {
                Debug.Log(this.id + " DieAction.GetActionResult was killed by " + simAttackerEntityState);
                return new ActionResult(this, ApplyAction(simBoardState));
            }
        }
        if (simEntityState.mobData?.canKillOnTouch == true) {
            if (simEntityState.mobData.Value.touchPower > simAttackerEntityState.touchDefense) {
                Debug.Log(this.id + " DieAction.GetActionResult killed " + simAttackerEntityState + " in retaliation");
                return new ActionResult(this, ApplyAction(simBoardState));
            }
        }
        Debug.Log(simEntityState.id + " DieAction.GetActionResult tied with " + this.attackerId);
        return new ActionResult(this, null);
    }

    static bool CanEntityAttackOtherEntity(EntityState aAttackingEntity, EntityState aDefendingEntity) {
        if (aAttackingEntity.team == TeamEnum.NEUTRAL) {
            return true;
        }
        return aAttackingEntity.team != aDefendingEntity.team;
    }

    public override BoardState ApplyAction(BoardState aBoardState) {
        EntityState simEntityState = aBoardState.GetEntityById(this.id);
        EntityState simAttackerEntityState = aBoardState.GetEntityById(this.attackerId);
        // if the attacker isnt neutral
        Debug.Assert(CanEntityAttackOtherEntity(simAttackerEntityState, simEntityState));
        if (simAttackerEntityState.mobData == null) {
            return BoardState.SuspendEntity(aBoardState, simAttackerEntityState.id);
        }
        if (simAttackerEntityState.mobData.Value.canKillOnTouch == true) {
            if (simAttackerEntityState.mobData.Value.touchPower > simEntityState.touchDefense) {
                return BoardState.SuspendEntity(aBoardState, simEntityState.id);
            }
        }
        if (simEntityState.mobData?.canKillOnTouch == true) {
            if (simEntityState.mobData.Value.touchPower > simAttackerEntityState.touchDefense) {
                return BoardState.SuspendEntity(aBoardState, simAttackerEntityState.id);
            }
        }
        throw new Exception(this.id + " DieAction.ApplyAction failed to apply action");
    }
}

public class FallAction : EntityAction {

    public FallAction(int aId) {

    }

    public override void Enter() {
        throw new NotImplementedException();
    }

    public override void Update() {
        throw new NotImplementedException();
    }

    public override void Exit() {
        throw new NotImplementedException();
    }

    public override ActionResult GetActionResult(BoardState aBoardState) {
        throw new NotImplementedException();
    }

    public override BoardState ApplyAction(BoardState aBoardState) {
        throw new NotImplementedException();
    }
}

public class PushAction : EntityAction {
    int pusherId;
    Vector3 startPosition;
    Vector3 endPosition;

    public PushAction(int aId, int aPusherId, Vector2Int aDirection) {
        this.priorityList = new List<EntityActionEnum> {EntityActionEnum.DieAction, EntityActionEnum.PushAction};
        this.id = aId;
        this.direction = aDirection;
        this.entityBase = GM.boardManager.GetEntityBaseById(aId);
        this.entityBrain = this.entityBase.entityBrain;
        this.pusherId = aPusherId;
    }

    public override void Enter() {
        Debug.Log(this.id + " PushAction entered");
        EntityState entityState = GM.boardManager.GetEntityById(this.id);
        this.startPosition = this.entityBase.transform.position;
        this.endPosition = Util.V2IOffsetV3(entityState.pos + this.direction, entityState.size);
        BoardState nextBoardState = ApplyAction(GM.boardManager.currentState);
        GM.boardManager.UpdateBoardState(nextBoardState, new HashSet<int>{this.id});
    }

    public override void Update() {
        if (this.t < 1) {
            this.t += Time.deltaTime / 1f;
            this.entityBase.transform.position = Vector3.Lerp(this.startPosition, this.endPosition, this.t);
        }
        else {
            Debug.Log(this.id + " PushAction done");
            this.entityBrain.needsNewAction = true;
        }
    }

    public override void Exit() {
        Debug.Log(this.id + "PushAction exited");
    }

    public override ActionResult GetActionResult(BoardState aBoardState) {
        // Debug.Log(this.id + " PushAction.GetActionResult");
        BoardState simBoardState = aBoardState;
        EntityState simEntityState = simBoardState.GetEntityById(this.id);
        Vector2Int startPos = simEntityState.pos;
        Vector2Int endPos = startPos + this.direction;
        if (!simEntityState.mobData.HasValue) {
            Debug.Log(this.id + " PushAction.ApplyAction done on a non-mob entity RETURNING");
            return new ActionResult(this, null);
        }
        if (simEntityState.mobData.Value.canBePushed == false) {
            Debug.Log(this.id + " PushAction.ApplyAction cant be pushed RETURNING");
            return new ActionResult(this, null);
        }
        Dictionary<Vector2Int, BoardCell> affectedBoardSlice = simBoardState.GetBoardCellSlice(endPos, simEntityState.size);
        HashSet<int> affectedEntityIdSet = new HashSet<int>();
        foreach (BoardCell affectedBoardCell in affectedBoardSlice.Values) {
            int? affectedEntityId = affectedBoardCell.GetEntityId(simEntityState.isFront);
            if (affectedEntityId.HasValue && affectedEntityId.Value != simEntityState.id) {
                // Debug.Log(this.id + " PushAction.ApplyAction encountered entity id " + affectedEntityId.Value + " at pos " + affectedBoardCell.pos);
                affectedEntityIdSet.Add(affectedEntityId.Value);
            }
        }
        Debug.Log(this.id + " PushAction.ApplyAction affectedEntityIdSet count " + affectedEntityIdSet.Count);
        List<ActionResult> requiredActionResultList = new List<ActionResult>();
        foreach (int affectedEntityId in affectedEntityIdSet) {
            bool foundValidActionForAffectedEntity = false;
            foreach (EntityActionEnum requiredActionEnum in this.priorityList) {
                EntityAction requiredAction;
                switch (requiredActionEnum) {
                    case EntityActionEnum.MoveAction:
                        throw new NotImplementedException();
                    case EntityActionEnum.FallAction:
                        throw new NotImplementedException();
                    case EntityActionEnum.DieAction:
                        requiredAction = new DieAction(affectedEntityId, simEntityState.id);
                        break;
                    case EntityActionEnum.PushAction:
                        requiredAction = new PushAction(affectedEntityId, simEntityState.id, this.direction);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
                Debug.Log("--- UP STACK to " + affectedEntityId + "---");
                ActionResult requiredActionResult = requiredAction.GetActionResult(simBoardState);
                Debug.Log("--- DOWN STACK from " + affectedEntityId + " ---");

                if (requiredActionResult.boardState.HasValue) {
                    simBoardState = requiredActionResult.boardState.Value;
                    foundValidActionForAffectedEntity = true;
                    requiredActionResultList.Add(requiredActionResult);
                    break;
                }
                else {
                    Debug.Log(this.id + " PushAction.GetActionResult failed to do " + requiredAction + " on " + affectedEntityId);
                }
            }
            if (!foundValidActionForAffectedEntity) {
                Debug.Log(this.id + " MoveAction.GetActionResult exhausted possible actions on " + affectedEntityId + " RETURNING");
                return new ActionResult(this, null);
            }
        }

        simBoardState = ApplyAction(simBoardState);
        Debug.Log(this.id + " PushAction.GetActionResult returned a valid result RETURNING");
        return new ActionResult(this, simBoardState, requiredActionResultList);
    }

    public override BoardState ApplyAction(BoardState aBoardState) {
        EntityState simEntityState = aBoardState.GetEntityById(this.id);
        Vector2Int newPos = simEntityState.pos + this.direction;
        simEntityState = EntityState.SetPos(simEntityState, newPos);
        return BoardState.UpdateEntity(aBoardState, simEntityState);
    }
}

public class MoveAction : EntityAction {
    Vector3 startPosition;
    Vector3 endPosition;

    public MoveAction(int aId, Vector2Int aDirection) {
        this.priorityList = new List<EntityActionEnum>{EntityActionEnum.DieAction, EntityActionEnum.PushAction};
        this.id = aId;
        this.direction = aDirection;
        this.entityBase = GM.boardManager.GetEntityBaseById(aId);
        this.entityBrain = this.entityBase.entityBrain;
    }


    public override void Enter() {
        // Debug.Log(this.id + " MoveAction entered");
        EntityState entityState = GM.boardManager.GetEntityById(this.entityBase.id);
        this.startPosition = this.entityBase.transform.position;
        this.endPosition = Util.V2IOffsetV3(entityState.pos + this.direction, entityState.size, entityState.isFront);
        BoardState nextBoardState = ApplyAction(GM.boardManager.currentState);
        GM.boardManager.UpdateBoardState(nextBoardState);
    }

    public override void Update() {
        if (this.t < 1) {
            this.t += Time.deltaTime / 1f;
            this.entityBase.transform.position = Vector3.Lerp(this.startPosition, this.endPosition, this.t);
        }
        else {
            // Debug.Log(this.id + " MoveAction done");
            this.entityBrain.needsNewAction = true;
        }
    }

    public override void Exit() {
        // Debug.Log(this.id + " MoveAction exited");
    }

    public override ActionResult GetActionResult(BoardState aBoardState) {
        Debug.Log(this.id + " MoveAction.GetActionResult");
        BoardState simBoardState = aBoardState;
        EntityState simEntityState = simBoardState.GetEntityById(this.id);
        Vector2Int startPos = simEntityState.pos;
        Vector2Int endPos = startPos + this.direction;
        if (!simEntityState.mobData.HasValue) {
            Debug.Log(this.id + " MoveAction.GetActionResult is not a mob RETURNING");
            return new ActionResult(this, null);
        }
        if (!simBoardState.DoesFloorExist(endPos, simEntityState.id)) {
            Debug.Log(this.id + " MoveAction.GetActionResult couldnt find floor (pre-sweep) RETURNING");
            return new ActionResult(this, null);
        }
        Dictionary<Vector2Int, BoardCell> affectedBoardSlice = simBoardState.GetBoardCellSlice(endPos, simEntityState.size);
        // getting a list of entities inside the new position
        HashSet<int> affectedEntityIdSet = new HashSet<int>();
        foreach (BoardCell affectedBoardCell in affectedBoardSlice.Values) {
            int? affectedEntityId = affectedBoardCell.GetEntityId(simEntityState.isFront);
            if (affectedEntityId.HasValue && affectedEntityId.Value != simEntityState.id) {
                affectedEntityIdSet.Add(affectedBoardCell.frontEntityId.Value);
            }
        }
        List<ActionResult> requiredActionResultList = new List<ActionResult>();
        // for each entity thats gonna get affected by this action
        foreach (int affectedEntityId in affectedEntityIdSet) {
            // flag thats true if i can get it out of the way for my action.
            bool foundValidActionForAffectedEntity = false;
            foreach (EntityActionEnum requiredActionEnum in this.priorityList) {
                EntityAction requiredAction;
                // for each possible action that nextEntity can perform
                switch (requiredActionEnum) {
                    case EntityActionEnum.MoveAction:
                        throw new NotImplementedException();
                    case EntityActionEnum.FallAction:
                        throw new NotImplementedException();
                    case EntityActionEnum.DieAction:
                        requiredAction = new DieAction(affectedEntityId, simEntityState.id);
                        break;
                    case EntityActionEnum.PushAction:
                        requiredAction = new PushAction(affectedEntityId, simEntityState.id, this.direction);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
                // simulate that action and get result
                Debug.Log("--- UP STACK to " + affectedEntityId + "---");
                ActionResult requiredActionResult = requiredAction.GetActionResult(simBoardState);
                Debug.Log("--- DOWN STACK from " + affectedEntityId + " ---");
                // if nextAction was valid
                if (requiredActionResult.boardState.HasValue) {
                    simBoardState = requiredActionResult.boardState.Value;
                    foundValidActionForAffectedEntity = true;
                    requiredActionResultList.Add(requiredActionResult);
                }
                else {
                    Debug.Log(this.id + " MoveAction.GetActionResult failed to do " + requiredAction.GetType() + " on " + affectedEntityId);
                }
            }
            if (!foundValidActionForAffectedEntity) {
                Debug.Log(this.id + " MoveAction.GetActionResult exhausted possible actions on " + affectedEntityId + " RETURNING");
                return new ActionResult(this, null);
            }
        }
        // another floor check
        if (!simBoardState.DoesFloorExist(endPos, simEntityState.id)) {
            Debug.Log(this.id + " MoveAction.GetActionResult couldnt find floor (post-sweep) RETURNING");
            return new ActionResult(this, null);
        }
        simBoardState = ApplyAction(simBoardState);
        Debug.Log(this.id + " MoveAction.GetActionResult returned a valid result RETURNING");
        return new ActionResult(this, simBoardState, requiredActionResultList);
    }

    public override BoardState ApplyAction(BoardState aBoardState) {
        EntityState simEntityState = aBoardState.GetEntityById(this.id);
        Vector2Int newPos = simEntityState.pos + this.direction;
        // Debug.Log("MoveAction.ApplyAction set pos to " + newPos);
        simEntityState = EntityState.SetPos(simEntityState, newPos);
        return BoardState.UpdateEntity(aBoardState, simEntityState);
    }
}

public class TurnAction : EntityAction {
    Quaternion startRotation;
    Quaternion endRotation;
    float turnSpeed;

    public TurnAction(int aId) {
        this.priorityList = new List<EntityActionEnum>();
        this.id = aId;
        this.direction = Vector2Int.zero;
        this.entityBase = GM.boardManager.GetEntityBaseById(aId);
        this.entityBrain = this.entityBase.entityBrain;
    }

    public override void Enter() {
        // Debug.Log(this.id + " TurnAction entered");
        this.startRotation = this.entityBase.transform.rotation;
        this.endRotation = Quaternion.AngleAxis(180, Vector3.up) * this.startRotation;
        BoardState nextBoardState = ApplyAction(GM.boardManager.currentState);
        // TODO: remove hardcoded turnSpeed
        this.turnSpeed = 1f;
        GM.boardManager.UpdateBoardState(nextBoardState);
    }

    public override void Update() {
        if (this.t < 1) {
            this.t += Time.deltaTime / this.turnSpeed;
            this.entityBase.transform.rotation = Quaternion.Lerp(this.startRotation, this.endRotation, this.t);
        }
        else {
            // Debug.Log(this.id + " TurnAction done");
            this.entityBrain.needsNewAction = true;
        }
    }

    public override void Exit() {
        this.entityBase.transform.rotation = this.endRotation;
        // Debug.Log(this.id + " TurnAction exiting");
    }

    public override ActionResult GetActionResult(BoardState aBoardState) {
        BoardState simBoardState = aBoardState;
        simBoardState = ApplyAction(simBoardState);
        return new ActionResult(this, simBoardState);
    }

    public override BoardState ApplyAction(BoardState aBoardState) {
        BoardState simBoardState = aBoardState;
        EntityState simEntityState = aBoardState.GetEntityById(this.id);
        Vector2Int newFacing = simEntityState.facing == Vector2Int.left ? Vector2Int.right : Vector2Int.left;
        simEntityState = EntityState.SetFacing(simEntityState, newFacing);
        return BoardState.UpdateEntity(simBoardState, simEntityState);
    }
}

public abstract class EntityAction : StateMachineState {
    public List<EntityActionEnum> priorityList = new List<EntityActionEnum>();
    public int id;
    public Vector2Int direction;
    public EntityBrain entityBrain;
    public EntityBase entityBase;
    public float t;

    public abstract void Enter();
    public abstract void Update();
    public abstract void Exit();
    // public abstract ActionResult GetResults(BoardState aBoardState);
    public abstract ActionResult GetActionResult(BoardState aBoardState);
    public abstract BoardState ApplyAction(BoardState aBoardState);

}

public readonly struct ActionResult {
    // a copy of boardstate
    public readonly EntityAction entityAction;
    public readonly BoardState? boardState;
    public readonly List<ActionResult> requiredActionResultTree;

    public ActionResult(EntityAction aEntityAction, BoardState? aBoardState, List<ActionResult> aRequiredActionResultTree = null) {
        this.entityAction = aEntityAction;
        this.boardState = aBoardState;
        this.requiredActionResultTree = aRequiredActionResultTree ?? new List<ActionResult>();
    }
}
