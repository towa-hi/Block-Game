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
    Dictionary<Vector2Int, BoardCell> boardCellDictTemplate;
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
        // NOTE: if this isn't performant, clear the dict instead of replacing it
        SetBoardCellDict(aBoardState);
        OnUpdateBoardState?.Invoke(this.currentState);
    }

    public void SetBoardCellDict(BoardState aBoardState) {
        this.boardCellDict = new Dictionary<Vector2Int, BoardCell>(this.boardCellDictTemplate);
        foreach (KeyValuePair<int, EntityState> kvp in aBoardState.entityDict) {
            foreach(Vector2Int currentPos in Util.V2IInRect(kvp.Value.pos, kvp.Value.size)) {
                BoardCell updatedCell = this.boardCellDict[currentPos];
                updatedCell.entityState = kvp.Value;
                this.boardCellDict[currentPos] = updatedCell;
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
    
    public void MoveEntity(Vector2Int aPos, EntityState aEntityState) {
        HashSet<EntityState> ignoreSet = new HashSet<EntityState> {aEntityState};
        if (IsRectEmpty(aPos, aEntityState.size, ignoreSet)) {
            UpdateEntityAndBoardState(EntityState.SetPos(aEntityState, aPos));
            aEntityState.entityBase.ResetTempView();
        } else {
            throw new Exception("MoveEntity - invalid move");
        }
    }

    public void SetEntityFacing(EntityState aEntityState, Vector2Int aFacing) {
        if (Util.IsDirection(aFacing)) {
            UpdateEntityAndBoardState(EntityState.SetFacing(aEntityState, aFacing));
        } else {
            throw new Exception("SetEntityFacing - invalid facing direction");
        }
    }

    public void SetEntityDefaultColor(EntityState aEntityState, Color aColor) {
        UpdateEntityAndBoardState(EntityState.SetDefaultColor(aEntityState, aColor));
    }

    public void SetEntityIsFixed(EntityState aEntityState, bool aIsFixed) {
        UpdateEntityAndBoardState(EntityState.SetIsFixed(aEntityState, aIsFixed));
    }

    public void SetEntityTeam(EntityState aEntityState, TeamEnum aTeam) {
        UpdateEntityAndBoardState(EntityState.SetTeam(aEntityState, aTeam));
    }

    public void SetEntityNodes(EntityState aEntityState, HashSet<Vector2Int> aUpNodes, HashSet<Vector2Int> aDownNodes) {
        if (aUpNodes != null && aDownNodes != null) {
            UpdateEntityAndBoardState(EntityState.SetNodes(aEntityState, aUpNodes, aDownNodes));
        } else {
            throw new Exception("SetEntityNodes - params can't be null");
        }
    }

    public void SetEntityTouchDefense(EntityState aEntityState, int aTouchDefense) {
        if (0 <= aTouchDefense && aTouchDefense <= 999) {
            UpdateEntityAndBoardState(EntityState.SetTouchDefense(aEntityState, aTouchDefense));
        } else {
            throw new Exception("SetEntityTouchDefense - invalid touchDefense");
        }
    }

    public void SetEntityFallDefense(EntityState aEntityState, int aFallDefense) {
        if (0 <= aFallDefense && aFallDefense <= 999) {
            UpdateEntityAndBoardState(EntityState.SetFallDefense(aEntityState, aFallDefense));
        } else {
            throw new Exception("SetEntityFallDefense - invalid touchDefense");
        }
    }

    public bool CanEditorPlaceSchema(Vector2Int aPos, EntitySchema aEntitySchema) {
        // TODO: finish this
        if (IsRectEmpty(aPos, aEntitySchema.size)) {
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

    public bool IsRectEmpty(Vector2Int aOrigin, Vector2Int aSize, HashSet<EntityState> aIgnoreSet = null) {
        if (IsRectInBoard(aOrigin, aSize)) {
            foreach (KeyValuePair<Vector2Int, BoardCell> kvp in GetBoardGridSlice(aOrigin, aSize)) {
                if (kvp.Value.entityState != null) {
                    if (aIgnoreSet.All(entityState => kvp.Value.entityState.Value.id != entityState.id)) {
                        return false;
                    }
                }
            }
            return true;
        }
        return false;
    }

    public void AddEntity(EntitySchema aEntitySchema, Vector2Int aPos, Vector2Int aFacing, Color aDefaultColor, bool aIsFixed = false, bool aIsBoundary = false) {
        // create the new entity without the id 
        EntityState newEntity = EntityState.CreateEntityState(aEntitySchema, aPos, aFacing, aDefaultColor, aIsFixed, aIsBoundary);
        (BoardState newBoard, EntityState newEntityWithId) = BoardState.AddEntity(this.currentState, newEntity);
        // addEntity in boardstate will add id to the new version of that entity inside the tuple as nweEntityWithId
        // 
        UpdateBoardState(newBoard);
        // make the entity base
        GameObject entityPrefab = Instantiate(GM.LoadEntityPrefabByFilename(newEntityWithId.prefabPath), Util.V2IOffsetV3(newEntityWithId.pos, newEntityWithId.size), Quaternion.identity,  this.transform); 
        EntityBase entityBase = entityPrefab.GetComponent<EntityBase>();
        this.entityBaseDict[newEntityWithId.id] = entityBase;
        entityBase.Init(newEntityWithId);
        // put the entitybase inside a dict with the id so we can get it later
        // update board 
        // UpdateBoardState(newBoard);
    }

    public void RemoveEntity(int aId) {
        EntityBase entityBase = this.entityBaseDict[aId];
        BoardState newBoard = BoardState.RemoveEntity(this.currentState, aId);
        this.entityBaseDict.Remove(aId);
        Destroy(entityBase);
        UpdateBoardState(newBoard);
    }

    public void Init() {
        this.entityBaseDict = new Dictionary<int, EntityBase>();
        BoardState newBoard = BoardState.GenerateBlankBoard();
        this.boardCellDictTemplate = new Dictionary<Vector2Int, BoardCell>();
        for (int x = 0; x < newBoard.size.x; x++) {
            for (int y = 0; y < newBoard.size.y; y++) {
                Vector2Int currentPos = new Vector2Int(x, y);
                BoardCell newCell = new BoardCell(currentPos);
                this.boardCellDictTemplate[currentPos] = newCell;
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

    public EntityState? GetEntityAtMousePos() {
        Vector2Int mousePosV2 = GM.inputManager.mousePosV2;
        if (IsPosInBoard(mousePosV2)) {
            EntityState? entityAtMousePos = this.boardCellDict[GM.inputManager.mousePosV2].entityState;
            return entityAtMousePos;
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
