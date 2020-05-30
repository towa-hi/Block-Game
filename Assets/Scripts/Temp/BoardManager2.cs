using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Sirenix.OdinInspector;

public struct BoardState {
    public Dictionary<int, EntityState> entityDict;
    public string title;
    public string creator;
    public int par;
    public Vector2Int size;
    public int attempts;
    public int currentId;

    public static BoardState GenerateBlankBoard() {
        BoardState newBoard = new BoardState();
        newBoard.entityDict = new Dictionary<int, EntityState>();
        newBoard.title = "Uninitialized Board";
        newBoard.creator = Config.USERNAME;
        newBoard.par = 5;
        newBoard.size = new Vector2Int(40, 24);
        newBoard.attempts = 0;
        return newBoard;
    }

    public bool CanMoveEntity(EntityState aEntityState, Vector2Int aPos) {
        return true;
    }


    public static Tuple<BoardState, EntityState> AddEntity(BoardState aBoardState, EntityState aEntityState) {
        int id = aBoardState.currentId;
        // id always set here
        aEntityState.id = id;
        // never change currentId outside of here
        aBoardState.currentId += 1;
        aBoardState.entityDict[id] = aEntityState;
        return new Tuple<BoardState, EntityState>(aBoardState, aEntityState);
    }
    
    public static BoardState RemoveEntity(BoardState aBoardState, int aId) {
        aBoardState.entityDict.Remove(aId);

        return aBoardState;

    }
    public static BoardState SetTitle(BoardState aBoardState, string aTitle) {
        aBoardState.title = aTitle;
        return aBoardState;
    }

    public static BoardState UpdateEntity(BoardState aBoardState, EntityState aEntityState) {
        aBoardState.entityDict[aEntityState.id] = aEntityState;
        return aBoardState;
    }
    // public static BoardState SetPar(BoardState aBoardState, int aPar) {

    // }
}
















public struct EntityState {
    public int id;
    // never change these
    public string name;
    public Vector2Int size;
    public EntityTypeEnum entityType;
    public bool isBoundary;
    public string prefabPath;

    public Vector2Int pos;
    public Vector2Int facing;
    public Color defaultColor;
    public bool isFixed;
    public TeamEnum team;

    public HashSet<Vector2Int> upNodes;
    public HashSet<Vector2Int> downNodes;

    public int touchDefense;
    public int fallDefense;

    public bool CustomEquals(EntityState aOther) {        
        if (GetType() != aOther.GetType())
        {
            return false;
        }
        
        if (
            this.id == aOther.id &&
            this.name == aOther.name &&
            this.size == aOther.size &&
            this.entityType == aOther.entityType &&
            this.isBoundary == aOther.isBoundary &&
            this.prefabPath == aOther.prefabPath &&
            this.pos == aOther.pos &&
            this.facing == aOther.facing &&
            this.defaultColor == aOther.defaultColor &&
            this.isFixed == aOther.isFixed &&
            this.team == aOther.team &&
            this.upNodes == aOther.upNodes &&
            this.downNodes == aOther.downNodes &&
            this.touchDefense == aOther.touchDefense &&
            this.fallDefense == aOther.fallDefense
        ) {
            return true;
        } else {
            return false;
        }
    }

    public static EntityState GenerateNewEntity(EntitySchema aEntitySchema, Vector2Int aPos, Vector2Int aFacing, Color aDefaultColor, bool aIsFixed = false, bool aIsBoundary = false) {
        EntityState newEntityState = new EntityState();

        newEntityState.size = aEntitySchema.size;
        newEntityState.entityType = aEntitySchema.type;
        newEntityState.isBoundary = aIsBoundary;
        newEntityState.prefabPath = aEntitySchema.prefabPath;

        newEntityState.pos = aPos;
        newEntityState.facing = aFacing;
        newEntityState.defaultColor = aDefaultColor;
        newEntityState.isFixed = aIsFixed;
        newEntityState.team = aEntitySchema.defaultTeam;
        
        newEntityState.upNodes = new HashSet<Vector2Int>();
        newEntityState.downNodes = new HashSet<Vector2Int>();

        newEntityState.touchDefense = aEntitySchema.touchDefense;
        newEntityState.fallDefense = aEntitySchema.fallDefense;

        newEntityState.name = newEntityState.GenerateName();

        return newEntityState;
    }

