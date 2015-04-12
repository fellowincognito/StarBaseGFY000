/*
 * StarBase GFY000
 * BaseCacheAbstract
 * Abstract base class for the rest of most caches
 * Created 12 April 2015
 * Author: Fellow Incognito (fellowincognito@gmail.com)
 * SEE LICENSE FOR TERMS AND RIGHTS
 */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public abstract class BaseCache<T> : System.IDisposable
{
    protected Queue<T> m_cache;
    protected Queue<T> m_overFlow;

    protected int m_capacity;
    protected bool m_isInit = false;
    protected float m_updateTimer;
    protected float m_updateTimerDelta;

    #region Simple Get/Sets
    public int GetCapacity() { return m_capacity; }
    public void SetCapacity(int newCap)
    {
        if (newCap > 0)
            m_capacity = newCap;
        else
        {
            Debug.LogError("Trying to set the capacity for " + this.GetType().Name + " below 1, not allowing it. Try: " + newCap);
        }
    }

    public float GetTimerDelta() { return m_updateTimerDelta; }
    public void SetTimerDelta(float newDelta)
    {
        if (newDelta < 0.01f)
        {
            Debug.LogError("Trying to set the timerDelta for " + this.GetType().Name + " below 0.01, not allowing it. Try: " + newDelta);
        }
        else
        {
            m_updateTimerDelta = newDelta;
        }
    }

    public bool IsInitDone() { return m_isInit; }

    public int GetCacheCount() { return m_cache.Count; }
    public int GetOverflowcount() { return m_overFlow.Count; }
    #endregion

    //Create the gameobjects to count
    public virtual void Init(int count, float timerDelta)
    {
        m_capacity = count;

        m_cache = new Queue<T>(m_capacity);

        //Create count many objects for future usage
        for (int i = 0; i < m_capacity; i++)
        {
            T newGO = CreateNewObject();
            m_cache.Enqueue(newGO);
        }

        m_updateTimerDelta = timerDelta;

        m_overFlow = new Queue<T>(Mathf.RoundToInt(count / 10));

        m_isInit = true;
    }

    public virtual T RequestObject()
    {
        T objToReturn;

        int step = 0;

        //Return oldest from the cache queue
        if (m_cache.Count > 0)
        {
            //http://stackoverflow.com/questions/2252259/queuet-dequeue-returns-null
            objToReturn = m_cache.Dequeue();
            step = 1;
        }
        else
        {
            {
                //Check to see if overflow has any, and take it from there
                if (m_overFlow.Count > 0)
                {

                    objToReturn = m_overFlow.Dequeue();
                    step = 2;
                }
                else
                {
                    //Bone dry, so create a new one
                    objToReturn = CreateNewObject();
                    step = 3;
                }
            }
        }

        if (objToReturn == null)
        {
            Debug.LogError("MEssage is null " + step);
        }

        return objToReturn;
    }

    public void ReturnObject(T obj)
    {
        //If we're already at capacity, put it into the overflow queue for eventual deletion
        if (m_cache.Count > m_capacity)
        {
            lock (m_overFlow)
            //using (TimedLock.Lock(m_overFlow))
            {
                m_overFlow.Enqueue(obj);
            }
        }
        else
        {
            lock (m_cache)
            //using (TimedLock.Lock(m_cache))
            {
                m_cache.Enqueue(obj);
            }
        }
    }

    public abstract void Maintenance();

    protected abstract T CreateNewObject();

    public override string ToString()
    {
        string text = string.Format("{0} | Cache: {1} | Overflow: {2} | UpdateDelta: {3} | Cap.: {4}", this.GetType().Name, m_cache.Count, m_overFlow.Count, m_updateTimerDelta, m_capacity);

        return text;
    }

    //////////////////////////////////////////////////////////////////////////////
    //IDisposable overrides
    protected bool isDisposed = false;

    public virtual void Dispose()
    {
        Dispose(true);
        System.GC.SuppressFinalize(this);
    }

    protected abstract void Dispose(bool disposing);

    ~BaseCache()
    {
        Dispose(false);
    }
}

///Abstract class for all gameobject based caches
public abstract class BaseObjectCache : BaseCache<GameObject>
{
    public override void Maintenance()
    {
        if (m_updateTimer > Time.time)
            return;

        int initialCount;

        GameObject obj;

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


    ~BaseObjectCache()
    {
        Dispose(false);
    }
}
