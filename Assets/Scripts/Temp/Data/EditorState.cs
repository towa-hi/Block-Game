using System.Linq;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using JetBrains.Annotations;
using UnityEngine;

public struct EditorState {
    public bool isFront;
    public EditTabEnum activeTab;
    public List<EntitySchema> frontContentList;
    public List<BgSchema> backContentList;
    public EntitySchema selectedSchema;
    public bool hasSelectedEntity;
    public int selectedEntityId;
    // public EntityData selectedEntityData;
    // public BgData selectedBgData;
    // public bool isPlacing {
    //     get {
    //         return (this.selectedEntityData != null || this.selectedBgData != null);
    //     }
    // }

    [SuppressMessage("ReSharper", "ReturnValueOfPureMethodIsNotUsed")]
    public static EditorState CreateEditorState() {
        EditorState newEditorState = new EditorState {
            isFront = true,
            frontContentList = Resources.LoadAll("ScriptableObjects/Entities", typeof(EntitySchema)).Cast<EntitySchema>().ToList(),
            backContentList = Resources.LoadAll("ScriptableObjects/Bg", typeof(BgSchema)).Cast<BgSchema>().ToList(),
            selectedSchema = null,
        };
        // TODO: this probably wont even work
        newEditorState.frontContentList.OrderBy(entitySchema => entitySchema.name.ToLower());
        newEditorState.backContentList.OrderBy(bgSchema => bgSchema.name.ToLower());
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

    public static EditorState SetSelectedEntityId(EditorState aEditorState, bool aHasSelectedEntity, int aId = -1) {
        aEditorState.hasSelectedEntity = aHasSelectedEntity;
        aEditorState.selectedEntityId = aId;
        return aEditorState;
    }
    
    public static EditorState ClearSelection(EditorState aEditorState) {
        // aState.selectedEntityData = null;
        // aState.selectedBgData = null;
        return aEditorState;
    }

    public static EditorState SetIsFront(EditorState aEditorState, bool aIsFront) {
        aEditorState.isFront = aIsFront;
        aEditorState.selectedSchema = null;
        // aState.selectedEntityData = null;
        // aState.selectedBgData = null;
        // aState.cursorMode = CursorModeEnum.SELECTING;
        return aEditorState;
    }
    
    public static EditorState SetActiveTab(EditorState aEditorState, EditTabEnum aEditTab) {
        aEditorState.activeTab = aEditTab;
        aEditorState.selectedSchema = null;
        // aState.selectedEntityData = null;
        // aState.selectedBgData = null;
        return aEditorState;
    }

    public static EditorState SetCursorSelect(EditorState aEditorState, CursorModeEnum aCursorMode) {
        // aState.cursorMode = aCursorMode;
        return aEditorState;
    }
}