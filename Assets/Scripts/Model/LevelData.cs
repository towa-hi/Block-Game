﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

// holds mutable characteristics of the level like attempts
public class LevelData {
    public LevelSchema levelSchema;
    public int attempts;

    public LevelData(LevelSchema aLevelSchema, int aAttempts) {
        this.levelSchema = aLevelSchema;
        this.attempts = aAttempts;
    }
}