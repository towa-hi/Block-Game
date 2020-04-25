using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

// entities with this component can be selected and can stick to other entities
public class INodal : IComponent {
    // set by init
    public HashSet<Vector2Int> upNodes;
    public HashSet<Vector2Int> downNodes;

    public override void Init() {
        this.upNodes = new HashSet<Vector2Int>();
        this.downNodes = new HashSet<Vector2Int>();
        GenerateNodes();
    }

    public override void DoFrame() {
        
    }

    public void GenerateNodes() {
        // generate nodes automatically based on the size of entity
        // if entity is fixed and on the boundary of the level, nodes pointing outside the level are not added
        bool hasUpNodes = true;
        bool hasDownNodes = true;
        if (this.entityBase.isFixed) {
            if (this.entityBase.pos.y + this.entityBase.size.y == BoardManager.Instance.levelGrid.size.y) {
                hasUpNodes = false;
            }
            if (this.entityBase.pos.y == 0) {
                hasDownNodes = false;
            }
        }
        for (int x = 0; x < this.entityBase.size.x; x++) {
            if (hasUpNodes) {
                Vector2Int topPos = new Vector2Int(x, this.entityBase.size.y - 1);
                this.upNodes.Add(topPos);
            }

            if (hasDownNodes) {
                Vector2Int botPos = new Vector2Int(x, 0);
                this.downNodes.Add(botPos);
            }
        }
    }
    
    public void UseNodes(HashSet<Vector2Int> aUpNodes, HashSet<Vector2Int> aDownNodes) {
        this.upNodes = aUpNodes;
        this.downNodes = aDownNodes;
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
            Vector2Int absoluteNodePos = this.entityBase.pos + relativeNodePos;
            absoluteNodePosSet.Add(absoluteNodePos);
        }
        return absoluteNodePosSet;
    }

    public bool HasNodeOnThisAbsolutePosition(Vector2Int aPos, bool aIsNodePointingUp) {
        foreach (Vector2Int absoluteNodePos in GetAbsoluteNodePosSet(aIsNodePointingUp)) {
            if (absoluteNodePos == aPos) {
                return true;
            }
        }
        return false;
    }
    
    void OnDrawGizmos() {
        // draw upNodes
        Vector3 zOffset = new Vector3(0, 0, -1.01f);
        Gizmos.color = Color.red;
        foreach (Vector2Int upNode in this.upNodes) {
            Vector2Int currentPos = this.entityBase.pos + upNode;
            Vector3 arrowOrigin = Util.V2IOffsetV3(new Vector2Int(1, 1) ,currentPos) + zOffset;
            DrawArrow.Instance.ForGizmo(arrowOrigin, new Vector3(0, 0.5f, 0));
        }
        Gizmos.color = Color.blue;
        foreach (Vector2Int downNode in this.downNodes) {
            Vector2Int currentPos = this.entityBase.pos + downNode;
            Vector3 arrowOrigin = Util.V2IOffsetV3(new Vector2Int(1, 1) ,currentPos) + zOffset;
            DrawArrow.Instance.ForGizmo(arrowOrigin, new Vector3(0, -0.5f, 0));
        }
        // draw downNodes
        
    }
}
