/*
 * StarBase GFY000
 * GroundManager_Visuals
 * The main visual component for the GroundManager data
 * Created 16 April 2015
 * Author: Fellow Incognito (fellowincognito@gmail.com)
 * SEE LICENSE FOR TERMS AND RIGHTS
 */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GroundManager_Visuals : MonoBehaviour
{
    #region Vars
    GroundManager.GroundData m_associatedGroundData;
    Vec2Int m_groundTileDim;

    BaseGroundObject[] m_groundTilesGOs;

    bool m_doHighlight;
    Vec2Int m_highLightStartPoint;
    Vec2Int m_highLightWidthHeight;

    bool m_doTempHighLight;
    Vec2Int m_tempLightStartPoint;
    Vec2Int m_tempLightWidthHeight;

    [SerializeField]
    List<HologramObject> m_tempSelectHolograms;
    [SerializeField]
    List<HologramObject> m_selectedHolograms;
    List<Vec2Int> m_tempSelectedTiles;
    List<Vec2Int> m_selectedTiles;

    protected BaseGroundObject m_objectHologram;
    #endregion

    #region Gets/Sets
    public Vec2Int HighlightedOrigin
    {
        get { return m_highLightStartPoint; }
    }

    public Vec2Int HighlightedWidthHeight
    {
        get { return m_highLightWidthHeight; }
    }

    public Vec2Int CurrentHighlightOrigin
    {
        get { return m_tempLightStartPoint; }
    }

    public Vec2Int CurrentHighlightWidthHeight
    {
        get { return m_tempLightWidthHeight; }
    }

    public List<Vec2Int> SelectedTiles
    {
        get { return m_selectedTiles; }
    }

    public BaseGroundObject GetTileGO(int index)
    {
        if (index < 0 || index > m_groundTilesGOs.Length - 1)
        {
            return null;
        }

        return m_groundTilesGOs[index];
    }

    public void SetTileGO(int index, BaseGroundObject newObject)
    {
        m_groundTilesGOs[index] = newObject;
    }
    #endregion

    public void Init(Vec2Int dim, GroundManager.GroundData groundData)
    {
        m_groundTileDim = dim;
        m_associatedGroundData = groundData;

        m_groundTilesGOs = new BaseGroundObject[m_groundTileDim.x * m_groundTileDim.y];
        m_tempSelectHolograms = new List<HologramObject>();
        m_selectedHolograms = new List<HologramObject>();

        m_tempSelectedTiles = new List<Vec2Int>();
        m_selectedTiles = new List<Vec2Int>();

        SetUpDebug();
    }

    void SetUpDebug()
    {
        if (GroundManager.Singleton.debugCheckerGO == null)
            return;

        GroundManager.Singleton.debugCheckerGO.transform.localScale = new Vector3(m_groundTileDim.x, m_groundTileDim.y, 1f);
        GroundManager.Singleton.debugCheckerGO.transform.localPosition = new Vector3(m_groundTileDim.x * 0.5f, -0.001f, m_groundTileDim.y * 0.5f);
        GroundManager.Singleton.debugCheckerGO.renderer.material.SetTextureScale("_MainTex", new Vector2(m_groundTileDim.x * 0.5f, m_groundTileDim.y * 0.5f));
    }

    public void ClearSelection()
    {
        m_highLightStartPoint = m_highLightWidthHeight = Vec2Int.Zero;
        ReturnSelectedHolograms();
        ReturnTempHolograms();
    }

    public void UpdateGraphicalTile(int tileIndex, Vec2Int xz, int neighborIndex)
    {
        TileObject tilePiece = CacheManager.Singleton.RequestGroundTile();
        //tilePiece.transform.parent = this.transform;
        //tilePiece.transform.position = new Vector3(xz.x + tilePiece.defaultOffset.x, tilePiece.defaultOffset.y, xz.y + tilePiece.defaultOffset.z);
        //tilePiece.gameObject.SetActive(true);

        tilePiece.AssignToPosition(xz, 0f, true);

        SetTileGO(tileIndex, tilePiece);
    }

    /// <summary>
    /// Useful for wall tiles
    /// </summary>
    /// <param name="tileIndex"></param>
    /// <param name="xz"></param>
    /// <param name="neighborIndex"></param>
    /// <param name="wallType"></param>
    public void UpdateGraphicalTile(int tileIndex, Vec2Int xz, int neighborIndex, GroundManager.WallType wallType)
    {
        WallObject wallPiece = null; //m_tilePos
        float rot = 0f;

        if (m_groundTilesGOs[tileIndex] == null)
        {

            if (wallType == GroundManager.WallType.Corner)
            {
                wallPiece = CacheManager.Singleton.RequestWallCorner();//(WallObject)GameObject.Instantiate(PrefabAssets.Singleton.prefabWallCorner);

                //Rotation
                if (neighborIndex == 6)
                {
                    //wallPiece.transform.rotation = Quaternion.Euler(0, 90f + wallPiece.defaultOrientation, 0f);
                    rot = 90f + wallPiece.defaultOrientation;
                }
                else if (neighborIndex == 1 || neighborIndex == 3 || neighborIndex == 2)
                {
                    //wallPiece.transform.rotation = Quaternion.Euler(0, 180f + wallPiece.defaultOrientation, 0f);
                    rot = 180f + wallPiece.defaultOrientation;
                }
                else if (neighborIndex == 9)
                {
                    //wallPiece.transform.rotation = Quaternion.Euler(0, 270f + wallPiece.defaultOrientation, 0f);
                    rot = 270f + wallPiece.defaultOrientation;
                }
            }
            else if (wallType == GroundManager.WallType.Cross)
            {
                wallPiece = CacheManager.Singleton.RequestWallCross();//(WallObject)GameObject.Instantiate(PrefabAssets.Singleton.prefabWallCross);
            }
            else if (wallType == GroundManager.WallType.ThreeWay)
            {
                wallPiece = CacheManager.Singleton.RequestWallT();

                if (neighborIndex == 13 || neighborIndex == 77 || neighborIndex == 205)
                {
                    rot = wallPiece.defaultOrientation + 270f;
                }
                else if (neighborIndex == 135 || neighborIndex == 7)
                {
                    rot = wallPiece.defaultOrientation + 90f;
                }
                else if (neighborIndex == 11 || neighborIndex == 151 || neighborIndex == 75 || neighborIndex == 139)
                {
                    rot = wallPiece.defaultOrientation + 180f;
                }
            }
            else //if (wallType == WallType.Straight)
            {
                wallPiece = CacheManager.Singleton.RequestWallStraight();//(WallObject)GameObject.Instantiate(PrefabAssets.Singleton.prefabWallStraight);

                //Horizontal
                if (neighborIndex == 37 || neighborIndex == 149 || neighborIndex == 133 || neighborIndex == 21 || neighborIndex == 69 || neighborIndex == 5 || neighborIndex == 101
                    || neighborIndex == 229 || neighborIndex == 213 || neighborIndex == 117 || neighborIndex == 181 || neighborIndex == 197 || neighborIndex == 53)
                {
                    //wallPiece.transform.rotation = Quaternion.Euler(0, wallPiece.defaultOrientation + 90f, 0f);
                    rot = wallPiece.defaultOrientation;
                }
            }

            wallPiece.AssignToPosition(xz, rot, true);
            wallPiece.SetWallType(wallType);
        }
        else
        {
            wallPiece = (m_groundTilesGOs[tileIndex] as WallObject);

            //If the exist type doesn't match the desired type, return it, and then get a new one
            if (wallPiece.WallType != wallType)
            {
                switch (wallPiece.WallType)
                {
                    case GroundManager.WallType.Corner:
                        CacheManager.Singleton.ReturnWallCorner(wallPiece);
                        break;

                    case GroundManager.WallType.Cross:
                        CacheManager.Singleton.ReturnWallCross(wallPiece);
                        break;

                    case GroundManager.WallType.Straight:
                        CacheManager.Singleton.ReturnWallStraight(wallPiece);
                        break;

                    case GroundManager.WallType.ThreeWay:
                        CacheManager.Singleton.ReturnWallT(wallPiece);
                        break;
                }

                SetTileGO(tileIndex, null);
                wallPiece = null;
                UpdateGraphicalTile(tileIndex, xz, neighborIndex, wallType);
                return;
            }
        }

        SetTileGO(tileIndex, wallPiece);
    }

    #region Holograms
    public void ReturnTempHolograms()
    {
        for (int i = 0; i < m_tempSelectHolograms.Count; i++)
        {
            CacheManager.Singleton.ReturnHighLightTile(m_tempSelectHolograms[i]);
        }
        m_tempSelectHolograms.Clear();
    }

    private void BuildTempHolograms()
    {
        for (int i = 0; i < m_tempSelectedTiles.Count; i++)
        {
            Color tileColor = new Color(0.94f, 0.87f, 0f, 0.9f);
            int index = m_associatedGroundData.ConvertVec2IntToInt(m_tempSelectedTiles[i]);
            if (m_associatedGroundData.m_groundTiles[index] != 0)
            {
                tileColor = new Color(1f, 0f, 0f, 0.9f);
            }

            HologramObject holo = CacheManager.Singleton.RequestHighLightTile();
            holo.tilePos.x = m_tempSelectedTiles[i].x;
            holo.tilePos.y = m_tempSelectedTiles[i].y;

            holo.gameObject.transform.position = new Vector3(holo.tilePos.x, 0f, holo.tilePos.y);
            holo.gameObject.SetActive(true);
            holo.SetColor(tileColor);

            m_tempSelectHolograms.Add(holo);
        }
    }

    public void ReturnSelectedHolograms()
    {
        for (int i = 0; i < m_selectedHolograms.Count; i++)
        {
            CacheManager.Singleton.ReturnHighLightTile(m_selectedHolograms[i]);
        }
        m_selectedHolograms.Clear();
    }

    private void BuildSelectedHolograms()
    {
        for (int i = 0; i < m_selectedTiles.Count; i++)
        {
            Color tileColor = new Color(0.94f, 0.87f, 0f, 0.9f);
            int index = m_associatedGroundData.ConvertVec2IntToInt(m_selectedTiles[i]);
            if (m_associatedGroundData.m_groundTiles[index] != 0)
            {
                tileColor = new Color(1f, 0f, 0f, 0.9f);
            }

            HologramObject holo = CacheManager.Singleton.RequestHighLightTile();
            holo.tilePos.x = m_selectedTiles[i].x;
            holo.tilePos.y = m_selectedTiles[i].y;

            holo.gameObject.transform.position = new Vector3(holo.tilePos.x, 0f, holo.tilePos.y);
            holo.gameObject.SetActive(true);
            holo.SetColor(tileColor);

            m_selectedHolograms.Add(holo);
        }
    }

    public void ShowTempObject(UIManager.UIObjectType objectTypeToDisplay, Vector3 position)
    {
        //If no hologram, grab one
        if (m_objectHologram == null)
        {
            m_objectHologram = CacheManager.Singleton.RequestInteriorDoor();
            m_objectHologram.gameObject.SetActive(true);
            m_objectHologram.transform.parent = null;
        }

        Vec2Int tilePos = GroundManager.Singleton.ConvertWorldPositionToTile(position);

        m_objectHologram.transform.position = new Vector3(tilePos.x + m_objectHologram.defaultOffset.x, 0, tilePos.y + m_objectHologram.defaultOffset.z);
    }
    #endregion

    public void SetHighlightTiles(Vector3 startPointWorld, Vector3 endPointWorld, WorldOrthoCamera.SelectionType selectType)
    {
        m_doHighlight = true;

        int xT = (int)(startPointWorld.x);
        int zT = (int)(startPointWorld.z);

        int x2T = (int)(endPointWorld.x);
        int z2T = (int)(endPointWorld.z);

        float xRemainder = Mathf.Abs(startPointWorld.x - endPointWorld.x);
        float zRemainder = Mathf.Abs(startPointWorld.z - endPointWorld.z);

        m_highLightStartPoint.x = Mathf.Min(xT, x2T);
        m_highLightStartPoint.y = Mathf.Min(zT, z2T);

        int maxX = Mathf.Max(xT, x2T);
        int maxZ = Mathf.Max(zT, z2T);

        m_highLightWidthHeight.x = maxX - m_highLightStartPoint.x + (xRemainder > 0.05f ? 1 : 0);
        m_highLightWidthHeight.y = maxZ - m_highLightStartPoint.y + +(zRemainder > 0.05f ? 1 : 0);

        if (selectType == WorldOrthoCamera.SelectionType.Select)
        {
            m_selectedTiles.Clear();

            m_selectedTiles.AddRange(m_tempSelectedTiles);
        }
        else if (selectType == WorldOrthoCamera.SelectionType.Addition)
        {
            for (int i = 0; i < m_tempSelectedTiles.Count; i++)
            {
                bool alreadyPresent = false;
                for (int j = 0; j < m_selectedTiles.Count; j++)
                {
                    if (m_tempSelectedTiles[i] == m_selectedTiles[j])
                    {
                        alreadyPresent = true;
                    }
                }

                if (!alreadyPresent)
                {
                    m_selectedTiles.Add(m_tempSelectedTiles[i]);
                    alreadyPresent = false;
                }
            }
            //m_selectedTiles.AddRange(m_tempSelectedTiles);
        }
        else if (selectType == WorldOrthoCamera.SelectionType.Subtraction)
        {
            //We're subtracting, so we need to go through the temp selected list, and then remove it from the selected list if present
            for (int i = 0; i < m_tempSelectedTiles.Count; i++)
            {
                for (int j = 0; j < m_selectedTiles.Count; j++)
                {
                    if (m_tempSelectedTiles[i] == m_selectedTiles[j])
                    {
                        m_selectedTiles.RemoveAt(j);
                        break;
                    }
                }
            }
        }
        else
        {
            Debug.LogWarning("Should not be here. Debug");
        }

        ReturnSelectedHolograms();
        BuildSelectedHolograms();

        ReturnTempHolograms();
    }

    public void SetTempHighlight(Vector3 tempStart, Vector3 tempEnd)
    {
        m_doTempHighLight = true;

        int xT = (int)(tempStart.x);
        int zT = (int)(tempStart.z);

        int x2T = (int)(tempEnd.x);
        int z2T = (int)(tempEnd.z);

        float xRemainder = Mathf.Abs(tempStart.x - tempEnd.x);
        float zRemainder = Mathf.Abs(tempStart.z - tempEnd.z);
        //Debug.Log("XRem " + xRemainder + " zRem " + zRemainder);

        Vec2Int prevStartPoint = m_tempLightStartPoint;
        Vec2Int prevEndPoint = m_tempLightWidthHeight;

        m_tempLightStartPoint.x = Mathf.Min(xT, x2T);
        m_tempLightStartPoint.y = Mathf.Min(zT, z2T);

        int maxX = Mathf.Max(xT, x2T);
        int maxZ = Mathf.Max(zT, z2T);

        //It'll be 0, but uf the x or z remainder is > 0.25f, it'll add one. Otherwise the selection area is too small to select
        m_tempLightWidthHeight.x = maxX - m_tempLightStartPoint.x + (xRemainder > 0.05f ? 1 : 0);
        m_tempLightWidthHeight.y = maxZ - m_tempLightStartPoint.y + (zRemainder > 0.05f ? 1 : 0);

        //Experiment with the highlight tiles
        //Only do it if one or more of the values are different than the previous frame
        if (prevStartPoint.x != m_tempLightStartPoint.x || prevStartPoint.y != m_tempLightStartPoint.y || prevEndPoint.x != m_tempLightWidthHeight.x || prevEndPoint.y != m_tempLightWidthHeight.y)
        {
            Debug.Log(" m_tempLightStartPoint " + m_tempLightStartPoint + " m_tempLightWidthHeight " + m_tempLightWidthHeight);
            //ReturnHolograms();
            m_tempSelectedTiles.Clear();

            //for (int x = 0; x < m_tempLightWidthHeight.x; x++)
            //{
            //    for (int z = 0; z < m_tempLightWidthHeight.y; z++)
            //    {
            //        HologramObject holo = CacheManager.Singleton.RequestHighLightTile();
            //        holo.tilePos.x = x + m_tempLightStartPoint.x;
            //        holo.tilePos.y = z + m_tempLightStartPoint.y;

            //        holo.gameObject.transform.position = new Vector3(holo.tilePos.x, 0, holo.tilePos.y);
            //        holo.gameObject.SetActive(true);

            //        m_tempSelectHolograms.Add(holo);
            //    }
            //}

            for (int x = 0; x < m_tempLightWidthHeight.x; x++)
            {
                for (int z = 0; z < m_tempLightWidthHeight.y; z++)
                {
                    m_tempSelectedTiles.Add(new Vec2Int(x + m_tempLightStartPoint.x, z + m_tempLightStartPoint.y));
                }
            }

            ReturnTempHolograms();
            BuildTempHolograms();
        }
    }

    #region Assembly
    public void AssembleTile(int tileIndex)
    {
        m_groundTilesGOs[tileIndex].BuildObject();
    }
    #endregion

    #region Debug/Editor
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
                m_tempLightStartPoint.x + m_tempLightWidthHeight.x,
                0,
                m_tempLightStartPoint.y);

            Vector3 point3b = new Vector3(
                m_tempLightStartPoint.x + m_tempLightWidthHeight.x,
                0,
                m_tempLightStartPoint.y + m_tempLightWidthHeight.y);

            Vector3 point4b = new Vector3(
                m_tempLightStartPoint.x,
                0,
                m_tempLightStartPoint.y + m_tempLightWidthHeight.y);

            Debug.DrawLine(point1b, point2b, Color.yellow);
            Debug.DrawLine(point2b, point3b, Color.yellow);
            Debug.DrawLine(point3b, point4b, Color.yellow);
            Debug.DrawLine(point4b, point1b, Color.yellow);
        }

        if (m_associatedGroundData != null && m_associatedGroundData.m_groundTiles != null)
        {
            Debug.DrawLine(new Vector3(0, 0f, 0), new Vector3(0, 0f, m_groundTileDim.y), Color.magenta);
            Debug.DrawLine(new Vector3(0, 0f, m_groundTileDim.y), new Vector3(m_groundTileDim.x, 0f, m_groundTileDim.y), Color.magenta);
            Debug.DrawLine(new Vector3(m_groundTileDim.x, 0f, m_groundTileDim.y), new Vector3(m_groundTileDim.x, 0f, 0), Color.magenta);
            Debug.DrawLine(new Vector3(m_groundTileDim.x, 0f, 0), new Vector3(0, 0f, 0), Color.magenta);
        }
    }
    #endregion
}