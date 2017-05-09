using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Matrix4x4Extension
{
    public static Matrix4x4 SwapOrder(this Matrix4x4 matrix)
    {
        Matrix4x4 ret = new Matrix4x4();
        for (int j = 0; j < 16; j++)
            ret[j] = matrix[GetSwappedIndex(j)];
        return ret;
    }

    private static int GetSwappedIndex(int index)
    {
        int row = index % 4;
        int col = index / 4;
        return row * 4 + col;
    }

    public static Quaternion GetRotation(this Matrix4x4 matrix)
    {
        Vector3 forward;
        forward.x = matrix.m02;
        forward.y = matrix.m12;
        forward.z = matrix.m22;

        Vector3 upwards;
        upwards.x = matrix.m01;
        upwards.y = matrix.m11;
        upwards.z = matrix.m21;

        return Quaternion.LookRotation(forward, upwards);
    }

    public static Vector3 GetPosition(this Matrix4x4 matrix)
    {
        Vector3 position;
        position.x = matrix.m03;
        position.y = matrix.m13;
        position.z = matrix.m23;
        return position;
    }

    public static Vector3 GetScale(this Matrix4x4 matrix)
    {
        Vector3 scale;
        scale.x = new Vector4(matrix.m00, matrix.m10, matrix.m20, matrix.m30).magnitude;
        scale.y = new Vector4(matrix.m01, matrix.m11, matrix.m21, matrix.m31).magnitude;
        scale.z = new Vector4(matrix.m02, matrix.m12, matrix.m22, matrix.m32).magnitude;
        return scale;
    }
}
