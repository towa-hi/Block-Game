using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using Sirenix.OdinInspector;

public class PickerModePanelBase : SerializedMonoBehaviour {
    public List<PickerItemBase> contentList;
    public List<PickerItemBase> fullEntityList;
    // set by editor
    public GameObject content;
    public GameObject pickerItemMaster;
    public InputField pickerModeSearchField;

    // TODO: figure out why clicking on the scroll bar makes everything spaz out
    void Start() {
        // load all the entitySchemas in the ScriptableObjects folder
        List<EntitySchema> schemaList = Resources.LoadAll("ScriptableObjects/Entities", typeof(EntitySchema)).Cast<EntitySchema>().ToList();
        // sort by name
        schemaList.OrderBy(entitySchema => entitySchema.name.ToLower());
        // instantiate a pickerItem for each schema
        foreach (EntitySchema entitySchema in schemaList) {
            GameObject pickerItem = Instantiate(pickerItemMaster, content.transform);
            PickerItemBase pickerItemBase = pickerItem.GetComponent<PickerItemBase>();
            pickerItemBase.Init(entitySchema);
            this.fullEntityList.Add(pickerItemBase);
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
    
    // hides pickerItems not in contentList
    public void SetContent() {
        foreach(PickerItemBase pickerItem in this.fullEntityList) {
            pickerItem.gameObject.SetActive(false);
        }
        foreach(PickerItemBase pickerItem in this.contentList) {
            pickerItem.gameObject.SetActive(true);
        }
    }
}