    string GenerateName() {
        string nameString = this.entityType.ToString() + " " + this.size;
        if (this.isBoundary) {
            nameString += " (boundary)";
        }
        nameString += GetHashCode();
        return nameString;
    }

    public static EntityState SetPos(EntityState aEntityState, Vector2Int aPos) {
        aEntityState.pos = aPos;
        return aEntityState;
    }

    public static EntityState SetFacing(EntityState aEntityState, Vector2Int aFacing) {
        if (Util.IsDirection(aFacing)) {
            aEntityState.facing = aFacing;
        }
        return aEntityState;
    }

    public static EntityState SetDefaultColor(EntityState aEntityState, Color aColor) {
        aEntityState.defaultColor = aColor;
        return aEntityState;
    }

    public static EntityState SetIsFixed(EntityState aEntityState, bool aIsFixed) {
        aEntityState.isFixed = aIsFixed;
        return aEntityState;
    }

    public static EntityState SetTeam(EntityState aEntityState, TeamEnum aTeam) {
        aEntityState.team = aTeam;
        return aEntityState;
    }

    public static EntityState SetNodes(EntityState aEntityState, HashSet<Vector2Int> aUpNodes, HashSet<Vector2Int> aDownNodes) {
        Debug.Assert(aUpNodes != null);
        Debug.Assert(aDownNodes != null);
        aEntityState.upNodes = aUpNodes;
        aEntityState.downNodes = aDownNodes;
        return aEntityState;
    }

    public static EntityState SetTouchDefense(EntityState aEntityState, int aTouchDefense) {
        if (0 <= aTouchDefense && aTouchDefense <= 999) {
            aEntityState.touchDefense = aTouchDefense;
            return aEntityState;
        } else {
            throw new System.Exception("tried to set invalid touchDefense");
        }
    }

    public static EntityState SetFallDefense(EntityState aEntityState, int aFallDefense) {
        if (0 <= aFallDefense && aFallDefense <= 999) {
            aEntityState.fallDefense = aFallDefense;
            return aEntityState;
        } else {
            throw new System.Exception("tried to set invalid fallDefense");
        }
    }
}






public delegate void OnUpdateStateHandler();


public class BoardManager2 : SerializedMonoBehaviour {
    public Dictionary<int, EntityBase2> entityBaseDict;
    
    public BoardState boardState;
    public event OnUpdateStateHandler OnUpdateBoardState;


    public GameObject testEntityObject;

    public void Init() {
        this.entityBaseDict = new Dictionary<int, EntityBase2>();
        EntitySchema tallBoy = AssetDatabase.LoadAssetAtPath<EntitySchema>("Assets/Resources/ScriptableObjects/Entities/Blocks/1x11 block.asset");
        EntitySchema longBoy = AssetDatabase.LoadAssetAtPath<EntitySchema>("Assets/Resources/ScriptableObjects/Entities/Blocks/20x1 block.asset");
        BoardState newBoard = BoardState.GenerateBlankBoard();
        UpdateBoardState(newBoard);

        AddEntity(longBoy, new Vector2Int(0, 0), Constants.DEFAULTFACING, Constants.DEFAULTCOLOR, true, true);
        
        AddEntity(longBoy, new Vector2Int(20, 0), Constants.DEFAULTFACING, Constants.DEFAULTCOLOR, true, true);

        AddEntity(longBoy, new Vector2Int(0, 23), Constants.DEFAULTFACING, Constants.DEFAULTCOLOR, true, true);
        AddEntity(longBoy, new Vector2Int(20, 23), Constants.DEFAULTFACING, Constants.DEFAULTCOLOR, true, true);

        AddEntity(tallBoy, new Vector2Int(0, 1), Constants.DEFAULTFACING, Constants.DEFAULTCOLOR, true, true);
        AddEntity(tallBoy, new Vector2Int(0, 12), Constants.DEFAULTFACING, Constants.DEFAULTCOLOR, true, true);

        AddEntity(tallBoy, new Vector2Int(39, 1), Constants.DEFAULTFACING, Constants.DEFAULTCOLOR, true, true);
        AddEntity(tallBoy, new Vector2Int(39, 12), Constants.DEFAULTFACING, Constants.DEFAULTCOLOR, true, true);
    }

