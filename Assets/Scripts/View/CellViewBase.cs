using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

public class CellViewBase: SerializedMonoBehaviour {
    public GameCell gameCell;
    // set by editor
    public MeshRenderer panelRenderer;

    public void Init(GameCell aGameCell) {
        this.gameCell = aGameCell;
        this.name = "Cell " + this.gameCell.pos.ToString();
    }

    public void SetPanelVisbility() {
        this.panelRenderer.enabled = this.gameCell.panelVisible;
    }

    public void SetColor() {
        this.panelRenderer.material.color = this.gameCell.panelColor;
    }

    void OnDrawGizmos() {
        Gizmos.color = Color.white;
        if (this.gameCell.entityData != null) {
            if (this.gameCell.entityData.isFixed) {
                Gizmos.color = Color.red;
            } else {
                Gizmos.color = Color.green;
            }
        }
        Gizmos.DrawSphere(transform.position + new Vector3(0, 0, -1f), 0.1f);
    }
}
