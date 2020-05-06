using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using System.Linq;
using Sirenix.OdinInspector;

[System.Serializable]
public class OnPickerItemClick : UnityEvent<EntitySchema>{};

public class PickerModePanelBase : SerializedMonoBehaviour {
    List<PickerModeItemBase> contentList;
    List<PickerModeItemBase> fullEntityList;
    // set by editor
    public GameObject content;
    public GameObject pickerItemMaster;
    public InputField pickerModeSearchField;
    public OnPickerItemClick pickerItemClick = new OnPickerItemClick();

    // TODO: figure out why clicking on the scroll bar makes everything spaz out
    void Start() {
        this.contentList = new List<PickerModeItemBase>();
        this.fullEntityList = new List<PickerModeItemBase>();
        // load all the entitySchemas in the ScriptableObjects folder
        List<EntitySchema> schemaList = Resources.LoadAll("ScriptableObjects/Entities", typeof(EntitySchema)).Cast<EntitySchema>().ToList();
        // sort by name
        schemaList.OrderBy(entitySchema => entitySchema.name.ToLower());
        // instantiate a pickerItem for each schema
        foreach (EntitySchema entitySchema in schemaList) {
            GameObject pickerItem = Instantiate(pickerItemMaster, content.transform);
            PickerModeItemBase pickerModeItemBase = pickerItem.GetComponent<PickerModeItemBase>();
            pickerModeItemBase.Init(entitySchema, this);
            this.fullEntityList.Add(pickerModeItemBase);
        }
        this.contentList = this.fullEntityList;
    }

    public void OnPickerModeSearchFieldValueChange(string aString) {
        // if empty
        if (aString.Length == 0) {
            // show all
            this.contentList = this.fullEntityList;
        } else {
            // filter fullEntityList
            this.contentList = this.fullEntityList.Where(x => x.text.text.ToLower().Contains(aString.ToLower())).ToList();
        }
        SetContent();
    }
    
    public void OnPickerModeItemClicked(EntitySchema aEntitySchema) {
        pickerItemClick.Invoke(aEntitySchema);
    }

    // hides pickerItems not in contentList
    public void SetContent() {
        foreach(PickerModeItemBase pickerItem in this.fullEntityList) {
            pickerItem.gameObject.SetActive(false);
        }
        foreach(PickerModeItemBase pickerItem in this.contentList) {
            pickerItem.gameObject.SetActive(true);
        }
    }
}
