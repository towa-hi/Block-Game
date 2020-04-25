using UnityEngine;
using System.Collections;
using UnityEditor;
using System.Collections.Generic;
    
// debug class that draws an arrow with unity gizmos
// original author is VRARDAJ from forum.unity.com
public class DrawArrow : Singleton<DrawArrow> {
    Dictionary<string, DebugLineData> linesToDraw;
    List<string> keysToUpdate;
    
    private void Awake() {
        linesToDraw = new Dictionary<string, DebugLineData>();
        keysToUpdate = new List<string>();
    }
    
    public void ForGizmo(Vector3 pos, Vector3 direction, float arrowHeadLength = 0.25f, float arrowHeadAngle = 20.0f) {
        Gizmos.DrawRay(pos, direction);
    
        Vector3 right = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 180 + arrowHeadAngle, 0) * new Vector3(0, 0, 1);
        Vector3 left = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 180 - arrowHeadAngle, 0) * new Vector3(0, 0, 1);
        Gizmos.DrawRay(pos + direction, right * arrowHeadLength);
        Gizmos.DrawRay(pos + direction, left * arrowHeadLength);
    }
    
    public void ForGizmo(Vector3 pos, Vector3 direction, Color color, float arrowHeadLength = 0.25f, float arrowHeadAngle = 20.0f) {
        Gizmos.color = color;
        Gizmos.DrawRay(pos, direction);
    
        Vector3 right = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 180 + arrowHeadAngle, 0) * new Vector3(0, 0, 1);
        Vector3 left = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 180 - arrowHeadAngle, 0) * new Vector3(0, 0, 1);
        Gizmos.DrawRay(pos + direction, right * arrowHeadLength);
        Gizmos.DrawRay(pos + direction, left * arrowHeadLength);
    }
    
    public void ForGizmoWithMagnitude(string key, Vector3 pos, Vector3 direction, Color color, float arrowHeadLength = 0.25f, float arrowHeadAngle = 20.0f) {
        DebugLineData lineData = new DebugLineData();
        lineData.drawing = true;
        lineData.pos = pos;
        lineData.direction = direction;
        lineData.color = color;
        lineData.arrowHeadLength = arrowHeadLength;
        lineData.arrowHeadAngle = arrowHeadAngle;
    
        if (!linesToDraw.ContainsKey(key))
            linesToDraw.Add(key, lineData);
        else linesToDraw[key] = lineData;
    }
    
    private void ForGizmoWithMagnitudeInternal(DebugLineData lineData) {
        Gizmos.color = lineData.color;
        Gizmos.DrawRay(lineData.pos, lineData.direction);
        Vector3 right = Quaternion.LookRotation(lineData.direction) * Quaternion.Euler(0, 180 + lineData.arrowHeadAngle, 0) * new Vector3(0, 0, 1);
        Vector3 left = Quaternion.LookRotation(lineData.direction) * Quaternion.Euler(0, 180 - lineData.arrowHeadAngle, 0) * new Vector3(0, 0, 1);
        Gizmos.DrawRay(lineData.pos + lineData.direction, right * lineData.arrowHeadLength);
        Gizmos.DrawRay(lineData.pos + lineData.direction, left * lineData.arrowHeadLength);
        Vector3 midPoint = lineData.pos + 0.5f * lineData.direction + new Vector3(0, 0.1f, 0);
        string magnitude = lineData.direction.magnitude.ToString();
        Handles.Label(midPoint, magnitude);
    }
    
    public void ForDebug(Vector3 pos, Vector3 direction, float arrowHeadLength = 0.25f, float arrowHeadAngle = 20.0f) {
        Debug.DrawRay(pos, direction);
        Vector3 right = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 180 + arrowHeadAngle, 0) * new Vector3(0, 0, 1);
        Vector3 left = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 180 - arrowHeadAngle, 0) * new Vector3(0, 0, 1);
        Debug.DrawRay(pos + direction, right * arrowHeadLength);
        Debug.DrawRay(pos + direction, left * arrowHeadLength);
    }

    public void ForDebug(Vector3 pos, Vector3 direction, Color color, float arrowHeadLength = 0.25f, float arrowHeadAngle = 20.0f) {
        Debug.DrawRay(pos, direction, color);
        Vector3 right = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 180 + arrowHeadAngle, 0) * new Vector3(0, 0, 1);
        Vector3 left = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 180 - arrowHeadAngle, 0) * new Vector3(0, 0, 1);
        Debug.DrawRay(pos + direction, right * arrowHeadLength, color);
        Debug.DrawRay(pos + direction, left * arrowHeadLength, color);
    }
    
    private void OnDrawGizmos() {
        if (linesToDraw != null && linesToDraw.Count > 0) {
            foreach (KeyValuePair<string, DebugLineData> lineDataPair in linesToDraw)
                if (lineDataPair.Value.drawing) {
                    ForGizmoWithMagnitudeInternal(lineDataPair.Value);
                    keysToUpdate.Add(lineDataPair.Key);
                }
            for (int i = 0; i < keysToUpdate.Count; i++) {
                DebugLineData lineData = linesToDraw[keysToUpdate[i]];
                lineData.drawing = false;
                linesToDraw[keysToUpdate[i]] = lineData;
            }
            if (keysToUpdate.Count > 0) {
                keysToUpdate.Clear();
            }
        }
    }
}
    
public struct DebugLineData {
    public bool drawing;
    public Vector3 pos;
    public Vector3 direction;
    public Color color;
    public float arrowHeadLength;
    public float arrowHeadAngle;
}
    
