using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

public class PreviewCubeBase : SerializedMonoBehaviour {
    public Vector2Int size;
    public Vector2Int pos;
    public Renderer myRenderer;

    public void SetSize(Vector2Int aSize) {
        this.size = aSize;
        this.transform.localScale = Util.V2IToV3(aSize) + Constants.BLOCKTHICCNESS;
    }

    public void SetPos(Vector2Int aPos) {
        this.pos = aPos;
        this.transform.position = Util.V2IOffsetV3(this.pos, this.size);
    }

    public void SetColor(Color aColor) {
        this.myRenderer.material.SetColor("_LineColor", aColor);
    }

    public void SetActive(bool aIsActive) {
        this.gameObject.SetActive(aIsActive);
    }

    public void SetAsEntity(EntityData aEntityData) {
        if (aEntityData != null) {
            SetSize(aEntityData.size);
            SetPos(aEntityData.pos);
        } else {
            throw new System.Exception("SetAsEntity is null");
        }
    }
}
