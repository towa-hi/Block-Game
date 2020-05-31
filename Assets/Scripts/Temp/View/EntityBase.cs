using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;


public class EntityBase : BoardStateListener {
    // temp
    public int id;
    public EntityState oldEntityState;
    public GameObject model;
    public Renderer modelRenderer;
    public Renderer[] childRenderers;

    void Awake() {
        this.id = -42069;
        this.model = this.transform.GetChild(0).gameObject;
        this.modelRenderer = this.model.GetComponent<Renderer>();
        this.childRenderers = this.model.GetComponentsInChildren<Renderer>();
    }

    public override void OnUpdateBoardState(BoardState aBoardState) {
        // when id is -1, this wont recieve any boardupdates because it hasn't been
        // assigned an ID yet by BoardManager.CreateView
        if (this.id == -42069) {
            return;
        }
        EntityState newEntityState = aBoardState.entityDict[this.id];
        if (!this.oldEntityState.CustomEquals(newEntityState)) {
            // print(id + " updated entity state");
            this.transform.position = Util.V2IOffsetV3(newEntityState.pos, newEntityState.size);
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

    void OnDrawGizmos() {
        Gizmos.color = Color.red;
        Vector2Int size = this.oldEntityState.size;
        Vector3 sizeV3 = new Vector3(size.x, size.y * Constants.BLOCKHEIGHT, 2f);
        Gizmos.DrawWireCube(this.transform.position, sizeV3);
    }
}
