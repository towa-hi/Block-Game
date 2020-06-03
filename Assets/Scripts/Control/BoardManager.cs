using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;

public delegate void OnUpdateBoardStateHandler(BoardState aBoardState);

public class BoardManager : SerializedMonoBehaviour {
    
    [SerializeField] private BoardState boardState;
    public BoardState currentState {
        get { return this.boardState; }
    }

    public Dictionary<Vector2Int, BoardCell> boardCellDict;
    public event OnUpdateBoardStateHandler OnUpdateBoardState;
    public Dictionary<int, EntityBase> entityBaseDict;

    private void Awake() {
        Init();
    }

    public void UpdateBoardState(BoardState aBoardState) {
        if (OnUpdateBoardState != null) {
            print("BoardManager - Updating BoardState for " + OnUpdateBoardState?.GetInvocationList().Length + " delegates");
        }
        else {
            print("BoardManager - Updating BoardState for no delegates");
        }
        this.boardState = aBoardState;
        SetBoardCellDict(aBoardState);
        OnUpdateBoardState?.Invoke(this.currentState);
    }

    public void SetBoardCellDict(BoardState aBoardState) {
        foreach (KeyValuePair<Vector2Int, BoardCell> kvp in this.boardCellDict) {
            BoardCell currentCell = kvp.Value;
            currentCell.backEntityState = null;
            currentCell.frontEntityState = null;
        }

        foreach (KeyValuePair<int, EntityState> kvp in aBoardState.entityDict) {
            EntityState foundEntity = kvp.Value;
            foreach (Vector2Int currentPos in Util.V2IInRect(foundEntity.pos, foundEntity.data.size)) {
                BoardCell currentCell = this.boardCellDict[currentPos];
                if (foundEntity.data.isFront) {
                    currentCell.frontEntityState = foundEntity;
                }
                else {
                    currentCell.backEntityState = foundEntity;
                }
            }
        }
    }
    
    public void UpdateEntityAndBoardState(EntityState aEntityState) {
        BoardState newBoardState = BoardState.UpdateEntity(this.currentState, aEntityState);
        UpdateBoardState(newBoardState);
    }

    public void SetTitle(string aTitle) {
        if (0 < aTitle.Length && aTitle.Length <= Constants.MAXTITLELENGTH) {
            // TODO: validate titles so they can be valid filenames here
            // char[] invalidFileNameChars = Path.GetInvalidFileNameChars();
            BoardState newBoardState = BoardState.SetTitle(this.currentState, aTitle);
            UpdateBoardState(newBoardState);
        }
        else {
            throw new Exception("SetTitle - invalid title");
        }
    }

    public void SetPar(int aPar) {
        if (0 < aPar && aPar <= Constants.MAXPAR) {
            BoardState newBoardState = BoardState.SetPar(this.currentState, aPar);
            UpdateBoardState(newBoardState);
        }
    }
    
    public void MoveEntity(int aId, Vector2Int aPos) {
        EntityState entityState = GetEntityById(aId);
        HashSet<EntityState> ignoreSet = new HashSet<EntityState> {entityState};
        if (IsRectEmpty(aPos, entityState.data.size, ignoreSet, entityState.data.isFront)) {
            UpdateEntityAndBoardState(EntityState.SetPos(entityState, aPos));
            GetEntityBaseById(aId).ResetTempView();
        } else {
            throw new Exception("MoveEntity - invalid move");
        }
    }

    public void SetEntityFacing(int aId, Vector2Int aFacing) {
        EntityState entityState = GetEntityById(aId);
        if (Util.IsDirection(aFacing)) {
            UpdateEntityAndBoardState(EntityState.SetFacing(entityState, aFacing));
        } else {
            throw new Exception("SetEntityFacing - invalid facing direction");
        }
    }

    public void SetEntityDefaultColor(int aId, Color aColor) {
        UpdateEntityAndBoardState(EntityState.SetDefaultColor(GetEntityById(aId), aColor));
    }

    public void SetEntityIsFixed(int aId, bool aIsFixed) {
        UpdateEntityAndBoardState(EntityState.SetIsFixed(GetEntityById(aId), aIsFixed));
    }

    public void SetEntityTeam(int aId, TeamEnum aTeam) {
        UpdateEntityAndBoardState(EntityState.SetTeam(GetEntityById(aId), aTeam));
    }

    public void SetEntityNodes(int aId, HashSet<Vector2Int> aUpNodes, HashSet<Vector2Int> aDownNodes) {
        EntityState entityState = GetEntityById(aId);
        if (aUpNodes != null && aDownNodes != null) {
            UpdateEntityAndBoardState(EntityState.SetNodes(entityState, aUpNodes, aDownNodes));
        } else {
            throw new Exception("SetEntityNodes - params can't be null");
        }
    }

