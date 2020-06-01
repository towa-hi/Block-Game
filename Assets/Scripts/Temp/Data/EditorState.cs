using System.Linq;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using UnityEngine;

public struct EditorState {
    public bool isFront;
    public EditTabEnum activeTab;
    public List<EntitySchema> frontContentList;
    public List<BgSchema> backContentList;
    public Object selectedSchema;
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

    public static EditorState SetSelectedSchema(EditorState aState, EntitySchema aEntitySchema) {
        aState.selectedSchema = aEntitySchema;
        return aState;
    }

    public static EditorState ClearSelectedSchema(EditorState aState) {
        aState.selectedSchema = null;
        return aState;
    }

    // public static EditorState SetSelectionToMousePos(EditorState aState) {
    //     // if (aState.isFront) {
    //     //     aState.selectedEntityData = GM.boardData.GetEntityDataAtPos(GM.inputManager.mousePosV2);
    //     // } else {
    //     //     aState.selectedBgData = GM.boardData.backgroundData.GetBgDataAtPos(GM.inputManager.mousePosV2);
    //     // }
    //     return aState;
    // }

    public static EditorState ClearSelection(EditorState aState) {
        // aState.selectedEntityData = null;
        // aState.selectedBgData = null;
        return aState;
    }

    public static EditorState SetIsFront(EditorState aState, bool aIsFront) {
        aState.isFront = aIsFront;
        aState.selectedSchema = null;
        // aState.selectedEntityData = null;
        // aState.selectedBgData = null;
        // aState.cursorMode = CursorModeEnum.SELECTING;
        return aState;
    }
    
    public static EditorState SetActiveTab(EditorState aState, EditTabEnum aEditTab) {
        aState.activeTab = aEditTab;
        aState.selectedSchema = null;
        // aState.selectedEntityData = null;
        // aState.selectedBgData = null;
        return aState;
    }

    public static EditorState SetCursorSelect(EditorState aState, CursorModeEnum aCursorMode) {
        // aState.cursorMode = aCursorMode;
        return aState;
    }
}