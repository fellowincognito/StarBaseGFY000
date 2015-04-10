/*
 * StarBase GFY000
 * CameraManager
 * Has references to and manages all the cameras. Your one stop camera shop.
 * Created 10 April 2015
 * Author: Fellow Incognito (fellowincognito@gmail.com)
 * SEE LICENSE FOR TERMS AND RIGHTS
 */

using UnityEngine;
using System.Collections;

public class CameraManager : MonoBehaviour {

    #region Singleton
    protected static CameraManager m_singleton = null;
    public static CameraManager Singleton
    {
        get
        {
            return m_singleton;
        }
    }

    void Awake()
    {
        m_singleton = this;
    }
    #endregion

    #region Vars
    public WorldOrthoCamera m_worldCamera;
    Coroutine m_lerpCoroutine;
    #endregion

    #region Gets/Sets
    public WorldOrthoCamera worldCamera
    {
        get { return m_worldCamera; }
    }
    #endregion

    public void SetCameraPosition(Vector3 newPosition, bool snapImmediate)
    {
        if (snapImmediate)
        {
            newPosition.y = 0f;
            m_worldCamera.transform.position = newPosition;
        }
        else
        {
            if (m_lerpCoroutine != null)
            {
                StopCoroutine(m_lerpCoroutine);
            }

            m_lerpCoroutine = StartCoroutine(LerpCameraPosition(newPosition, 1f));
        }
    }

    IEnumerator LerpCameraPosition(Vector3 destinationPoint, float lerpTime)
    {
        float timeDelta = 0f;
        float max = lerpTime;

        while (timeDelta < max)
        {
            float perTime = timeDelta / max;

            Vector3 newPos = Vector3.Lerp(m_worldCamera.transform.position, destinationPoint, perTime);
            timeDelta += Time.deltaTime;
            m_worldCamera.transform.position = newPos;
            yield return new WaitForEndOfFrame();
        }
    }
}
