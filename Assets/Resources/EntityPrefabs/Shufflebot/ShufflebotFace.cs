using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

public class ShufflebotFace : SerializedMonoBehaviour {
    public EntityBase entityBase;
    public EntityData entityData;
    public Renderer myRenderer;
    public Texture2D leftTex;
    public Texture2D rightTex;
    public Vector2Int facing;

    void Start() {
        this.entityData = this.entityBase.entityData;
        this.facing = Vector2Int.zero;
        this.myRenderer = GetComponent<Renderer>();
    }
    
    private void Update() {
        if (this.entityData.facing != this.facing) {
            if (this.entityData.facing == Vector2Int.left) {
                this.myRenderer.material.SetTexture("_BaseMap", leftTex);
            } else if (this.entityData.facing == Vector2Int.right) {
                this.myRenderer.material.SetTexture("_BaseMap", rightTex);
            }
            this.facing = this.entityData.facing;
        }

    }

    
}
