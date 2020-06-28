using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using UnityEngine;
using Sirenix.OdinInspector;

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
        this.stateMachine.ChangeState(new WaitAction(this));
        this.needsNewAction = true;
        Debug.Log(this.id + ": brain created");
    }

    public void DoFrame() {
        if (this.needsNewAction) {
            Debug.Log(this.id + ": brain choosing next state...");
            this.needsNewAction = false;
            EntityAction newAction = ChooseNextAction();
            Debug.Log(this.id + ": brain chose " + newAction.GetType());
            ExecuteAction(newAction);
            this.stateMachine.ChangeState(newAction);
        }
        this.stateMachine.Update();
    }

    EntityAction ChooseNextAction() {
        return new WaitAction(this);
    }

    static void ExecuteAction(EntityAction aEntityAction) {

    }
}

public class WaitAction : EntityAction {
    public WaitAction(EntityBrain aEntityBrain) {
        this.entityBrain = aEntityBrain;
        this.entityBase = aEntityBrain.entityBase;
    }

    public override void Enter() {
    }

    public override void Update() {
    }

    public override void Exit() {
    }

    public override ActionResult GetResults() {
        return new ActionResult(this, true, false);
    }
}

public class MoveAction : EntityAction {
    Vector2Int direction;
    Vector2Int startPos;
    Vector2Int endPos;
    EntityState initialState;

    public MoveAction(int aId, Vector2Int aDirection) {
        this.entityBase = GM.boardManager.GetEntityBaseById(aId);
        this.entityBrain = this.entityBase.entityBrain;
        this.initialState = GM.boardManager.GetEntityById(this.entityBase.id);
        this.startPos = this.initialState.pos;
        this.direction = aDirection;
        this.endPos = this.startPos + this.direction;
        Debug.Assert(this.initialState.mobData.HasValue);
        this.moveSpeed = this.initialState.mobData.Value.moveSpeed;
    }

    public override void Enter() {
    }

    public override void Update() {
    }

    public override void Exit() {
    }

    public override ActionResult GetResults() {
        Dictionary<int, EntityAction> preActionDict = new Dictionary<int, EntityAction>();
        EntityState currentState = GM.boardManager.GetEntityById(this.entityBase.id);
        // if no floor at endPos, return false;
        if (!PlayManager.DoesFloorExist(this.endPos, currentState.id)) {
            return new ActionResult(this, false);
        }
        // TODO: finish this later
        return new ActionResult(this, true);

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
    public abstract ActionResult GetResults();
}

public readonly struct ActionResult {
    // a copy of boardstate
    public readonly EntityAction entityAction;
    public readonly bool isActionValid;
    public readonly bool doesEntityDie;
    public readonly ImmutableDictionary<int, EntityAction> preActionDict;

    public ActionResult(EntityAction aEntityAction, bool aIsActionValid = true, bool aDoesEntityDie = false, Dictionary<int, EntityAction> aPreActionDict = null) {
        this.entityAction = aEntityAction;
        this.isActionValid = aIsActionValid;
        this.doesEntityDie = aDoesEntityDie;
        this.preActionDict = aPreActionDict?.ToImmutableDictionary();
    }
}
