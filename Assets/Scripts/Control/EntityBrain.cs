using System;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
// ReSharper disable InconsistentNaming

public class EntityBrain {
    public readonly StateMachine stateMachine;
    public readonly EntityBase entityBase;
    readonly int id;
    public bool needsNewAction;

    public EntityBrain(EntityBase aEntityBase) {
        this.entityBase = aEntityBase;
        this.id = aEntityBase.id;
        this.stateMachine = new StateMachine();
        this.stateMachine.ChangeState(new WaitAction(this.id));
        this.needsNewAction = true;
    }

    public void BeforeDoFrame() {
        if (this.needsNewAction) {
            EntityState frameEntityState = GM.boardManager.GetEntityById(this.id);
            if (!frameEntityState.isSuspended) {
                ActionResult? preActionResult = CheckFrameAction(frameEntityState);
                if (preActionResult != null) {
                    Debug.Log(this.id + "doing preAction" + preActionResult.Value.entityAction.GetType());
                    ChangeAction(preActionResult.Value);
                }
            }
        }
    }

    public void DoFrame() {
        if (this.needsNewAction) {
            EntityState frameEntityState = GM.boardManager.GetEntityById(this.id);
            if (!frameEntityState.isSuspended) {
                ActionResult newActionResult = ChooseNextAction(frameEntityState);
                ChangeAction(newActionResult);
            }
        }
        this.stateMachine.Update();
    }

    ActionResult? CheckFrameAction(EntityState aEntityState) {
        if (aEntityState.mobData?.canFall != true) {
            return null;
        }
        FallAction fallAction = new FallAction(this.id);
        ActionResult fallActionResult = fallAction.GetActionResult(GM.boardManager.currentState);
        return fallActionResult.boardState != null ? (ActionResult?) fallActionResult : null;
    }

