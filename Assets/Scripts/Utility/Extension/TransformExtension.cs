using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class TransformExtension
{
    /// <summary>
    /// Set's a transform object's values from a Matrix4x4 object.
    /// </summary>
	public static void SetMatrix(this Transform trans, Matrix4x4 matrix)
    {
        trans.localScale = matrix.GetScale();
        trans.rotation = matrix.GetRotation();
        trans.position = matrix.GetPosition();
    }
}
