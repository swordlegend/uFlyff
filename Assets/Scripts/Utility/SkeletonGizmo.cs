using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkeletonGizmo : MonoBehaviour
{

    private void DrawRayRecursive(Transform t)
    {
        foreach(Transform child in t)
        {
            Gizmos.DrawLine(t.position, child.position);
            DrawRayRecursive(child);
        }
    }

    private void OnDrawGizmos()
    {
        this.DrawRayRecursive(this.transform);
    }
}
