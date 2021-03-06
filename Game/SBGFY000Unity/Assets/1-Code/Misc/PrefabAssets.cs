﻿/*
 * StarBase GFY000
 * PrefabAssets
 * Maintains the list of the prefabs used by the game
 * Created 10 April 2015
 * Author: Fellow Incognito (fellowincognito@gmail.com)
 * SEE LICENSE FOR TERMS AND RIGHTS
 */

using UnityEngine;
using System.Collections;

public class PrefabAssets : MonoBehaviour 
{
    #region Singleton
    protected static PrefabAssets _singleton = null;
    public static PrefabAssets Singleton
    {
        get
        {
            return _singleton;
        }
    }

    void Awake()
    {
        _singleton = this;
    }
    #endregion

    //Colors
    public Color hologramColor;
    public Color hologramColorError;

    //Game Objects
    public GameObject tileObject;

    public PlayerCharacter prefabPlayerCharacter;
    
    //Props
    public TileObject prefabGroundTile;
    public DoorObject prefabInteriorDoor;
    public WallObject prefabWallStraight;
    public WallObject prefabWallCorner;
    public WallObject prefabWallT;
    public WallObject prefabWallCross;

    public ResourceObject prefabAsteroid;

    //Materials
    public Material hologramMat;
    public Material defaultDiffuse;
    public Material groundGray;
    public Material wallMatGray;
    public Material wallHologramMat;
    public Material doorMatGray;
    public Material doorHologramMat; 
}
