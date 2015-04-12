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

    GroundTileCache m_highLightTileCache;
    #endregion

    public void Init()
    {
        m_highLightTileCache = new GroundTileCache();
        m_highLightTileCache.Init(256, (1f / 5f));

        this.enabled = true;
    }

    void Update()
    {
        //Do the maintenance for each cache
        m_highLightTileCache.Maintenance();

        if (m_updateTimer > Time.time)
            return;

        m_updateTimer = Time.time + m_updateTimerDelta;
    }

    void OnDestroy()
    {
        try
        {
            m_highLightTileCache.Dispose();
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
}