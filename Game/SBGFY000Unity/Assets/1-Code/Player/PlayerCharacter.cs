/*
 * StarBase GFY000
 * PlayerCharacter
 * Represents a player character
 * Created 10 April 2015
 * Author: Fellow Incognito (fellowincognito@gmail.com)
 * SEE LICENSE FOR TERMS AND RIGHTS
 */

using UnityEngine;
using System.Collections;

public class PlayerCharacter : MonoBehaviour
{
    Vector3 m_goalLook;
    public float turningRate = 12.5f;

    //http://answers.unity3d.com/questions/717637/how-do-you-smoothly-transitionlerp-into-a-new-rota.html
    //http://answers.unity3d.com/questions/185346/rotate-object-to-face-mouse-cursor-for-xz-based-to.html


    void Update()
    {
        Vector3 moveDir = new Vector3(0, 0, 0);

        if (Input.GetKey(KeyCode.A))
        {
            moveDir.x -= 1f;
        }

        if (Input.GetKey(KeyCode.D))
        {
            moveDir.x += 1f;
        }

        if (Input.GetKey(KeyCode.W))
        {
            moveDir.z += 1f;
        }

        if (Input.GetKey(KeyCode.S))
        {
            moveDir.z -= 1f;
        }

        this.gameObject.transform.Translate(moveDir * Time.deltaTime * 2f, Space.Self);

        UpdateAim();

        if (!UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())
        {
            MouseInput();
        }
    }

    void UpdateAim()
    {
        m_goalLook = CameraManager.Singleton.worldCamera.WorldPointFromMouse();

        //Figure out rotation difference between current and goal
        Quaternion targetRotation = Quaternion.LookRotation(m_goalLook - transform.position);

        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, turningRate * Time.deltaTime);
    }

    void MouseInput()
    {
        if (Input.GetMouseButton(0))
        {
            Vector3 aimPos = CameraManager.Singleton.worldCamera.WorldPointFromMouse();

            Vec2Int tilePos = GroundManager.Singleton.ConvertWorldPositionToTile(aimPos);

            GroundManager.GroundTileType tileType = GroundManager.Singleton.GetTileType(tilePos);

            Debug.Log(string.Format("Tile pos {0} type {1}", tilePos, tileType));

            GroundManager.Singleton.AttemptBuildTile(this, tilePos, aimPos);
        }
    }
}