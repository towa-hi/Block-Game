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
        // load 
        List<EntitySchema> schemaList = Resources.LoadAll("ScriptableObjects/Entities", typeof(EntitySchema)).Cast<EntitySchema>().ToList();
        schemaList.OrderBy(entitySchema => entitySchema.name.ToLower());
        foreach (EntitySchema entitySchema in schemaList) {
            GameObject pickerItem = Instantiate(pickerItemMaster, content.transform);
            PickerItemBase pickerItemBase = pickerItem.GetComponent<PickerItemBase>();
            pickerItemBase.Init(entitySchema);
            this.fullEntityList.Add(pickerItemBase);
        }
        this.contentList = this.fullEntityList;
    }

    public void OnPickerModeSearchFieldValueChange(string aString) {
        print(aString);
        if (aString.Length == 0) {
            // show all
            this.contentList = this.fullEntityList;
        } else {
            this.contentList = fullEntityList.Where(x => x.text.text.ToLower().Contains(aString.ToLower())).ToList();
        }
        SetContent();
    }

    public void SetContent() {
        foreach(PickerItemBase pickerItem in this.fullEntityList) {
            pickerItem.gameObject.SetActive(false);
        }
        foreach(PickerItemBase pickerItem in this.contentList) {
            pickerItem.gameObject.SetActive(true);
        }
    }
}
