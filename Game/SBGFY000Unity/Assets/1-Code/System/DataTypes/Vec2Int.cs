/*
 * StarBase GFY000
 * Vec2Int
 * A grouping of two ints, functions similar to Vector2, but uses discrete integers for all those cases when Vector2s won't cut it
 * Created 10 April 2015
 * Author: Fellow Incognito (fellowincognito@gmail.com)
 * SEE LICENSE FOR TERMS AND RIGHTS
 */

using UnityEngine;
using System.Collections;

//Useful to store various data as ints, especially when it comes to the image stuff
[System.Serializable]
public struct Vec2Int
{
    public int x;
    public int y;

    public static Vec2Int Zero = new Vec2Int(0, 0);

    public Vec2UShort ToVec2UShort()
    {
        return new Vec2UShort((ushort)x, (ushort)y);
    }

    public Vec2Int(int newX)
    {
        x = newX;
        y = 0;
    }

    public Vec2Int(int newX, int newY)
    {
        x = newX;
        y = newY;
    }

    public Vec2Int(Vector2 vec2)
    {
        x = (int)vec2.x;
        y = (int)vec2.y;
    }

    public Vec2Int(Vector3 vec3)
    {
        x = Mathf.RoundToInt(vec3.x);
        y = Mathf.RoundToInt(vec3.y);
    }

    public static bool operator ==(Vec2Int m, Vec2Int n)
    {
        if (m.x == n.x && m.y == n.y)
            return true;

        return false;
    }

    public static bool operator !=(Vec2Int m, Vec2Int n)
    {
        if (m.x != n.x || m.y != n.y)
            return true;

        return false;
    }

    public static Vec2Int operator +(Vec2Int a, Vec2Int b)
    {
        return new Vec2Int(a.x + b.x, a.y + b.y);
    }

    public static Vec2Int operator -(Vec2Int a, Vec2Int b)
    {
        return new Vec2Int(a.x - b.x, a.y - b.y);
    }

    // Override the Object.Equals(object o) method:
    public override bool Equals(object o)
    {
        try
        {
            return (bool)(this == (Vec2Int)o);
        }
        catch
        {
            return false;
        }
    }

    // Override the Object.GetHashCode() method:
    public override int GetHashCode()
    {
        return x + y;
    }

    public override string ToString()
    {
        return string.Format("x: {0}, y: {1}", x, y);
    }

    public static Vec2Int operator *(Vec2Int m, int v)
    {
        return new Vec2Int(m.x * v, m.y * v);
    }

    public static Vec2Int operator *(Vec2Int m, float v)
    {
        return new Vec2Int(Mathf.RoundToInt(m.x * v), Mathf.RoundToInt(m.y * v));
    }

}

/*
 * Vec2UShort
*/

public struct Vec2UShort
{
    public ushort x;
    public ushort y;

    public static Vec2UShort Zero = new Vec2UShort(0, 0);

    public Vec2UShort(ushort newX)
    {
        x = newX;
        y = 0;
    }

    public Vec2UShort(ushort newX, ushort newY)
    {
        x = newX;
        y = newY;
    }

    public Vec2UShort(int newX, int newY)
    {
        x = (ushort)newX;
        y = (ushort)newY;
    }

    public Vec2UShort(float newX, float newY)
    {
        x = (ushort)newX;
        y = (ushort)newY;
    }

    public Vec2Int ToVec2Int()
    {
        return new Vec2Int((int)x, (int)y);
    }

    public override string ToString()
    {
        return string.Format("x: {0}, y: {1}", x, y);
    }

    public static Vec2UShort operator *(Vec2UShort m, int v)
    {
        return new Vec2UShort(m.x * v, m.y * v);
    }

    public static Vec2UShort operator *(Vec2UShort m, float v)
    {
        return new Vec2UShort((ushort)Mathf.RoundToInt(m.x * v), (ushort)Mathf.RoundToInt(m.y * v));
    }
}