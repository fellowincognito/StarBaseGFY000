/*
 * StarBase GFY000
 * GameManager
 * Is the most highest level, must authorativate game organizer and state keeping class
 * Created 10 April 2015
 * Author: Fellow Incognito (fellowincognito@gmail.com)
 * SEE LICENSE FOR TERMS AND RIGHTS
 */

using UnityEngine;
using System.Collections;

public class GameManager : MonoBehaviour 
{
    #region Singleton
    protected static GameManager m_singleton = null;
    public static GameManager Singleton
    {
        get
        {
            return m_singleton;
        }
    }

    void Awake()
    {
        m_singleton = this;
    }
    #endregion

    #region Vars
    PlayerCharacter m_currentPlayerChar;

    public Vec2Int playableSize = new Vec2Int(16, 16);
    #endregion

    #region Gets/Sets
    public PlayerCharacter playerchar
    {
        get { return m_currentPlayerChar; }
    }
    #endregion

	// Use this for initialization
	void Start () 
    {
        Init();
	}

    void Init()
    {
        CacheManager.Singleton.Init();
        GroundManager.Singleton.Init(playableSize);

        PickRandomStartPointInWorld();
    }

    //Pick a random start point in world near the center of the playable area
    void PickRandomStartPointInWorld()
    {
        Vec2Int randSpot = new Vec2Int((int)(GroundManager.Singleton.GroundSize.x * 0.5f), (int)(GroundManager.Singleton.GroundSize.y * 0.5f));

        CameraManager.Singleton.SetCameraPosition(new Vector3(randSpot.x, 0f, randSpot.y), false);
    }
	
	// Update is called once per frame
	void Update () {
	
	}
}
