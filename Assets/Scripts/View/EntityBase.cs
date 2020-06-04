using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using Sirenix.Utilities;


public class EntityBase : BoardStateListener {
    [SerializeField] bool isTempPos;
    public GameObject model;
    Renderer modelRenderer;
    HashSet<Renderer> childRenderers;
    EntityImmutableData data;
    EntityState oldEntityState;
    [ShowInInspector] EntityState entityState {
        get {
            return GM.boardManager.GetEntityById(this.data.id);
        }
    }

    void Awake() {
        this.model = this.transform.GetChild(0).gameObject;
        this.modelRenderer = this.model.GetComponent<Renderer>();
        this.childRenderers = this.model.GetComponentsInChildren<Renderer>().ToHashSet();
    }

    public void Init(EntityState aEntityState) {
        this.data = aEntityState.data;
        this.transform.position = Util.V2IOffsetV3(aEntityState.pos, this.data.size, this.data.isFront);
        SetColor(aEntityState.defaultColor);
        this.name = this.data.name + " Id: " + this.data.id;
        if (aEntityState.hasNodes) {
            foreach (Vector2Int upNode in aEntityState.upNodes) {
                Vector2Int currentPos = aEntityState.pos + upNode;
                Vector3 currentPosition = Util.V2IOffsetV3(currentPos, new Vector2Int(1, 1));
                float studX = currentPosition.x;
                float studY = currentPosition.y + (Constants.BLOCKHEIGHT / 2);
                GameObject stud = Instantiate(GM.instance.studPrefab, new Vector3(studX, studY, 0), Quaternion.identity);
                stud.transform.SetParent(this.model.transform, true);
                Renderer studRenderer = stud.GetComponent<Renderer>();
                studRenderer.material.color = aEntityState.defaultColor;
                this.childRenderers.Add(studRenderer);
            }
            print("completed drawing nodes");
        }
    }

    protected override void OnUpdateBoardState(BoardState aBoardState) {
        // when id is -42069, this wont recieve any boardupdates because it hasn't been
        // assigned an ID yet by BoardManager.CreateView
        if (this.data.id == Constants.PLACEHOLDERINT) {
            return;
        }
        EntityState newEntityState = aBoardState.entityDict[this.data.id];
        if (!this.oldEntityState.CustomEquals(newEntityState)) {
            // print(id + " updated entity state");
            this.transform.position = Util.V2IOffsetV3(newEntityState.pos, newEntityState.data.size, this.data.isFront);
            SetColor(newEntityState.defaultColor);
            this.oldEntityState = newEntityState;
        }
    }

    public void SetColor(Color aColor) {
        this.modelRenderer.material.color = aColor;
        foreach (Renderer childRenderer in this.childRenderers) {
            childRenderer.material.color = aColor;
        }
    }

    public void SetTempViewPosition(Vector2Int aPos) {
        this.transform.position = Util.V2IOffsetV3(aPos, this.data.size, this.data.isFront);
        this.isTempPos = true;
    }

    public void ResetTempView() {
        EntityState currentState = GM.boardManager.GetEntityById(this.data.id);
        this.transform.position = Util.V2IOffsetV3(currentState.pos, this.data.size, this.data.isFront);
        this.isTempPos = false;
    }
    
    void OnDrawGizmos() {
        Gizmos.color = Color.red;
        Vector2Int size = this.data.size;
        Vector3 sizeV3 = new Vector3(size.x, size.y * Constants.BLOCKHEIGHT, 2f);
        Gizmos.DrawWireCube(this.transform.position, sizeV3);
        if (this.entityState.hasNodes) {
            EntityState entityState = this.entityState;
            Vector3 zOffset = new Vector3(0, 0, -1.01f);
            Gizmos.color = Color.red;
            foreach (Vector2Int upNode in entityState.upNodes) {
                Vector2Int currentPos = this.entityState.pos + upNode;
                Vector3 arrowOrigin = Util.V2IOffsetV3(currentPos, new Vector2Int(1, 1)) + zOffset;
                DrawArrow.I.ForGizmo(arrowOrigin, new Vector3(0, 0.5f, 0));
            }

            Gizmos.color = Color.blue;
            foreach (Vector2Int downNode in entityState.downNodes) {
                Vector2Int currentPos = this.entityState.pos + downNode;
                Vector3 arrowOrigin = Util.V2IOffsetV3(currentPos, new Vector2Int(1, 1)) + zOffset;
                DrawArrow.I.ForGizmo(arrowOrigin, new Vector3(0, -0.5f, 0));
            }
        }
    }
}
