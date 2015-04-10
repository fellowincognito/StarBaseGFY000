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
    #endregion

    #region Gets/Sets
    public PlayerCharacter playerchar
    {
        get { return m_currentPlayerChar; }
    }
    #endregion

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
