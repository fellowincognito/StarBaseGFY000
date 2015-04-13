/*
 * StarBase GFY000
 * WorldOrthoCamera
 * Handles things like interacting with the world and movement
 * Created 10 April 2015
 * Author: Fellow Incognito (fellowincognito@gmail.com)
 * SEE LICENSE FOR TERMS AND RIGHTS
 */

using UnityEngine;
using System.Collections;

public class WorldOrthoCamera : MonoBehaviour
{
    #region Enums
    public enum SelectionType
    {
        Select,
        Addition,
        Subtraction
    }
    #endregion

    Camera thisCamera;

    Vector3 m_startClickPos;
    Vector3 m_endClickPos;

    Vector3 m_startDragPos;
    Vector3 m_currentMousePos;

    Plane castPlane;

    bool m_doDisplayDrag;
    bool m_doDisplaySelection;

    bool m_followPlayer;

    public bool FollowPlayer
    {
        get { return m_followPlayer; }
        set { m_followPlayer = value; }
    }


    //http://forum.unity3d.com/threads/preventing-ugui-mouse-click-from-passing-through-gui-controls.272114/
    //public UnityEngine.EventSystems.EventSystem eventSystem;

    // Use this for initialization
    void Start()
    {
        thisCamera = this.gameObject.GetComponentInChildren<Camera>();
        castPlane = new Plane(Vector3.up, 0f);
    }

    // Update is called once per frame
    void Update()
    {
        if (!UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())
        {
            MouseClicking();
        }

        Movement();
    }

    void Movement()
    {
        if (m_followPlayer)
            return;

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
    }

    void LateUpdate()
    {
        if (m_followPlayer)
        {
            this.gameObject.transform.position = GameManager.Singleton.playerchar.transform.position;
        }
    }

    void MouseClicking()
    {
        if (m_followPlayer)
            return;

        SelectionType selectType = SelectionType.Select;

        if (Input.GetKey(KeyCode.LeftShift))
        {
            selectType = SelectionType.Addition;
        }
        else if (Input.GetKey(KeyCode.LeftControl))
        {
            selectType = SelectionType.Subtraction;
        }

        if (Input.GetMouseButtonDown(0))
        {
            if (!m_doDisplayDrag)
            {
                m_doDisplayDrag = true;
                m_startDragPos = WorldPointFromMouse();
            }
        }

        if (Input.GetMouseButtonUp(0))
        {
            if (m_doDisplayDrag)
            {
                m_doDisplayDrag = false;
                m_doDisplaySelection = true;
                m_startClickPos = m_startDragPos;
                m_endClickPos = m_currentMousePos;

                //If the selection mode is tiles, then send it to the GroundManager
                if (CameraManager.Singleton.selectionMode == CameraManager.CameraSelectionMode.Tiles)
                {
                    GroundManager.Singleton.SetHighlightTiles(m_startClickPos, m_endClickPos, selectType);
                }
            }
        }

        if (m_doDisplayDrag)
        {
            m_currentMousePos = WorldPointFromMouse();

            //If the selection mode is tiles, then send it to the GroundManager
            if (CameraManager.Singleton.selectionMode == CameraManager.CameraSelectionMode.Tiles)
            {
                GroundManager.Singleton.SetTempHighlight(m_startDragPos, m_currentMousePos);
            }
        }
    }

    void OnDrawGizmos()
    {
        if (m_doDisplayDrag)
        {
            Vector3 point2a = new Vector3(m_startDragPos.x + (m_currentMousePos.x - m_startDragPos.x), m_startDragPos.y, m_startDragPos.z);
            Vector3 point4a = new Vector3(m_startDragPos.x, m_startDragPos.y, m_startDragPos.z + (m_currentMousePos.z - m_startDragPos.z));

            Debug.DrawLine(m_startDragPos, point2a);
            Debug.DrawLine(point2a, m_currentMousePos);
            Debug.DrawLine(m_currentMousePos, point4a);
            Debug.DrawLine(point4a, m_startDragPos);
        }

        if (m_doDisplaySelection)
        {
            Vector3 point2b = new Vector3(m_startClickPos.x + (m_endClickPos.x - m_startClickPos.x), m_startClickPos.y, m_startClickPos.z);
            Vector3 point4b = new Vector3(m_startClickPos.x, m_startClickPos.y, m_startClickPos.z + (m_endClickPos.z - m_startClickPos.z));

            Debug.DrawLine(m_startClickPos, point2b, Color.green);
            Debug.DrawLine(point2b, m_endClickPos, Color.green);
            Debug.DrawLine(m_endClickPos, point4b, Color.green);
            Debug.DrawLine(point4b, m_startClickPos, Color.green);
        }
    }

    void DoDebugClicking(Vector3 point)
    {
        Debug.DrawLine(new Vector3(point.x - 0.25f, point.y, point.z), new Vector3(point.x + 0.25f, point.y, point.z), Color.red, 1.0f);
        Debug.DrawLine(new Vector3(point.x, point.y - 0.25f, point.z), new Vector3(point.x, point.y + 0.25f, point.z), Color.green, 1.0f);
        Debug.DrawLine(new Vector3(point.x, point.y, point.z - 0.25f), new Vector3(point.x, point.y, point.z + 0.25f), Color.blue, 1.0f);

        GameObject go = (GameObject)GameObject.CreatePrimitive(PrimitiveType.Sphere);
        go.transform.position = point;
        go.transform.localScale = Vector3.one * 0.25f;
    }

    public Vector3 WorldPointFromMouse()
    {
        Vector3 result = thisCamera.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0));
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        float distance = 0f;
        bool didHit = castPlane.Raycast(ray, out distance);

        //Debug.Log("Did hit " + didHit + " distance " + distance);

        if (!didHit)
        {
            Debug.LogError("Missed the plane");
        }

        result += ray.direction * distance;

        result.y = 0f;

        return result;
    }
}
