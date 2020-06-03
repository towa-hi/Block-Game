using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;


public class EntityBase : BoardStateListener {
    // public int id;
    [SerializeField] bool isTempPos;
    public GameObject model;
    public Renderer modelRenderer;
    public Renderer[] childRenderers;
    public EntityImmutableData data;
    EntityState oldEntityState;
    void Awake() {
        this.model = this.transform.GetChild(0).gameObject;
        this.modelRenderer = this.model.GetComponent<Renderer>();
        this.childRenderers = this.model.GetComponentsInChildren<Renderer>();
    }

    public void Init(EntityState aEntityState) {
        // this.id = aEntityState.data.id;
        this.data = aEntityState.data;
        this.transform.position = Util.V2IOffsetV3(aEntityState.pos, this.data.size, this.data.isFront);
        SetColor(aEntityState.defaultColor);
        this.oldEntityState = aEntityState;
        this.name = this.data.name + " Id: " + this.data.id;
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
    }
}
