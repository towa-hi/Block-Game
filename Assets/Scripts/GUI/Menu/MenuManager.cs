using Sirenix.OdinInspector;
using UnityEngine;

namespace Menu {

public class MenuManager : SerializedMonoBehaviour {

    public void OnStartGameButtonClick() {
        print("start button clicked");
    }

    public void OnEditorButtonClick() {
        print("edit button clicked");
    }

    public void OnOptionsButtonClick() {
        print("options button clicked");
    }

    public void OnExitButtonClick() {
        print("exit button clicked");
        Application.Quit();
    }
}

}
