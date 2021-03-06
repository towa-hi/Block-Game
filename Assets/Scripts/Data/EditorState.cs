﻿using System.Linq;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Schema;
using UnityEngine;

public struct EditorState {
    public bool isInitialized;
    public bool isFront;
    public EditTabEnum activeTab;
    public List<EntitySchema> frontContentList;
    public List<EntitySchema> backContentList;
    public EntitySchema selectedSchema;
    public bool hasSelectedEntity;
    public int selectedEntityId;

    [SuppressMessage("ReSharper", "ReturnValueOfPureMethodIsNotUsed")]
    public static EditorState CreateEditorState() {
        EditorState newEditorState = new EditorState {
            isInitialized = true,
            isFront = true,
            frontContentList = Resources.LoadAll("ScriptableObjects/Entities", typeof(EntitySchema)).Cast<EntitySchema>().ToList(),
            backContentList = Resources.LoadAll("ScriptableObjects/BGs", typeof(EntitySchema)).Cast<EntitySchema>().ToList(),
            selectedSchema = null,
        };
        // TODO: this probably wont even work
        newEditorState.frontContentList.OrderBy(entitySchema => entitySchema.name.ToLower());
        newEditorState.backContentList.OrderBy(bgSchema => bgSchema.name.ToLower());
        newEditorState.hasSelectedEntity = false;
        newEditorState.selectedEntityId = Constants.PLACEHOLDERINT;
        return newEditorState;
    }

    public static EditorState SetSelectedSchema(EditorState aEditorState, EntitySchema aEntitySchema) {
        aEditorState.selectedSchema = aEntitySchema;
        return aEditorState;
    }

    public static EditorState ClearSelectedSchema(EditorState aEditorState) {
        aEditorState.selectedSchema = null;
        return aEditorState;
    }

    public static EditorState SetSelectedEntityId(EditorState aEditorState, int aId) {
        aEditorState.hasSelectedEntity = true;
        aEditorState.selectedEntityId = aId;
        return aEditorState;
    }

    public static EditorState ClearSelectedEntityId(EditorState aEditorState) {
        aEditorState.hasSelectedEntity = false;
        aEditorState.selectedEntityId = Constants.PLACEHOLDERINT;
        return aEditorState;
    }
    
    public static EditorState SetIsFront(EditorState aEditorState, bool aIsFront) {
        aEditorState.isFront = aIsFront;
        aEditorState.selectedSchema = null;
        aEditorState.hasSelectedEntity = false;
        aEditorState.selectedEntityId = -42069;
        return aEditorState;
    }
    
    public static EditorState SetActiveTab(EditorState aEditorState, EditTabEnum aEditTab) {
        aEditorState.activeTab = aEditTab;
        aEditorState.selectedSchema = null;
        aEditorState.hasSelectedEntity = false;
        aEditorState.selectedEntityId = -42069;
        return aEditorState;
    }

}