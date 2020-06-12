using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

public class BackgroundScroll : SerializedMonoBehaviour {
    public float speed = 0.2f;
    Renderer myRenderer;
    void Awake() {
        this.myRenderer = GetComponent<Renderer>();
    }
    void Update() {
        Vector2 offset = new Vector2(0,Time.time * this.speed);
        this.myRenderer.material.mainTextureOffset = offset;
    }
}
