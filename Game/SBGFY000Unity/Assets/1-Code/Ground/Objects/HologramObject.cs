/*
 * StarBase GFY000
 * HighlightObject
 * The script that is attached to a gameobject used for a hologram
 * Created 12 April 2015
 * Author: Fellow Incognito (fellowincognito@gmail.com)
 * SEE LICENSE FOR TERMS AND RIGHTS
 */

using UnityEngine;
using System.Collections;

public class HologramObject : MonoBehaviour
{
    public Vec2Int tilePos;

    void Awake()
    {
        tilePos = new Vec2Int(-1, -1);
    }
}