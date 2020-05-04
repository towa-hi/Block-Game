using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

// static class for global constants... wow.
public static class Constants {
    public static float BLOCKHEIGHT = 1.5f;
    public static Vector3 BLOCKTHICCNESS = new Vector3(0, 0, 2.0f);
    public static float DRAGTHRESHOLD = 0.2f;
    public static Vector2Int DEFAULTFACING = new Vector2Int(1, 0);
    public static Color DEFAULTCOLOR = new Color(0.7f, 0.7f, 0.7f);
}
