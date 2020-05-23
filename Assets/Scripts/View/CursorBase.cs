using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

public class CursorBase : SerializedMonoBehaviour {
    Vector2Int size;
    Vector2Int pos;
    Vector3 zOffset = new Vector3(0, 0, -1.01f);

    // TODO: have a layer for bg stuff that offsets the position of the cursor back by z = 2f
    public SpriteRenderer myRenderer;
    public void SetPos(Vector2Int aPos) {
        this.pos = aPos;
        this.transform.position = Util.V2IToV3(this.pos) + zOffset;
    }
    public void SetSize(Vector2Int aSize) {
        this.size = aSize;
        this.transform.position = Util.V2IToV3(this.pos) + zOffset;
        this.myRenderer.size = new Vector2(this.size.x, this.size.y * Constants.BLOCKHEIGHT);
    }

    public void SetAsEntity(EntityData aEntityData) {
        SetSize(aEntityData.size);
        SetPos(aEntityData.pos);
    }

    public void ResetCursorOnMousePos() {
        SetSize(new Vector2Int(1, 1));
        SetPos(GM.inputManager.mousePosV2);
        SetColor(Color.white);
    }

    public void SetColor(Color aColor) {
        this.myRenderer.color = aColor;
    }

    public void SetVisible(bool aIsVisible) {
        this.myRenderer.enabled = aIsVisible;
    }
}
