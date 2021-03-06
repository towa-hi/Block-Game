﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

public class Util {

    // convert a Vector2Int point to Vector3 point with 0 as z 
    public static Vector3 V2IToV3(Vector2Int aPos, bool aIsFront = true) {
        return new Vector3(aPos.x, aPos.y * (float)Constants.BLOCKHEIGHT, aIsFront ? 0 : Constants.BGZOFFSET);
    }

    // convert a Vector2Int position to corresponding Vector3 position with offset for block size
    public static Vector3 V2IOffsetV3(Vector2Int aPos, Vector2Int aSize, bool aIsFront = true) {
        float newX = aPos.x + (float)aSize.x/2;
        float newY = (aPos.y + (float)aSize.y/2) * Constants.BLOCKHEIGHT;
        float newZ = aIsFront ? 0 : Constants.BGZOFFSET;
        return new Vector3(newX, newY, newZ);
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
        int xBoundLeft = aBoundPos.x;
        int xBoundRight = aBoundPos.x + aBoundSize.x;
        int yBoundDown = aBoundPos.y;
        int yBoundUp = aBoundPos.y + aBoundSize.y;

        int xRectLeft = aRectPos.x;
        int xRectRight = aRectPos.x + aRectSize.x;
        int yRectDown = aRectPos.y;
        int yRectUp = aRectPos.y + aRectSize.y;

        if (xRectLeft < xBoundLeft) {
            return false;
        }
        if (xRectRight > xBoundRight) {
            return false;
        }
        if (yRectDown < yBoundDown) {
            return false;
        }
        if (yRectUp > yBoundUp) {
            return false;
        }
        return true;
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

    public static Vector2Int[] V2IArrayInRect(Vector2Int aOrigin, Vector2Int aSize) {
        Vector2Int[] posArray = new Vector2Int[aSize.x * aSize.y];
        int index = 0;
        for (int y = aOrigin.y; y < aOrigin.y + aSize.y; y++) {
            for (int x = aOrigin.x; x < aOrigin.x + aSize.x; x++) {
                posArray[index] = new Vector2Int(x, y);
                index++;
            }
        }
        return posArray;
    }

    public static IEnumerable<Vector2Int> V2IInRect(Vector2Int aOrigin, Vector2Int aSize) {
        List<Vector2Int> V2IList = new List<Vector2Int>();
        for (int x = aOrigin.x; x < aOrigin.x + aSize.x; x++) {
            for (int y = aOrigin.y; y < aOrigin.y + aSize.y; y++) {
                V2IList.Add(new Vector2Int(x, y));
            }
        }
        // foreach (Vector2Int v2i in V2IList) {
        //     Debug.Log(v2i);
        // }
        return V2IList;
    }

    // will return true on (0, 0)
    public static bool IsDirection (Vector2Int aDirection) {
        if (-1 <= aDirection.x && aDirection.x <= 1) {
            if (-1 <= aDirection.y && aDirection.y <= 1) {
                return true;
            }
        }
        return false;
    }

    public static HashSet<EntityState> ConvertIdSetToEntityStateSet(HashSet<int> aIdSet) {
        HashSet<EntityState> entityStateSet = new HashSet<EntityState>();
        foreach (int id in aIdSet) {
            entityStateSet.Add(GM.boardManager.GetEntityById(id));
        }
        return entityStateSet;
    }

    public static HashSet<int> ConvertEntityStateSetToIdSet(HashSet<EntityState> aEntitySet) {
        HashSet<int> entityIdSet = new HashSet<int>();
        foreach (EntityState entityState in aEntitySet) {
            entityIdSet.Add(entityState.id);
        }
        return entityIdSet;
    }

    // public static Quaternion FacingToQuaternion(Vector2Int aFacing) {
    //     Debug.Assert(IsDirection(aFacing));
    //     Debug.Assert(aFacing.x != 0);
    //     Debug.Assert(aFacing.y == 0);
    //     if (aFacing == Vector2Int.right) {
    //         return new Quaternion();
    //     } else if (aFacing == Vector2Int.left) {
    //         return new Quaternion(0, -1, 0,);
    //     }
    // }

    public static int GetFlatIndexFromPos(int aX, int aY, Vector2Int aSize) {
        return aY * aSize.x + aX;
    }

    public static int GetFlatIndexFromPos(Vector2Int aPos, Vector2Int aSize) {
        return aPos.y * aSize.x + aPos.x;
    }
}
