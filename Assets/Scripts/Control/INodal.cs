using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

// entities with this component can be selected and can stick to other entities
public class INodal : IComponent {
    public HashSet<Vector2Int> upNodes {
        get {
            return this.entityData.upNodes;
        }
        set {
            this.entityData.upNodes = value;
        }
    }
    public HashSet<Vector2Int> downNodes {
        get {
            return this.entityData.downNodes;
        }
        set {
            this.entityData.downNodes = value;
        }
    }
    Dictionary<Vector2Int, GameObject> studDict;
    [Header("Set In Editor")]
    public GameObject studMaster;

    public override void Init() {
        this.studDict = new Dictionary<Vector2Int, GameObject>();
        if (!this.entityData.componentsAreInitialized) {
            GenerateNodes();
        }
        foreach(Vector2Int upNode in this.upNodes) {
            Vector2Int currentPos = this.entityData.pos + upNode;
            Vector3 currentPosition = Util.V2IOffsetV3(currentPos, new Vector2Int(1,1));
            float studX = currentPosition.x;
            float studY = currentPosition.y + (Constants.BLOCKHEIGHT / 2);
            GameObject stud = Instantiate(this.studMaster, new Vector3(studX, studY, 0), Quaternion.identity);
            stud.transform.SetParent(this.entityView.transform, true);
            stud.GetComponent<Renderer>().material.color = this.entityData.defaultColor;
            this.studDict.Add(upNode, stud);
        }
        DrawNodes();
        this.entityView.SetChildRenderers();
        this.entityView.SetColor(this.entityData.defaultColor);
    }

    public override void DoFrame() {
        
    }

    public void DrawNodes() {
        foreach (KeyValuePair<Vector2Int, GameObject> kvp in this.studDict) {
            kvp.Value.SetActive(this.upNodes.Contains(kvp.Key));
        }
    }

    public void GenerateNodes() {
        // generate nodes automatically based on the size of entity
        // if entity is a boundary, nodes pointing outside the level are not added
        bool hasUpNodes = true;
        bool hasDownNodes = true;
        if (this.entityData.isBoundary) {
            if (this.entityData.pos.y + this.entityData.size.y == GM.boardData.size.y) {
                hasUpNodes = false;
            }
            if (this.entityData.pos.y == 0) {
                hasDownNodes = false;
            }
        }
        for (int x = 0; x < this.entityData.size.x; x++) {
            if (hasUpNodes) {
                Vector2Int topPos = new Vector2Int(x, this.entityData.size.y - 1);
                this.upNodes.Add(topPos);
            }

            if (hasDownNodes) {
                Vector2Int botPos = new Vector2Int(x, 0);
                this.downNodes.Add(botPos);
            }
        }
    }
    
    public void AddNode(Vector2Int aPos, bool aUpDown) {
        print("adding node at " + aPos);
        if (aUpDown) {
            this.upNodes.Add(aPos);
        } else {
            this.downNodes.Add(aPos);
        }
        DrawNodes();
    }

    public void RemoveNode(Vector2Int aPos, bool aUpDown) {
        print("removing node at " + aPos);
        if (aUpDown) {
            this.upNodes.Remove(aPos);
        } else {
            this.downNodes.Remove(aPos);
        }
        DrawNodes();
    }

    public HashSet<Vector2Int> GetRelativeNodePosSet(bool aIsUp) {
        if (aIsUp) {
            return this.upNodes;
        } else {
            return this.downNodes;
        }
    }

    public HashSet<Vector2Int> GetAbsoluteNodePosSet(bool aIsUp) {
        HashSet<Vector2Int> absoluteNodePosSet = new HashSet<Vector2Int>();
        HashSet<Vector2Int> relativeNodePosSet = GetRelativeNodePosSet(aIsUp);
        foreach (Vector2Int relativeNodePos in relativeNodePosSet) {
            Vector2Int absoluteNodePos = this.entityData.pos + relativeNodePos;
            absoluteNodePosSet.Add(absoluteNodePos);
        }
        return absoluteNodePosSet;
    }

    public bool HasNodeOnAbsolutePosition(Vector2Int aPos, bool aIsNodePointingUp) {
        foreach (Vector2Int absoluteNodePos in GetAbsoluteNodePosSet(aIsNodePointingUp)) {
            if (absoluteNodePos == aPos) {
                return true;
            }
        }
        return false;
    }
    
    void OnDrawGizmos() {
        if (this.upNodes != null && this.downNodes != null) {
            Vector3 zOffset = new Vector3(0, 0, -1.01f);
            Gizmos.color = Color.red;
            foreach (Vector2Int upNode in this.upNodes) {
                Vector2Int currentPos = this.entityData.pos + upNode;
                Vector3 arrowOrigin = Util.V2IOffsetV3(currentPos, new Vector2Int(1, 1)) + zOffset;
                DrawArrow.I.ForGizmo(arrowOrigin, new Vector3(0, 0.5f, 0));
            }
            Gizmos.color = Color.blue;
            foreach (Vector2Int downNode in this.downNodes) {
                Vector2Int currentPos = this.entityData.pos + downNode;
                Vector3 arrowOrigin = Util.V2IOffsetV3(currentPos, new Vector2Int(1, 1)) + zOffset;
                DrawArrow.I.ForGizmo(arrowOrigin, new Vector3(0, -0.5f, 0));
            }
        }
    }

}
