﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
// ReSharper disable InconsistentNaming

// static class for global constants... wow.
public static class Constants {
    public static readonly int PLACEHOLDERINT = -42069;
    public static readonly int MAXPAR = 99;
    public static readonly int MAXTITLELENGTH = 36;

    public static readonly float BGZOFFSET = 2f;
    public static readonly float BLOCKHEIGHT = 1.25f;
    public static readonly float DRAGTHRESHOLD = 0.2f;
    public static readonly float PUSHSPEED = 1.25f;
    public static readonly float DEATHSTATETIME = 2f;
    public static readonly float GRAVITY = 0.5f;

    public static readonly Vector3 BLOCKTHICCNESS = new Vector3(0, 0, 2.0f);
    public static readonly Vector2Int DEFAULTFACING = new Vector2Int(1, 0);
    public static readonly Color DEFAULTCOLOR = new Color(1f, 1f, 1f, 1f);
    // temp
    public static readonly float SPAWNTIME = 1f;
    public static readonly Vector2Int MAXBOARDSIZE = new Vector2Int(40, 24);
}
