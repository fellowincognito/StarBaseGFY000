﻿/*
 * StarBase GFY000
 * WallCornerCache
 * Cache for the Wall Corner
 * Created 13 April 2015
 * Author: Fellow Incognito (fellowincognito@gmail.com)
 * SEE LICENSE FOR TERMS AND RIGHTS
 */

using UnityEngine;
using System.Collections;


public class WallCornerCache : BaseCache<WallObject>
{
    protected override WallObject CreateNewObject()
    {
        WallObject wo = (WallObject)GameObject.Instantiate(PrefabAssets.Singleton.prefabWallCorner);
        wo.gameObject.transform.parent = CacheManager.Singleton.gameObject.transform;
        wo.gameObject.SetActive(false);

        return wo;
    }

    public override void Maintenance()
    {
        if (m_updateTimer > Time.time)
            return;

        int initialCount;

        WallObject obj;

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


    ~WallCornerCache()
    {
        Dispose(false);
    }
}

/*
 * StarBase GFY000
 * WallCrossCache
 * Cache for the Wall Cross
 * Created 13 April 2015
 * Author: Fellow Incognito (fellowincognito@gmail.com)
 * SEE LICENSE FOR TERMS AND RIGHTS
 */

public class WallCrossCache : BaseCache<WallObject>
{
    protected override WallObject CreateNewObject()
    {
        WallObject wo = (WallObject)GameObject.Instantiate(PrefabAssets.Singleton.prefabWallCross);
        wo.gameObject.transform.parent = CacheManager.Singleton.gameObject.transform;
        wo.gameObject.SetActive(false);

        return wo;
    }

    public override void Maintenance()
    {
        if (m_updateTimer > Time.time)
            return;

        int initialCount;

        WallObject obj;

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


    ~WallCrossCache()
    {
        Dispose(false);
    }
}

/*
 * StarBase GFY000
 * WallStraightCache
 * Cache for the Wall Straight
 * Created 13 April 2015
 * Author: Fellow Incognito (fellowincognito@gmail.com)
 * SEE LICENSE FOR TERMS AND RIGHTS
 */

public class WallStraightCache : BaseCache<WallObject>
{
    protected override WallObject CreateNewObject()
    {
        WallObject wo = (WallObject)GameObject.Instantiate(PrefabAssets.Singleton.prefabWallStraight);
        wo.gameObject.transform.parent = CacheManager.Singleton.gameObject.transform;
        wo.gameObject.SetActive(false);

        return wo;
    }

    public override void Maintenance()
    {
        if (m_updateTimer > Time.time)
            return;

        int initialCount;

        WallObject obj;

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


    ~WallStraightCache()
    {
        Dispose(false);
    }
}

/*
 * StarBase GFY000
 * WallTCache
 * Cache for the Wall T Section
 * Created 13 April 2015
 * Author: Fellow Incognito (fellowincognito@gmail.com)
 * SEE LICENSE FOR TERMS AND RIGHTS
 */

public class WallTCache : BaseCache<WallObject>
{
    protected override WallObject CreateNewObject()
    {
        WallObject wo = (WallObject)GameObject.Instantiate(PrefabAssets.Singleton.prefabWallT);
        wo.gameObject.transform.parent = CacheManager.Singleton.gameObject.transform;
        wo.gameObject.SetActive(false);

        return wo;
    }

    public override void Maintenance()
    {
        if (m_updateTimer > Time.time)
            return;

        int initialCount;

        WallObject obj;

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


    ~WallTCache()
    {
        Dispose(false);
    }
}
