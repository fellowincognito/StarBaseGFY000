/*
 * StarBase GFY000
 * TempNetworkPlaceHolder
 * A temporary class that represents the incoming and outgoing portions of the network, which is not yet written
 * Created 17 April 2015
 * Author: Fellow Incognito (fellowincognito@gmail.com)
 * SEE LICENSE FOR TERMS AND RIGHTS
 */

using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class TempNetworkPlaceHolder : MonoBehaviour
{
    #region Singleton
    protected static TempNetworkPlaceHolder _singleton = null;
    public static TempNetworkPlaceHolder Singleton
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

    #region Client
    /// <summary>
    /// Client calls this when submitting a list of tiles to the server. Server will then authenticate tiles, update its copy of the tiles as appropriate, and rebroadcast as necessary to other players
    /// </summary>
    /// <param name="selectedTiles"></param>
    public void Client_SendBuildRoomTiles(int[] selectedTiles)
    {
        //For now, we just reroute the list to server portion of ground manager
        GroundManager.Singleton.Server_ReceiveBuildRoomTiles(selectedTiles);
    }

    /// <summary>
    /// Client calls this when submitting a tile index that they are trying to build
    /// </summary>
    /// <param name="tileIndex"></param>
    public void Client_SendAssembleTile(int tileIndex)
    {
        //For now, reroute to Ground Manager directly
        GroundManager.Singleton.Server_ReceiveAssembleTile(tileIndex);
    }

    public void Client_SendObjectPlacement(GroundManager.GroundTileType tileType)
    {

    }
    #endregion

    #region Server
    /// <summary>
    /// Server calls this to send approved tiles to players for when a room is built. It'll be up to the player to determine the correct walls
    /// </summary>
    /// <param name="approvedTiles"></param>
    public void Server_SendBuildRoomTiles(int[] approvedTiles)
    {
        //For now, just reroute the list to client portion of ground manager
        GroundManager.Singleton.Client_ReceiveBuildRoomTiles(approvedTiles);
    }

    /// <summary>
    /// Server calls this to send to players that a tile index has been assembled (hologram been replaced with solid wall)
    /// </summary>
    /// <param name="tileIndex"></param>
    public void Server_SendAssembleTile(int tileIndex)
    {
        //For now reroute to client
        GroundManager.Singleton.Client_ReceiveTileAssemble(tileIndex);
    }
    #endregion
}