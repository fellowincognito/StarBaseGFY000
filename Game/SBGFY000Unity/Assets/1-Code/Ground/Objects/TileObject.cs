/*
 * StarBase GFY000
 * TileObject
 * A ground tile
 * Created 10 April 2015
 * Author: Fellow Incognito (fellowincognito@gmail.com)
 * SEE LICENSE FOR TERMS AND RIGHTS
 */

using UnityEngine;
using System.Collections;

public class TileObject : BaseGroundObject
{
    public GameObject tileArt;

    // Use this for initialization
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {

    }

    public override void AssignToPosition(Vec2Int position, float rot, bool asHologram)
    {
        base.AssignToPosition(position, rot, asHologram);

        if (asHologram)
        {
            tileArt.renderer.material = PrefabAssets.Singleton.hologramMat;
            ToggleCollider(false);
        }
        else
        {
            ToggleCollider(true);
        }
    }

    public override void SetMaterial(Material mat)
    {
        base.SetMaterial(mat);

        tileArt.renderer.material = mat;
    }

    public override void BuildObject()
    {
        SetMaterial(PrefabAssets.Singleton.groundGray);
    }
}