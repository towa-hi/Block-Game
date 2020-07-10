using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Sirenix.OdinInspector;
using Sirenix.Utilities;

public class EntityBase : MonoBehaviour {
    public bool recievesUpdates;
    public int id;
    public bool isTempPos;
    public GameObject model;
    Renderer modelRenderer;
    Material originalMaterial;
    Material ditheringMaterial;
    HashSet<Renderer> childRenderers;
    EntityState oldEntityState;
    bool needsFirstUpdate;
    static readonly int AlbedoColor = Shader.PropertyToID("_AlbedoColor");

    [SerializeField] public EntityBrain entityBrain;

    [ShowInInspector]
    EntityState entityState {
        get {
            // TODO: make this not throw errors on inspection with Application.isPlaying
            return GM.boardManager.GetEntityById(this.id);
        }
    }

    public bool isMarked;

    #region Lifecycle

    void Awake() {
        this.recievesUpdates = false;
        this.id = Constants.PLACEHOLDERINT;
        this.model = this.transform.GetChild(0).gameObject;
        this.modelRenderer = this.model.GetComponent<Renderer>();
        this.childRenderers = this.model.GetComponentsInChildren<Renderer>().ToHashSet();
        this.needsFirstUpdate = true;
        this.isMarked = false;
        this.originalMaterial = this.modelRenderer.material;
        this.ditheringMaterial = Resources.Load<Material>("Materials/Dithering");
    }

    void OnEnable() {
        GM.boardManager.OnUpdateBoardState += OnUpdateBoardState;
        if (GM.boardManager.currentState.isInitialized) {
            OnUpdateBoardState(GM.boardManager.currentState);
        }
    }

    void OnDisable() {
        GM.boardManager.OnUpdateBoardState -= OnUpdateBoardState;
    }

    void Update() {
        if (this.isMarked) {
            this.markerT += Time.deltaTime / 2.0f;
            if (this.markerT > 1) {
                this.isMarked = false;
                this.markerT = 0f;
            }
        }
    }

    public void Init(EntityState aEntityState) {
        this.id = aEntityState.id;
        this.transform.position = Util.V2IOffsetV3(aEntityState.pos, aEntityState.size, aEntityState.isFront);
        this.name = aEntityState.name;
        foreach (Node node in aEntityState.GetNodes()) {
            if (!node.hasUp) {
                continue;
            }
            Vector3 currentPosition = Util.V2IOffsetV3(node.absolutePos, new Vector2Int(1, 1));
            float studX = currentPosition.x;
            float studY = currentPosition.y + (Constants.BLOCKHEIGHT / 2);
            GameObject stud = Instantiate(GM.instance.studPrefab, new Vector3(studX, studY, 0), Quaternion.identity);
            stud.transform.SetParent(this.model.transform, true);
            Renderer studRenderer = stud.GetComponent<Renderer>();
            studRenderer.material.color = aEntityState.defaultColor;
            this.childRenderers.Add(studRenderer);
        }
        SetColor(aEntityState.defaultColor);
        this.oldEntityState = aEntityState;
        this.recievesUpdates = true;

        this.entityBrain = new EntityBrain(this);
    }

    public void DoFrame() {
        this.entityBrain.DoFrame();
    }

    #endregion

    #region Listeners

    public void OnUpdateBoardState(BoardState aBoardState) {
        if (!this.recievesUpdates) {
            return;
        }
        // when id is -42069, this wont recieve any boardupdates because it hasn't been
        // assigned an ID yet by BoardManager.CreateView
        if (this.id == Constants.PLACEHOLDERINT) {
            return;
        }
        // if first update
        if (this.needsFirstUpdate) {
            // this.oldEntityState = newEntityState;
            // this.needsFirstUpdate = false;
        }
        EntityState newEntityState = aBoardState.entityDict[this.id];
        if (!this.oldEntityState.defaultColor.Equals(newEntityState.defaultColor)) {
            SetColor(newEntityState.defaultColor);
        }
    }

    #endregion

    #region View

    public void SetDithering(bool aIsDithering) {
        if (aIsDithering) {
            foreach (Renderer childRenderer in this.childRenderers) {
                childRenderer.material = this.ditheringMaterial;
                childRenderer.material.SetColor(AlbedoColor, this.entityState.defaultColor);
            }

        }
        else {
            foreach (Renderer childRenderer in this.childRenderers) {
                childRenderer.material = this.originalMaterial;
            }
        }
    }

    public void SetColor(Color aColor) {
        this.modelRenderer.material.color = aColor;
        foreach (Renderer childRenderer in this.childRenderers) {
            childRenderer.material.color = aColor;
        }
    }

    public void SetTempViewPosition(Vector2Int aPos) {
        this.transform.position = Util.V2IOffsetV3(aPos, this.entityState.size, this.entityState.isFront);
        if (!this.isTempPos) {
            SetDithering(true);
        }
        this.isTempPos = true;
    }

    public void ResetView() {
        EntityState currentState = GM.boardManager.GetEntityById(this.id);
        this.transform.position = Util.V2IOffsetV3(currentState.pos, this.entityState.size, this.entityState.isFront);
        if (this.isTempPos) {
            SetDithering(false);
        }
        this.isTempPos = false;

    }

    #endregion

    float markerT;
    float markerDuration = 1f;
    Color markerColor = Color.white;

    public void SetMarker(Color aColor, float aDuration) {
        this.isMarked = true;
        this.markerDuration = aDuration;
        this.markerColor = aColor;

    }

    void OnDrawGizmos() {
        // TODO: figure out why this doesnt draw gizmos for id == 0 for some reason

        if (GM.boardManager != null && GM.boardManager.currentState.entityDict.ContainsKey(this.id)) {
            EntityState currentEntityState = this.entityState;
            if (this.isMarked) {
                this.markerT += Time.deltaTime / this.markerDuration;
            }
            if (this.markerT > 1) {
                this.isMarked = false;
                this.markerT = 0;
            }
            Gizmos.color = this.isMarked ? this.markerColor : Color.white;
            Vector2Int size = this.entityState.size;
            Vector3 position = Util.V2IOffsetV3(this.entityState.pos, size, currentEntityState.isFront);
            Vector3 sizeV3 = new Vector3(size.x, size.y * Constants.BLOCKHEIGHT, 2f);
            if (currentEntityState.isFront) {
                if (currentEntityState.isSuspended) {
                    Gizmos.color = Color.red;
                }
                Gizmos.DrawWireCube(position, sizeV3);
            }
            Vector3 zOffset = new Vector3(0, 0, -1.01f);
            foreach (Node node in currentEntityState.GetNodes()) {
                Vector3 arrowOrigin = Util.V2IOffsetV3(node.absolutePos, new Vector2Int(1, 1)) + zOffset;
                if (node.hasUp) {
                    Gizmos.color = Color.red;
                    Vector3 direction = new Vector3(0f, 0.5f, 0f);
                    DrawArrow.I.ForGizmo(arrowOrigin, direction);
                }
                if (node.hasDown) {
                    Gizmos.color = Color.blue;
                    Vector3 direction = new Vector3(0f, -0.5f, 0f);
                    DrawArrow.I.ForGizmo(arrowOrigin, direction);
                }
            }
        }
    }
}
