using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

// holds data concerning what the entity looks like. 
// this.gameObject is a child of the gameObject holding EntityBase
public class EntityView : SerializedMonoBehaviour {
    // set by init
    public Renderer myRenderer;
    public Renderer[] childRenderers;
    // public Color defaultColor;
    private Coroutine colorFadeCoroutine;
    public EntityData entityData;
    // set by prefab
    public EntityBase entityBase;
    public bool isGhost;

    public void Init(EntityData aEntityData) {
        this.entityData = aEntityData;
        this.myRenderer = GetComponent<Renderer>();
        // do any special accomidations to the entity depending on type. mainly to adjust BLOCK size
        switch (aEntityData.entitySchema.type) {
            case EntityTypeEnum.BLOCK:
                this.transform.localScale = Util.V2IToV3(aEntityData.entitySchema.size) + Constants.BLOCKTHICCNESS;
                break;
            case EntityTypeEnum.MOB:
                // TODO: this just sets a capsule to a reasonable size for now
                // this.transform.localScale = new Vector3(2, 2, 2);
                break;
        }

        // // cache any renderer components in children after they've been made in OnEntityViewInit
        // this.childRenderers = GetComponentsInChildren<Renderer>();

        // this.defaultColor = aEntityData.defaultColor;
        SetColor(this.entityData.defaultColor);
    }

    public void SetChildRenderers() {
        this.childRenderers = GetComponentsInChildren<Renderer>();
    }

    // sets color of entity material without changing defaultColor
    public void SetColor(Color aColor) {
        this.myRenderer.material.color = aColor;
        foreach (Renderer childRenderer in this.childRenderers) {
            childRenderer.material.color = aColor;
        }
    }


    // temporarily highlights the entity and then have it fade away
    // does not change defaultColor
    public void TempHighlight(Color aColor) {
        // if in the middle of another colorfade, stop it immediately
        if (this.colorFadeCoroutine != null) {
            StopCoroutine(this.colorFadeCoroutine);
        }
        // set object to new color
        SetColor(aColor);
        // start a new colorFade
        this.colorFadeCoroutine = StartCoroutine(ColorFade(1f));
        // when that colorFade is done, set color to avoid float bullshit
        SetColor(this.entityData.defaultColor);
    }

    public IEnumerator ColorFade(float aDuration) {
        Color currentColor = this.myRenderer.material.color;
        float t = 0f;
        while (t < 1) {
            t += Time.deltaTime / aDuration;
            SetColor(Color.Lerp(currentColor, this.entityData.defaultColor, t));
            yield return null;
        }
    }

    public void SetGhost(bool aIsGhost) {
        this.isGhost = aIsGhost;
        Color currentColor = this.myRenderer.material.color;
        if (this.isGhost) {
            currentColor.a = 0.5f;
        } else {
            currentColor.a = 1f;
        }
        this.myRenderer.material.color = currentColor;
        foreach(Renderer renderer in this.childRenderers) {
            renderer.material.color = currentColor;
        }
    }
}
