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
    BaseGroundObject[] m_groundTilesGOs;
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

    //Keep a reference to these so we don't have to keep casting back and forth during the fairly tight loops
    ushort m_shortCut_emptyIndex;
    ushort m_shortCut_groundIndex;
    ushort m_shortCut_wallIndex;
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

        if (index < 0 || index > m_groundTiles.Length)
        {
            return GroundTileType.None;
        }

        return (GroundTileType)m_groundTiles[index];
    }

    public GroundTileType GetTileType(int x, int z)
    {
        int index = (x * m_groundTileDim.x) + z;

        if (index < 0 || index > m_groundTiles.Length)
        {
            return GroundTileType.None;
        }

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

    public void SetTileToType(int index, ushort value)
    {
        m_groundTiles[index] = value;
    }

    public void SetTileGO(int index, BaseGroundObject newObject)
    {
        m_groundTilesGOs[index] = newObject;
    }

    #region Neighbor Related Things
    /// <summary>
    /// Returns true if tile is empty (0)
    /// </summary>
    /// <param name="x"></param>
    /// <param name="z"></param>
    /// <returns></returns>
    private bool IsNeighborATileType(int x, int z, ushort targetValue)
    {
        //Check the bounds to make sure we are within them
        if (x < 0 || z < 0 || x > m_groundTileDim.x - 1 || z > m_groundTileDim.y - 1)
        {
            //Debug.Log("out of bounds " + x + " " + y);
            return false;
        }
        
        return (m_groundTiles[(m_groundTileDim.x * x) + z] == targetValue) ? true : false;
    }

    public int ComputeWallNeighborIndex(int x, int z)
    {
        int index = 0;

        //Up
        if (IsNeighborATileType(x, z - 1, m_shortCut_wallIndex))
            index |= 1;

        //Right
        if (IsNeighborATileType(x + 1, z, m_shortCut_wallIndex))
            index |= 2;

        //Down
        if (IsNeighborATileType(x, z + 1, m_shortCut_wallIndex))
            index |= 4;

        //Left
        if (IsNeighborATileType(x - 1, z, m_shortCut_wallIndex))
            index |= 8;

        //Lower Left
        if (IsNeighborATileType(x - 1, z - 1, m_shortCut_wallIndex))
            index |= 16;

        //Lower Right
        if (IsNeighborATileType(x + 1, z - 1, m_shortCut_wallIndex))
            index |= 32;

        //Upper Right
        if (IsNeighborATileType(x + 1, z + 1, m_shortCut_wallIndex))
            index |= 64;

        //Upper Left
        if (IsNeighborATileType(x - 1, z + 1, m_shortCut_wallIndex))
            index |= 128;

        return index;
    }

    public int ComputeEmptyNeighborIndex(int x, int z)
    {
        int index = 0;

        //Up
        if (IsNeighborATileType(x, z - 1, m_shortCut_emptyIndex))
            index |= 1;

        //Right
        if (IsNeighborATileType(x + 1, z, m_shortCut_emptyIndex))
            index |= 2;

        //Down
        if (IsNeighborATileType(x, z + 1, m_shortCut_emptyIndex))
            index |= 4;

        //Left
        if (IsNeighborATileType(x - 1, z, m_shortCut_emptyIndex))
            index |= 8;

        //Lower Left
        if (IsNeighborATileType(x - 1, z - 1, m_shortCut_emptyIndex))
            index |= 16;

        //Lower Right
        if (IsNeighborATileType(x + 1, z - 1, m_shortCut_emptyIndex))
            index |= 32;

        //Upper Right
        if (IsNeighborATileType(x + 1, z + 1, m_shortCut_emptyIndex))
            index |= 64;

        //Upper Left
        if (IsNeighborATileType(x - 1, z + 1, m_shortCut_emptyIndex))
            index |= 128;

        return index;
    }
    #endregion
    #endregion

    public void Init(Vec2Int tileDim)
    {
        m_groundTileDim = tileDim;
        m_groundTiles = new ushort[m_groundTileDim.x * m_groundTileDim.y];
        m_groundTilesGOs = new BaseGroundObject[m_groundTiles.Length];
        m_tempSelectHolograms = new List<HologramObject>();
        m_selectedHolograms = new List<HologramObject>();
        
        m_tempSelectedTiles = new List<Vec2Int>();
        m_selectedTiles = new List<Vec2Int>();

        m_shortCut_emptyIndex = (ushort)GroundTileType.None;
        m_shortCut_groundIndex = (ushort)GroundTileType.Ground;
        m_shortCut_wallIndex = (ushort)GroundTileType.Wall; ;

        SetUpDebug();
    }

    public void ClearSelection()
    {
        m_highLightStartPoint = m_highLightWidthHeight = Vec2Int.Zero;
        ReturnSelectedHolograms();
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

            SetTileToType(index, (ushort)tileType);
            modifiedIndices.Add(new Vec2Int(m_selectedTiles[i].x, m_selectedTiles[i].y));
        }

        //Now that the tiles have been assigned, should check if its a TempRoom, and if so, we need to do further checking (for assigning walls and shit)
        if (tileType == GroundTileType.TempRoom)
        {
            //First go through and figure out which should be just floor tiles. Floor tiles will be ones with a neighbor on all sides (value of 255)
            //Start at the end and work towards start so we won't skip tiles due to removed indices shifting things about
            for (int i = modifiedIndices.Count - 1; i > -1 ; i--)
            {
                ProcessForFloorTiles(ref modifiedIndices, i);
            }

            //All ground tiles should have been removed from ProcessForFloorTiles, so modifiedIndices should only retain wall tiles. Now we need to figure out what specific tiles they are
            for (int i = 0; i < modifiedIndices.Count; i++)
            {
                ProcessForWallTiles(modifiedIndices[i], 1);
            }
        }
    }

    private void ProcessForFloorTiles(ref List<Vec2Int> suspectTiles, int listIndex)
    {
        Vec2Int tile = suspectTiles[listIndex];
        int tileIndex = ConvertVec2IntToInt(tile);
        int neighborIndex = ComputeEmptyNeighborIndex(tile.x, tile.y);

        Debug.Log("Floor " + tile.x + ", " + tile.y + " neighbor " + neighborIndex);

        bool isEdge = false;

        if (tile.x == 0 || tile.x == m_groundTileDim.x - 1 || tile.y == 0 || tile.y == m_groundTileDim.y - 1)
        {
            isEdge = true;
        }

        //This tile is surrounded on all sides by some kind of tile/wall/something, and thus this must be a ground tile
        //In this case, it's surrounded by "TempRoom"
        if (neighborIndex == 0 && !isEdge)
        {
            SetTileToType(tileIndex, m_shortCut_groundIndex);
            UpdateGraphicalTile(tileIndex, tile, neighborIndex);

            //Remove it from the list as we're going to be reiterating through again for walls
            suspectTiles.RemoveAt(listIndex);
        }
        else
        {
            //Otherwise it must be some kind of wall, and we'll figure out what exactly elsewhere
            SetTileToType(tileIndex, m_shortCut_wallIndex);
        }
    }

    private void ProcessForWallTiles(Vec2Int tile, int allowAnotherLevel)
    {
        if (allowAnotherLevel < 0)
            return;

        allowAnotherLevel--;

        int tileIndex = ConvertVec2IntToInt(tile);
        int neighborIndex = ComputeWallNeighborIndex(tile.x, tile.y);
        //int otherNeighbor = 0;

        Debug.Log("Wall " + tile.x + ", " + tile.y + " neighbor " + neighborIndex);

        #region Large Switch for all neighbor conditions
        switch (neighborIndex)
        {
            case 1: 
                SetTileToType(tileIndex, m_shortCut_wallIndex);
                UpdateGraphicalTile(tileIndex, tile, neighborIndex, WallType.Straight);

                //ProcessForWallTiles(new Vec2Int(tile.x, tile.y), 1);
                break;

            case 2: 
                SetTileToType(tileIndex, m_shortCut_wallIndex);
                UpdateGraphicalTile(tileIndex, tile, neighborIndex, WallType.Straight);
                break;

            case 3: 
                SetTileToType(tileIndex, m_shortCut_wallIndex);
                UpdateGraphicalTile(tileIndex, tile, neighborIndex, WallType.Corner);
                break;

            case 4: 
                SetTileToType(tileIndex, m_shortCut_wallIndex);
                UpdateGraphicalTile(tileIndex, tile, neighborIndex, WallType.Straight);

                
                break;

            case 5: 
                SetTileToType(tileIndex, m_shortCut_wallIndex);
                UpdateGraphicalTile(tileIndex, tile, neighborIndex, WallType.Straight);
                break;

            case 6: SetTileToType(tileIndex, m_shortCut_wallIndex);
                UpdateGraphicalTile(tileIndex, tile, neighborIndex, WallType.Corner); 
                break;

            case 7: 
                SetTileToType(tileIndex, m_shortCut_wallIndex);
                UpdateGraphicalTile(tileIndex, tile, neighborIndex, WallType.ThreeWay);

                //ProcessForWallTiles(new Vec2Int(tile.x, tile.y), 1);
                break;

            case 8: 
                SetTileToType(tileIndex, m_shortCut_wallIndex);
                UpdateGraphicalTile(tileIndex, tile, neighborIndex, WallType.Straight);
                break;

            case 9: 
                SetTileToType(tileIndex, m_shortCut_wallIndex);
                UpdateGraphicalTile(tileIndex, tile, neighborIndex, WallType.Corner);
                break;

            case 10:
                SetTileToType(tileIndex, m_shortCut_wallIndex);
                UpdateGraphicalTile(tileIndex, tile, neighborIndex, WallType.Straight); 
                break;

            case 11:
                SetTileToType(tileIndex, m_shortCut_wallIndex);
                UpdateGraphicalTile(tileIndex, tile, neighborIndex, WallType.ThreeWay); 
                break;

            case 12:
                SetTileToType(tileIndex, m_shortCut_wallIndex);
                UpdateGraphicalTile(tileIndex, tile, neighborIndex, WallType.Corner); 
                break;

            case 13:
                SetTileToType(tileIndex, m_shortCut_wallIndex);
                UpdateGraphicalTile(tileIndex, tile, neighborIndex, WallType.ThreeWay); 
                break;

            case 14:
                SetTileToType(tileIndex, m_shortCut_wallIndex);
                UpdateGraphicalTile(tileIndex, tile, neighborIndex, WallType.ThreeWay); 
                break;

            case 15:
                SetTileToType(tileIndex, m_shortCut_wallIndex);
                UpdateGraphicalTile(tileIndex, tile, neighborIndex, WallType.Cross); 
                break;

            case 16:
                SetTileToType(tileIndex, m_shortCut_wallIndex);
                UpdateGraphicalTile(tileIndex, tile, neighborIndex, WallType.Cross); 
                break;

            case 17:
                SetTileToType(tileIndex, m_shortCut_wallIndex);
                UpdateGraphicalTile(tileIndex, tile, neighborIndex, WallType.Straight); 
                break;

            case 18: SetTileToType(tileIndex, m_shortCut_wallIndex);
                UpdateGraphicalTile(tileIndex, tile, neighborIndex, WallType.Straight); 
                break;

            case 19:
                SetTileToType(tileIndex, m_shortCut_wallIndex);
                UpdateGraphicalTile(tileIndex, tile, neighborIndex, WallType.Corner); 
                break;

            case 20:
                SetTileToType(tileIndex, m_shortCut_wallIndex);
                UpdateGraphicalTile(tileIndex, tile, neighborIndex, WallType.Straight); 
                break;

            case 21:
                SetTileToType(tileIndex, m_shortCut_wallIndex);
                UpdateGraphicalTile(tileIndex, tile, neighborIndex, WallType.Straight);

                ProcessForWallTiles(new Vec2Int(tile.x, tile.y - 1), 1);
                break;

            case 22:
                SetTileToType(tileIndex, m_shortCut_wallIndex);
                UpdateGraphicalTile(tileIndex, tile, neighborIndex, WallType.Corner); 
                break;

            case 23:
                SetTileToType(tileIndex, m_shortCut_wallIndex);
                UpdateGraphicalTile(tileIndex, tile, neighborIndex, WallType.ThreeWay); 
                break;

            case 24:
                SetTileToType(tileIndex, m_shortCut_wallIndex);
                UpdateGraphicalTile(tileIndex, tile, neighborIndex, WallType.Straight); 
                break;

            case 25: SetTileToType(tileIndex, m_shortCut_wallIndex);
                UpdateGraphicalTile(tileIndex, tile, neighborIndex, WallType.Corner); 
                break;

            case 26:
                SetTileToType(tileIndex, m_shortCut_wallIndex);
                UpdateGraphicalTile(tileIndex, tile, neighborIndex, WallType.Straight);

                ProcessForWallTiles(new Vec2Int(tile.x - 1, tile.y), 1);
                break;

            case 27:
                SetTileToType(tileIndex, m_shortCut_wallIndex);
                UpdateGraphicalTile(tileIndex, tile, neighborIndex, WallType.ThreeWay); 
                break;

            case 28:
                SetTileToType(tileIndex, m_shortCut_wallIndex);
                UpdateGraphicalTile(tileIndex, tile, neighborIndex, WallType.Corner); 
                break;

            case 29: 
                SetTileToType(tileIndex, m_shortCut_wallIndex);
                UpdateGraphicalTile(tileIndex, tile, neighborIndex, WallType.ThreeWay); 
                break;

            case 30: 
                SetTileToType(tileIndex, m_shortCut_wallIndex);
                UpdateGraphicalTile(tileIndex, tile, neighborIndex, WallType.ThreeWay); 
                break;

            case 31: SetTileToType(tileIndex, m_shortCut_wallIndex);
                UpdateGraphicalTile(tileIndex, tile, neighborIndex, WallType.Cross); 
                break;

            case 32: 
                SetTileToType(tileIndex, m_shortCut_wallIndex);
                UpdateGraphicalTile(tileIndex, tile, neighborIndex, WallType.Cross); 
                break;

            case 33: 
                SetTileToType(tileIndex, m_shortCut_wallIndex);
                UpdateGraphicalTile(tileIndex, tile, neighborIndex, WallType.Straight); 
                break;

            case 34: 
                SetTileToType(tileIndex, m_shortCut_wallIndex);
                UpdateGraphicalTile(tileIndex, tile, neighborIndex, WallType.Straight); 
                break;

            case 35:
                SetTileToType(tileIndex, m_shortCut_wallIndex);
                UpdateGraphicalTile(tileIndex, tile, neighborIndex, WallType.Corner); 
                break;

            case 36: 
                SetTileToType(tileIndex, m_shortCut_wallIndex);
                UpdateGraphicalTile(tileIndex, tile, neighborIndex, WallType.Straight); 
                break;

            case 37: 
                SetTileToType(tileIndex, m_shortCut_wallIndex);
                UpdateGraphicalTile(tileIndex, tile, neighborIndex, WallType.Straight);

                ProcessForWallTiles(new Vec2Int(tile.x, tile.y - 1), 1);
                break;

            case 38:
                SetTileToType(tileIndex, m_shortCut_wallIndex);
                UpdateGraphicalTile(tileIndex, tile, neighborIndex, WallType.Corner); 
                break;

            case 39:
                SetTileToType(tileIndex, m_shortCut_wallIndex);
                UpdateGraphicalTile(tileIndex, tile, neighborIndex, WallType.ThreeWay); 
                break;

            case 40: 
                SetTileToType(tileIndex, m_shortCut_wallIndex);
                UpdateGraphicalTile(tileIndex, tile, neighborIndex, WallType.Straight); 
                break;

            case 41: 
                SetTileToType(tileIndex, m_shortCut_wallIndex);
                UpdateGraphicalTile(tileIndex, tile, neighborIndex, WallType.Corner); 
                break;

            case 42: 
                SetTileToType(tileIndex, m_shortCut_wallIndex);
                UpdateGraphicalTile(tileIndex, tile, neighborIndex, WallType.Straight);

                ProcessForWallTiles(new Vec2Int(tile.x + 1, tile.y), 1);
                break;

            case 43: 
                SetTileToType(tileIndex, m_shortCut_wallIndex);
                UpdateGraphicalTile(tileIndex, tile, neighborIndex, WallType.ThreeWay); 
                break;

            case 44: 
                SetTileToType(tileIndex, m_shortCut_wallIndex);
                UpdateGraphicalTile(tileIndex, tile, neighborIndex, WallType.Corner); 
                break;

            case 45: 
                SetTileToType(tileIndex, m_shortCut_wallIndex);
                UpdateGraphicalTile(tileIndex, tile, neighborIndex, WallType.ThreeWay); 
                break;

            case 46: 
                SetTileToType(tileIndex, m_shortCut_wallIndex);
                UpdateGraphicalTile(tileIndex, tile, neighborIndex, WallType.ThreeWay); 
                break;

            case 47:
                SetTileToType(tileIndex, m_shortCut_wallIndex);
                UpdateGraphicalTile(tileIndex, tile, neighborIndex, WallType.Cross); 
                break;

            case 48:
                SetTileToType(tileIndex, m_shortCut_wallIndex);
                UpdateGraphicalTile(tileIndex, tile, neighborIndex, WallType.Cross); 
                break;

            case 49:
                SetTileToType(tileIndex, m_shortCut_wallIndex);
                UpdateGraphicalTile(tileIndex, tile, neighborIndex, WallType.Straight);

                ProcessForWallTiles(new Vec2Int(tile.x - 1, tile.y), 1);
                break;

            case 50:
                SetTileToType(tileIndex, m_shortCut_wallIndex);
                UpdateGraphicalTile(tileIndex, tile, neighborIndex, WallType.Straight); 
                break;

            case 51:
                SetTileToType(tileIndex, m_shortCut_wallIndex);
                UpdateGraphicalTile(tileIndex, tile, neighborIndex, WallType.Corner); 
                break;

            case 52:
                SetTileToType(tileIndex, m_shortCut_wallIndex);
                UpdateGraphicalTile(tileIndex, tile, neighborIndex, WallType.Straight); 
                break;

            case 53:
                SetTileToType(tileIndex, m_shortCut_wallIndex);
                UpdateGraphicalTile(tileIndex, tile, neighborIndex, WallType.Straight);

                ProcessForWallTiles(new Vec2Int(tile.x, tile.y - 1), 1);
                break;

            case 54:
                SetTileToType(tileIndex, m_shortCut_wallIndex);
                UpdateGraphicalTile(tileIndex, tile, neighborIndex, WallType.Corner); 
                break;

            case 55:
                SetTileToType(tileIndex, m_shortCut_wallIndex);
                UpdateGraphicalTile(tileIndex, tile, neighborIndex, WallType.ThreeWay); 
                break;

            case 56:
                SetTileToType(tileIndex, m_shortCut_wallIndex);
                UpdateGraphicalTile(tileIndex, tile, neighborIndex, WallType.Straight); 
                break;

            case 57:
                SetTileToType(tileIndex, m_shortCut_wallIndex);
                UpdateGraphicalTile(tileIndex, tile, neighborIndex, WallType.Corner); 
                break;

            case 58: SetTileToType(tileIndex, m_shortCut_wallIndex);
                UpdateGraphicalTile(tileIndex, tile, neighborIndex, WallType.Straight);

                ProcessForWallTiles(new Vec2Int(tile.x + 1, tile.y), 1);
                ProcessForWallTiles(new Vec2Int(tile.x - 1, tile.y), 1);
                break;

            case 59: 
                SetTileToType(tileIndex, m_shortCut_wallIndex);
                UpdateGraphicalTile(tileIndex, tile, neighborIndex, WallType.ThreeWay); 
                break;

            case 60:
                SetTileToType(tileIndex, m_shortCut_wallIndex);
                UpdateGraphicalTile(tileIndex, tile, neighborIndex, WallType.Corner); 
                break;

            case 61:
                SetTileToType(tileIndex, m_shortCut_wallIndex);
                UpdateGraphicalTile(tileIndex, tile, neighborIndex, WallType.ThreeWay); 
                break;

            case 62:
                SetTileToType(tileIndex, m_shortCut_wallIndex);
                UpdateGraphicalTile(tileIndex, tile, neighborIndex, WallType.ThreeWay); 
                break;

            case 63: 
                SetTileToType(tileIndex, m_shortCut_wallIndex);
                UpdateGraphicalTile(tileIndex, tile, neighborIndex, WallType.Cross); 
                break;

            case 64:
                SetTileToType(tileIndex, m_shortCut_wallIndex);
                UpdateGraphicalTile(tileIndex, tile, neighborIndex, WallType.Cross); 
                break;

            case 65:
                SetTileToType(tileIndex, m_shortCut_wallIndex);
                UpdateGraphicalTile(tileIndex, tile, neighborIndex, WallType.Straight); 
                break;

            case 66:
                SetTileToType(tileIndex, m_shortCut_wallIndex);
                UpdateGraphicalTile(tileIndex, tile, neighborIndex, WallType.Straight); 
                break;

            case 67:
                SetTileToType(tileIndex, m_shortCut_wallIndex);
                UpdateGraphicalTile(tileIndex, tile, neighborIndex, WallType.Corner); 
                break;

            case 68:
                SetTileToType(tileIndex, m_shortCut_wallIndex);
                UpdateGraphicalTile(tileIndex, tile, neighborIndex, WallType.Straight); 
                break;

            case 69:
                SetTileToType(tileIndex, m_shortCut_wallIndex);
                UpdateGraphicalTile(tileIndex, tile, neighborIndex, WallType.Straight);

                ProcessForWallTiles(new Vec2Int(tile.x, tile.y + 1), 1);
                break;

            case 70:
                SetTileToType(tileIndex, m_shortCut_wallIndex);
                UpdateGraphicalTile(tileIndex, tile, neighborIndex, WallType.Corner); 
                break;

            case 71:
                SetTileToType(tileIndex, m_shortCut_wallIndex);
                UpdateGraphicalTile(tileIndex, tile, neighborIndex, WallType.ThreeWay); 
                break;

            case 72:
                SetTileToType(tileIndex, m_shortCut_wallIndex);
                UpdateGraphicalTile(tileIndex, tile, neighborIndex, WallType.Straight); 
                break;

            case 73:
                SetTileToType(tileIndex, m_shortCut_wallIndex);
                UpdateGraphicalTile(tileIndex, tile, neighborIndex, WallType.Corner); 
                break;

            case 74:
                SetTileToType(tileIndex, m_shortCut_wallIndex);
                UpdateGraphicalTile(tileIndex, tile, neighborIndex, WallType.Straight);

                ProcessForWallTiles(new Vec2Int(tile.x + 1, tile.y), 1);
                break;

            case 75:
                SetTileToType(tileIndex, m_shortCut_wallIndex);
                UpdateGraphicalTile(tileIndex, tile, neighborIndex, WallType.ThreeWay); 
                break;

            case 76:
                SetTileToType(tileIndex, m_shortCut_wallIndex);
                UpdateGraphicalTile(tileIndex, tile, neighborIndex, WallType.Corner); 
                break;

            case 77:
                SetTileToType(tileIndex, m_shortCut_wallIndex);
                UpdateGraphicalTile(tileIndex, tile, neighborIndex, WallType.ThreeWay); 
                break;

            case 78:
                SetTileToType(tileIndex, m_shortCut_wallIndex);
                UpdateGraphicalTile(tileIndex, tile, neighborIndex, WallType.ThreeWay); 
                break;

            case 79:
                SetTileToType(tileIndex, m_shortCut_wallIndex);
                UpdateGraphicalTile(tileIndex, tile, neighborIndex, WallType.Cross); 
                break;

            case 80:
                SetTileToType(tileIndex, m_shortCut_wallIndex);
                UpdateGraphicalTile(tileIndex, tile, neighborIndex, WallType.Cross); 
                break;

            case 81:
                SetTileToType(tileIndex, m_shortCut_wallIndex);
                UpdateGraphicalTile(tileIndex, tile, neighborIndex, WallType.Straight); 
                break;

            case 82:
                SetTileToType(tileIndex, m_shortCut_wallIndex);
                UpdateGraphicalTile(tileIndex, tile, neighborIndex, WallType.Straight); 
                break;

            case 83:
                SetTileToType(tileIndex, m_shortCut_wallIndex);
                UpdateGraphicalTile(tileIndex, tile, neighborIndex, WallType.Corner); 
                break;

            case 84:
                SetTileToType(tileIndex, m_shortCut_wallIndex);
                UpdateGraphicalTile(tileIndex, tile, neighborIndex, WallType.Straight); 
                break;

            case 85:
                SetTileToType(tileIndex, m_shortCut_wallIndex);
                UpdateGraphicalTile(tileIndex, tile, neighborIndex, WallType.Straight); 
                break;

            case 86:
                SetTileToType(tileIndex, m_shortCut_wallIndex);
                UpdateGraphicalTile(tileIndex, tile, neighborIndex, WallType.Corner); 
                break;

            case 87:
                SetTileToType(tileIndex, m_shortCut_wallIndex);
                UpdateGraphicalTile(tileIndex, tile, neighborIndex, WallType.ThreeWay); 
                break;

            case 88:
                SetTileToType(tileIndex, m_shortCut_wallIndex);
                UpdateGraphicalTile(tileIndex, tile, neighborIndex, WallType.Straight); 
                break;

            case 89:
                SetTileToType(tileIndex, m_shortCut_wallIndex);
                UpdateGraphicalTile(tileIndex, tile, neighborIndex, WallType.Corner); 
                break;

            case 90:
                SetTileToType(tileIndex, m_shortCut_wallIndex);
                UpdateGraphicalTile(tileIndex, tile, neighborIndex, WallType.Straight); 
                break;

            case 91:
                SetTileToType(tileIndex, m_shortCut_wallIndex);
                UpdateGraphicalTile(tileIndex, tile, neighborIndex, WallType.ThreeWay); 
                break;

            case 92:
                SetTileToType(tileIndex, m_shortCut_wallIndex);
                UpdateGraphicalTile(tileIndex, tile, neighborIndex, WallType.Corner); 
                break;

            case 93:
                SetTileToType(tileIndex, m_shortCut_wallIndex);
                UpdateGraphicalTile(tileIndex, tile, neighborIndex, WallType.ThreeWay); 
                break;

            case 94:
                SetTileToType(tileIndex, m_shortCut_wallIndex);
                UpdateGraphicalTile(tileIndex, tile, neighborIndex, WallType.ThreeWay); 
                break;

            case 95:
                SetTileToType(tileIndex, m_shortCut_wallIndex);
                UpdateGraphicalTile(tileIndex, tile, neighborIndex, WallType.Cross); 
                break;

            case 96:
                SetTileToType(tileIndex, m_shortCut_wallIndex);
                UpdateGraphicalTile(tileIndex, tile, neighborIndex, WallType.Cross); 
                break;

            case 97:
                SetTileToType(tileIndex, m_shortCut_wallIndex);
                UpdateGraphicalTile(tileIndex, tile, neighborIndex, WallType.Straight); 
                break;

            case 98:
                SetTileToType(tileIndex, m_shortCut_wallIndex);
                UpdateGraphicalTile(tileIndex, tile, neighborIndex, WallType.Straight); 
                break;

            case 99:
                SetTileToType(tileIndex, m_shortCut_wallIndex);
                UpdateGraphicalTile(tileIndex, tile, neighborIndex, WallType.Corner); 
                break;

            case 100:
                SetTileToType(tileIndex, m_shortCut_wallIndex);
                UpdateGraphicalTile(tileIndex, tile, neighborIndex, WallType.Straight); 
                break;

            case 101:
                SetTileToType(tileIndex, m_shortCut_wallIndex);
                UpdateGraphicalTile(tileIndex, tile, neighborIndex, WallType.Straight);

                ProcessForWallTiles(new Vec2Int(tile.x, tile.y + 1), 1);
                ProcessForWallTiles(new Vec2Int(tile.x, tile.y - 1), 1);
                break;

            case 102:
                SetTileToType(tileIndex, m_shortCut_wallIndex);
                UpdateGraphicalTile(tileIndex, tile, neighborIndex, WallType.Corner); 
                break;

            case 103:
                SetTileToType(tileIndex, m_shortCut_wallIndex);
                UpdateGraphicalTile(tileIndex, tile, neighborIndex, WallType.ThreeWay); 
                break;

            case 104:
                SetTileToType(tileIndex, m_shortCut_wallIndex);
                UpdateGraphicalTile(tileIndex, tile, neighborIndex, WallType.Straight); 
                break;

            case 105: 
                SetTileToType(tileIndex, m_shortCut_wallIndex);
                UpdateGraphicalTile(tileIndex, tile, neighborIndex, WallType.Corner); 
                break;

            case 106: 
                SetTileToType(tileIndex, m_shortCut_wallIndex);
                UpdateGraphicalTile(tileIndex, tile, neighborIndex, WallType.Straight);

                ProcessForWallTiles(new Vec2Int(tile.x + 1 , tile.y), 1);
                break;

            case 107:
                SetTileToType(tileIndex, m_shortCut_wallIndex);
                UpdateGraphicalTile(tileIndex, tile, neighborIndex, WallType.Cross); 
                break;

            case 108: 
                SetTileToType(tileIndex, m_shortCut_wallIndex);
                UpdateGraphicalTile(tileIndex, tile, neighborIndex, WallType.Corner); 
                break;
                
            case 109: 
                SetTileToType(tileIndex, m_shortCut_wallIndex);
                UpdateGraphicalTile(tileIndex, tile, neighborIndex, WallType.ThreeWay); 
                break;

            case 110:
                SetTileToType(tileIndex, m_shortCut_wallIndex);
                UpdateGraphicalTile(tileIndex, tile, neighborIndex, WallType.ThreeWay); 
                break;

            case 111:
                SetTileToType(tileIndex, m_shortCut_wallIndex);
                UpdateGraphicalTile(tileIndex, tile, neighborIndex, WallType.ThreeWay); 
                break;

            case 112:
                SetTileToType(tileIndex, m_shortCut_wallIndex);
                UpdateGraphicalTile(tileIndex, tile, neighborIndex, WallType.Straight); 
                break;

            case 113:
                SetTileToType(tileIndex, m_shortCut_wallIndex);
                UpdateGraphicalTile(tileIndex, tile, neighborIndex, WallType.Straight); 
                break;

            case 114: 
                SetTileToType(tileIndex, m_shortCut_wallIndex);
                UpdateGraphicalTile(tileIndex, tile, neighborIndex, WallType.Straight); 
                break;

            case 115:
                SetTileToType(tileIndex, m_shortCut_wallIndex);
                UpdateGraphicalTile(tileIndex, tile, neighborIndex, WallType.Corner); 
                break;

            case 116:
                SetTileToType(tileIndex, m_shortCut_wallIndex);
                UpdateGraphicalTile(tileIndex, tile, neighborIndex, WallType.Straight); 
                break;

            case 117:
                SetTileToType(tileIndex, m_shortCut_wallIndex);
                UpdateGraphicalTile(tileIndex, tile, neighborIndex, WallType.Straight);

                ProcessForWallTiles(new Vec2Int(tile.x, tile.y - 1), 1);
                break;

            case 118:
                SetTileToType(tileIndex, m_shortCut_wallIndex);
                UpdateGraphicalTile(tileIndex, tile, neighborIndex, WallType.Corner); 
                break;

            case 119:
                SetTileToType(tileIndex, m_shortCut_wallIndex);
                UpdateGraphicalTile(tileIndex, tile, neighborIndex, WallType.ThreeWay); 
                break;

            case 120:
                SetTileToType(tileIndex, m_shortCut_wallIndex);
                UpdateGraphicalTile(tileIndex, tile, neighborIndex, WallType.Straight); 
                break;

            case 121:
                SetTileToType(tileIndex, m_shortCut_wallIndex);
                UpdateGraphicalTile(tileIndex, tile, neighborIndex, WallType.Corner); 
                break;

            case 122:
                SetTileToType(tileIndex, m_shortCut_wallIndex);
                UpdateGraphicalTile(tileIndex, tile, neighborIndex, WallType.Straight);

                ProcessForWallTiles(new Vec2Int(tile.x + 1, tile.y), 1);
                break;

            case 123: 
                SetTileToType(tileIndex, m_shortCut_wallIndex);
                UpdateGraphicalTile(tileIndex, tile, neighborIndex, WallType.ThreeWay); 
                break;

            case 124:
                SetTileToType(tileIndex, m_shortCut_wallIndex);
                UpdateGraphicalTile(tileIndex, tile, neighborIndex, WallType.Corner); 
                break;

            case 125: 
                SetTileToType(tileIndex, m_shortCut_wallIndex);
                UpdateGraphicalTile(tileIndex, tile, neighborIndex, WallType.ThreeWay); 
                break;

            case 126: 
                SetTileToType(tileIndex, m_shortCut_wallIndex);
                UpdateGraphicalTile(tileIndex, tile, neighborIndex, WallType.ThreeWay); 
                break;

            case 127:
                SetTileToType(tileIndex, m_shortCut_wallIndex);
                UpdateGraphicalTile(tileIndex, tile, neighborIndex, WallType.Cross); 
                break;

            case 128:
                SetTileToType(tileIndex, m_shortCut_wallIndex);
                UpdateGraphicalTile(tileIndex, tile, neighborIndex, WallType.Cross); 
                break;

            case 129: 
                SetTileToType(tileIndex, m_shortCut_wallIndex);
                UpdateGraphicalTile(tileIndex, tile, neighborIndex, WallType.Straight); 
                break;

            case 130: 
                SetTileToType(tileIndex, m_shortCut_wallIndex);
                UpdateGraphicalTile(tileIndex, tile, neighborIndex, WallType.Straight); 
                break;

            case 131:
                SetTileToType(tileIndex, m_shortCut_wallIndex);
                UpdateGraphicalTile(tileIndex, tile, neighborIndex, WallType.Corner); 
                break;

            case 132: 
                SetTileToType(tileIndex, m_shortCut_wallIndex);
                UpdateGraphicalTile(tileIndex, tile, neighborIndex, WallType.Straight); 
                break;

            case 133: 
                SetTileToType(tileIndex, m_shortCut_wallIndex);
                UpdateGraphicalTile(tileIndex, tile, neighborIndex, WallType.Straight);

                ProcessForWallTiles(new Vec2Int(tile.x, tile.y + 1), 1);
                break;

            case 134: 
                SetTileToType(tileIndex, m_shortCut_wallIndex);
                UpdateGraphicalTile(tileIndex, tile, neighborIndex, WallType.Corner); 
                break;

            case 135: 
                SetTileToType(tileIndex, m_shortCut_wallIndex);
                UpdateGraphicalTile(tileIndex, tile, neighborIndex, WallType.ThreeWay); 
                break;

            case 136: 
                SetTileToType(tileIndex, m_shortCut_wallIndex);
                UpdateGraphicalTile(tileIndex, tile, neighborIndex, WallType.Straight);

                ProcessForWallTiles(new Vec2Int(tile.x, tile.y - 1), 1);
                break;

            case 137: 
                SetTileToType(tileIndex, m_shortCut_wallIndex);
                UpdateGraphicalTile(tileIndex, tile, neighborIndex, WallType.Corner); 
                break;

            case 138: 
                SetTileToType(tileIndex, m_shortCut_wallIndex);
                UpdateGraphicalTile(tileIndex, tile, neighborIndex, WallType.Straight);

                ProcessForWallTiles(new Vec2Int(tile.x - 1, tile.y), 1);
                break;

            case 139: 
                SetTileToType(tileIndex, m_shortCut_wallIndex);
                UpdateGraphicalTile(tileIndex, tile, neighborIndex, WallType.ThreeWay); 
                break;

            case 140: 
                SetTileToType(tileIndex, m_shortCut_wallIndex);
                UpdateGraphicalTile(tileIndex, tile, neighborIndex, WallType.Corner); 
                break;

            case 141: 
                SetTileToType(tileIndex, m_shortCut_wallIndex);
                UpdateGraphicalTile(tileIndex, tile, neighborIndex, WallType.ThreeWay); 
                break;

            case 142: 
                SetTileToType(tileIndex, m_shortCut_wallIndex);
                UpdateGraphicalTile(tileIndex, tile, neighborIndex, WallType.ThreeWay); 
                break;

            case 143: 
                SetTileToType(tileIndex, m_shortCut_wallIndex);
                UpdateGraphicalTile(tileIndex, tile, neighborIndex, WallType.Cross);                 
                break;

            case 144: 
                SetTileToType(tileIndex, m_shortCut_wallIndex);
                UpdateGraphicalTile(tileIndex, tile, neighborIndex, WallType.Cross); 
                break;

            case 145: 
                SetTileToType(tileIndex, m_shortCut_wallIndex);
                UpdateGraphicalTile(tileIndex, tile, neighborIndex, WallType.Straight); 
                break;

            case 146: 
                SetTileToType(tileIndex, m_shortCut_wallIndex);
                UpdateGraphicalTile(tileIndex, tile, neighborIndex, WallType.Straight); 
                break;

            case 147: 
                SetTileToType(tileIndex, m_shortCut_wallIndex);
                UpdateGraphicalTile(tileIndex, tile, neighborIndex, WallType.Corner); 
                break;

            case 148: 
                SetTileToType(tileIndex, m_shortCut_wallIndex);
                UpdateGraphicalTile(tileIndex, tile, neighborIndex, WallType.Straight); 
                break;

            case 149: 
                SetTileToType(tileIndex, m_shortCut_wallIndex);
                UpdateGraphicalTile(tileIndex, tile, neighborIndex, WallType.Straight);

                ProcessForWallTiles(new Vec2Int(tile.x, tile.y + 1), 1);
                ProcessForWallTiles(new Vec2Int(tile.x, tile.y - 1), 1);
                break;

            case 150: 
                SetTileToType(tileIndex, m_shortCut_wallIndex);
                UpdateGraphicalTile(tileIndex, tile, neighborIndex, WallType.Corner); 
                break;

            case 151: 
                SetTileToType(tileIndex, m_shortCut_wallIndex);
                UpdateGraphicalTile(tileIndex, tile, neighborIndex, WallType.ThreeWay); 
                break;

            case 152: 
                SetTileToType(tileIndex, m_shortCut_wallIndex);
                UpdateGraphicalTile(tileIndex, tile, neighborIndex, WallType.Straight); 
                break;

            case 153: 
                SetTileToType(tileIndex, m_shortCut_wallIndex);
                UpdateGraphicalTile(tileIndex, tile, neighborIndex, WallType.Corner); 
                break;

            case 154: 
                SetTileToType(tileIndex, m_shortCut_wallIndex);
                UpdateGraphicalTile(tileIndex, tile, neighborIndex, WallType.Straight);

                ProcessForWallTiles(new Vec2Int(tile.x - 1, tile.y), 1);
                break;

            case 155: 
                SetTileToType(tileIndex, m_shortCut_wallIndex);
                UpdateGraphicalTile(tileIndex, tile, neighborIndex, WallType.ThreeWay); 
                break;

            case 156: 
                SetTileToType(tileIndex, m_shortCut_wallIndex);
                UpdateGraphicalTile(tileIndex, tile, neighborIndex, WallType.Corner); 
                break;

            case 157: 
                SetTileToType(tileIndex, m_shortCut_wallIndex);
                UpdateGraphicalTile(tileIndex, tile, neighborIndex, WallType.ThreeWay); 
                break;

            case 158: 
                SetTileToType(tileIndex, m_shortCut_wallIndex);
                UpdateGraphicalTile(tileIndex, tile, neighborIndex, WallType.ThreeWay); 
                break;

            case 159: 
                SetTileToType(tileIndex, m_shortCut_wallIndex);
                UpdateGraphicalTile(tileIndex, tile, neighborIndex, WallType.Cross); 
                break;

            case 160: 
                SetTileToType(tileIndex, m_shortCut_wallIndex);
                UpdateGraphicalTile(tileIndex, tile, neighborIndex, WallType.Cross); 
                break;

            case 161: 
                SetTileToType(tileIndex, m_shortCut_wallIndex);
                UpdateGraphicalTile(tileIndex, tile, neighborIndex, WallType.Straight); 
                break;

            case 162: 
                SetTileToType(tileIndex, m_shortCut_wallIndex);
                UpdateGraphicalTile(tileIndex, tile, neighborIndex, WallType.Straight); 
                break;

            case 163: 
                SetTileToType(tileIndex, m_shortCut_wallIndex);
                UpdateGraphicalTile(tileIndex, tile, neighborIndex, WallType.Corner); 
                break;

            case 164: 
                SetTileToType(tileIndex, m_shortCut_wallIndex);
                UpdateGraphicalTile(tileIndex, tile, neighborIndex, WallType.Straight); 
                break;

            case 165: 
                SetTileToType(tileIndex, m_shortCut_wallIndex);
                UpdateGraphicalTile(tileIndex, tile, neighborIndex, WallType.Straight); 
                break;

            case 166: 
                SetTileToType(tileIndex, m_shortCut_wallIndex);
                UpdateGraphicalTile(tileIndex, tile, neighborIndex, WallType.Corner); 
                break;

            case 167: 
                SetTileToType(tileIndex, m_shortCut_wallIndex);
                UpdateGraphicalTile(tileIndex, tile, neighborIndex, WallType.ThreeWay); 
                break;

            case 168: 
                SetTileToType(tileIndex, m_shortCut_wallIndex);
                UpdateGraphicalTile(tileIndex, tile, neighborIndex, WallType.Straight); 
                break;

            case 169: 
                SetTileToType(tileIndex, m_shortCut_wallIndex);
                UpdateGraphicalTile(tileIndex, tile, neighborIndex, WallType.Corner); 
                break;

            case 170: 
                SetTileToType(tileIndex, m_shortCut_wallIndex);
                UpdateGraphicalTile(tileIndex, tile, neighborIndex, WallType.Straight); 
                break;

            case 171: 
                SetTileToType(tileIndex, m_shortCut_wallIndex);
                UpdateGraphicalTile(tileIndex, tile, neighborIndex, WallType.ThreeWay); 
                break;

            case 172: 
                SetTileToType(tileIndex, m_shortCut_wallIndex);
                UpdateGraphicalTile(tileIndex, tile, neighborIndex, WallType.Corner); 
                break;

            case 173: 
                SetTileToType(tileIndex, m_shortCut_wallIndex);
                UpdateGraphicalTile(tileIndex, tile, neighborIndex, WallType.ThreeWay); 
                break;

            case 174: 
                SetTileToType(tileIndex, m_shortCut_wallIndex);
                UpdateGraphicalTile(tileIndex, tile, neighborIndex, WallType.ThreeWay); 
                break;

            case 175: 
                SetTileToType(tileIndex, m_shortCut_wallIndex);
                UpdateGraphicalTile(tileIndex, tile, neighborIndex, WallType.Cross); 
                break;

            case 176: 
                SetTileToType(tileIndex, m_shortCut_wallIndex);
                UpdateGraphicalTile(tileIndex, tile, neighborIndex, WallType.Cross); 
                break;

            case 177: 
                SetTileToType(tileIndex, m_shortCut_wallIndex);
                UpdateGraphicalTile(tileIndex, tile, neighborIndex, WallType.Straight); 
                break;

            case 178: 
                SetTileToType(tileIndex, m_shortCut_wallIndex);
                UpdateGraphicalTile(tileIndex, tile, neighborIndex, WallType.Straight); 
                break;

            case 179: 
                SetTileToType(tileIndex, m_shortCut_wallIndex);
                UpdateGraphicalTile(tileIndex, tile, neighborIndex, WallType.Corner); 
                break;

            case 180: 
                SetTileToType(tileIndex, m_shortCut_wallIndex);
                UpdateGraphicalTile(tileIndex, tile, neighborIndex, WallType.Straight); 
                break;

            case 181: 
                SetTileToType(tileIndex, m_shortCut_wallIndex);
                UpdateGraphicalTile(tileIndex, tile, neighborIndex, WallType.Straight);

                ProcessForWallTiles(new Vec2Int(tile.x, tile.y - 1 ), 1);
                break;

            case 182: 
                SetTileToType(tileIndex, m_shortCut_wallIndex);
                UpdateGraphicalTile(tileIndex, tile, neighborIndex, WallType.Corner); 
                break;

            case 183: 
                SetTileToType(tileIndex, m_shortCut_wallIndex);
                UpdateGraphicalTile(tileIndex, tile, neighborIndex, WallType.ThreeWay); 
                break;

            case 184: 
                SetTileToType(tileIndex, m_shortCut_wallIndex);
                UpdateGraphicalTile(tileIndex, tile, neighborIndex, WallType.Straight); 
                break;

            case 185: 
                SetTileToType(tileIndex, m_shortCut_wallIndex);
                UpdateGraphicalTile(tileIndex, tile, neighborIndex, WallType.Corner); 
                break;

            case 186: 
                SetTileToType(tileIndex, m_shortCut_wallIndex);
                UpdateGraphicalTile(tileIndex, tile, neighborIndex, WallType.Straight);

                ProcessForWallTiles(new Vec2Int(tile.x - 1, tile.y), 1);
                break;

            case 187: 
                SetTileToType(tileIndex, m_shortCut_wallIndex);
                UpdateGraphicalTile(tileIndex, tile, neighborIndex, WallType.ThreeWay); 
                break;

            case 188: 
                SetTileToType(tileIndex, m_shortCut_wallIndex);
                UpdateGraphicalTile(tileIndex, tile, neighborIndex, WallType.Corner); 
                break;

            case 189: 
                SetTileToType(tileIndex, m_shortCut_wallIndex);
                UpdateGraphicalTile(tileIndex, tile, neighborIndex, WallType.ThreeWay); 
                break;

            case 190: 
                SetTileToType(tileIndex, m_shortCut_wallIndex);
                UpdateGraphicalTile(tileIndex, tile, neighborIndex, WallType.ThreeWay); 
                break;

            case 191: 
                SetTileToType(tileIndex, m_shortCut_wallIndex);
                UpdateGraphicalTile(tileIndex, tile, neighborIndex, WallType.Cross); 
                break;

            case 192: 
                SetTileToType(tileIndex, m_shortCut_wallIndex);
                UpdateGraphicalTile(tileIndex, tile, neighborIndex, WallType.Cross); 
                break;

            case 193: 
                SetTileToType(tileIndex, m_shortCut_wallIndex);
                UpdateGraphicalTile(tileIndex, tile, neighborIndex, WallType.Cross); 
                break;

            case 194: 
                SetTileToType(tileIndex, m_shortCut_wallIndex);
                UpdateGraphicalTile(tileIndex, tile, neighborIndex, WallType.Cross); 
                break;

            case 195: 
                SetTileToType(tileIndex, m_shortCut_wallIndex);
                UpdateGraphicalTile(tileIndex, tile, neighborIndex, WallType.Corner); 
                break;

            case 196: 
                SetTileToType(tileIndex, m_shortCut_wallIndex);
                UpdateGraphicalTile(tileIndex, tile, neighborIndex, WallType.Straight); 
                break;

            case 197: 
                SetTileToType(tileIndex, m_shortCut_wallIndex);
                UpdateGraphicalTile(tileIndex, tile, neighborIndex, WallType.Straight);

                ProcessForWallTiles(new Vec2Int(tile.x, tile.y + 1), 1);
                break;

            case 198: 
                SetTileToType(tileIndex, m_shortCut_wallIndex);
                UpdateGraphicalTile(tileIndex, tile, neighborIndex, WallType.Corner); 
                break;

            case 199: 
                SetTileToType(tileIndex, m_shortCut_wallIndex);
                UpdateGraphicalTile(tileIndex, tile, neighborIndex, WallType.ThreeWay); 
                break;

            case 200: 
                SetTileToType(tileIndex, m_shortCut_wallIndex);
                UpdateGraphicalTile(tileIndex, tile, neighborIndex, WallType.Straight); 
                break;

            case 201: 
                SetTileToType(tileIndex, m_shortCut_wallIndex);
                UpdateGraphicalTile(tileIndex, tile, neighborIndex, WallType.Corner); 
                break;

            case 202: 
                SetTileToType(tileIndex, m_shortCut_wallIndex);
                UpdateGraphicalTile(tileIndex, tile, neighborIndex, WallType.Straight);

                ProcessForWallTiles(new Vec2Int(tile.x + 1, tile.y), 1);
                ProcessForWallTiles(new Vec2Int(tile.x - 1, tile.y), 1);
                break;

            case 203: 
                SetTileToType(tileIndex, m_shortCut_wallIndex);
                UpdateGraphicalTile(tileIndex, tile, neighborIndex, WallType.ThreeWay); 
                break;

            case 204: 
                SetTileToType(tileIndex, m_shortCut_wallIndex);
                UpdateGraphicalTile(tileIndex, tile, neighborIndex, WallType.Corner); 
                break;

            case 205: 
                SetTileToType(tileIndex, m_shortCut_wallIndex);
                UpdateGraphicalTile(tileIndex, tile, neighborIndex, WallType.ThreeWay); 
                break;

            case 206: 
                SetTileToType(tileIndex, m_shortCut_wallIndex);
                UpdateGraphicalTile(tileIndex, tile, neighborIndex, WallType.ThreeWay); 
                break;

            case 207: 
                SetTileToType(tileIndex, m_shortCut_wallIndex);
                UpdateGraphicalTile(tileIndex, tile, neighborIndex, WallType.Cross); 
                break;

            case 208: 
                SetTileToType(tileIndex, m_shortCut_wallIndex);
                UpdateGraphicalTile(tileIndex, tile, neighborIndex, WallType.Straight);

                ProcessForWallTiles(new Vec2Int(tile.x + 1, tile.y), 1);
                break;

            case 209: 
                SetTileToType(tileIndex, m_shortCut_wallIndex);
                UpdateGraphicalTile(tileIndex, tile, neighborIndex, WallType.Straight); 
                break;

            case 210: 
                SetTileToType(tileIndex, m_shortCut_wallIndex);
                UpdateGraphicalTile(tileIndex, tile, neighborIndex, WallType.Straight); 
                break;

            case 211: 
                SetTileToType(tileIndex, m_shortCut_wallIndex);
                UpdateGraphicalTile(tileIndex, tile, neighborIndex, WallType.Straight); 
                break;

            case 212: 
                SetTileToType(tileIndex, m_shortCut_wallIndex);
                UpdateGraphicalTile(tileIndex, tile, neighborIndex, WallType.Corner); 
                break;

            case 213: 
                SetTileToType(tileIndex, m_shortCut_wallIndex);
                UpdateGraphicalTile(tileIndex, tile, neighborIndex, WallType.Straight);

                ProcessForWallTiles(new Vec2Int(tile.x, tile.y + 1), 1);
                break;

            case 214: 
                SetTileToType(tileIndex, m_shortCut_wallIndex);
                UpdateGraphicalTile(tileIndex, tile, neighborIndex, WallType.Corner); 
                break;

            case 215: 
                SetTileToType(tileIndex, m_shortCut_wallIndex);
                UpdateGraphicalTile(tileIndex, tile, neighborIndex, WallType.ThreeWay); 
                break;

            case 216: 
                SetTileToType(tileIndex, m_shortCut_wallIndex);
                UpdateGraphicalTile(tileIndex, tile, neighborIndex, WallType.Straight); 
                break;

            case 217: 
                SetTileToType(tileIndex, m_shortCut_wallIndex);
                UpdateGraphicalTile(tileIndex, tile, neighborIndex, WallType.Corner); 
                break;

            case 218: 
                SetTileToType(tileIndex, m_shortCut_wallIndex);
                UpdateGraphicalTile(tileIndex, tile, neighborIndex, WallType.Straight);

                ProcessForWallTiles(new Vec2Int(tile.x - 1, tile.y), 1);
                break;

            case 219: 
                SetTileToType(tileIndex, m_shortCut_wallIndex);
                UpdateGraphicalTile(tileIndex, tile, neighborIndex, WallType.ThreeWay); 
                break;

            case 220: 
                SetTileToType(tileIndex, m_shortCut_wallIndex);
                UpdateGraphicalTile(tileIndex, tile, neighborIndex, WallType.Corner); 
                break;

            case 221: 
                SetTileToType(tileIndex, m_shortCut_wallIndex);
                UpdateGraphicalTile(tileIndex, tile, neighborIndex, WallType.ThreeWay); 
                break;

            case 222: 
                SetTileToType(tileIndex, m_shortCut_wallIndex);
                UpdateGraphicalTile(tileIndex, tile, neighborIndex, WallType.ThreeWay); 
                break;

            case 223: 
                SetTileToType(tileIndex, m_shortCut_wallIndex);
                UpdateGraphicalTile(tileIndex, tile, neighborIndex, WallType.Cross); 
                break;

            case 224: 
                SetTileToType(tileIndex, m_shortCut_wallIndex);
                UpdateGraphicalTile(tileIndex, tile, neighborIndex, WallType.Straight); 
                break;

            case 225: 
                SetTileToType(tileIndex, m_shortCut_wallIndex);
                UpdateGraphicalTile(tileIndex, tile, neighborIndex, WallType.Straight); 
                break;

            case 226: 
                SetTileToType(tileIndex, m_shortCut_wallIndex);
                UpdateGraphicalTile(tileIndex, tile, neighborIndex, WallType.Straight); 
                break;

            case 227: 
                SetTileToType(tileIndex, m_shortCut_wallIndex);
                UpdateGraphicalTile(tileIndex, tile, neighborIndex, WallType.Corner); 
                break;

            case 228: 
                SetTileToType(tileIndex, m_shortCut_wallIndex);
                UpdateGraphicalTile(tileIndex, tile, neighborIndex, WallType.Straight); 
                break;

            case 229: 
                SetTileToType(tileIndex, m_shortCut_wallIndex);
                UpdateGraphicalTile(tileIndex, tile, neighborIndex, WallType.Straight);

                ProcessForWallTiles(new Vec2Int(tile.x, tile.y + 1), 1);
                break;

            case 230: 
                SetTileToType(tileIndex, m_shortCut_wallIndex);
                UpdateGraphicalTile(tileIndex, tile, neighborIndex, WallType.Corner); 
                break;

            case 231: 
                SetTileToType(tileIndex, m_shortCut_wallIndex);
                UpdateGraphicalTile(tileIndex, tile, neighborIndex, WallType.ThreeWay); 
                break;

            case 232: 
                SetTileToType(tileIndex, m_shortCut_wallIndex);
                UpdateGraphicalTile(tileIndex, tile, neighborIndex, WallType.Straight); 
                break;

            case 233: 
                SetTileToType(tileIndex, m_shortCut_wallIndex);
                UpdateGraphicalTile(tileIndex, tile, neighborIndex, WallType.Corner); 
                break;

            case 234: 
                SetTileToType(tileIndex, m_shortCut_wallIndex);
                UpdateGraphicalTile(tileIndex, tile, neighborIndex, WallType.Straight);

                ProcessForWallTiles(new Vec2Int(tile.x + 1, tile.y), 1);
                break;

            case 235: 
                SetTileToType(tileIndex, m_shortCut_wallIndex);
                UpdateGraphicalTile(tileIndex, tile, neighborIndex, WallType.ThreeWay); 
                break;

            case 236: 
                SetTileToType(tileIndex, m_shortCut_wallIndex);
                UpdateGraphicalTile(tileIndex, tile, neighborIndex, WallType.Corner); 
                break;

            case 237: 
                SetTileToType(tileIndex, m_shortCut_wallIndex);
                UpdateGraphicalTile(tileIndex, tile, neighborIndex, WallType.ThreeWay); 
                break;

            case 238: 
                SetTileToType(tileIndex, m_shortCut_wallIndex);
                UpdateGraphicalTile(tileIndex, tile, neighborIndex, WallType.ThreeWay); 
                break;

            case 239: 
                SetTileToType(tileIndex, m_shortCut_wallIndex);
                UpdateGraphicalTile(tileIndex, tile, neighborIndex, WallType.Cross); 
                break;

            case 240: 
                SetTileToType(tileIndex, m_shortCut_wallIndex);
                UpdateGraphicalTile(tileIndex, tile, neighborIndex, WallType.Straight); 
                break;

            case 241: 
                SetTileToType(tileIndex, m_shortCut_wallIndex);
                UpdateGraphicalTile(tileIndex, tile, neighborIndex, WallType.Straight); 
                break;

            case 242: 
                SetTileToType(tileIndex, m_shortCut_wallIndex);
                UpdateGraphicalTile(tileIndex, tile, neighborIndex, WallType.Straight); 
                break;

            case 243: 
                SetTileToType(tileIndex, m_shortCut_wallIndex);
                UpdateGraphicalTile(tileIndex, tile, neighborIndex, WallType.ThreeWay); 
                break;

            case 244: 
                SetTileToType(tileIndex, m_shortCut_wallIndex);
                UpdateGraphicalTile(tileIndex, tile, neighborIndex, WallType.Straight); 
                break;

            case 245: 
                SetTileToType(tileIndex, m_shortCut_wallIndex);
                UpdateGraphicalTile(tileIndex, tile, neighborIndex, WallType.Straight); 
                break;

            case 246: 
                SetTileToType(tileIndex, m_shortCut_wallIndex);
                UpdateGraphicalTile(tileIndex, tile, neighborIndex, WallType.Corner); 
                break;

            case 247: 
                SetTileToType(tileIndex, m_shortCut_wallIndex);
                UpdateGraphicalTile(tileIndex, tile, neighborIndex, WallType.ThreeWay); 
                break;

            case 248: 
                SetTileToType(tileIndex, m_shortCut_wallIndex);
                UpdateGraphicalTile(tileIndex, tile, neighborIndex, WallType.Straight); 
                break;

            case 249: 
                SetTileToType(tileIndex, m_shortCut_wallIndex);
                UpdateGraphicalTile(tileIndex, tile, neighborIndex, WallType.Corner); 
                break;

            case 250: 
                SetTileToType(tileIndex, m_shortCut_wallIndex);
                UpdateGraphicalTile(tileIndex, tile, neighborIndex, WallType.Straight); 
                break;

            case 251: 
                SetTileToType(tileIndex, m_shortCut_wallIndex);
                UpdateGraphicalTile(tileIndex, tile, neighborIndex, WallType.ThreeWay); 
                break;

            case 252: 
                SetTileToType(tileIndex, m_shortCut_wallIndex);
                UpdateGraphicalTile(tileIndex, tile, neighborIndex, WallType.Corner); 
                break;

            case 253: 
                SetTileToType(tileIndex, m_shortCut_wallIndex);
                UpdateGraphicalTile(tileIndex, tile, neighborIndex, WallType.ThreeWay); 
                break;

            case 254: 
                SetTileToType(tileIndex, m_shortCut_wallIndex);
                UpdateGraphicalTile(tileIndex, tile, neighborIndex, WallType.ThreeWay); 
                break;

            case 255: 
                SetTileToType(tileIndex, m_shortCut_wallIndex);
                UpdateGraphicalTile(tileIndex, tile, neighborIndex, WallType.Cross); 
                break;

            default:
                Debug.LogError(string.Format("Unhandled case {0}", neighborIndex));
                break;
        }
        #endregion
    }

    //private void ProcessTile(Vec2Int tilePos, int goDeeper)
    //{
    //    if (goDeeper < 1)
    //        return;

    //    goDeeper--;

    //    int tileIndex = ConvertTwoIntToInt(tilePos.x, tilePos.y);
    //    int neighborIndex = ComputeEmptyNeighborIndex(tilePos.x, tilePos.y);
    //    int otherNeighbor = 0;

    //    Debug.Log(string.Format("x {0} z {1}: neighborindex: {2}", tilePos.x, tilePos.y, neighborIndex));

    //    switch (neighborIndex)
    //    {
    //        //3 Way
    //        case 16:
    //        case 32:
    //        case 64:
    //        case 128:
    //            SetTileToType(tileIndex, m_shortCut_wallIndex);
    //            UpdateGraphicalTile(tileIndex, tilePos, neighborIndex, WallType.ThreeWay);
    //            break;

    //        //Straight wall, but also should update its neighbor to the left
    //        case 17:
    //        case 132:
    //            SetTileToType(tileIndex, m_shortCut_wallIndex);
    //            UpdateGraphicalTile(tileIndex, tilePos, neighborIndex, WallType.Straight);

    //            //otherNeighbor = ComputeEmptyNeighborIndex(tilePos.x + 1, tilePos.y);
    //            //ProcessTile(new Vec2Int(tilePos.x + 1, tilePos.y), goDeeper);
    //            break;

    //        //Straight wall, but also should update its neighbor to the right
    //        case 33:
    //        case 68:
    //            SetTileToType(tileIndex, m_shortCut_wallIndex);
    //            UpdateGraphicalTile(tileIndex, tilePos, neighborIndex, WallType.Straight);

    //            //otherNeighbor = ComputeEmptyNeighborIndex(tilePos.x - 1, tilePos.y);
    //            //ProcessTile(new Vec2Int(tilePos.x - 1, tilePos.y), goDeeper);
    //            break;

    //            //Straight wall, but also should update its neighbor to top
    //        case 24:
    //        case 34:
    //            SetTileToType(tileIndex, m_shortCut_wallIndex);
    //            UpdateGraphicalTile(tileIndex, tilePos, neighborIndex, WallType.Straight);

    //            //otherNeighbor = ComputeEmptyNeighborIndex(tilePos.x, tilePos.y + 1);
    //            //ProcessTile(new Vec2Int(tilePos.x, tilePos.y + 1), goDeeper);
    //            break;

    //        //Straight wall piece
    //        case 49:
    //        case 66:
    //        case 98:            
    //        case 136:
    //        case 152:
    //        case 196:
    //        case 245:
    //        case 250:
    //            SetTileToType(tileIndex, m_shortCut_wallIndex);
    //            UpdateGraphicalTile(tileIndex, tilePos, neighborIndex, WallType.Straight);
    //            break;

    //        //Corner wall piece
    //        case 185:
    //        case 230:
    //        case 220:
    //        case 115:
    //            SetTileToType(tileIndex, m_shortCut_wallIndex);
    //            UpdateGraphicalTile(tileIndex, tilePos, neighborIndex, WallType.Corner);
    //            break;

    //        //4 way cross piece
    //        case 255:
    //        case 247:
    //        case 251:
    //        case 253:
    //        case 254:
    //            SetTileToType(tileIndex, m_shortCut_wallIndex);
    //            UpdateGraphicalTile(tileIndex, tilePos, neighborIndex, WallType.Cross);
    //            break;

    //        //3 Way
    //        //case 33:
    //        //case 68:
    //        //    m_groundTiles[tileIndex] = (ushort)GroundTileType.Wall;
    //        //    UpdateGraphicalTile(tileIndex, tilePos, neighborIndex, WallType.ThreeWay);
    //        //    break;

    //        default:
    //            //Place a basic ground tile
    //            SetTileToType(tileIndex, m_shortCut_groundIndex);
    //            UpdateGraphicalTile(tileIndex, tilePos, neighborIndex);
    //            break;
    //    }
    //}

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
            Color tileColor = new Color(0.94f, 0.87f, 0f, 0.9f);
            int index = ConvertVec2IntToInt(m_tempSelectedTiles[i]);
            if (m_groundTiles[index] != 0)
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
            Color tileColor = new Color(0.94f, 0.87f, 0f, 0.9f);
            int index = ConvertVec2IntToInt(m_selectedTiles[i]);
            if (m_groundTiles[index] != 0)
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
    #endregion

    #region Graphical Components
    /// <summary>
    /// Used by ground (since there's only one type)
    /// </summary>
    public void UpdateGraphicalTile(int tileIndex, Vec2Int xz, int neighborIndex)
    {
        TileObject tilePiece = CacheManager.Singleton.RequestGroundTile(); //(TileObject)GameObject.Instantiate(PrefabAssets.Singleton.prefabGroundTile);
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
    public void UpdateGraphicalTile(int tileIndex, Vec2Int xz, int neighborIndex, WallType wallType)
    {
        WallObject wallPiece = null; //m_tilePos
        float rot = 0f;

        if (m_groundTilesGOs[tileIndex] == null)
        {

            if (wallType == WallType.Corner)
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
            else if (wallType == WallType.Cross)
            {
                wallPiece = CacheManager.Singleton.RequestWallCross();//(WallObject)GameObject.Instantiate(PrefabAssets.Singleton.prefabWallCross);
            }
            else if (wallType == WallType.ThreeWay)
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

                //if (neighborIndex == 32 || neighborIndex == 64)
                //{
                //    rot = wallPiece.defaultOrientation + 90f;
                //}
                //else if(neighborIndex == 16 || neighborIndex == 32)
                //{
                //    rot = wallPiece.defaultOrientation + 180f;
                //}
                //else if (neighborIndex == 24 || neighborIndex == 32)
                //{
                //    rot = wallPiece.defaultOrientation + 180f;
                //}
            }
            else //if (wallType == WallType.Straight)
            {
                wallPiece = CacheManager.Singleton.RequestWallStraight();//(WallObject)GameObject.Instantiate(PrefabAssets.Singleton.prefabWallStraight);

                //Horizontal
                if (neighborIndex == 37 || neighborIndex == 149 || neighborIndex == 133 || neighborIndex == 21 || neighborIndex == 69 || neighborIndex == 5 || neighborIndex == 101
                    || neighborIndex == 229 || neighborIndex == 213 || neighborIndex == 117 || neighborIndex == 181 || neighborIndex == 197 || neighborIndex == 53 )
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
                    case WallType.Corner:
                        CacheManager.Singleton.ReturnWallCorner(wallPiece);
                        break;

                    case WallType.Cross:
                        CacheManager.Singleton.ReturnWallCross(wallPiece);
                        break;

                    case WallType.Straight:
                        CacheManager.Singleton.ReturnWallStraight(wallPiece);
                        break;

                    case WallType.ThreeWay:
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
        //wallPiece.transform.parent = this.transform;
        //wallPiece.transform.position = new Vector3(xz.x + wallPiece.defaultOffset.x, wallPiece.defaultOffset.y, xz.y + wallPiece.defaultOffset.z);
        //wallPiece.gameObject.SetActive(true);
        //wallPiece.transform.rotation = Quaternion.Euler(0f, rot, 0f);
        //wallPiece.SetTilePos(xz);
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

    public void AttemptBuildTile(PlayerCharacter pc, Vec2Int tilePos, Vector3 clickPosition)
    {
        float dist = Vector3.SqrMagnitude(clickPosition - pc.transform.position);
        float testDist = 3f * 3f;

        int index = (tilePos.x * m_groundTileDim.x) + tilePos.y;

        if (index < 0 || index > m_groundTiles.Length)
        {
            return;
        }

        GroundManager.GroundTileType tileType = (GroundManager.GroundTileType)m_groundTiles[index];

        if (dist < testDist + 0.00001f && tileType != GroundTileType.None)
        {
            //SetTileToType(index, m_shortCut_groundIndex);
            //m_groundTilesGOs[index].renderer.material = DefaultFilesManager.Singleton.builtTile;

            if (m_groundTilesGOs[index] == null)
            {
                Debug.LogError("Error");
            }

            m_groundTilesGOs[index].BuildObject();
        }
    }
}