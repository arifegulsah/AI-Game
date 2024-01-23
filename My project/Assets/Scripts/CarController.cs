using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarController : MonoBehaviour
{
    public Rigidbody theRB; 
    public float maxSpeed = 30f;

    public void Start()
    {
        theRB.transform.parent = null; //spherein frame bas�na ekstra ilerlemesini engellemek i�in
    }
    public void Update() //oyuncular i�in visual yani g�r�n�r her saniye. bu y�zden position i�leri burada olmal�. 1 sn default
    {
        
        transform.position = theRB.position;
    }

    public void FixedUpdate() //oyuncular�n g�rmedi�i y�ksek frekansl� hesaplamalar yap�l�yo. .2 sn default
    {
        theRB.AddForce(new Vector3(0f, 0f, 2f));
    }
}
