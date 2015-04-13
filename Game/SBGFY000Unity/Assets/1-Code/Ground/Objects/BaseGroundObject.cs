/*
 * StarBase GFY000
 * BaseGroundObject
 * The base class for all static, world objects, like floor tiles, and walls
 * Created 10 April 2015
 * Author: Fellow Incognito (fellowincognito@gmail.com)
 * SEE LICENSE FOR TERMS AND RIGHTS
 */

using UnityEngine;
using System.Collections;

public class BaseGroundObject : MonoBehaviour
{
    public GroundManager.GroundTileType tileType;

    public Vector3 defaultOffset = Vector3.zero;

    public float defaultOrientation = 0f;

    public MeshCollider theCollider;

    [SerializeField]
    protected Vec2Int m_tilePos;

    public Vec2Int tilePos
    {
        get { return m_tilePos; }
    }

    void Awake()
    {
        theCollider = this.gameObject.GetComponent<MeshCollider>();
    }

    public void SetTilePos(Vec2Int pos)
    {
        m_tilePos = pos;
    }

    public virtual void SetMaterial(Material mat)
    {
        
    }

    public virtual void ToggleCollider(bool enableCollider)
    {
        if (theCollider != null)
        {
            theCollider.enabled = enableCollider;
        }
    }

    public virtual void AssignToPosition(Vec2Int position, float rot, bool asHologram)
    {
        this.transform.parent = this.transform;
        this.transform.position = new Vector3(position.x + defaultOffset.x, defaultOffset.y, position.y + defaultOffset.z);
        this.gameObject.SetActive(true);
        this.transform.rotation = Quaternion.Euler(0f, rot, 0f);
        this.SetTilePos(position);
    }

    public virtual void BuildObject() { }
}