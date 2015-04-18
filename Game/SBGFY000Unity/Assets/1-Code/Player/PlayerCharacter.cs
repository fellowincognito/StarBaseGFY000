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
    Rigidbody m_rigidBody;
    Vector3 m_position;
    CharacterController m_charController;

    //http://answers.unity3d.com/questions/717637/how-do-you-smoothly-transitionlerp-into-a-new-rota.html
    //http://answers.unity3d.com/questions/185346/rotate-object-to-face-mouse-cursor-for-xz-based-to.html

    void Awake()
    {
        m_rigidBody = this.GetComponent<Rigidbody>();
        m_rigidBody.isKinematic = true;
        m_position = this.transform.position;
        m_charController = this.GetComponent<CharacterController>();
    }

    void Update()
    {
        Vector3 moveDir = new Vector3(0, 0, 0);

        //if (Input.GetKey(KeyCode.A))
        //{
        //    moveDir.x -= 1f;
        //    //moveDir += -this.transform.right;
        //}

        //if (Input.GetKey(KeyCode.D))
        //{
        //    moveDir.x += 1f;
        //    //moveDir += this.transform.right;
        //}

        //if (Input.GetKey(KeyCode.W))
        //{
        //    moveDir.z += 1f;
        //    //moveDir += this.transform.forward;
        //}

        //if (Input.GetKey(KeyCode.S))
        //{
        //    moveDir.z -= 1f;
        //    //moveDir += -this.transform.forward;
        //}

        //moveDir = m_rigidBody.transform.TransformDirection(moveDir);

        moveDir = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
        moveDir = transform.TransformDirection(moveDir);

        //this.gameObject.transform.Translate(moveDir * Time.deltaTime * 2f, Space.Self);
        //m_position = m_rigidBody.transform.position + moveDir * Time.deltaTime * 2f;
        m_charController.Move(moveDir * Time.deltaTime * 2f);

        UpdateAim();

        if (!UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())
        {
            MouseInput();
        }
    }

    void FixedUpdate()
    {
        //m_rigidBody.MovePosition(m_position);
    }

    void UpdateAim()
    {
        m_goalLook = CameraManager.Singleton.worldCamera.WorldPointFromMouse();
        m_goalLook.y = this.transform.position.y;

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

            GroundManager.GroundTileType tileType = GroundManager.Singleton.ClientData.GetTileType(tilePos);

            Debug.Log(string.Format("Tile pos {0} type {1}", tilePos, tileType));

            GroundManager.Singleton.Client_AttemptBuildTile(this, tilePos, aimPos);
        }
    }
}