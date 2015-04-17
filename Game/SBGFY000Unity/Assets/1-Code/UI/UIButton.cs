/*
 * StarBase GFY000
 * UIButton
 * Gets attached to a button and contains some relevant data about the button to feed into whatever
 * Created 12 April 2015
 * Author: Fellow Incognito (fellowincognito@gmail.com)
 * SEE LICENSE FOR TERMS AND RIGHTS
 */

using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class UIButton : MonoBehaviour
{
    #region Enums
    public enum ButtonType
    {
        AcceptActionButton,
        CancelActionButton,

        BuildButton,
        CharacterButton,

        CreateRoomButton,
        CreateWallButton,
        CreateFloorButton,
        CreateObjectButton,
        CreateDemolishButton,

        CreateDoorButton,
    }

    public enum ToggleType
    {
        Yes,
        No
    }
    #endregion

    public ButtonType buttonType;
    public ToggleType toggleType;

    public void OnClick()
    {
        UIManager.Singleton.ButtonClick(this, buttonType);
    }
}