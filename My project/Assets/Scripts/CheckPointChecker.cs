using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckPointChecker : MonoBehaviour
{
    public CarController theCar;
    private void OnTriggerEnter(Collider other)
    {
        if(other.tag == "Checkpoint")
        {
            theCar.CheckPointHit(other.GetComponent<CheckPoints>().cpNumber);
        }
    }
}