    void ChangeAction(ActionResult aActionResult) {
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

    ActionResult ChooseNextAction(EntityState aEntityState) {
        switch (aEntityState.entityType) {
            case EntityTypeEnum.BG:
                return new WaitAction(this.id).GetActionResult(GM.boardManager.currentState);
            case EntityTypeEnum.BLOCK:
                return new WaitAction(this.id).GetActionResult(GM.boardManager.currentState);
            case EntityTypeEnum.PUSHABLE:
                return new WaitAction(this.id).GetActionResult(GM.boardManager.currentState);
            case EntityTypeEnum.MOB:
                return MobChooseNextAction(aEntityState);
            case EntityTypeEnum.SPECIALBLOCK:
                return new WaitAction(this.id).GetActionResult(GM.boardManager.currentState);
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    ActionResult MobChooseNextAction(EntityState aEntityState) {
        Debug.Assert(aEntityState.mobData.HasValue);
        switch (aEntityState.mobData.Value.movementType) {
            case MoveTypeEnum.INANIMATE:
                WaitAction waitAction = new WaitAction(this.id);
                return waitAction.GetActionResult(GM.boardManager.currentState);
            case MoveTypeEnum.PATROL:
                MoveAction moveForwardAction = new MoveAction(this.id, aEntityState.facing);
                // Debug.Log(this.id + " MobChooseNextAction on pos " + this.entityState.pos);
                ActionResult moveForwardResult = moveForwardAction.GetActionResult(GM.boardManager.currentState);
                if (moveForwardResult.boardState.HasValue) {
                    return moveForwardResult;
                }
                if (aEntityState.mobData.Value.canHop) {
                    MoveAction moveForwardUpAction = new MoveAction(this.id, aEntityState.facing + Vector2Int.up);
                    ActionResult moveForwardUpResult = moveForwardUpAction.GetActionResult(GM.boardManager.currentState);
                    if (moveForwardUpResult.boardState.HasValue) {
                        return moveForwardUpResult;
                    }
                    MoveAction moveForwardDownAction = new MoveAction(this.id, aEntityState.facing + Vector2Int.down);
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
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
}

public class WaitAction : EntityAction {
    bool hasFramePassed;
    public WaitAction(int aId) : base(aId, EntityActionEnum.WAIT) {
        this.hasFramePassed = false;
    }

    public override void Enter() {
        EntityState entityState = GM.boardManager.GetEntityById(this.id);
        if (entityState.currentAction != this.entityActionEnum) {
            UpdateBoardFromAction();
        }
    }

    public override void Update() {
        if (!this.hasFramePassed) {
            this.hasFramePassed = true;
        }
        else {
            this.entityBrain.needsNewAction = true;
        }
    }

    public override void Exit() {
    }

    public override ActionResult GetActionResult(BoardState aBoardState) {
        // Debug.Log(this.id + " WaitAction.GetActionResult");
        return new ActionResult(this, ApplyAction(aBoardState));
    }

    protected override BoardState ApplyAction(BoardState aBoardState) {
        EntityState simEntityState = aBoardState.GetEntityById(this.id);
        if (simEntityState.currentAction != this.entityActionEnum) {
            simEntityState = EntityState.SetAction(simEntityState, this.entityActionEnum);
            return BoardState.UpdateEntity(aBoardState, simEntityState, false);
        }
        return aBoardState;
    }
}

public class DieAction : EntityAction {
    public readonly int attackerId;
    readonly float timeToDie;
    Vector3 startScale;
    Vector3 endScale;

    public DieAction(int aId, int aAttackerId) : base(aId, EntityActionEnum.DIE) {
        this.attackerId = aAttackerId;
        // TODO: make this not hard coded
        this.timeToDie = 1f;
    }

    public override void Enter() {
        Debug.Log(this.id + " DieAction entered");
        // TODO: remove this hard coded float
        this.startScale = this.entityBase.transform.localScale;
        this.endScale = Vector3.zero;
        UpdateBoardFromAction();
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
        switch (GetTouchFightResult(aBoardState, this.id, this.attackerId)) {
            case FightResultEnum.DEFENDER_DIES:
                Debug.Log(this.id + " DieAction.GetActionResult defender dies");
                return new ActionResult(this, ApplyAction(simBoardState));
            case FightResultEnum.ATTACKER_DIES:
                Debug.Log(this.id + " DieAction.GetActionResult attacker dies");
                DieAction attackerDieAction = new DieAction(this.attackerId, this.id);
                return new ActionResult(attackerDieAction, attackerDieAction.ApplyAction(simBoardState));
            case FightResultEnum.TIE:
                Debug.Log(this.id + " DieAction.GetActionResult tie");
                return new ActionResult(this, null);
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    public static FightResultEnum GetTouchFightResult(BoardState aBoardState, int aDefenderId, int aAttackerId) {
        EntityState defender = aBoardState.GetEntityById(aDefenderId);
        EntityState attacker = aBoardState.GetEntityById(aAttackerId);
        // if attacker and defender are on different teams, or attacker and defender are on the same neutral team
        if (attacker.team != defender.team || attacker.team == TeamEnum.NEUTRAL) {
            if (attacker.mobData?.canKillOnTouch == true && attacker.mobData.Value.touchPower > defender.touchDefense) {
                return FightResultEnum.DEFENDER_DIES;

            }
            if (defender.mobData?.canKillOnTouch == true && defender.mobData.Value.touchPower > attacker.touchDefense) {
                return FightResultEnum.ATTACKER_DIES;
            }
        }
        return FightResultEnum.TIE;
    }

    protected override BoardState ApplyAction(BoardState aBoardState) {
        EntityState attacker = aBoardState.GetEntityById(this.attackerId);
        EntityState defender = aBoardState.GetEntityById(this.id);
        switch (GetTouchFightResult(aBoardState, defender.id, attacker.id)) {
            case FightResultEnum.DEFENDER_DIES:
                defender = EntityState.SetAction(defender, this.entityActionEnum);
                defender = EntityState.SetIsSuspended(defender, true);
                return BoardState.UpdateEntity(aBoardState, defender);
            case FightResultEnum.ATTACKER_DIES:
                throw new Exception("Attacker shouldn't die when ApplyAction gets called");
            case FightResultEnum.TIE:
                return aBoardState;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
}

public class FallAction : EntityAction {
    Vector3 startPosition;
    Vector3 endPosition;

    public FallAction(int aId) : base(aId, EntityActionEnum.FALL, Vector2Int.down, new List<EntityActionEnum> {EntityActionEnum.DIE}) {

    }

    public override void Enter() {
        Debug.Log(this.id + " FallAction entered");
        EntityState entityState = GM.boardManager.GetEntityById(this.id);
        this.startPosition = this.entityBase.transform.position;
        this.endPosition = Util.V2IOffsetV3(entityState.pos + this.direction, entityState.size);
        UpdateBoardFromAction();
    }

    public override void Update() {
        if (this.t < 1) {
            this.t += Time.deltaTime / Constants.GRAVITY;
            this.entityBase.transform.position = Vector3.Lerp(this.startPosition, this.endPosition, this.t);
        }
        else {
            // Debug.Log(this.id + " FallAction done t: " + this.t);
            this.entityBrain.needsNewAction = true;
        }
    }

    public override void Exit() {
        Debug.Log(this.id + " FallAction exited");
    }

    public override ActionResult GetActionResult(BoardState aBoardState) {
        // Debug.Log(this.id + " FallAction.GetActionResult");
        BoardState simBoardState = aBoardState;
        EntityState simEntityState = simBoardState.GetEntityById(this.id);
        Vector2Int startPos = simEntityState.pos;
        Vector2Int endPos = startPos + this.direction;
        if (!simEntityState.mobData.HasValue) {
            // Debug.Log(this.id + " FallAction.ApplyAction done on non-mob entity RETURNING");
            return new ActionResult(this, null);
        }
        if (!simEntityState.mobData.Value.canFall) {
            // Debug.Log(this.id + "FallAction.ApplyAction done on non fallable entity RETURNING");
            return new ActionResult(this, null);
        }
        Dictionary<Vector2Int, BoardCell> affectedBoardSlice = simBoardState.GetBoardCellSlice(endPos, simEntityState.size);
        // Getting a list of entities inside the new position
        HashSet<int> affectedEntityIdSet = new HashSet<int>();
        foreach (BoardCell affectedBoardCell in affectedBoardSlice.Values) {
            int? affectedEntityId = affectedBoardCell.GetEntityId(simEntityState.isFront);
            if (affectedEntityId.HasValue && affectedEntityId.Value != simEntityState.id) {
                affectedEntityIdSet.Add(affectedEntityId.Value);
            }
        }
        ActionResult? deathActionResult = null;
        List<ActionResult> requiredActionResultList = new List<ActionResult>();
        // for each entity thats gonna get affected by this action
        foreach (int affectedEntityId in affectedEntityIdSet) {
            // flag thats true if all entities i encounter can be moved out of the way somehow
            bool foundValidActionForAffectedEntity = false;
            foreach (EntityActionEnum requiredActionEnum in this.priorityList) {
                EntityAction requiredAction;
                // for each possible action that nextEntity can perform
                switch (requiredActionEnum) {
                    case EntityActionEnum.MOVE:
                        throw new NotImplementedException();
                    case EntityActionEnum.FALL:
                        throw new NotImplementedException();
                    case EntityActionEnum.DIE:
                        requiredAction = new DieAction(affectedEntityId, simEntityState.id);
                        break;
                    case EntityActionEnum.PUSH:
                        throw new NotImplementedException();
                    case EntityActionEnum.TURN:
                        throw new NotImplementedException();
                    case EntityActionEnum.WAIT:
                        throw new NotImplementedException();
                    default:
                        throw new NotImplementedException();
                }
                // Debug.Log("--- UP STACK to " + affectedEntityId + "---");
                ActionResult requiredActionResult = requiredAction.GetActionResult(simBoardState);
                // Debug.Log("--- DOWN STACK from " + affectedEntityId + " ---");
                if (requiredActionResult.boardState.HasValue) {
                    simBoardState = requiredActionResult.boardState.Value;
                    if (requiredActionResult.entityAction.id == this.id) {
                        if (requiredActionResult.entityAction.entityActionEnum == EntityActionEnum.DIE) {
                            deathActionResult = requiredActionResult;
                        }
                    }
                    else {
                        Debug.Assert(requiredActionResult.entityAction.id == affectedEntityId);
                        requiredActionResultList.Add(requiredActionResult);

                    }
                    foundValidActionForAffectedEntity = true;
                }
                else {
                    // Debug.Log(this.id + " FallAction.GetActionResult failed to do " + requiredAction + " on " + affectedEntityId);
                }
            }
            if (!foundValidActionForAffectedEntity) {
                // Debug.Log(this.id + " FallAction.GetActionResult exhausted possible actions on " + affectedEntityId + " RETURNING");
                return new ActionResult(this, null);
            }
            if (deathActionResult.HasValue) {
                Debug.Log(this.id + " FallAction.GetActionResult had only one viable move causing self-death");
                return deathActionResult.Value;
            }
        }
        simBoardState = ApplyAction(simBoardState);
        // Debug.Log(this.id + " FallAction.GetActionResult returned a valid result RETURNING");
        return new ActionResult(this, simBoardState, requiredActionResultList);
    }

    protected override BoardState ApplyAction(BoardState aBoardState) {
        EntityState simEntityState = ApplyActionGetEntityStateWithActionSet(aBoardState);
        Vector2Int newPos = simEntityState.pos + this.direction;
        simEntityState = EntityState.SetPos(simEntityState, newPos);
        return BoardState.UpdateEntity(aBoardState, simEntityState);
    }
}

public class PushAction : EntityAction {
    Vector3 startPosition;
    Vector3 endPosition;

    public PushAction(int aId, int aPusherId, Vector2Int aDirection) : base(aId, EntityActionEnum.PUSH, aDirection, new List<EntityActionEnum> {EntityActionEnum.DIE, EntityActionEnum.PUSH}) {
        Debug.Assert(aPusherId != this.id);
    }

    public override void Enter() {
        Debug.Log(this.id + " PushAction entered");
        EntityState entityState = GM.boardManager.GetEntityById(this.id);
        this.startPosition = this.entityBase.transform.position;
        this.endPosition = Util.V2IOffsetV3(entityState.pos + this.direction, entityState.size);
        UpdateBoardFromAction();
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
        Debug.Log(this.id + " PushAction exited");
    }

    public override ActionResult GetActionResult(BoardState aBoardState) {
        // Debug.Log(this.id + " PushAction.GetActionResult");
        BoardState simBoardState = aBoardState;
        EntityState simEntityState = simBoardState.GetEntityById(this.id);
        Vector2Int startPos = simEntityState.pos;
        Vector2Int endPos = startPos + this.direction;
        if (simEntityState.currentAction == EntityActionEnum.FALL) {
            Debug.Log(this.id + " PushAction.ApplyAction done on falling entity RETURNING");
            return new ActionResult(this, null);
        }
        if (!simEntityState.mobData.HasValue) {
            Debug.Log(this.id + " PushAction.ApplyAction done on a non-mob entity RETURNING");
            return new ActionResult(this, null);
        }
        if (simEntityState.mobData.Value.canBePushed == false) {
            Debug.Log(this.id + " PushAction.ApplyAction cant be pushed RETURNING");
            return new ActionResult(this, null);
        }
        // if (!aBoardState.DoesFloorExist(simEntityState.pos, simEntityState.id)) {
        //     Debug.Log(this.id + " PushAction.ApplyAction cant be pushed because no floor RETURNING");
        //     return new ActionResult(this, null);
        // }
        Dictionary<Vector2Int, BoardCell> affectedBoardSlice = simBoardState.GetBoardCellSlice(endPos, simEntityState.size);
        // getting a list of entities inside the new position
        HashSet<int> affectedEntityIdSet = new HashSet<int>();
        foreach (BoardCell affectedBoardCell in affectedBoardSlice.Values) {
            int? affectedEntityId = affectedBoardCell.GetEntityId(simEntityState.isFront);
            if (affectedEntityId.HasValue && affectedEntityId.Value != simEntityState.id) {
                affectedEntityIdSet.Add(affectedEntityId.Value);
            }
        }
        ActionResult? deathActionResult = null;
        List<ActionResult> requiredActionResultList = new List<ActionResult>();
        // for each entity thats gonna get affected by this action
        foreach (int affectedEntityId in affectedEntityIdSet) {
            // flag thats true if all entities i encounter can be moved out of the way somehow
            bool foundValidActionForAffectedEntity = false;
            foreach (EntityActionEnum requiredActionEnum in this.priorityList) {
                EntityAction requiredAction;
                // for each possible action that nextEntity can perform
                switch (requiredActionEnum) {
                    case EntityActionEnum.MOVE:
                        throw new NotImplementedException();
                    case EntityActionEnum.FALL:
                        throw new NotImplementedException();
                    case EntityActionEnum.DIE:
                        requiredAction = new DieAction(affectedEntityId, simEntityState.id);
                        break;
                    case EntityActionEnum.PUSH:
                        Vector2Int pushDirection = new Vector2Int(this.direction.x, 0);
                        requiredAction = new PushAction(affectedEntityId, simEntityState.id, pushDirection);
                        break;
                    case EntityActionEnum.TURN:
                        throw new NotImplementedException();
                    case EntityActionEnum.WAIT:
                        throw new NotImplementedException();
                    default:
                        throw new ArgumentOutOfRangeException();
                }
                // simulate that action and get result
                Debug.Log(this.id + " --- UP STACK to " + affectedEntityId + "---");
                ActionResult requiredActionResult = requiredAction.GetActionResult(simBoardState);
                Debug.Log(this.id + " --- DOWN STACK from " + affectedEntityId + " ---");
                // if nextAction was valid
                if (requiredActionResult.boardState.HasValue) {
                    simBoardState = requiredActionResult.boardState.Value;
                    // if the action is one perscribed for me by the thing i encountered
                    if (requiredActionResult.entityAction.id == this.id) {
                        if (requiredActionResult.entityAction.entityActionEnum == EntityActionEnum.DIE) {
                            deathActionResult = requiredActionResult;
                        }
                    }
                    else {
                        Debug.Assert(requiredActionResult.entityAction.id == affectedEntityId);
                        requiredActionResultList.Add(requiredActionResult);
                    }
                    foundValidActionForAffectedEntity = true;
                }
                else {
                    Debug.Log(this.id + " PushAction.GetActionResult failed to do " + requiredAction + " on " + affectedEntityId);
                }
            }
            if (!foundValidActionForAffectedEntity) {
                Debug.Log(this.id + " PushAction.GetActionResult exhausted possible actions on " + affectedEntityId + " RETURNING");
                return new ActionResult(this, null);
            }
            if (deathActionResult.HasValue) {
                // requiredActionResultList.Clear();
                Debug.Log(this.id + " PushAction.GetActionResult had only one viable move causing self-death");
                return deathActionResult.Value;
            }
        }
        simBoardState = ApplyAction(simBoardState);
        Debug.Log(this.id + " PushAction.GetActionResult returned a valid result RETURNING");
        return new ActionResult(this, simBoardState, requiredActionResultList);
    }

    protected override BoardState ApplyAction(BoardState aBoardState) {
        EntityState simEntityState = ApplyActionGetEntityStateWithActionSet(aBoardState);
        Vector2Int newPos = simEntityState.pos + this.direction;
        simEntityState = EntityState.SetPos(simEntityState, newPos);
        return BoardState.UpdateEntity(aBoardState, simEntityState);
    }
}

public class MoveAction : EntityAction {
    Vector3 startPosition;
    Vector3 endPosition;

    public MoveAction(int aId, Vector2Int aDirection) : base(aId, EntityActionEnum.MOVE, aDirection, new List<EntityActionEnum>{EntityActionEnum.DIE, EntityActionEnum.PUSH}) {

    }


    public override void Enter() {
        // Debug.Log(this.id + " MoveAction entered");
        EntityState entityState = GM.boardManager.GetEntityById(this.entityBase.id);
        this.startPosition = this.entityBase.transform.position;
        this.endPosition = Util.V2IOffsetV3(entityState.pos + this.direction, entityState.size, entityState.isFront);
        UpdateBoardFromAction();
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
                affectedEntityIdSet.Add(affectedEntityId.Value);
            }
        }
        ActionResult? deathActionResult = null;
        List<ActionResult> requiredActionResultList = new List<ActionResult>();
        // for each entity thats gonna get affected by this action
        foreach (int affectedEntityId in affectedEntityIdSet) {
            // flag thats true if all entities i encounter can be moved out of the way somehow
            bool foundValidActionForAffectedEntity = false;
            foreach (EntityActionEnum requiredActionEnum in this.priorityList) {
                EntityAction requiredAction;
                // for each possible action that nextEntity can perform
                switch (requiredActionEnum) {
                    case EntityActionEnum.MOVE:
                        throw new NotImplementedException();
                    case EntityActionEnum.FALL:
                        throw new NotImplementedException();
                    case EntityActionEnum.DIE:
                        requiredAction = new DieAction(affectedEntityId, simEntityState.id);
                        break;
                    case EntityActionEnum.PUSH:
                        Vector2Int pushDirection = new Vector2Int(this.direction.x, 0);
                        requiredAction = new PushAction(affectedEntityId, simEntityState.id, pushDirection);
                        break;
                    case EntityActionEnum.TURN:
                        throw new NotImplementedException();
                    case EntityActionEnum.WAIT:
                        throw new NotImplementedException();
                    default:
                        throw new ArgumentOutOfRangeException();
                }
                // simulate that action and get result
                Debug.Log(this.id + " --- UP STACK to " + affectedEntityId + "---");
                ActionResult requiredActionResult = requiredAction.GetActionResult(simBoardState);
                Debug.Log(this.id + " --- DOWN STACK from " + affectedEntityId + " ---");
                // if nextAction was valid
                if (requiredActionResult.boardState.HasValue) {
                    simBoardState = requiredActionResult.boardState.Value;
                    // if the action is one perscribed for me by the thing i encountered
                    if (requiredActionResult.entityAction.id == this.id) {
                        if (requiredActionResult.entityAction.entityActionEnum == EntityActionEnum.DIE) {
                            deathActionResult = requiredActionResult;
                        }
                    }
                    else {
                        Debug.Assert(requiredActionResult.entityAction.id == affectedEntityId);
                        requiredActionResultList.Add(requiredActionResult);
                    }
                    foundValidActionForAffectedEntity = true;
                }
                else {
                    Debug.Log(this.id + " MoveAction.GetActionResult failed to do " + requiredAction.GetType() + " on " + affectedEntityId);
                }
            }
            if (!foundValidActionForAffectedEntity) {
                Debug.Log(this.id + " MoveAction.GetActionResult exhausted possible actions on " + affectedEntityId + " RETURNING");
                return new ActionResult(this, null);
            }
            if (deathActionResult.HasValue) {
                requiredActionResultList.Clear();
                Debug.Log(this.id + " MoveAction.GetActionResult had only one viable move causing self-death");
                return deathActionResult.Value;
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

    protected override BoardState ApplyAction(BoardState aBoardState) {
        EntityState simEntityState = ApplyActionGetEntityStateWithActionSet(aBoardState);
        Vector2Int newPos = simEntityState.pos + this.direction;
        // Debug.Log("MoveAction.ApplyAction set pos to " + newPos);
        simEntityState = EntityState.SetPos(simEntityState, newPos);
        return BoardState.UpdateEntity(aBoardState, simEntityState);
    }
}

public class TurnAction : EntityAction {
    Quaternion startRotation;
    Quaternion endRotation;
    // TODO: remove hardcoded turnSpeed
    const float turnSpeed = 1f;

    public TurnAction(int aId) : base(aId, EntityActionEnum.TURN, Vector2Int.zero) {

    }

    public override void Enter() {
        Debug.Log(this.id + " TurnAction entered");
        this.startRotation = this.entityBase.transform.rotation;
        this.endRotation = Quaternion.AngleAxis(180, Vector3.up) * this.startRotation;
        UpdateBoardFromAction();
    }

    public override void Update() {
        if (this.t < 1) {
            this.t += Time.deltaTime / turnSpeed;
            this.entityBase.transform.rotation = Quaternion.Lerp(this.startRotation, this.endRotation, this.t);
        }
        else {
            Debug.Log(this.id + " TurnAction done");
            this.entityBrain.needsNewAction = true;
        }
    }

    public override void Exit() {
        this.entityBase.transform.rotation = this.endRotation;
        Debug.Log(this.id + " TurnAction exiting");
    }

    public override ActionResult GetActionResult(BoardState aBoardState) {
        BoardState simBoardState = aBoardState;
        simBoardState = ApplyAction(simBoardState);
        return new ActionResult(this, simBoardState);
    }

    protected override BoardState ApplyAction(BoardState aBoardState) {
        EntityState simEntityState = ApplyActionGetEntityStateWithActionSet(aBoardState);
        Vector2Int newFacing = simEntityState.facing == Vector2Int.left ? Vector2Int.right : Vector2Int.left;
        simEntityState = EntityState.SetFacing(simEntityState, newFacing);
        return BoardState.UpdateEntity(aBoardState, simEntityState, false);
    }
}

public abstract class EntityAction : StateMachineState {
    public readonly EntityActionEnum entityActionEnum;
    public readonly List<EntityActionEnum> priorityList;
    public readonly int id;
    public readonly Vector2Int direction;
    public readonly EntityBrain entityBrain;
    public readonly EntityBase entityBase;
    public readonly bool canBeInterrupted;
    protected float t;
    // EntityAction should require a constructor with at least the id of the entity
    // this constructor should set
    // this.entityActionEnum
    // this.priorityList
    // this.id
    // this.direction
    // this.entityBrain
    // this.entityBase

    protected EntityAction(int aId, EntityActionEnum aEntityActionEnum, Vector2Int? aDirection = null, List<EntityActionEnum> aPriorityList = null) {
        this.entityActionEnum = aEntityActionEnum;
        this.priorityList = aPriorityList ?? new List<EntityActionEnum>();
        this.id = aId;
        this.direction = aDirection ?? Vector2Int.zero;
        this.entityBase = GM.boardManager.GetEntityBaseById(this.id);
        this.entityBrain = this.entityBase.entityBrain;
        switch (this.entityActionEnum) {
            case EntityActionEnum.MOVE:
                this.canBeInterrupted = true;
                break;
            case EntityActionEnum.FALL:
                this.canBeInterrupted = true;
                break;
            case EntityActionEnum.DIE:
                this.canBeInterrupted = false;
                break;
            case EntityActionEnum.PUSH:
                this.canBeInterrupted = true;
                break;
            case EntityActionEnum.TURN:
                this.canBeInterrupted = true;
                break;
            case EntityActionEnum.WAIT:
                this.canBeInterrupted = true;
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }


    // Enter() must instantiate all local variables
    public abstract void Enter();
    // always called from Enter()
    protected void UpdateBoardFromAction(HashSet<int> aEntitiesToUpdate = null) {
        GM.boardManager.UpdateBoardState(ApplyAction(GM.boardManager.currentState), aEntitiesToUpdate ?? new HashSet<int>{this.id});
    }

    // Update() needs to follow this template
    // if (this.t < 1) {
    //     this.t += Time.deltaTime / 1f;
    //     // some kind of lerp;
    // }
    // else {
    //     this.entityBrain.needsNewAction = true;
    // }
    public abstract void Update();

    public abstract void Exit();
    // public abstract ActionResult GetResults(BoardState aBoardState);

    // GetActionResult and ApplyAction must not use local variables that get set by Enter()
    public abstract ActionResult GetActionResult(BoardState aBoardState);
    protected abstract BoardState ApplyAction(BoardState aBoardState);
    // ApplyActionInit() always called from ApplyAction
    protected EntityState ApplyActionGetEntityStateWithActionSet(BoardState aBoardState) {
        EntityState simEntityState = aBoardState.GetEntityById(this.id);
        simEntityState = EntityState.SetAction(simEntityState, this.entityActionEnum);
        return simEntityState;
    }
}

public readonly struct ActionResult {
    public readonly EntityAction entityAction;
    public readonly BoardState? boardState;
    public readonly List<ActionResult> requiredActionResultTree;

    public ActionResult(EntityAction aEntityAction, BoardState? aBoardState, List<ActionResult> aRequiredActionResultTree = null) {
        this.entityAction = aEntityAction;
        this.boardState = aBoardState;
        this.requiredActionResultTree = aRequiredActionResultTree ?? new List<ActionResult>();
    }
}
