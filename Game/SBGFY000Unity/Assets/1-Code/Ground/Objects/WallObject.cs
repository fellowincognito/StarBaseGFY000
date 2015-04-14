/*
 * StarBase GFY000
 * WallObject
 * A wall object
 * Created 10 April 2015
 * Author: Fellow Incognito (fellowincognito@gmail.com)
 * SEE LICENSE FOR TERMS AND RIGHTS
 */

using UnityEngine;
using System.Collections;

public class WallObject : BaseGroundObject
{    
    public GameObject bottomArt;
    public GameObject topArt; //This piece can be hidden when the player wants to see more

    [SerializeField]
    protected GroundManager.WallType m_wallType;

    public GroundManager.WallType WallType
    {
        get { return m_wallType; }
    }

    // Use this for initialization
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {

    }

    public override void SetMaterial(Material mat)
    {
        base.SetMaterial(mat);

        bottomArt.renderer.material = mat;
        topArt.renderer.material = mat;
    }

    public void SetWallType(GroundManager.WallType wallType)
    {
        m_wallType = wallType;
    }

    public override void AssignToPosition(Vec2Int position, float rot, bool asHologram)
    {
        base.AssignToPosition(position, rot, asHologram);

        if (asHologram)
        {
            topArt.renderer.material = PrefabAssets.Singleton.wallHologramMat;
            bottomArt.renderer.material = PrefabAssets.Singleton.wallHologramMat;
            ToggleCollider(false);
        }
        else
        {
            topArt.renderer.material = PrefabAssets.Singleton.wallMatGray;
            bottomArt.renderer.material = PrefabAssets.Singleton.wallMatGray;
            ToggleCollider(true);
        }
    }

    public override void BuildObject()
    {
        SetMaterial(PrefabAssets.Singleton.wallMatGray);
    }
    
}