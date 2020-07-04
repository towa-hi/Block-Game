using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using UnityEngine;
using Sirenix.OdinInspector;
// ReSharper disable InconsistentNaming

public class EntityBrain {
    [SerializeField] public StateMachine stateMachine;
    public EntityBase entityBase;
    readonly int id;
    [SerializeField] public bool needsNewAction;
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
            Debug.Log(this.id + ": brain choosing next state...");
            this.needsNewAction = false;
            ActionResult newActionResult = ChooseNextAction();
            ExecuteActionResult(newActionResult);
            this.stateMachine.ChangeState(newActionResult.entityAction);
        }
        this.stateMachine.Update();
    }

    public void ChangeAction(EntityAction aEntityAction) {
        this.stateMachine.ChangeState(aEntityAction);
        this.needsNewAction = false;
    }

    void ExecuteActionResult(ActionResult aActionResult) {
        Debug.Assert(aActionResult.boardState.HasValue);
        ExecuteRecursively(aActionResult);
        // GM.boardManager.UpdateBoardState(aActionResult.boardState.Value);

    }

    void ExecuteRecursively(ActionResult aActionResult) {
        foreach (ActionResult result in aActionResult.actionList) {
            ExecuteRecursively(result);
        }
        Debug.Log("ExecuteActionResult entityId: " + aActionResult.entityAction.initialState.id + " entityAction: " + aActionResult.entityAction.GetType());
        EntityBrain brain = GM.boardManager.GetEntityBaseById(aActionResult.entityAction.initialState.id).entityBrain;
        if (aActionResult.entityAction is DieAction) {
            brain.ChangeAction(aActionResult.entityAction);
            GM.boardManager.GetEntityBaseById(aActionResult.entityAction.initialState.id).isDying = true;
            GM.playManager.StartEntityDeath(aActionResult.entityAction.initialState.id);
        }
        else {
            brain.ChangeAction(aActionResult.entityAction);
            GM.boardManager.UpdateBoardState(aActionResult.entityAction.ApplyActionToBoardState(GM.boardManager.currentState));
        }
    }

    ActionResult ChooseNextAction() {
        EntityState currentState = GM.boardManager.GetEntityById(this.id);
        WaitAction waitAction = new WaitAction(this.id);
        switch (currentState.entityType) {
            case EntityTypeEnum.BG:
                return waitAction.GetResults(GM.boardManager.currentState);
            case EntityTypeEnum.BLOCK:
                return waitAction.GetResults(GM.boardManager.currentState);
            case EntityTypeEnum.PUSHABLE:
                return waitAction.GetResults(GM.boardManager.currentState);
            case EntityTypeEnum.MOB:
                return MobChooseNextAction();
            case EntityTypeEnum.SPECIALBLOCK:
                return waitAction.GetResults(GM.boardManager.currentState);
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    ActionResult MobChooseNextAction() {
        switch (this.entityState.mobData?.movementType) {
            case MoveTypeEnum.INANIMATE:
                WaitAction waitAction = new WaitAction(this.id);
                return waitAction.GetResults(GM.boardManager.currentState);
            case MoveTypeEnum.PATROL:
                MoveAction moveForwardAction = new MoveAction(this.id, this.entityState.facing);
                ActionResult moveForwardResult = moveForwardAction.GetResults(GM.boardManager.currentState);
                if (moveForwardResult.boardState.HasValue) {
                    return moveForwardResult;
                }
                if (this.entityState.mobData?.canHop == true) {
                    MoveAction moveForwardUpAction = new MoveAction(this.id, this.entityState.facing + Vector2Int.up);
                    ActionResult moveForwardUpResult = moveForwardUpAction.GetResults(GM.boardManager.currentState);
                    if (moveForwardUpResult.boardState.HasValue) {
                        return moveForwardUpResult;
                    }
                    MoveAction moveForwardDownAction = new MoveAction(this.id, this.entityState.facing + Vector2Int.down);
                    ActionResult moveForwardDownResult = moveForwardDownAction.GetResults(GM.boardManager.currentState);
                    if (moveForwardDownResult.boardState.HasValue) {
                        return moveForwardDownResult;
                    }
                }
                TurnAction turnAction = new TurnAction(this.id);
                ActionResult turnResult = turnAction.GetResults(GM.boardManager.currentState);
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

public enum EAE {
    MoveAction,
    FallAction,
    DieAction,
    TurnAction,
    WaitAction,
    // PushAction,
}

public class WaitAction : EntityAction {

    public WaitAction(int aId) {
        this.initialState = GM.boardManager.GetEntityById(aId);
    }

    public override void Enter() {

    }

    public override void Update() {
    }

    public override void Exit() {
    }

    public override ActionResult GetResults(BoardState aBoardState) {
        return new ActionResult(this, aBoardState);
    }

    public override BoardState ApplyActionToBoardState(BoardState aBoardState) {
        return aBoardState;
    }
}

public class DieAction : EntityAction {
    EntityState attackerInitialState;

    public DieAction(int aId, EntityState aAttackerInitialState) {
        this.attackerInitialState = aAttackerInitialState;
        this.initialState = GM.boardManager.GetEntityById(aId);

    }

    public override void Enter() {
        Debug.Log(this.initialState.id + " DieAction entered");
    }

    public override void Update() {
        if (this.t < 1) {
            this.t += Time.deltaTime / 1f;
        }
        else {
            Debug.Log(this.initialState.id + " DieAction done");
            GM.playManager.FinishEntityDeath(this.initialState.id, this.initialState.team == TeamEnum.PLAYER);
        }
    }

    public override void Exit() {
        Debug.Log(this.initialState.id + " DieAction exited");
    }

    public override ActionResult GetResults(BoardState aBoardState) {
        BoardState simBoardState = aBoardState;
        EntityState simEntityState = aBoardState.GetEntityById(this.initialState.id);
        EntityState simAttackerEntityState = aBoardState.GetEntityById(this.attackerInitialState.id);
        if (simAttackerEntityState.team != TeamEnum.NEUTRAL && simAttackerEntityState.team == simEntityState.team) {
            return new ActionResult(this, null);
        }
        Debug.Log(simEntityState.id + " DieAction.GetResult() defender canKillOnTouch:" + simEntityState.mobData?.canKillOnTouch + " touchPower: " + simEntityState.mobData?.touchPower);
        Debug.Log(simEntityState.id + " DieAction.GetResult() attacker canKillOnTouch:" + simAttackerEntityState.mobData?.canKillOnTouch + " touchPower: " + simAttackerEntityState.mobData?.touchPower);
        List<ActionResult> actionList = new List<ActionResult>();
        if (simAttackerEntityState.mobData?.canKillOnTouch == true) {
            if (simAttackerEntityState.mobData.Value.touchPower >= simEntityState.touchDefense) {
                Debug.Log(simEntityState.id + " DieAction.GetResults() " + simAttackerEntityState.id + " just killed " + simEntityState.id + " in a sim");
                return new ActionResult(this, BoardState.RemoveEntity(simBoardState, simEntityState.id), actionList);
            }
        }
        if (simEntityState.mobData?.canKillOnTouch == true) {
            if (simEntityState.mobData.Value.touchPower >= simAttackerEntityState.touchDefense) {
                Debug.Log(simEntityState.id + " DieAction.GetResults() " + simEntityState.id + " just killed " + simAttackerEntityState.id + " in a sim");
                return new ActionResult(this, BoardState.RemoveEntity(simBoardState, simAttackerEntityState.id), actionList);
            }
        }
        Debug.Log(simEntityState.id + " DieAction.GetResults() is didn't work");
        return new ActionResult(this, null);
    }

    public override BoardState ApplyActionToBoardState(BoardState aBoardState) {
        return BoardState.RemoveEntity(aBoardState, this.initialState.id);
    }
}

public class FallAction : EntityAction {

    public FallAction(int aId) {
        this.initialState = GM.boardManager.GetEntityById(aId);

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

    public override ActionResult GetResults(BoardState aBoardState) {
        throw new NotImplementedException();
    }

    public override BoardState ApplyActionToBoardState(BoardState aBoardState) {
        throw new NotImplementedException();
    }
}


public class MoveAction : EntityAction {

    public MoveAction(int aId, Vector2Int aDirection) {
        this.priorityList = new List<EAE> {EAE.DieAction, EAE.MoveAction};
        this.entityBase = GM.boardManager.GetEntityBaseById(aId);
        this.entityBrain = this.entityBase.entityBrain;
        this.initialState = GM.boardManager.GetEntityById(this.entityBase.id);
        this.startPosition = Util.V2IOffsetV3(this.initialState.pos, this.initialState.size);
        this.direction = aDirection;
        this.endPosition = Util.V2IOffsetV3(this.initialState.pos + this.direction, this.initialState.size);
        // Debug.Assert(this.initialState.mobData.HasValue);
        // this.moveSpeed = this.initialState.mobData.Value.moveSpeed;
    }

    Vector3 startPosition;
    Vector3 endPosition;
    public override void Enter() {
        EntityState currentEntityState = GM.boardManager.GetEntityById(this.entityBase.id);
        Debug.Log(this.initialState.id + " MoveAction entered");
    }

    public override void Update() {
        if (this.t < 1) {
            this.t += Time.deltaTime / 1f;
            this.entityBase.transform.position = Vector3.Lerp(this.startPosition, this.endPosition, this.t);
        }
        else {
            Debug.Log("MoveAction done");
            this.entityBrain.needsNewAction = true;
        }
    }

    public override void Exit() {
        Debug.Log(this.initialState.id + " MoveAction exited");
    }

    public override ActionResult GetResults(BoardState aBoardState) {
        BoardState simBoardState = aBoardState;
        EntityState simEntityState = aBoardState.GetEntityById(this.initialState.id);
        Vector2Int startPos = simEntityState.pos;
        Vector2Int endPos = startPos + this.direction;
        // if not a mob return false
        if (!simEntityState.mobData.HasValue) {
            Debug.Log(simEntityState.id + " MoveAction.GetResults() done on non mob");
            return new ActionResult(this, null);
        }
        // get all entities inside area of effect
        Dictionary<Vector2Int, BoardCell> areaOfEffect = simBoardState.GetBoardCellSlice(endPos, simEntityState.size);
        HashSet<int> effectedEntityIds = new HashSet<int>();
        foreach (BoardCell boardCell in areaOfEffect.Values) {
            if (boardCell.frontEntityId.HasValue && boardCell.frontEntityId.Value != simEntityState.id) {
                effectedEntityIds.Add(boardCell.frontEntityId.Value);
            }
        }
        Debug.Log(simEntityState.id + " MoveAction.GetResults() effects this many entities: " + effectedEntityIds.Count);
        List<ActionResult> actionList = new List<ActionResult>();
        // for each entity to effect
        foreach (int effectedEntityId in effectedEntityIds) {
            // go down the priority list
            bool foundValidAction = false;
            foreach (EAE eae in this.priorityList) {
                EntityAction nextAction;
                switch (eae) {
                    case EAE.MoveAction:
                        // TODO: remove this and erplace it with push for now
                        Vector2Int pushDirection = new Vector2Int(this.direction.x, 0);
                        nextAction = new MoveAction(effectedEntityId, pushDirection);
                        break;
                    case EAE.FallAction:
                        nextAction = new FallAction(effectedEntityId);
                        break;
                    case EAE.DieAction:
                        nextAction = new DieAction(effectedEntityId, simEntityState);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
                Debug.Log(simEntityState.id + " MoveAction attempting" + nextAction.GetType() + "on entity: " + effectedEntityId);
                Debug.Log("--- UP STACK ---");
                ActionResult actionResult = nextAction.GetResults(simBoardState);
                Debug.Log("--- DOWN STACK ---");
                if (actionResult.boardState.HasValue) {
                    Debug.Log(simEntityState.id + " MoveAction set simBoardState on " + effectedEntityId);
                    simBoardState = actionResult.boardState.Value;
                    foundValidAction = true;
                    actionList.Add(actionResult);
                    // actionList.AddRange(actionResult.actionList);
                    break;
                }
                else {
                    Debug.Log(simEntityState.id + " MoveAction didn't work on " + effectedEntityId);
                }
            }
            if (!foundValidAction) {
                // exhausted list of actions
                Debug.Log(simEntityState.id + " MoveAction exhausted list of options");
                return new ActionResult(this, null);
            }
        }
        // if no floor at endPos, return false;
        if (!simBoardState.DoesFloorExist(endPos, simEntityState.id)) {
            Debug.Log(simEntityState.id + " MoveAction.GetResults() couldn't find floor");
            return new ActionResult(this, null);
        }
        // simBoardState contains the final state of the thing
        simBoardState = ApplyActionToBoardState(simBoardState);
        Debug.Log(simEntityState.id + " MoveAction returning new sim state");
        return new ActionResult(this, simBoardState, actionList);

    }

    public override BoardState ApplyActionToBoardState(BoardState aBoardState) {
        EntityState entityState = aBoardState.GetEntityById(this.initialState.id);
        Vector2Int endPos = entityState.pos + this.direction;
        return BoardState.UpdateEntity(aBoardState, EntityState.SetPos(entityState, endPos));
    }
}

public class TurnAction : EntityAction {
    Quaternion startRotation;
    Quaternion endRotation;

    public TurnAction(int aId) {
        this.initialState = GM.boardManager.GetEntityById(aId);
        this.entityBase = GM.boardManager.GetEntityBaseById(aId);
        this.entityBrain = this.entityBase.entityBrain;
        EntityState entityState = GM.boardManager.GetEntityById(aId);
        Debug.Assert(Util.IsDirection(entityState.facing));
    }

    public override void Enter() {
        Debug.Log("TurnAction entered");
    }

    public override void Update() {
        if (this.t < 1) {
            this.t += Time.deltaTime / 1f;
        }
        else {
            Debug.Log("TurnAction done");
            this.entityBrain.needsNewAction = true;
        }
    }

    public override void Exit() {
    }

    public override ActionResult GetResults(BoardState aBoardState) {
        BoardState simBoardState = aBoardState;
        simBoardState = ApplyActionToBoardState(simBoardState);
        Debug.Log(this.initialState.id + " TurnAction returning new sim state");
        return new ActionResult(this, simBoardState);
    }

    public override BoardState ApplyActionToBoardState(BoardState aBoardState) {
        EntityState entityState = aBoardState.GetEntityById(this.initialState.id);
        Vector2Int newFacing = entityState.facing == Vector2Int.left ? Vector2Int.right : Vector2Int.left;
        return BoardState.UpdateEntity(aBoardState, EntityState.SetFacing(entityState, newFacing));
    }
}
public abstract class EntityAction : StateMachineState {
    public List<EAE> priorityList = new List<EAE>();
    public EntityBrain entityBrain;
    public EntityBase entityBase;
    public EntityState initialState;
    public Vector2Int direction;
    public float moveSpeed;
    public float t;

    public abstract void Enter();
    public abstract void Update();
    public abstract void Exit();
    public abstract ActionResult GetResults(BoardState aBoardState);
    public abstract BoardState ApplyActionToBoardState(BoardState aBoardState);
}

public struct ActionResult {
    // a copy of boardstate
    public EntityAction entityAction;
    public bool isActionValid;
    public List<ActionResult> actionList;
    public BoardState? boardState;

    public ActionResult(EntityAction aEntityAction, BoardState? aBoardState, List<ActionResult> aActionList = null) {
        this.entityAction = aEntityAction;
        this.boardState = aBoardState;
        this.actionList = aActionList ?? new List<ActionResult>();
        this.isActionValid = aBoardState != null;
    }

}
