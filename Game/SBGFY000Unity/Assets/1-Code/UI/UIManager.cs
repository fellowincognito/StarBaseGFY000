/*
 * StarBase GFY000
 * UIManager
 * The organizer and main driving force behind the UI. Separated from the UI Canvas to maintain a decoupled approach with MVC in mind
 * Created 12 April 2015
 * Author: Fellow Incognito (fellowincognito@gmail.com)
 * SEE LICENSE FOR TERMS AND RIGHTS
 */

using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class UIManager : MonoBehaviour
{
    #region Singleton
    protected static UIManager _singleton = null;
    public static UIManager Singleton
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
    public enum UIMenuState
    {
        TopLevel,
        Administrator,
        Character
    }

    public enum CreationAction
    {
        None,
        Room,
        Wall,
        Floor,
        Object,
        Demolish
    }

    public enum UIObjectType
    {
        None,
        Door
    }
    #endregion

    public Canvas UICanvasRoot;
    public GameObject UIAdministratorPanel;
    public GameObject UIBuildPanel;
    public GameObject UIConfirmationPanel;
    public GameObject UIObjectPanel; 

    public Text UIMousePos;

    [SerializeField]
    protected bool m_isInBuildingMode;
    [SerializeField]
    protected CreationAction m_currentPendingCreationAction;
    [SerializeField]
    protected UIObjectType m_currentPendingObjectType;

    #region Gets/Sets
    public CreationAction PendingCreationAction
    {
        get { return m_currentPendingCreationAction; }
    }

    public UIObjectType PendingObjectType
    {
        get { return m_currentPendingObjectType; }
    }
    #endregion

    public void Init()
    {
    }

    public void ButtonClick(UIButton button, UIButton.ButtonType buttonType)
    {
        bool success = false;

        switch (buttonType)
        {
                //Accept/Cancel current actions (submit stuff to the server for authentication and propegation to other players)
            case UIButton.ButtonType.AcceptActionButton:
                success = ButtonAcceptAction();
                break;
            case UIButton.ButtonType.CancelActionButton:
                success = ButtonCancelAction();
                break;
            
                //Used to switch between character and build mode
            case UIButton.ButtonType.BuildButton:
                success = ButtonBuildAction();
                break;            
            case UIButton.ButtonType.CharacterButton:
                success = ButtonCharacterAction();
                break;

                //Creation actions. Highly dependent on several states
            case UIButton.ButtonType.CreateDemolishButton:
                success = ButtonDemolishAction();
                break;
            case UIButton.ButtonType.CreateFloorButton:
                success = ButtonFloorAction();
                break;
            case UIButton.ButtonType.CreateObjectButton:
                success = ButtonObjectAction();
                break;
            case UIButton.ButtonType.CreateRoomButton:
                success = ButtonRoomAction();
                break;
            case UIButton.ButtonType.CreateWallButton:
                success = ButtonWallAction();
                break;

            case UIButton.ButtonType.CreateDoorButton:
                success = ButtonDoorAction();
                break;

            default:
                Debug.LogError("Unrecognized button type " + buttonType.ToString());
                break;
        }

        //TODO based on success, can play a pass/fail sound or do other nice stuff
    }

    #region Button Action Logic
    /// <summary>
    /// Player has pressed "Accept" button, so we're probably going to be processing some kind of pending action, like room creation
    /// </summary>
    /// <returns>TRUE if the button action is successful, FALSE if there's any obstacle/failure</returns>
    protected bool ButtonAcceptAction()
    {
        if (m_currentPendingCreationAction == CreationAction.Room)
        {
            GroundManager.Singleton.Client_Submit_SetTiles(GroundManager.GroundTileType.TempRoom);
        }

        GroundManager.Singleton.ClearSelection();
        CleanUpPendingAction();

        return true;
    }

    /// <summary>
    /// Player has pressed "Cancel" button, so we're probably going to be clearing out a pending action, like room creation
    /// </summary>
    /// <returns>TRUE if the button action is successful, FALSE if there's any obstacle/failure</returns>
    protected bool ButtonCancelAction()
    {
        if (m_currentPendingCreationAction == CreationAction.None)
        {
            Debug.Log("We shouldn't be here. Debug.");
            return false;
        }

        GroundManager.Singleton.ClearSelection();
        CleanUpPendingAction();


        return true;
    }

    /// <summary>
    /// Player has pressed "Build", so we need to turn on the Build Options Panel
    /// </summary>
    /// <returns>TRUE if the button action is successful, FALSE if there's any obstacle/failure</returns>
    protected bool ButtonBuildAction()
    {
        //Turn on the UIBuildPanel gameObject
        UIBuildPanel.SetActive(true);
        //Make sure the UIConfirmationPanel is off until we need it later
        UIConfirmationPanel.SetActive(false);

        GameManager.Singleton.SwitchToBuildMode();

        return true;
    }

    /// <summary>
    /// Player has pressed "Character", so we need to remove them from Build mode and spawn their character
    /// </summary>
    /// <returns>TRUE if the button action is successful, FALSE if there's any obstacle/failure</returns>
    protected bool ButtonCharacterAction()
    {
        //Turn off the UIBuildPanel gameObject
        UIBuildPanel.SetActive(false);
        //Make sure the UIConfirmationPanel is off
        UIConfirmationPanel.SetActive(false);

        GameManager.Singleton.SwitchToCharacterMode();

        return true;
    }

    /// <summary>
    /// Player has pressed "Demolish" so we will want to add that as the pending action as well as possibly clear up other pending actions
    /// </summary>
    /// <returns>TRUE if the button action is successful, FALSE if there's any obstacle/failure</returns>
    protected bool ButtonDemolishAction()
    {
        return true;
    }

    /// <summary>
    /// Player has pressed "Floor" so we will want to add that as the pending action as well as possibly clear up other pending actions
    /// </summary>
    /// <returns>TRUE if the button action is successful, FALSE if there's any obstacle/failure</returns>
    protected bool ButtonFloorAction()
    {
        return true;
    }

    /// <summary>
    /// Player has pressed "Object" so we will want to add that as the pending action as well as possibly clear up other pending actions
    /// </summary>
    /// <returns>TRUE if the button action is successful, FALSE if there's any obstacle/failure</returns>
    protected bool ButtonObjectAction()
    {
        //Turn off the UIBuildPanel gameObject
        UIBuildPanel.SetActive(false);
        //Make sure the UIConfirmationPanel is off
        UIConfirmationPanel.SetActive(false);
        //Turn on the Object Panel
        UIObjectPanel.SetActive(true);
        return true;
    }

    /// <summary>
    /// Player has pressed "Room" so we will want to add that as the pending action as well as possibly clear up other pending actions
    /// </summary>
    /// <returns>TRUE if the button action is successful, FALSE if there's any obstacle/failure</returns>
    protected bool ButtonRoomAction()
    {
        //There's another pending action, so we should probably clean it up first
        if (m_currentPendingCreationAction != CreationAction.None)
        {
            CleanUpPendingAction();
        }

        CameraManager.Singleton.selectionMode = CameraManager.CameraSelectionMode.Tiles;

        m_currentPendingCreationAction = CreationAction.Room;
        return true;
    }

    /// <summary>
    /// Player has pressed "Wall" so we will want to add taht as the pending action as well as possibly clear up other pending action
    /// </summary>
    /// <returns>TRUE if the button action is successful, FALSE if there's any obstacle/failure</returns>
    protected bool ButtonWallAction()
    {
        return true;
    }

    protected bool ButtonDoorAction()
    {
        //There's another pending action, so we should probably clean it up first
        if (m_currentPendingCreationAction != CreationAction.None)
        {
            CleanUpPendingAction();
        }

        CameraManager.Singleton.selectionMode = CameraManager.CameraSelectionMode.Tiles;

        m_currentPendingCreationAction = CreationAction.Object;
        m_currentPendingObjectType = UIObjectType.Door;

        return true;
    }

    public void CleanUpPendingAction()
    {
        CameraManager.Singleton.selectionMode = CameraManager.CameraSelectionMode.None;
        m_currentPendingObjectType = UIObjectType.None;
    }
    #endregion

    public void SetMousePosition(string text)
    {
        UIMousePos.text = text;
    }

    void Update()
    {
        if (m_currentPendingCreationAction != CreationAction.None)
        {
            if (GroundManager.Singleton.ClientData.Visuals.HighlightedWidthHeight.x != 0 && GroundManager.Singleton.ClientData.Visuals.HighlightedWidthHeight.y != 0)
            {
                //The selected width/height is not 0, so we should show the confirmation panel
                if (!UIConfirmationPanel.activeSelf)
                {
                    UIConfirmationPanel.SetActive(true);
                }
            }
            else //No tiles/objects have been selected, so 
            {
                if (UIConfirmationPanel.activeSelf)
                {
                    UIConfirmationPanel.SetActive(false);
                }
            }
        }
    }
}