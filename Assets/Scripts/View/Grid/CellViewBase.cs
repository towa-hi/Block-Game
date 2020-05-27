using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

[SelectionBase]
public class CellViewBase: SerializedMonoBehaviour {
    public GameCell gameCell;
    [Header("Set In Editor")]
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
        SetPanelVisbility(true);
        // if in the middle of another colorfade, stop it immediately
        if (this.colorFadeCoroutine != null) {
            StopCoroutine(this.colorFadeCoroutine);
        }
        // set object to new color
        // SetColor(aColor);
        // start a new colorFade
        this.colorFadeCoroutine = StartCoroutine(ColorFade(aColor, 2.5f));
        
    }

    public IEnumerator ColorFade(Color aColor, float aDuration) {
        this.panelRenderer.material.color = aColor;
        // Color currentColor = this.panelRenderer.material.color;
        float t = 0f;
        while (t < 1) {
            // we use unscaledDeltaTime here or else this effect will pause when the game is paused
            t += Time.unscaledDeltaTime  / aDuration;
            Color currentColor = this.panelRenderer.material.color;
            currentColor.a = Mathf.Lerp(aColor.a, 0f, t);
            this.panelRenderer.material.color = currentColor;
            yield return null;
        }
        SetPanelVisbility(false);
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
