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

    #region Enums
    public enum PlayerMode
    {
        Administrator,
        Character
    }
    #endregion

    #region Vars
    PlayerCharacter m_currentPlayerChar;

    public Vec2Int playableSize = new Vec2Int(16, 16);

    PlayerMode m_playingMode;
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
        m_playingMode = PlayerMode.Administrator;
	}

    void Init()
    {
        CacheManager.Singleton.Init();
        GroundManager.Singleton.Init(playableSize);
        UIManager.Singleton.Init();
        CameraManager.Singleton.Init();

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

    public void SwitchToCharacterMode()
    {
        //Already in that mode
        if (m_playingMode == PlayerMode.Character)
        {
            return;
        }

        m_playingMode = PlayerMode.Character;

        m_currentPlayerChar = (PlayerCharacter)GameObject.Instantiate(PrefabAssets.Singleton.prefabPlayerCharacter, CameraManager.Singleton.worldCamera.transform.position, Quaternion.identity);

        CameraManager.Singleton.worldCamera.FollowPlayer = true;
    }

    public void SwitchToBuildMode()
    {
        if (m_playingMode == PlayerMode.Administrator)
        {
            return;
        }

        m_playingMode = PlayerMode.Administrator;

        if (m_currentPlayerChar != null)
        {
            Destroy(m_currentPlayerChar.gameObject);
        }

        m_currentPlayerChar = null;

        CameraManager.Singleton.worldCamera.FollowPlayer = false;
    }
}