    public void SetEntityTouchDefense(int aId, int aTouchDefense) {
        EntityState entityState = GetEntityById(aId);
        if (0 <= aTouchDefense && aTouchDefense <= 999) {
            UpdateEntityAndBoardState(EntityState.SetTouchDefense(entityState, aTouchDefense));
        } else {
            throw new Exception("SetEntityTouchDefense - invalid touchDefense");
        }
    }

    public void SetEntityFallDefense(int aId, int aFallDefense) {
        EntityState entityState = GetEntityById(aId);
        if (0 <= aFallDefense && aFallDefense <= 999) {
            UpdateEntityAndBoardState(EntityState.SetFallDefense(entityState, aFallDefense));
        } else {
            throw new Exception("SetEntityFallDefense - invalid touchDefense");
        }
    }

    public bool CanEditorPlaceSchema(Vector2Int aPos, EntitySchema aEntitySchema) {
        // TODO: finish this
        if (IsRectEmpty(aPos, aEntitySchema.size, null, aEntitySchema.isFront)) {
            return true;
        }

        return false;
    }

    public bool IsPosInBoard(Vector2Int aPos) {
        return Util.IsInside(aPos, Vector2Int.zero, this.currentState.size);
    }

    public bool IsRectInBoard(Vector2Int aOrigin, Vector2Int aSize) {
        return Util.IsRectInside(aOrigin, aSize, Vector2Int.zero, this.currentState.size);
    }

    public Dictionary<Vector2Int, BoardCell> GetBoardGridSlice(Vector2Int aOrigin, Vector2Int aSize) {
        if (IsRectInBoard(aOrigin, aSize)) {
            Dictionary<Vector2Int, BoardCell> sliceDict = new Dictionary<Vector2Int, BoardCell>();
            foreach (Vector2Int currentPos in Util.V2IInRect(aOrigin, aSize)) {
                if (this.boardCellDict.ContainsKey(currentPos)) {
                    sliceDict[currentPos] = this.boardCellDict[currentPos];
                }
            }
            return sliceDict;
        }

        throw new Exception("GetBoardGridSlice - rect not in board");
    }

    public bool IsRectEmpty(Vector2Int aOrigin, Vector2Int aSize, HashSet<EntityState> aIgnoreSet = null, bool aIsFront = true) {
        // print("IsRectEmpty started aOrigin: " + aOrigin + "aSize: " + aSize + "aIsFront: " + aIsFront);
        if (IsRectInBoard(aOrigin, aSize)) {
            foreach (KeyValuePair<Vector2Int, BoardCell> kvp in GetBoardGridSlice(aOrigin, aSize)) {
                if (aIsFront && kvp.Value.frontEntityState != null) {
                    // if aIgnoreSet exists
                    if (aIgnoreSet != null) {
                        // if found front entity is inside aIgnoreSet
                        if (aIgnoreSet.All(entityState => kvp.Value.frontEntityState.Value.data.id != entityState.data.id)) {
                            // return false because an that id is blocking
                            return false;
                        }
                    }
                    else {
                        // return false because something is blocking and theres no aIgnoreSet
                        return false;
                    }
                }
                else if (!aIsFront && kvp.Value.backEntityState != null) {
                    if (aIgnoreSet != null) {
                        // if found back entity is inside aIgnoreSet
                        if (aIgnoreSet.All(entityState => kvp.Value.backEntityState.Value.data.id != entityState.data.id)) {
                            // return false because an that id is blocking
                            return false;
                        }
                    }
                    else {
                        // return false because something is blocking and theres no aIgnoreSet
                        return false;
                    }
                }
            }
            // return true because all cells were empty or had ignored entities
            return true;
        }
        Debug.Log("IsRectEmpty - returned false because tried to evaluate out of bounds");
        // return false because rect isnt even in grid
        return false;
    }

    public void AddEntity(EntitySchema aEntitySchema, Vector2Int aPos, Vector2Int aFacing, Color aDefaultColor, bool aIsFixed = false, bool aIsBoundary = false) {
        // if the area isn't clear, throw an exception
        if (!IsRectEmpty(aPos, aEntitySchema.size, null, aEntitySchema.isFront)) {
            throw new Exception("AddEntity - Position is invalid");
        }
        // generate a fresh entityState without an ID
        EntityState newEntityStateWithoutId = EntityState.CreateEntityState(aEntitySchema, aPos, aFacing, aDefaultColor, aIsFixed, aIsBoundary);
        // add it to the board and get the new boardState and entityState with ID back
        (BoardState newBoard, EntityState newEntityStateWithId) = BoardState.AddEntity(this.currentState, newEntityStateWithoutId);
        // update the boardState
        UpdateBoardState(newBoard);
        // set the new EntityPosition
        Vector3 newEntityPosition = Util.V2IOffsetV3(newEntityStateWithId.pos, newEntityStateWithId.data.size, newEntityStateWithId.data.isFront);
        // instantiate a new gameObject entityPrefab from the schemas prefabPath
        GameObject entityPrefab = Instantiate(GM.LoadEntityPrefabByFilename(newEntityStateWithId.data.prefabPath), newEntityPosition, Quaternion.identity,  this.transform); 
        // get the entityBase
        EntityBase entityBase = entityPrefab.GetComponent<EntityBase>();
        // add it to the entityBaseDict
        this.entityBaseDict[newEntityStateWithId.data.id] = entityBase;
        // initialize entityBase with the newest state
        entityBase.Init(newEntityStateWithId);
    }

