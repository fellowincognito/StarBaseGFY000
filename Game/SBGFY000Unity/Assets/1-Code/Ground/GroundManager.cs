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

    #region Enums
    //Used to determine general presence and whether passable
    //Individual or transitional definitions are declared elsewhere
    public enum GroundTileType
    {
        None = 0, //Empty Space
        TempRoom, //Used to temporarily before ground or wall is figured out
        Ground,
        Wall,
        Door,
        Teleporter,
        Resource,        
    }

    public enum WallType
    {
        Straight,
        Corner,
        ThreeWay,
        Cross
    }
    #endregion

    #region Vars
    public GameObject debugCheckerGO;

    ushort[] m_groundTiles;
    Vec2Int m_groundTileDim;

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
    #endregion

    #region Gets/Sets
    public Vec2Int GroundSize
    {
        get { return m_groundTileDim; }
    }

    public Vec2Int SelectedOrigin
    {
        get { return m_highLightStartPoint; }
    }

    public Vec2Int SelectedWidthHeight
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
    public GroundTileType GetTileType(Vec2Int tilePos)
    {
        int index = (tilePos.x * m_groundTileDim.x) + tilePos.y;

        return (GroundTileType)m_groundTiles[index];
    }

    public GroundTileType GetTileType(int x, int z)
    {
        int index = (x * m_groundTileDim.x) + z;

        return (GroundTileType)m_groundTiles[index];
    }

    public int ConvertVec2IntToInt(Vec2Int position)
    {
        return (position.x * m_groundTileDim.x) + position.y;
    }

    public int ConvertTwoIntToInt(int x, int z)
    {
        return (x * m_groundTileDim.x) + z;
    }

    #region Neighbor Related Things
    private bool IsATilePresentAtLocation(int x, int z)
    {
        //Check the bounds to make sure we are within them
        if (x < 0 || z < 0 || x > m_groundTileDim.x - 1 || z > m_groundTileDim.y - 1)
        {
            //Debug.Log("out of bounds " + x + " " + y);
            return false;
        }

        //Since we're using bools here, just return the value from the array now that we've bounds checked
        return (m_groundTiles[(m_groundTileDim.x * x) + z] == 0) ? true : false;
    }

    private int ComputeNeighborIndex(int x, int z)
    {
        int index = 0;

        //Up
        if (IsATilePresentAtLocation(x, z - 1))
            index |= 1;

        //Right
        if (IsATilePresentAtLocation(x + 1, z))
            index |= 2;

        //Down
        if (IsATilePresentAtLocation(x, z + 1))
            index |= 4;

        //Left
        if (IsATilePresentAtLocation(x - 1, z))
            index |= 8;

        //Upper Left
        if (IsATilePresentAtLocation(x - 1, z - 1))
            index |= 16;

        //Upper Right
        if (IsATilePresentAtLocation(x + 1, z - 1))
            index |= 32;

        //Lower Right
        if (IsATilePresentAtLocation(x + 1, z + 1))
            index |= 64;

        //Lower Left
        if (IsATilePresentAtLocation(x - 1, z + 1))
            index |= 128;

        return index;
    }
    #endregion
    #endregion

    public void Init(Vec2Int tileDim)
    {
        m_groundTileDim = tileDim;
        m_groundTiles = new ushort[m_groundTileDim.x * m_groundTileDim.y];
        m_tempSelectHolograms = new List<HologramObject>();
        m_selectedHolograms = new List<HologramObject>();
        
        m_tempSelectedTiles = new List<Vec2Int>();
        m_selectedTiles = new List<Vec2Int>();

        SetUpDebug();
    }

    public void ClearSelection()
    {
        m_highLightStartPoint = m_highLightWidthHeight = Vec2Int.Zero;
        ReturnTempHolograms();
    }

    public void SetTiles(GroundTileType tileType)
    {
        //Clear existing highlights
        ReturnSelectedHolograms();
        ReturnTempHolograms();

        List<Vec2Int> modifiedIndices = new List<Vec2Int>();

        //for (int x = 0; x < m_highLightWidthHeight.x; x++)
        //{
        //    for (int z = 0; z < m_highLightWidthHeight.y; z++)
        //    {
        //        int index = ConvertTwoIntToInt(x + m_highLightStartPoint.x, z + m_highLightStartPoint.y);

        //        GroundTileType curTileTypeAtPos = GetTileType(x + m_highLightStartPoint.x, z + m_highLightStartPoint.y);

        //        if (curTileTypeAtPos != GroundTileType.None)
        //        {
        //            Debug.Log(string.Format("Tile at {0},{1} is already {2}, not changing to {3}", x + m_highLightStartPoint.x, z + m_highLightStartPoint.y, curTileTypeAtPos, tileType));
        //            continue;
        //        }

        //        m_groundTiles[index] = (ushort)tileType;
        //        modifiedIndices.Add(new Vec2Int(x + m_highLightStartPoint.x, z + m_highLightStartPoint.y));
        //    }
        //}
        for (int i = 0; i < m_selectedTiles.Count; i++)
        {
            int index = ConvertTwoIntToInt(m_selectedTiles[i].x, m_selectedTiles[i].y);

            GroundTileType curTileTypeAtPos = GetTileType(m_selectedTiles[i].x, m_selectedTiles[i].y);

            if (curTileTypeAtPos != GroundTileType.None)
            {
                Debug.Log(string.Format("Tile at {0},{1} is already {2}, not changing to {3}", m_selectedTiles[i].x, m_selectedTiles[i].y, curTileTypeAtPos, tileType));
                continue;
            }

            m_groundTiles[index] = (ushort)tileType;
            modifiedIndices.Add(new Vec2Int(m_selectedTiles[i].x, m_selectedTiles[i].y));
        }

        //Now that the tiles have been assigned, should check if its a TempRoom, and if so, we need to do further checking (for assigning walls and shit)
        if (tileType == GroundTileType.TempRoom)
        {
            for (int i = 0; i < modifiedIndices.Count; i++)
            {
                int tileIndex = ConvertTwoIntToInt(modifiedIndices[i].x, modifiedIndices[i].y);
                int neighborIndex = ComputeNeighborIndex(modifiedIndices[i].x, modifiedIndices[i].y);

                Debug.Log(string.Format("x {0} z {1}: neighborindex: {2}", modifiedIndices[i].x, modifiedIndices[i].y, neighborIndex));

                switch (neighborIndex)
                {
                    //Straight wall piece
                    case 152:
                    case 98:
                    case 49:
                    case 196:
                    case 245:
                    case 250:
                        m_groundTiles[tileIndex] = (ushort)GroundTileType.Wall;
                        UpdateGraphicalTile(tileIndex, modifiedIndices[i], neighborIndex, WallType.Straight);
                        break;

                    //Corner wall piece
                    case 185:
                    case 230:
                    case 220:
                    case 115:
                        m_groundTiles[tileIndex] = (ushort)GroundTileType.Wall;
                        UpdateGraphicalTile(tileIndex, modifiedIndices[i], neighborIndex, WallType.Corner);
                        break;

                    //4 way cross piece
                    case 255:
                    case 247:
                    case 251:
                    case 253:
                    case 254:
                        m_groundTiles[tileIndex] = (ushort)GroundTileType.Wall;
                        UpdateGraphicalTile(tileIndex, modifiedIndices[i], neighborIndex, WallType.Cross);
                        break;

                    default:
                        //Place a basic ground tile
                        m_groundTiles[tileIndex] = (ushort)GroundTileType.Ground;
                        UpdateGraphicalTile(tileIndex, modifiedIndices[i], neighborIndex);
                        break;
                }
            }
        }
    }

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
            m_selectedTiles.AddRange(m_tempSelectedTiles);
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
        if(prevStartPoint.x != m_tempLightStartPoint.x || prevStartPoint.y != m_tempLightStartPoint.y || prevEndPoint.x != m_tempLightWidthHeight.x || prevEndPoint.y != m_tempLightWidthHeight.y)
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

    #region Holograms
    private void ReturnTempHolograms()
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
            HologramObject holo = CacheManager.Singleton.RequestHighLightTile();
            holo.tilePos.x = m_tempSelectedTiles[i].x;
            holo.tilePos.y = m_tempSelectedTiles[i].y;

            holo.gameObject.transform.position = new Vector3(holo.tilePos.x, 0f, holo.tilePos.y);
            holo.gameObject.SetActive(true);

            m_tempSelectHolograms.Add(holo);
        }
    }

    private void ReturnSelectedHolograms()
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
            HologramObject holo = CacheManager.Singleton.RequestHighLightTile();
            holo.tilePos.x = m_selectedTiles[i].x;
            holo.tilePos.y = m_selectedTiles[i].y;

            holo.gameObject.transform.position = new Vector3(holo.tilePos.x, 0f, holo.tilePos.y);
            holo.gameObject.SetActive(true);

            m_selectedHolograms.Add(holo);
        }
    }
    #endregion

    #region Graphical Components
    /// <summary>
    /// Used by ground (since there's only one type)
    /// </summary>
    public void UpdateGraphicalTile(int tileIndex, Vec2Int xz, int neighborIndex)
    {
        TileObject tilePiece = (TileObject)GameObject.Instantiate(PrefabAssets.Singleton.prefabGroundTile);
        tilePiece.transform.parent = this.transform;
        tilePiece.transform.position = new Vector3(xz.x + tilePiece.defaultOffset.x, tilePiece.defaultOffset.y, xz.y + tilePiece.defaultOffset.z);
        tilePiece.gameObject.SetActive(true);
    }

    /// <summary>
    /// Useful for wall tiles
    /// </summary>
    /// <param name="tileIndex"></param>
    /// <param name="xz"></param>
    /// <param name="neighborIndex"></param>
    /// <param name="wallType"></param>
    public void UpdateGraphicalTile(int tileIndex, Vec2Int xz, int neighborIndex, WallType wallType)
    {
        WallObject wallPiece = null; //m_tilePos
        float rot = 0f;
        if (wallType == WallType.Corner)
        {
            wallPiece = (WallObject)GameObject.Instantiate(PrefabAssets.Singleton.prefabWallCorner);
            
            //Rotation
            if (neighborIndex == 185)
            {
                //wallPiece.transform.rotation = Quaternion.Euler(0, 90f + wallPiece.defaultOrientation, 0f);
                rot = 90f + wallPiece.defaultOrientation;
            }
            else if (neighborIndex == 220)
            {
                //wallPiece.transform.rotation = Quaternion.Euler(0, 180f + wallPiece.defaultOrientation, 0f);
                rot = 180f + wallPiece.defaultOrientation;
            }
            else if (neighborIndex == 230)
            {
                //wallPiece.transform.rotation = Quaternion.Euler(0, 270f + wallPiece.defaultOrientation, 0f);
                rot = 270f + wallPiece.defaultOrientation;
            }
        }
        else if (wallType == WallType.Cross)
        {
            wallPiece = (WallObject)GameObject.Instantiate(PrefabAssets.Singleton.prefabWallCross);
        }
        else //if (wallType == WallType.Straight)
        {
            wallPiece = (WallObject)GameObject.Instantiate(PrefabAssets.Singleton.prefabWallStraight);
            
            //Horizontal
            //if (neighborIndex == 49 || neighborIndex == 196)
            //case 152:
            //        case 98:
            //        case 49:
            //        case 196:
            //        case 245:
            //        case 250:
            if(neighborIndex == 98 || neighborIndex == 152)
            {
                //wallPiece.transform.rotation = Quaternion.Euler(0, wallPiece.defaultOrientation + 90f, 0f);
                rot = wallPiece.defaultOrientation;
            }
        }

        wallPiece.transform.parent = this.transform;
        wallPiece.transform.position = new Vector3(xz.x + wallPiece.defaultOffset.x, wallPiece.defaultOffset.y, xz.y + wallPiece.defaultOffset.z);
        wallPiece.gameObject.SetActive(true);
        wallPiece.transform.rotation = Quaternion.Euler(0f, rot, 0f);
        wallPiece.SetTilePos(xz);
    }
    #endregion

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