using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

// singletons can be called with SingletonName.Instance
public class Singleton<T> : SerializedMonoBehaviour where T : SerializedMonoBehaviour {
    private static bool shuttingDown = false;
    private static object mlock = new object();
    private static T instance;

    public static T Instance {
        get {
            if (shuttingDown) {
                return null;
            }
            lock (mlock) {
                if (instance == null) {
                    instance = (T)FindObjectOfType(typeof(T));
                    if (instance == null) {
                        var singletonObject = new GameObject();
                        instance = singletonObject.AddComponent<T>();
                        singletonObject.name = typeof(T).ToString() + "(Singleton)";
                        DontDestroyOnLoad(singletonObject);
                    }
                }
                return instance;
            }
        }
    }

    private void OnApplicationQuit() {
        shuttingDown = true;
    }

    public void OnDestroy() {
        shuttingDown = true;
    }
}
