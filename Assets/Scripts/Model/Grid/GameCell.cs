﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

// holds all the information regarding one particular cell in the level grid
public class GameCell {
    // set by constructor
    public Vector2Int pos;
    public EntityBase entityBase;
    public Vector2Int push;
    public bool panelVisible;
    public Color panelColor;
    // set by GridViewBase
    public CellViewBase cellViewBase;

    public GameCell(Vector2Int aPos) {
        this.pos = aPos;
        this.entityBase = null;
        this.push = Vector2Int.down;
        this.panelVisible = false;
        this.panelColor = Color.white;
        
    }

    public void RegisterEntity(EntityBase aEntityBase) {
        this.entityBase = aEntityBase;
    }

    public void SetPanelVisibility(bool aPanelVisible) {
        this.panelVisible = aPanelVisible;
        cellViewBase.SetPanelVisbility();
    }

    public void SetColor(Color aColor) {
        this.panelColor = aColor;
        cellViewBase.SetColor();
    }
}
