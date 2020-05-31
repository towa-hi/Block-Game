using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Sirenix.OdinInspector;

public delegate void OnUpdateBoardStateHandler(BoardState aBoardState);

public class BoardManager : SerializedMonoBehaviour {
    
    [SerializeField]
    BoardState currentState;
    public BoardState boardState {
        get {
            return this.currentState;
        }
    }

    public event OnUpdateBoardStateHandler OnUpdateBoardState;

    public Dictionary<int, EntityBase> entityBaseDict;

    void Awake() {
        Init();
    }

    // special function called by GM.OnUpdateGameState delegate
    public void OnUpdateGameState(GameState aGameState) {
        
    }

    public void Init() {
        this.entityBaseDict = new Dictionary<int, EntityBase>();
        BoardState newBoard = BoardState.GenerateBlankBoard();
        UpdateBoardState(newBoard);
        // TODO: move this somewhere sensible
        CreateBoundaries();
    }

    public void CreateBoundaries() {
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

    public void AddEntity(EntitySchema aEntitySchema, Vector2Int aPos, Vector2Int aFacing, Color aDefaultColor, bool aIsFixed = false, bool aIsBoundary = false) {
        // create the new entity without the id 
        EntityState newEntity = EntityState.CreateEntityState(aEntitySchema, aPos, aFacing, aDefaultColor, aIsFixed, aIsBoundary);
        var tuple = BoardState.AddEntity(this.boardState, newEntity);
        // addEntity in boardstate will add id to the new version of that entity inside the tuple as nweEntityWithId
        BoardState newBoard = tuple.Item1;
        EntityState newEntityWithId = tuple.Item2;
        UpdateBoardState(newBoard);
        // make the entity base
        EntityBase entityBase2 = CreateEntityBase(newEntityWithId);
        // put the entitybase inside a dict with the id so we can get it later
        this.entityBaseDict[newEntityWithId.id] = entityBase2;
        // update board 
        UpdateBoardState(newBoard);
    }

    EntityBase CreateEntityBase(EntityState aEntityState) {
        GameObject entityPrefab = Instantiate(GM.LoadEntityPrefabByFilename(aEntityState.prefabPath), Util.V2IOffsetV3(aEntityState.pos, aEntityState.size), Quaternion.identity,  this.transform); 
        EntityBase entityBase = entityPrefab.GetComponent<EntityBase>();
        // id must be assigned here or entity will not recieve
        entityBase.id = aEntityState.id;
        entityBase.name = aEntityState.name + " Id: " + aEntityState.id;
        return entityBase;
    }

    public void RemoveEntity(int aId) {
        EntityState entityState = this.boardState.entityDict[aId];
        EntityBase entityBase = this.entityBaseDict[aId];

        BoardState newBoard = BoardState.RemoveEntity(this.boardState, aId);
        UpdateBoardState(newBoard);
        this.entityBaseDict.Remove(aId);
    }

    public void MoveEntity(EntityState aEntityState, Vector2Int aPos) {
        aEntityState.pos = aPos;
        BoardState newState = BoardState.UpdateEntity(this.boardState, aEntityState);
        UpdateBoardState(newState);
    }

    public void MoveEntityTest() {
        print("doing moveEntityTest");
        MoveEntity(this.boardState.entityDict[0], new Vector2Int(5, 5));
    }

    public bool CanEditorPlaceSchema(Vector2Int aPos, EntitySchema aEntitySchema) {
        // TODO: finish this
        return true;
    }

    public void UpdateBoardState(BoardState aBoardState) {
        this.currentState = aBoardState;
        this.OnUpdateBoardState?.Invoke(this.boardState);
    }
}
