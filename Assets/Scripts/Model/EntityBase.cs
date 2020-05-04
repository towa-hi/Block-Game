using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

// all entities have this as the base component. holds important gameplay data and entity state
// but doesn't hold graphics data or graphics state
public class EntityBase : SerializedMonoBehaviour {
    // set in init
    public Vector2Int pos;
    public Vector2Int facing;
    public Vector2Int size;
    public EntityTypeEnum type;
    public bool isFixed;
    public bool isBoundary;
    public HashSet<IComponent> iComponentSet;
    public EntityView entityView;
    // TODO: this should be removed later
    public EntityData initialEntityData;
    
    public void Init(EntityData aEntityData) {
        this.pos = aEntityData.pos;
        this.facing = aEntityData.facing;
        this.size = aEntityData.entitySchema.size;
        this.type = aEntityData.entitySchema.type;
        this.isFixed = aEntityData.isFixed;
        this.isBoundary = aEntityData.isBoundary;
        this.iComponentSet = new HashSet<IComponent>();
        this.entityView = this.transform.GetChild(0).GetComponent<EntityView>();
        this.name = GenerateName();
        // remove this later
        this.initialEntityData = aEntityData;
        foreach (IComponent iComponent in GetComponents(typeof(IComponent))) {
            iComponentSet.Add(iComponent);
            iComponent.Init();
        }
        this.entityView.Init(aEntityData);
    }

    string GenerateName() {
        string nameString = this.type.ToString() + " " + this.size;
        if (this.isBoundary) {
            nameString += " (boundary)";
        }
        nameString += this.GetHashCode();
        return nameString;
    }
    public List<Vector2Int> GetOccupiedPos() {
        List<Vector2Int> occupiedPosList = new List<Vector2Int>();
        for (int x = this.pos.x; x < this.pos.x + this.size.x; x++) {
            for (int y = this.pos.y; y < this.pos.y + this.size.y; y++) {
                Vector2Int currentPos = new Vector2Int(x, y);
                occupiedPosList.Add(currentPos);
            }
        }
        return occupiedPosList;
    }

    public IComponent GetCachedIComponent<T>() {
        foreach (IComponent iComponent in this.iComponentSet) {
            if (iComponent.GetType() == typeof(T)) {
                return iComponent;
            }
        }
        return null;
    }

    public void SelfDestruct() {
        print("self destructing goodbye");
    }
}