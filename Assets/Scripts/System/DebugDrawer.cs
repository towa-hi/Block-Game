using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

public class DebugDrawer : SerializedMonoBehaviour {
    [SerializeField] List<MarkerInfo> activeMarkerList;

    void Awake() {
        this.activeMarkerList = new List<MarkerInfo>();
    }

    bool isTrue = false;
    void Update() {
        this.isTrue = true;
        if (this.isTrue) {
            this.isTrue = true;
        }
    }

    public void SetMarker(Vector2Int aPos, Color aColor, float aDuration) {
        this.activeMarkerList.Add(new MarkerInfo(aPos, aColor, aDuration));
    }
    
    class MarkerInfo {
        public Vector2Int pos;
        public Color color;
        public float duration;
        public float t;
        
        public MarkerInfo(Vector2Int aPos, Color aColor, float aDuration) {
            this.pos = aPos;
            this.color = aColor;
            this.duration = aDuration;
        }
    }
    
    void OnDrawGizmos() {
        if (this.activeMarkerList == null) {
            return;
        }
        List<MarkerInfo> markerTrashList = new List<MarkerInfo>();
        foreach (MarkerInfo marker in this.activeMarkerList) {
            Gizmos.color = marker.color;
            Gizmos.DrawSphere(Util.V2IOffsetV3(marker.pos, Vector2Int.one) + new Vector3(0, 0, -1f), 0.2f);
            marker.t += Time.deltaTime / marker.duration;
            if (marker.t > 1) {
                markerTrashList.Add(marker);
            }
        }
        foreach (MarkerInfo markerTrash in markerTrashList) {
            this.activeMarkerList.Remove(markerTrash);
        }
    }
}
