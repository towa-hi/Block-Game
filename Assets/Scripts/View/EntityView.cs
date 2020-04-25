using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

// holds data concerning what the entity looks like. 
// this.gameObject is a child of the gameObject holding EntityBase
public class EntityView : SerializedMonoBehaviour {
    // set by init
    public Renderer myRenderer;
    // set by editor
    public EntityBase entityBase;
    public Color defaultColor;

    private Coroutine colorFadeCoroutine;
    
    public void Init(EntityData aEntityData) {
        this.myRenderer = GetComponent<Renderer>();
        // do any special accomidations to the entity depending on type. mainly to adjust BLOCK size
        switch (aEntityData.entitySchema.type) {
            case EntityTypeEnum.BLOCK:
                this.transform.localScale = Util.V2IToV3(aEntityData.entitySchema.size) + Constants.BLOCKTHICCNESS;
                break;
            case EntityTypeEnum.MOB:
                // TODO: this just sets a capsule to a reasonable size for now
                this.transform.localScale = new Vector3(2, 2, 2);
                break;
        }
        this.defaultColor = aEntityData.color;
        SetColor(this.defaultColor);
    }

    // sets color of entity material without changing defaultColor
    public void SetColor(Color aColor) {
        this.myRenderer.material.color = aColor;
    }

    // temporarily highlights the entity and then have it fade away
    // does not change defaultColor
    public void TempHighlight(Color aColor) {
        // if in the middle of another colorfade, stop it immediately
        if (this.colorFadeCoroutine != null) {
            StopCoroutine(colorFadeCoroutine);
        }
        // set object to new color
        SetColor(aColor);
        // start a new colorFade
        this.colorFadeCoroutine = StartCoroutine(ColorFade(1f));
        // when that colorFade is done, set color to avoid float bullshit
        SetColor(this.defaultColor);
    }

    public IEnumerator ColorFade(float aDuration) {
        Color currentColor = this.myRenderer.material.color;
        float t = 0f;
        while (t < 1) {
            t += Time.deltaTime / aDuration;
            this.myRenderer.material.color = Color.Lerp(currentColor, this.defaultColor, t);
            yield return null;
        }
    }
}