    public void AddEntity(EntitySchema aEntitySchema, Vector2Int aPos, Vector2Int aFacing, Color aDefaultColor, bool aIsFixed = false, bool aIsBoundary = false) {
        // create the new entity without the id 
        EntityState newEntity = EntityState.GenerateNewEntity(aEntitySchema, aPos, aFacing, aDefaultColor, aIsFixed, aIsBoundary);
        var tuple = BoardState.AddEntity(this.boardState, newEntity);
        // addEntity in boardstate will add id to the new version of that entity inside the tuple as nweEntityWithId
        BoardState newBoard = tuple.Item1;
        EntityState newEntityWithId = tuple.Item2;

        // make the entity base
        EntityBase2 entityBase2 = CreateView(newEntityWithId);
        // put the entitybase inside a dict with the id so we can get it later
        this.entityBaseDict[newEntityWithId.id] = entityBase2;
        // update board 
        UpdateBoardState(newBoard);
    }

    public void RemoveEntity(int aId) {
        EntityState entityState = this.boardState.entityDict[aId];
        EntityBase2 entityBase = this.entityBaseDict[aId];

        BoardState newBoard = BoardState.RemoveEntity(this.boardState, aId);
        UpdateBoardState(newBoard);
        this.entityBaseDict.Remove(aId);
    }

    public void Update() {
        // foreach (KeyValuePair<int, EntityState> kvp in this.boardState.entityDict) {
        //     UpdateEntityState(kvp.Value);
        // }
        // UpdateBoardState(this.boardState);
    }

    public EntityBase2 CreateView(EntityState aEntityState) {
        GameObject newEntityView = Instantiate(this.testEntityObject, Util.V2IOffsetV3(aEntityState.pos, aEntityState.size), Quaternion.identity, this.transform); 
        EntityBase2 entityBase = newEntityView.GetComponent<EntityBase2>();
        entityBase.id = aEntityState.id;
        entityBase.name = aEntityState.name + " id: " + aEntityState.id;
        return entityBase;
    }

    // public static BoardState UpdateGameTick(BoardState aOldBoardState, float aDeltaTime) {
    //     Dictionary<int, EntityState> newEntityDict = new Dictionary<int, EntityState>();
    //     foreach (KeyValuePair<int, EntityState> kvp in aOldBoardState.entityDict) {
    //         // newEntityDict[kvp.Key] = MoveEntity(kvp.Value);
    //     }
    //     aOldBoardState.entityDict = newEntityDict;
    //     return aOldBoardState;
    // }

    public void MoveEntity(EntityState aEntityState, Vector2Int aPos) {
        aEntityState.pos = aPos;
        BoardState newState = BoardState.UpdateEntity(this.boardState, aEntityState);
        UpdateBoardState(newState);
    }

    public void MoveEntityTest() {
        print("doing moveEntityTest");
        MoveEntity(this.boardState.entityDict[0], new Vector2Int(5, 5));
    }

    public void UpdateBoardState(BoardState aBoardState) {
        this.boardState = aBoardState;
        this.OnUpdateBoardState?.Invoke();
    }
}
