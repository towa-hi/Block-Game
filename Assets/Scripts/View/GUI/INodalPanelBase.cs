using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using Sirenix.OdinInspector;

public struct NodeToggleStruct {
    public Vector2Int node;
    public bool upDown;
    public bool toggled;

    public NodeToggleStruct(Vector2Int aNode, bool aUpDown, bool aToggled) {
        this.node = aNode;
        this.upDown = aUpDown;
        this.toggled = aToggled;
    }
}

[System.Serializable]
public class OnNodeToggle : UnityEvent<NodeToggleStruct>{};

public class INodalPanelBase : SerializedMonoBehaviour {
    public GameObject topPanel;
    public GameObject botPanel;
    public GameObject iNodalToggleMaster;
    private Image entityBoundsImage;
    public OnNodeToggle onNodeToggle = new OnNodeToggle();

    public void SetEntity(EntityData aEntityData) {
        foreach (Transform toggle in topPanel.transform) {
            Destroy(toggle.gameObject);
        }
        foreach (Transform toggle in botPanel.transform) {
            Destroy(toggle.gameObject);
        }
        if (aEntityData != null) {
            if (!aEntityData.isBoundary) {
                INodal nodal = aEntityData.entityBase.GetCachedIComponent<INodal>() as INodal;
                if (nodal != null) {
                    this.gameObject.SetActive(true);
                    for (int x = 0; x < aEntityData.size.x; x++) {
                        
                        Vector2Int currentTopPos = new Vector2Int(x, aEntityData.size.y - 1);
                        
                        GameObject nodeUpToggle = Instantiate(iNodalToggleMaster, topPanel.transform);
                        nodeUpToggle.GetComponent<Toggle>().onValueChanged.AddListener((value) => processToggle(currentTopPos, true, value));
                        nodeUpToggle.GetComponent<Toggle>().SetIsOnWithoutNotify(nodal.upNodes.Contains(currentTopPos));
                        Vector2Int currentBotPos = new Vector2Int(x, 0);
                        GameObject nodeDownToggle = Instantiate(iNodalToggleMaster, botPanel.transform);
                        nodeDownToggle.GetComponent<Toggle>().onValueChanged.AddListener((value) => processToggle(currentBotPos, false, value));
                        nodeDownToggle.GetComponent<Toggle>().SetIsOnWithoutNotify(nodal.downNodes.Contains(currentBotPos));
                    }
                }
            }
        } else {
            this.gameObject.SetActive(false);
        }
        
    }

    void processToggle(Vector2Int aNode, bool aUpDown, bool aToggled) {
        NodeToggleStruct nodeToggleStruct = new NodeToggleStruct(aNode, aUpDown, aToggled);
        this.onNodeToggle.Invoke(nodeToggleStruct);
    }
}
