/*
 * StarBase GFY000
 * DoorObject
 * A door, with open, close and frame stuff
 * Created 10 April 2015
 * Author: Fellow Incognito (fellowincognito@gmail.com)
 * SEE LICENSE FOR TERMS AND RIGHTS
 */

using UnityEngine;
using System.Collections;

public class DoorObject : BaseGroundObject
{
    public enum DoorState
    {
        Closed,
        Open,
        Locked
    }

    DoorState _doorState;

    public GameObject DoorFrameArt;
    public GameObject DoorOpenArt;
    public GameObject DoorCloseArt;

    // Use this for initialization
    void Start()
    {
        _doorState = DoorState.Closed;
    }

    // Update is called once per frame
    void Update()
    {

    }

    public override void SetMaterial(Material mat)
    {
        DoorFrameArt.renderer.material = mat;
        DoorOpenArt.renderer.material = mat;
        DoorCloseArt.renderer.material = mat;

        base.SetMaterial(mat);
    }

    public override void SetMaterialColor(Color color)
    {
        base.SetMaterialColor(color);

        DoorFrameArt.renderer.material.SetColor("_Color", color);
        DoorOpenArt.renderer.material.SetColor("_Color", color);
        DoorCloseArt.renderer.material.SetColor("_Color", color);
    }

    public override void AssignToPosition(Vec2Int position, float rot, bool asHologram)
    {
        base.AssignToPosition(position, rot, asHologram);

        if (asHologram)
        {
            SetMaterial(PrefabAssets.Singleton.doorHologramMat);
            ToggleCollider(false);
        }
        else
        {
            SetMaterial(PrefabAssets.Singleton.doorMatGray);
            ToggleCollider(true);
        }
    }

    public override void BuildObject()
    {
        SetMaterial(PrefabAssets.Singleton.doorMatGray);

        theCollider.enabled = true;
    }

    public void OpenDoor()
    {
        if (_doorState == DoorState.Locked)
        {
            //Don't can't be opened, play a "denied" sound?
            return;
        }

        if (_doorState == DoorState.Closed)
        {
            DoorOpenArt.SetActive(true);
            DoorCloseArt.SetActive(false);
            _doorState = DoorState.Open;
        }
    }

    public void CloseDoor()
    {
        if (_doorState == DoorState.Locked)
        {
            //Don't can't be opened, play a "denied" sound?
            return;
        }

        if (_doorState == DoorState.Open)
        {
            DoorOpenArt.SetActive(false);
            DoorCloseArt.SetActive(true);
            _doorState = DoorState.Closed;
        }
    }

    //Allows doors to be locked open
    public void LockDoor()
    {
        _doorState = DoorState.Locked;

        //Play a lock sound? Switch skins? TODO
    }

    public void UnlockDoor()
    {
        //We're unlocking the door, but was the door locked open or closed?
        //Tell by the state of the art
        if (DoorOpenArt.activeSelf)
        {
            //We must be locked open, so unlock and close the door
            _doorState = DoorState.Open;
            CloseDoor();
        }
        else
        {
            //We must be locked closed, so unlock and close the door
            _doorState = DoorState.Closed;
            CloseDoor();
        }
    }
}