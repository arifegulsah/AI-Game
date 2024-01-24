using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraSwitcher : MonoBehaviour
{
    public GameObject[] cameras;
    private int currentCam;

    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            currentCam++;

            if(currentCam >= cameras.Length)
            {
                currentCam = 0;
            }


            //switch cameras kodu. setactive ile aç kapa kamera gameobjectlerini
            for(int i = 0; i < cameras.Length; i++)
            {
                if(i == currentCam)
                {
                    cameras[i].SetActive(true);
                }
                else
                {
                    cameras[i].SetActive(false);
                }
            }
        }
    }

}
