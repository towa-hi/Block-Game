using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

public class EditorColorPicker : SerializedMonoBehaviour {
    public GameObject colorPickerBar;
    public GameObject colorButtonMaster;
    [Header("Set In Editor")]
    public EditPanelBase2 editPanelBase;
    void Awake() {
        List<Color> colorList = new List<Color>();
        colorList.Add(Constants.DEFAULTCOLOR);
        colorList.Add(Color.black);
        colorList.Add(Color.blue);
        colorList.Add(Color.cyan);
        colorList.Add(Color.gray);
        colorList.Add(Color.green);
        colorList.Add(Color.magenta);
        colorList.Add(Color.red);
        colorList.Add(Color.white);
        colorList.Add(Color.yellow);
        foreach (Color color in colorList) {
            EditorColorPickerItem pickerItem = Instantiate(colorButtonMaster, this.colorPickerBar.transform).GetComponent<EditorColorPickerItem>();
            pickerItem.Init(color, this.editPanelBase);
        }
    }
}
