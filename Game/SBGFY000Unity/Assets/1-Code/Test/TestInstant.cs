
//Simple test script to test instantiating wall objects

using UnityEngine;
using System.Collections;

public class TestInstant : MonoBehaviour
{
    public WallObject testWallPrefab;

    public void Start()
    {
        GameObject.Instantiate(testWallPrefab, Vector3.zero, Quaternion.identity);
    }
}