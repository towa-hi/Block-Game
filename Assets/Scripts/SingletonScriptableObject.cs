using System.Linq;
using UnityEngine;
using Sirenix.OdinInspector;

// singletons can be called with SingletonName.Instance
public abstract class SingletonScriptableObject<T> : SerializedScriptableObject where T : SerializedScriptableObject {
    static T _instance = null;
    public static T Instance {
        get {
            if (!_instance) {
                _instance = Resources.FindObjectsOfTypeAll<T>().FirstOrDefault();
            }
            return _instance;
        }
    }
}
