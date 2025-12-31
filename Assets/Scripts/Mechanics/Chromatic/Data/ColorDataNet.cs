using System;
using Unity.Netcode;
using Unity.Collections;
using UnityEngine;

[Serializable]
public struct ColorDataNet :
    INetworkSerializable,
    IEquatable<ColorDataNet>
{
    public FixedString32Bytes colorId;
    public float r;
    public float g;
    public float b;
    public float a;
    public float intensity;
    public int meshIndex;

    public ColorDataNet(ColorData so)
    {
        colorId = so.colorId;
        r = so.color.r;
        g = so.color.g;
        b = so.color.b;
        a = so.color.a;
        intensity = so.intensity;
        meshIndex = so.meshIndex;
    }

    public bool Equals(ColorDataNet other)
    {
        return colorId.Equals(other.colorId)
            && r == other.r
            && g == other.g
            && b == other.b
            && a == other.a
            && intensity == other.intensity
            && meshIndex == other.meshIndex;
    }

    public void NetworkSerialize<T>(BufferSerializer<T> serializer)
        where T : IReaderWriter
    {
        serializer.SerializeValue(ref colorId);
        serializer.SerializeValue(ref r);
        serializer.SerializeValue(ref g);
        serializer.SerializeValue(ref b);
        serializer.SerializeValue(ref a);
        serializer.SerializeValue(ref intensity);
        serializer.SerializeValue(ref meshIndex);
    }
}

public static class ColorDataMapper
{
    public static ColorDataNet ToNet(ColorData so)
    {
        return new ColorDataNet
        {
            colorId = so.colorId,
            r = so.color.r,
            g = so.color.g,
            b = so.color.b,
            a = so.color.a,
            intensity = so.intensity,
            meshIndex = so.meshIndex
        };
    }

    public static Color ToUnityColor(ColorDataNet net)
    {
        return new Color(net.r, net.g, net.b, net.a);
    }

    public static float ToUnityIntensity(ColorDataNet net)
    {
        return net.intensity;
    }

    public static string ToUnityColorId(ColorDataNet net)
    {
        return net.colorId.ToString();
    }

    public static float ToUnityMeshIndex(ColorDataNet net)
    {
        return net.meshIndex;
    }
}
