using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public static class BinaryReaderExtension
{
    /// <summary>
    /// Skips the passed in amount of bytes in the byte stream
    /// </summary>
    public static void Skip(this BinaryReader reader, int numBytes)
    {
        reader.ReadBytes(numBytes);
    }

    /// <summary>
    /// Reads two float32s from the stream and stores them in a Vector2 object.
    /// </summary>
    public static Vector2 ReadVector2(this BinaryReader reader)
    {
        Vector2 ret = Vector2.zero;
        ret.x = reader.ReadSingle();
        ret.y = reader.ReadSingle();
        return ret;
    }

    /// <summary>
    /// Reads three float32s from the stream and stores them in a Vector4 object.
    /// </summary>
    public static Vector3 ReadVector3(this BinaryReader reader)
    {
        Vector3 ret = Vector3.zero;
        ret.x = reader.ReadSingle();
        ret.y = reader.ReadSingle();
        ret.z = reader.ReadSingle();
        return ret;
    }

    /// <summary>
    /// Reads four float32s from the stream and stores them in a Vector4 object.
    /// </summary>
    public static Vector3 ReadVector4(this BinaryReader reader)
    {
        Vector4 ret = Vector4.zero;
        ret.x = reader.ReadSingle();
        ret.y = reader.ReadSingle();
        ret.z = reader.ReadSingle();
        ret.w = reader.ReadSingle();
        return ret;
    }

    /// <summary>
    /// Reads four float32s from the stream and stores them in a Quaternion object.
    /// </summary>
    public static Quaternion ReadQuaternion(this BinaryReader reader)
    {
        Quaternion quat = new Quaternion();
        quat.x = reader.ReadSingle();
        quat.y = reader.ReadSingle();
        quat.z = reader.ReadSingle();
        quat.w = reader.ReadSingle();
        return quat;
    }

    /// <summary>
    /// Reads sixteen float32s from the stream and stores them in a Matrix4x4 object. The default
    /// order of the read in matrix is row major, but can be swapped to column major by passing 
    /// true to the default parameter of swapOrder
    /// </summary>
    public static Matrix4x4 ReadMatrix4x4(this BinaryReader reader, bool swapOrder = false)
    {
        Matrix4x4 ret = Matrix4x4.zero;
        for(int x = 0; x < 4; x++)
            for(int y = 0; y < 4; y++)
            {
                ret[x, y] = reader.ReadSingle();
            }

        // this could possibly be optimized by changing the loop above 
        if (swapOrder)
            return ret.SwapOrder();
        else return ret;
    }
}
