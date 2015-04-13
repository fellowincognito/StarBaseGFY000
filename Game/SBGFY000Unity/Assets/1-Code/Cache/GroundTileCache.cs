/*
 * StarBase GFY000
 * GroundTileCache
 * Ground tiles
 * Created 12 April 2015
 * Author: Fellow Incognito (fellowincognito@gmail.com)
 * SEE LICENSE FOR TERMS AND RIGHTS
 */

using UnityEngine;
using System.Collections;

public class GroundTileCache : BaseCache<TileObject>
{
    protected override TileObject CreateNewObject()
    {
        TileObject newTile = (TileObject)GameObject.Instantiate(PrefabAssets.Singleton.prefabGroundTile);
        newTile.gameObject.transform.parent = CacheManager.Singleton.gameObject.transform;
        newTile.gameObject.SetActive(false);

        return newTile;
    }

    public override void Maintenance()
    {
        if (m_updateTimer > Time.time)
            return;

        int initialCount;

        TileObject obj;

        //First move some over from the overflow into the cache so we keep it as full as possible
        initialCount = m_capacity - m_cache.Count;

        //Copy initialCount (the difference between target capacity and current cache count) to cache
        //as long as overflow count is > 0
        for (int i = 0; i < initialCount && m_overFlow.Count > 0; i++)
        {
            obj = m_overFlow.Dequeue();
            m_cache.Enqueue(obj);
        }

        //Now destroy 10% of the remaining overflow this frame
        //TODO evaluate if destruction is better off in a CoRoutine
        initialCount = Mathf.Min(m_overFlow.Count,
            (m_overFlow.Count / 10) + 1);
        for (int i = 0; i < initialCount && m_overFlow.Count > 0; i++)
        {
            obj = m_overFlow.Dequeue();

            GameObject.Destroy(obj);
        }

        m_updateTimer = Time.time + m_updateTimerDelta;
    }

    //////////////////////////////////////////////////////////////////////////////
    //IDisposable overrides
    private bool isDisposed = false;

    public override void Dispose()
    {
        base.Dispose();
    }

    protected override void Dispose(bool disposing)
    {
        //DebugLogger.LogWarning(string.Format("Disposing of objects in {0}", this.GetType().FullName), DebugLogger.PrintState.Debug);
        if (!isDisposed)
        {
            if (disposing)
            {
                //Dispose here
                while (m_cache.Count > 0)
                {
                    GameObject.Destroy(m_cache.Dequeue());
                }

                m_cache.Clear();
                m_cache = null;

                while (m_overFlow.Count > 0)
                {
                    GameObject.Destroy(m_overFlow.Dequeue());
                }

                m_overFlow.Clear();
                m_overFlow = null;
            }
        }
        isDisposed = true;
    }


    ~GroundTileCache()
    {
        Dispose(false);
    }
}

public class HologramTileCache : BaseCache<HologramObject>
{
    protected override HologramObject CreateNewObject()
    {
        GameObject newGO = (GameObject)GameObject.Instantiate(PrefabAssets.Singleton.tileObject);
        newGO.renderer.material = new Material(PrefabAssets.Singleton.hologramMat);
        newGO.transform.parent = CacheManager.Singleton.gameObject.transform;
        HologramObject holo = newGO.AddComponent<HologramObject>();
        newGO.SetActive(false);

        return holo;
    }

    public override void Maintenance()
    {
        if (m_updateTimer > Time.time)
            return;

        int initialCount;

        HologramObject obj;

        //First move some over from the overflow into the cache so we keep it as full as possible
        initialCount = m_capacity - m_cache.Count;

        //Copy initialCount (the difference between target capacity and current cache count) to cache
        //as long as overflow count is > 0
        for (int i = 0; i < initialCount && m_overFlow.Count > 0; i++)
        {
            obj = m_overFlow.Dequeue();
            m_cache.Enqueue(obj);
        }

        //Now destroy 10% of the remaining overflow this frame
        //TODO evaluate if destruction is better off in a CoRoutine
        initialCount = Mathf.Min(m_overFlow.Count,
            (m_overFlow.Count / 10) + 1);
        for (int i = 0; i < initialCount && m_overFlow.Count > 0; i++)
        {
            obj = m_overFlow.Dequeue();

            GameObject.Destroy(obj);
        }

        m_updateTimer = Time.time + m_updateTimerDelta;
    }

    //////////////////////////////////////////////////////////////////////////////
    //IDisposable overrides
    private bool isDisposed = false;

    public override void Dispose()
    {
        base.Dispose();
    }

    protected override void Dispose(bool disposing)
    {
        //DebugLogger.LogWarning(string.Format("Disposing of objects in {0}", this.GetType().FullName), DebugLogger.PrintState.Debug);
        if (!isDisposed)
        {
            if (disposing)
            {
                //Dispose here
                while (m_cache.Count > 0)
                {
                    GameObject.Destroy(m_cache.Dequeue());
                }

                m_cache.Clear();
                m_cache = null;

                while (m_overFlow.Count > 0)
                {
                    GameObject.Destroy(m_overFlow.Dequeue());
                }

                m_overFlow.Clear();
                m_overFlow = null;
            }
        }
        isDisposed = true;
    }


    ~HologramTileCache()
    {
        Dispose(false);
    }
}

