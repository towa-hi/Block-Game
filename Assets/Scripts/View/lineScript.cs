using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

public class lineScript : SerializedMonoBehaviour {
    // Start is called before the first frame update
    float age;
    void Start()
    {
        age = 0f;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        age += 1f;
        if (age > 150) {
            Destroy(this.gameObject);
        }
    }
}
