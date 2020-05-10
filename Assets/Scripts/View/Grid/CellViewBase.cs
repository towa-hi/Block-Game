using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

[SelectionBase]
public class CellViewBase: SerializedMonoBehaviour {
    public GameCell gameCell;
    // set by editor
    public MeshRenderer panelRenderer;
    private Coroutine colorFadeCoroutine;
    public Color gizmoColor;

    public void Init(GameCell aGameCell) {
        this.gameCell = aGameCell;
        this.gizmoColor = Color.white;
        this.name = "Cell " + this.gameCell.pos.ToString();
    }

    public void SetPanelVisbility(bool aIsPanelVisible) {
        this.panelRenderer.enabled = aIsPanelVisible;
    }

    public void SetColor(Color aColor) {
        this.panelRenderer.material.color = aColor;
    }

    // temporarily highlights the entity and then have it fade away
    // does not change defaultColor
    public void TempHighlight(Color aColor) {
        print("started");
        SetPanelVisbility(true);
        // if in the middle of another colorfade, stop it immediately
        if (this.colorFadeCoroutine != null) {
            StopCoroutine(this.colorFadeCoroutine);
        }
        // set object to new color
        // SetColor(aColor);
        // start a new colorFade
        this.colorFadeCoroutine = StartCoroutine(ColorFade(aColor, 1f));
        
    }

    public IEnumerator ColorFade(Color aColor, float aDuration) {
        this.panelRenderer.material.color = aColor;
        // Color currentColor = this.panelRenderer.material.color;
        float t = 0f;
        while (t < 1) {
            t += Time.deltaTime / aDuration;
            Color currentColor = this.panelRenderer.material.color;
            currentColor.a = Mathf.Lerp(aColor.a, 0f, t);
            this.panelRenderer.material.color = currentColor;
            print("current opacity: " + currentColor.a);
            yield return null;
        }
        SetPanelVisbility(false);
        print("ended");
    }

    void OnDrawGizmos() {
        Gizmos.color = this.gizmoColor;
        if (this.gameCell != null) {
            if (this.gameCell.entityData != null) {
                if (this.gameCell.entityData.isFixed) {
                    Gizmos.color = Color.red;
                } else {
                    Gizmos.color = Color.green;
                }
                
            }
        }
        Gizmos.DrawSphere(transform.position + new Vector3(0, 0, -1f), 0.1f);
        
    }
}
