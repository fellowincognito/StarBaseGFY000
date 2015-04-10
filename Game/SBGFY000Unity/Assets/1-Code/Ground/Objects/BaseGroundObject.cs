/*
 * StarBase GFY000
 * BaseGroundObject
 * The base class for all static, world objects, like floor tiles, and walls
 * Created 10 April 2015
 * Author: Fellow Incognito (fellowincognito@gmail.com)
 * SEE LICENSE FOR TERMS AND RIGHTS
 */

using UnityEngine;
using System.Collections;

public class BaseGroundObject : MonoBehaviour
{
    public GroundManager.GroundTileType tileType;

    public Vector3 defaultOffset = Vector3.zero;

    public float defaultOrientation = 0f;

    protected Vec2Int m_tilePos;

    public Vec2Int tilePos
    {
        get { return m_tilePos; }
    }

    public void SetTilePos(Vec2Int pos)
    {
        m_tilePos = pos;
    }
}