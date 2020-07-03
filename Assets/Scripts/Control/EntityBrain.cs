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
    int id;
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
            EntityAction newAction = ChooseNextAction();
            Debug.Log(this.id + ": brain chose " + newAction.GetType());
            this.stateMachine.ChangeState(newAction);
        }
        this.stateMachine.Update();
    }

    EntityAction ChooseNextAction() {
        EntityState currentState = GM.boardManager.GetEntityById(this.id);
        switch (currentState.entityType) {
            case EntityTypeEnum.BG:
                return new WaitAction(this.id);
            case EntityTypeEnum.BLOCK:
                return new WaitAction(this.id);
            case EntityTypeEnum.PUSHABLE:
                return new WaitAction(this.id);
            case EntityTypeEnum.MOB:
                return MobChooseNextAction();
            case EntityTypeEnum.SPECIALBLOCK:
                return new WaitAction(this.id);
            default:
                throw new ArgumentOutOfRangeException();
        }
        return new WaitAction(this.id);
    }

    EntityAction MobChooseNextAction() {
        switch (this.entityState.mobData?.movementType) {
            case MoveTypeEnum.INANIMATE:
                return new WaitAction(this.id);
            case MoveTypeEnum.PATROL:
                MoveAction moveAction = new MoveAction(this.id, Vector2Int.right);
                (ActionResult moveResult, BoardState? moveBoardState) = moveAction.GetResults(GM.boardManager.currentState);
                if (moveBoardState.HasValue) {
                    GM.boardManager.UpdateBoardState(moveBoardState.Value);
                    return moveAction;
                }
                else {
                    return new WaitAction(this.id);
                }
            case MoveTypeEnum.FLY:
                return new WaitAction(this.id);
            case MoveTypeEnum.PATHPATROL:
                return new WaitAction(this.id);
            case MoveTypeEnum.PATHFLY:
                return new WaitAction(this.id);
            case null:
                return new WaitAction(this.id);
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    static void ExecuteAction(EntityAction aEntityAction) {

    }
}

public enum EAE {
    MoveAction,
    FallAction,
    DieAction,
    TurnAction,
    WaitAction,
}

public class ReadyAction : EntityAction {

    public ReadyAction(int aId) {

    }

    public override void Enter() {
    }

    public override void Update() {
    }

    public override void Exit() {
    }

    public override (ActionResult, BoardState?) GetResults(BoardState aBoardState) {
        throw new NotImplementedException();
    }
}
public class WaitAction : EntityAction {

    public WaitAction(int aId) {

    }

    public override void Enter() {
    }

    public override void Update() {
    }

    public override void Exit() {
    }

    public override (ActionResult, BoardState?) GetResults(BoardState aBoardState) {
        return (new ActionResult(this, true), aBoardState);
    }
}

public class DieAction : EntityAction {

    public DieAction(int defenderId, int attackerId) {

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

    public override (ActionResult, BoardState?) GetResults(BoardState aBoardState) {
        throw new NotImplementedException();
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

    public override (ActionResult, BoardState?) GetResults(BoardState aBoardState) {
        throw new NotImplementedException();
    }
}

public class MoveAction : EntityAction {
    Vector2Int direction;
    EntityState initialState;
    float t;
    // readonly List<EAE> priorityList = new List<EAE>{EAE.FallAction, EAE.DieAction, EAE.MoveAction};
    readonly List<EAE> priorityList = new List<EAE> {EAE.MoveAction};

    public MoveAction(int aId, Vector2Int aDirection) {
        this.entityBase = GM.boardManager.GetEntityBaseById(aId);
        this.entityBrain = this.entityBase.entityBrain;
        this.initialState = GM.boardManager.GetEntityById(this.entityBase.id);
        // Debug.Assert(this.initialState.mobData.HasValue);
        // this.moveSpeed = this.initialState.mobData.Value.moveSpeed;
        this.direction = aDirection;
    }

    public override void Enter() {


    }

    public override void Update() {
        if (this.t < 1) {
            this.t += Time.deltaTime / 1f;
        }
        else {
            Debug.Log("MoveAction done");
            this.entityBrain.needsNewAction = true;
        }
    }

    public override void Exit() {
    }

    public override (ActionResult, BoardState?) GetResults(BoardState aBoardState) {
        int id = this.entityBase.id;
        BoardState simBoardState = aBoardState;
        EntityState entityState = aBoardState.GetEntityById(this.entityBase.id);
        Vector2Int startPos = entityState.pos;
        Vector2Int endPos = startPos + this.direction;
        // if not a mob return false
        if (!entityState.mobData.HasValue) {
            Debug.Log(id + " MoveAction.GetResults() done on non mob");
            return (new ActionResult(this, false), null);
        }
        // get all entities inside area of effect
        Dictionary<Vector2Int, BoardCell> areaOfEffect = simBoardState.GetBoardCellSlice(endPos, entityState.size);
        HashSet<int> effectedEntityIds = new HashSet<int>();
        foreach (BoardCell boardCell in areaOfEffect.Values) {
            if (boardCell.frontEntityId.HasValue && boardCell.frontEntityId.Value != id) {
                effectedEntityIds.Add(boardCell.frontEntityId.Value);
            }
        }
        Debug.Log(id + " MoveAction.GetResults() effects this many entities: " + effectedEntityIds.Count);
        // for each entity to effect
        foreach (int effectedEntityId in effectedEntityIds) {
            // go down the priority list
            bool foundValidAction = false;
            foreach (EAE eae in this.priorityList) {
                EntityAction nextAction;
                switch (eae) {
                    case EAE.MoveAction:
                        nextAction = new MoveAction(effectedEntityId, this.direction);
                        break;
                    case EAE.FallAction:
                        nextAction = new FallAction(effectedEntityId);
                        break;
                    case EAE.DieAction:
                        nextAction = new DieAction(effectedEntityId, id);
                        break;
                    case EAE.TurnAction:
                        nextAction = new TurnAction(effectedEntityId);
                        break;
                    case EAE.WaitAction:
                        nextAction = new WaitAction(effectedEntityId);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
                Debug.Log("--- UP STACK ---");
                (ActionResult actionResult, BoardState? newBoardState) = nextAction.GetResults(simBoardState);
                Debug.Log("--- DOWN STACK ---");
                Debug.Log(id + " MoveAction attempting" + nextAction.GetType() + "on entity: " + effectedEntityId);
                if (newBoardState.HasValue) {
                    Debug.Log(id + " MoveAction set simBoardState on " + effectedEntityId);
                    simBoardState = newBoardState.Value;
                    foundValidAction = true;
                    break;
                }
                else {
                    Debug.Log(id + " MoveAction didn't work on " + effectedEntityId);
                }
            }
            if (!foundValidAction) {
                // exhausted list of actions
                Debug.Log(id + " MoveAction exhausted list of options");
                return (new ActionResult(this, false), null);
            }
        }
        // if no floor at endPos, return false;
        if (!simBoardState.DoesFloorExist(endPos, entityState.id)) {
            Debug.Log(id + " MoveAction.GetResults() couldn't find floor");
            return (new ActionResult(this, false), null);
        }
        // simBoardState contains the final state of the thing
        simBoardState = BoardState.UpdateEntity(simBoardState, EntityState.SetPos(entityState, endPos));
        Debug.Log(id + " MoveAction returning new sim state");
        return (new ActionResult(this, true), simBoardState);

    }
}

public class TurnAction : EntityAction {

    public TurnAction(int aId) {

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

    public override (ActionResult, BoardState?) GetResults(BoardState aBoardState) {
        throw new NotImplementedException();
    }
}
public abstract class EntityAction : StateMachineState {
    public EntityBrain entityBrain;
    public EntityBase entityBase;

    public Vector2Int direction;
    public float moveSpeed;

    public abstract void Enter();
    public abstract void Update();
    public abstract void Exit();
    public abstract (ActionResult, BoardState?) GetResults(BoardState aBoardState);

}

public readonly struct ActionResult {
    // a copy of boardstate
    public readonly EntityAction entityAction;
    public readonly bool isActionValid;
    public readonly ImmutableDictionary<int, EntityAction> preActionDict;

    public ActionResult(EntityAction aEntityAction, bool aIsActionValid = true, Dictionary<int, EntityAction> aPreActionDict = null) {
        this.entityAction = aEntityAction;
        this.isActionValid = aIsActionValid;
        this.preActionDict = aPreActionDict?.ToImmutableDictionary();
    }

}
