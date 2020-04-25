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
    public static Vector3 V2IOffsetV3(Vector2Int aSize, Vector2Int aPos) {
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

    // helper for node stuff to turn bools into vectors
    public static Vector2Int UpOrDown(bool aIsUp) {
        if (aIsUp) {
            return Vector2Int.up;
        } else {
            return  Vector2Int.down;
        }
    }
}
