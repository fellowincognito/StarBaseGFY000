/*
 * StarBase GFY000
 * GroundManager
 * The organizer and main driving force behind the ground tiles and all associated functionality
 * Created 10 April 2015
 * Author: Fellow Incognito (fellowincognito@gmail.com)
 * SEE LICENSE FOR TERMS AND RIGHTS
 */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GroundManager : MonoBehaviour
{
    #region Singleton
    protected static GroundManager _singleton = null;
    public static GroundManager Singleton
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

    //Used to determine general presence and whether passable
    //Individual or transitional definitions are declared elsewhere
    public enum GroundTileType
    {
        None = 0, //Empty Space
        Ground,
        Wall,
        Door,
        Teleporter,
        Resource
    }

    #region Vars
    public GameObject debugCheckerGO;

    ushort[] m_groundTiles;
    Vec2Int m_groundTileDim;

    bool m_doHighlight;
    Vec2Int m_highLightStartPoint;
    Vec2Int m_highLightWidthHeight;

    bool m_doTempHighLight;
    Vec2Int m_tempLightStartPoint;
    Vec2Int m_tempLightEndPoint;

    List<HologramObject> m_hologramTiles;
    #endregion

    #region Gets/Sets
    public Vec2Int GroundSize
    {
        get { return m_groundTileDim; }
    }
    #endregion

    #region Simple Helpers
    /// <summary>
    /// Converts a world position (Vector3) to a tile coordinate
    /// </summary>
    /// <param name="singlePoint"></param>
    /// <returns></returns>
    public Vec2Int ConvertWorldPositionToTile(Vector3 singlePoint)
    {
        int x = (int)(singlePoint.x);
        int z = (int)(singlePoint.z);

        return new Vec2Int(x, z);
    }

    /// <summary>
    /// Gets a tile data based on tile position x,z
    /// </summary>
    /// <param name="tilePos"></param>
    /// <returns></returns>
    public ushort GetTileType(Vec2Int tilePos)
    {
        int index = (tilePos.x * m_groundTileDim.x) + tilePos.y;

        return m_groundTiles[index];
    }
    #endregion

    public void Init(Vec2Int tileDim)
    {
        m_groundTileDim = tileDim;
        m_groundTiles = new ushort[m_groundTileDim.x * m_groundTileDim.y];
        m_hologramTiles = new List<HologramObject>();

        SetUpDebug();
    }

    public void SetTiles(Vector3 startPointWorld, Vector3 endPointWorld, WorldOrthoCamera.SelectionType selectType)
    {
        m_doHighlight = true;

        int xT = (int)(startPointWorld.x);
        int zT = (int)(startPointWorld.z);

        int x2T = (int)(endPointWorld.x);
        int z2T = (int)(endPointWorld.z);

        m_highLightStartPoint.x = Mathf.Min(xT, x2T);
        m_highLightStartPoint.y = Mathf.Min(zT, z2T);

        int maxX = Mathf.Max(xT, x2T);
        int maxZ = Mathf.Max(zT, z2T);

        m_highLightWidthHeight.x = maxX - m_highLightStartPoint.x + 1;
        m_highLightWidthHeight.y = maxZ - m_highLightStartPoint.y + 1;
    }

    public void SetTempHighlight(Vector3 tempStart, Vector3 tempEnd)
    {
        m_doTempHighLight = true;

        int xT = (int)(tempStart.x);
        int zT = (int)(tempStart.z);

        int x2T = (int)(tempEnd.x);
        int z2T = (int)(tempEnd.z);

        Vec2Int prevStartPoint = m_tempLightStartPoint;
        Vec2Int prevEndPoint = m_tempLightEndPoint;

        m_tempLightStartPoint.x = Mathf.Min(xT, x2T);
        m_tempLightStartPoint.y = Mathf.Min(zT, z2T);

        int maxX = Mathf.Max(xT, x2T);
        int maxZ = Mathf.Max(zT, z2T);

        m_tempLightEndPoint.x = maxX - m_tempLightStartPoint.x + 1;
        m_tempLightEndPoint.y = maxZ - m_tempLightStartPoint.y + 1;

        //Experiment with the highlight tiles
        //Only do it if one or more of the values are different than the previous frame
        if(prevStartPoint.x != m_tempLightStartPoint.x || prevStartPoint.y != m_tempLightStartPoint.y || prevEndPoint.x != m_tempLightEndPoint.x || prevEndPoint.y != m_tempLightEndPoint.y)
        {
            Debug.Log(" m_tempLightStartPoint " + m_tempLightStartPoint + " m_tempLightEndPoint " + m_tempLightEndPoint);
            for (int i = 0; i < m_hologramTiles.Count; i++)
            {
                CacheManager.Singleton.ReturnHighLightTile(m_hologramTiles[i]);
            }
            m_hologramTiles.Clear();

            for (int x = 0; x < m_tempLightEndPoint.x; x++)
            {
                for (int z = 0; z < m_tempLightEndPoint.y; z++)
                {
                    HologramObject holo = CacheManager.Singleton.RequestHighLightTile();
                    holo.tilePos.x = x + m_tempLightStartPoint.x;
                    holo.tilePos.y = z + m_tempLightStartPoint.y;

                    holo.gameObject.transform.position = new Vector3(holo.tilePos.x, 0, holo.tilePos.y);
                    holo.gameObject.SetActive(true);

                    m_hologramTiles.Add(holo);
                }
            }
        }        
    }

    #region Gizmos/Debug
    void SetUpDebug()
    {
        if (debugCheckerGO == null)
            return;

        debugCheckerGO.transform.localScale = new Vector3(m_groundTileDim.x, m_groundTileDim.y, 1f);
        debugCheckerGO.transform.localPosition = new Vector3(m_groundTileDim.x * 0.5f, -0.001f, m_groundTileDim.y * 0.5f);
        debugCheckerGO.renderer.material.SetTextureScale("_MainTex", new Vector2(m_groundTileDim.x * 0.5f, m_groundTileDim.y * 0.5f));
    }

    void OnDrawGizmos()
    {
        if (m_doHighlight)
        {
            Vector3 point1b = new Vector3(
                m_highLightStartPoint.x,
                0,
                m_highLightStartPoint.y);

            Vector3 point2b = new Vector3(
                m_highLightStartPoint.x + m_highLightWidthHeight.x,
                0,
                m_highLightStartPoint.y);

            Vector3 point3b = new Vector3(
                m_highLightStartPoint.x + m_highLightWidthHeight.x,
                0,
                m_highLightStartPoint.y + m_highLightWidthHeight.y);

            Vector3 point4b = new Vector3(
                m_highLightStartPoint.x,
                0,
                m_highLightStartPoint.y + m_highLightWidthHeight.y);

            Debug.DrawLine(point1b, point2b, Color.red);
            Debug.DrawLine(point2b, point3b, Color.red);
            Debug.DrawLine(point3b, point4b, Color.red);
            Debug.DrawLine(point4b, point1b, Color.red);
        }

        if (m_doTempHighLight)
        {
            Vector3 point1b = new Vector3(
                m_tempLightStartPoint.x,
                0,
                m_tempLightStartPoint.y);

            Vector3 point2b = new Vector3(
                m_tempLightStartPoint.x + m_tempLightEndPoint.x,
                0,
                m_tempLightStartPoint.y);

            Vector3 point3b = new Vector3(
                m_tempLightStartPoint.x + m_tempLightEndPoint.x,
                0,
                m_tempLightStartPoint.y + m_tempLightEndPoint.y);

            Vector3 point4b = new Vector3(
                m_tempLightStartPoint.x,
                0,
                m_tempLightStartPoint.y + m_tempLightEndPoint.y);

            Debug.DrawLine(point1b, point2b, Color.yellow);
            Debug.DrawLine(point2b, point3b, Color.yellow);
            Debug.DrawLine(point3b, point4b, Color.yellow);
            Debug.DrawLine(point4b, point1b, Color.yellow);
        }

        if (m_groundTiles != null)
        {
            Debug.DrawLine(new Vector3(0, 0f, 0), new Vector3(0, 0f, m_groundTileDim.y), Color.magenta);
            Debug.DrawLine(new Vector3(0, 0f, m_groundTileDim.y), new Vector3(m_groundTileDim.x, 0f, m_groundTileDim.y), Color.magenta);
            Debug.DrawLine(new Vector3(m_groundTileDim.x, 0f, m_groundTileDim.y), new Vector3(m_groundTileDim.x, 0f, 0), Color.magenta);
            Debug.DrawLine(new Vector3(m_groundTileDim.x, 0f, 0), new Vector3(0, 0f, 0), Color.magenta);
        }
    }
    #endregion
}