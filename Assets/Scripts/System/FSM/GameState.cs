﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

public interface GameState {
    void Enter();
    void Update();
    void Exit();
}

