using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

public class NodeToggle {
    public Vector2Int node;
    public bool isUp;
    public bool enabled;

    public NodeToggle(Vector2Int aNode, bool aIsUp, bool aEnabled) {
        this.node = aNode;
        this.isUp = aIsUp;
        this.enabled = aEnabled;
    }
}

public class GUINodeEditor : SerializedMonoBehaviour {
    public GameObject topPanel;
    public GameObject botPanel;
    public GameObject nodeTogglePrefab;

    // TODO: finish this
    public void SetEntity(EntityState aEntityState) {
        Debug.Assert(aEntityState.hasNodes);
        
    }
}
