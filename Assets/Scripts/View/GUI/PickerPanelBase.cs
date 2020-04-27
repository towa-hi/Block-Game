using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Sirenix.OdinInspector;


public class PickerPanelBase : SerializedMonoBehaviour {
    public EntitySchema[] entitySchemaArray;
    // set by editor
    public GameObject content;
    public GameObject pickerItemMaster;
    public List<PickerItemBase> contentList;

    void Start() {
        this.entitySchemaArray = Resources.LoadAll("ScriptableObjects/Entities", typeof(EntitySchema)).Cast<EntitySchema>().ToArray();
        foreach (EntitySchema entitySchema in this.entitySchemaArray) {
            GameObject pickerItem = Instantiate(pickerItemMaster, content.transform);
            PickerItemBase pickerItemBase = pickerItem.GetComponent<PickerItemBase>();
            pickerItemBase.Init(entitySchema);
            this.contentList.Add(pickerItemBase);
        }

    }

}