    public void RemoveEntity(int aId) {
        EntityBase entityBase = this.entityBaseDict[aId];
        // remove the entityBase from the entityBaseDict
        this.entityBaseDict.Remove(aId);
        // destroy the entitys gameObject
        Destroy(entityBase.gameObject);
        // remove entity from boardstate
        BoardState newBoard = BoardState.RemoveEntity(this.currentState, aId);
        // update the boardState
        UpdateBoardState(newBoard);
    }

    public void Init() {
        this.entityBaseDict = new Dictionary<int, EntityBase>();
        BoardState newBoard = BoardState.GenerateBlankBoard();
        this.boardCellDict = new Dictionary<Vector2Int, BoardCell>();
        for (int x = 0; x < newBoard.size.x; x++) {
            for (int y = 0; y < newBoard.size.y; y++) {
                Vector2Int currentPos = new Vector2Int(x, y);
                BoardCell newCell = new BoardCell(currentPos);
                this.boardCellDict[currentPos] = newCell;
            }
        }
        UpdateBoardState(newBoard);
        // TODO: move this somewhere sensible
        EntitySchema tallBoy = AssetDatabase.LoadAssetAtPath<EntitySchema>("Assets/Resources/ScriptableObjects/Entities/Blocks/1x11 block.asset");
        EntitySchema longBoy = AssetDatabase.LoadAssetAtPath<EntitySchema>("Assets/Resources/ScriptableObjects/Entities/Blocks/20x1 block.asset");
        
        AddEntity(longBoy, new Vector2Int(0, 0), Constants.DEFAULTFACING, Constants.DEFAULTCOLOR, true, true);
        AddEntity(longBoy, new Vector2Int(20, 0), Constants.DEFAULTFACING, Constants.DEFAULTCOLOR, true, true);

        AddEntity(longBoy, new Vector2Int(0, 23), Constants.DEFAULTFACING, Constants.DEFAULTCOLOR, true, true);
        AddEntity(longBoy, new Vector2Int(20, 23), Constants.DEFAULTFACING, Constants.DEFAULTCOLOR, true, true);

        AddEntity(tallBoy, new Vector2Int(0, 1), Constants.DEFAULTFACING, Constants.DEFAULTCOLOR, true, true);
        AddEntity(tallBoy, new Vector2Int(0, 12), Constants.DEFAULTFACING, Constants.DEFAULTCOLOR, true, true);

        AddEntity(tallBoy, new Vector2Int(39, 1), Constants.DEFAULTFACING, Constants.DEFAULTCOLOR, true, true);
        AddEntity(tallBoy, new Vector2Int(39, 12), Constants.DEFAULTFACING, Constants.DEFAULTCOLOR, true, true);
    }

    public EntityState? GetEntityAtMousePos(bool aIsFront = true) {
        Vector2Int mousePosV2 = GM.inputManager.mousePosV2;
        if (IsPosInBoard(mousePosV2)) {
            if (aIsFront) {
                EntityState? entityAtMousePos = this.boardCellDict[GM.inputManager.mousePosV2].frontEntityState;
                return entityAtMousePos;
            }
            else {
                EntityState? entityAtMousePos = this.boardCellDict[GM.inputManager.mousePosV2].backEntityState;
                return entityAtMousePos;
            }
            
        }
        else {
            return null;
        }
    }

    public EntityState GetEntityById(int aId) {
        if (this.currentState.entityDict.ContainsKey(aId)) {
            return this.currentState.entityDict[aId];
        }
        else {
            throw new Exception("GetEntityById - invalid id");
        }
    }

    public EntityBase GetEntityBaseById(int aId) {
        if (this.entityBaseDict.ContainsKey(aId)) {
            return this.entityBaseDict[aId];
        }
        else {
            throw new Exception("GetEntityBaseById - invalid id");
        }
    }
    // special function called by GM.OnUpdateGameState delegate
    public void OnUpdateGameState(GameState aGameState) {
        
    }
}
