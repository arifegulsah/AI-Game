using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RaceManager : MonoBehaviour
{
    public static RaceManager instance;

    public CheckPoints[] allCheckPoints;

    public int totalLaps;

    public void Awake()
    {
        instance = this;
    }

    public void Start()
    {
        for(int i= 0; i < allCheckPoints.Length; i++)
        {
            allCheckPoints[i].cpNumber = i;
        }
    }
}
