using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

public class Util {

    // convert a Vector2Int point to Vector3 point with 0 as z 
    public static Vector3 V2IToV3(Vector2Int aPos) {
        return new Vector3(aPos.x, aPos.y * (float)Constants.BLOCKHEIGHT, 0f);
    }

    // convert a Vector2Int position to corresponding Vector3 position with offset for block size
    public static Vector3 V2IOffsetV3(Vector2Int aPos, Vector2Int aSize) {
        float newX = (float)aPos.x + (float)aSize.x/2;
        float newY = ((float)aPos.y + (float)aSize.y/2) * (float)Constants.BLOCKHEIGHT;
        return new Vector3(newX, newY, 0);
    }

    // convert a Vector3 to a Vector2Int ignoring z
    public static Vector2Int V3ToV2I(Vector3 aPos) {
        return new Vector2Int((int) Mathf.Floor(aPos.x), (int) Mathf.Floor(aPos.y * 1.0f/Constants.BLOCKHEIGHT));
    }

    //check if a vector2int is within the bounds of two other vector2ints
    public static bool IsInside(Vector2Int aPos, Vector2Int aOrigin, Vector2Int aSize) {
        Rect boundingBox = new Rect(aOrigin, aSize);
        return boundingBox.Contains(aPos);
    }

    public static bool IsRectInside(Vector2Int aRectPos, Vector2Int aRectSize, Vector2Int aBoundPos, Vector2Int aBoundSize) {
        Rect rect = new Rect(aRectPos, aRectSize);
        Rect boundingBox = new Rect(aBoundPos, aBoundSize);
        return boundingBox.Overlaps(rect);
    }
    // helper for node stuff to turn bools into vectors
    public static Vector2Int UpOrDown(bool aIsUp) {
        if (aIsUp) {
            return Vector2Int.up;
        } else {
            return  Vector2Int.down;
        }
    }

    public static void SetLayerRecursively(GameObject aGameObject, int aLayer) {
        if (aGameObject == null) {
            return;
        } else {
            aGameObject.layer = aLayer;

            foreach (Transform child in aGameObject.transform) {
                if (child == null) {
                    continue;
                }
                SetLayerRecursively(child.gameObject, aLayer);
            }
        }
    }
    
    public static List<Vector2Int> V2IInRect(Vector2Int aOrigin, Vector2Int aSize) {
        List<Vector2Int> V2IList = new List<Vector2Int>();
        for (int x = aOrigin.x; x < aOrigin.x + aSize.x; x++) {
            for (int y = aOrigin.y; y < aOrigin.y + aSize.y; y++) {
                V2IList.Add(new Vector2Int(x, y));
            }
        }
        return V2IList;
    }
}
