using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlaySoundOnHit : MonoBehaviour
{
    public AudioSource soundToPlay;
    public int groundLayerNo = 8;

    public void Update()
    {
        
    }

    private void OnCollisionEnter(Collision other)
    {

        if(other.gameObject.layer != groundLayerNo)
        {
            soundToPlay.Stop();
            soundToPlay.pitch = Random.Range(0.8f, 1.2f); //devanlý ayný sesi verip durmasýn diye, range ile pitchle oynuyoruz
            soundToPlay.Play();
        }
        
    }
}
