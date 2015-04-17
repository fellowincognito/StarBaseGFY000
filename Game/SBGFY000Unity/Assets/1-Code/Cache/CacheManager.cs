/*
 * StarBase GFY000
 * CacheManager
 * Organizes and manages the cache
 * Created 12 April 2015
 * Author: Fellow Incognito (fellowincognito@gmail.com)
 * SEE LICENSE FOR TERMS AND RIGHTS
 */

using UnityEngine;
using System.Collections;

public class CacheManager : MonoBehaviour
{
    #region Singleton
    protected static CacheManager m_singleton = null;
    public static CacheManager Singleton
    {
        get
        {
            return m_singleton;
        }
    }

    void Awake()
    {
        m_singleton = this;

        //Turn ourself off as soon as we set the singleton as we don't want to potentially call Update() until we're explicitly inited
        this.enabled = false;
    }
    #endregion

    #region Vars
    private float m_updateTimer;
    private float m_updateTimerDelta = 1f / 10f;

    HologramTileCache m_highLightTileCache;
    GroundTileCache m_tileCache;
    WallCornerCache m_wallCornerCache;
    WallCrossCache m_wallCrossCache;
    WallStraightCache m_wallStraightCache;
    WallTCache m_wallTCache;
    InteriorDoorCache m_intDoorCache;
    #endregion

    public void Init()
    {
        m_highLightTileCache = new HologramTileCache();
        m_highLightTileCache.Init(256, (1f / 5f));

        m_tileCache = new GroundTileCache();
        m_tileCache.Init(256, (1f / 5f));

        m_wallCornerCache = new WallCornerCache();
        m_wallCornerCache.Init(64, (1f / 5f));

        m_wallCrossCache = new WallCrossCache();
        m_wallCrossCache.Init(64, (1f / 5f));
        
        m_wallStraightCache = new WallStraightCache();
        m_wallStraightCache.Init(256, (1f / 5f));
        
        m_wallTCache = new WallTCache();
        m_wallTCache.Init(32, (1f / 5f));

        m_intDoorCache = new InteriorDoorCache();
        m_intDoorCache.Init(16, (1f / 5f));

        this.enabled = true;
    }

    void Update()
    {
        //Do the maintenance for each cache
        m_highLightTileCache.Maintenance();
        m_wallCornerCache.Maintenance();
        m_wallCrossCache.Maintenance();
        m_wallStraightCache.Maintenance();
        m_wallTCache.Maintenance();
        m_intDoorCache.Maintenance();

        if (m_updateTimer > Time.time)
            return;

        m_updateTimer = Time.time + m_updateTimerDelta;
    }

    void OnDestroy()
    {
        try
        {
            m_highLightTileCache.Dispose();
            m_wallCornerCache.Dispose();
            m_wallCrossCache.Dispose();
            m_wallStraightCache.Dispose();
            m_wallTCache.Dispose();
            m_intDoorCache.Dispose();
        }
        catch (System.Exception ex)
        {
            Debug.LogWarning(string.Format("Problem in CacheManager OnDestroy(): {0}", ex.Message));
        }
    }

    #region Highlight Tile Management
    public HologramObject RequestHighLightTile()
    {
        return m_highLightTileCache.RequestObject();
    }

    public void ReturnHighLightTile(HologramObject holo)
    {
        holo.tilePos.x = -1;
        holo.tilePos.y = -1;
        holo.gameObject.transform.parent = this.transform;
        holo.gameObject.SetActive(false);

        m_highLightTileCache.ReturnObject(holo);
    }
    #endregion

    #region Ground Tile Management
    public TileObject RequestGroundTile()
    {
        return m_tileCache.RequestObject();
    }

    public void ReturnGroundTile(TileObject tile)
    {
        tile.SetTilePos(new Vec2Int(-1, -1));
        tile.gameObject.transform.parent = this.transform;
        tile.gameObject.SetActive(false);

        m_tileCache.ReturnObject(tile);
    }
    #endregion

    #region Wall Straight Management
    public WallObject RequestWallStraight()
    {
        return m_wallStraightCache.RequestObject();
    }

    public void ReturnWallStraight(WallObject wall)
    {
        wall.SetTilePos(new Vec2Int(-1, -1));
        wall.SetMaterial(PrefabAssets.Singleton.wallHologramMat);
        wall.gameObject.transform.parent = this.transform;
        wall.gameObject.SetActive(false);

        m_wallStraightCache.ReturnObject(wall);
    }
    #endregion

    #region Wall Corner Management
    public WallObject RequestWallCorner()
    {
        return m_wallCornerCache.RequestObject();
    }

    public void ReturnWallCorner(WallObject wall)
    {
        wall.SetTilePos(new Vec2Int(-1, -1));
        wall.SetMaterial(PrefabAssets.Singleton.wallHologramMat);
        wall.gameObject.transform.parent = this.transform;
        wall.gameObject.SetActive(false);

        m_wallCornerCache.ReturnObject(wall);
    }
    #endregion

    #region Wall Cross Management
    public WallObject RequestWallCross()
    {
        return m_wallCrossCache.RequestObject();
    }

    public void ReturnWallCross(WallObject wall)
    {
        wall.SetTilePos(new Vec2Int(-1, -1));
        wall.SetMaterial(PrefabAssets.Singleton.wallHologramMat);
        wall.gameObject.transform.parent = this.transform;
        wall.gameObject.SetActive(false);

        m_wallCrossCache.ReturnObject(wall);
    }
    #endregion

    #region Wall T Management
    public WallObject RequestWallT()
    {
        return m_wallTCache.RequestObject();
    }

    public void ReturnWallT(WallObject wall)
    {
        wall.SetTilePos(new Vec2Int(-1, -1));
        wall.SetMaterial(PrefabAssets.Singleton.wallHologramMat);
        wall.gameObject.transform.parent = this.transform;
        wall.gameObject.SetActive(false);

        m_wallTCache.ReturnObject(wall);
    }
    #endregion

    #region Interior Door Management
    public DoorObject RequestInteriorDoor()
    {
        return m_intDoorCache.RequestObject();
    }

    public void ReturnInteriorDoor(DoorObject door)
    {
        door.SetTilePos(new Vec2Int(-1, -1));
        door.gameObject.transform.parent = this.transform;
        door.gameObject.SetActive(false);

        m_intDoorCache.ReturnObject(door);
    }
    #endregion
}