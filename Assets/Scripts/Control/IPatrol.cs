using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

public class IPatrol : IComponent {
    Vector3 startPosition;
    Vector3 endPosition;
    Vector2Int destination;
    float t;
    Quaternion oldRotation;
    public StateMachine stateMachine;

    public override void DoFrame() {
    }

    public override void Init() {

    }
}
