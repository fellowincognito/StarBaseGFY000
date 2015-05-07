/*
 * StarBase GFY000
 * GameSceneManager
 * Handles the local game scene for the player, including mouse stuff, and whatever game state things
 * Created 7 May 2015
 * Author: Fellow Incognito (fellowincognito@gmail.com)
 * SEE LICENSE FOR TERMS AND RIGHTS
 */

using UnityEngine;
using System.Collections;

public class GameSceneManager : MonoBehaviour
{
    #region Enums
    public enum LocalPlayerState
    {
        None,
        Administrator,
        Player
    }

    public enum AdministratorMode
    {
        None,
        GroundSelection,
        ObjectSelection,
        GroundPlacement,
        ObjectPlacement
    }

    public enum PlayerMode
    {
        None,
        Moving
    }
    #endregion

    #region Vars
    protected LocalPlayerState m_localPlayerState;
    protected AdministratorMode m_adminModeState;
    protected PlayerMode m_playerModeState;
    #endregion
}