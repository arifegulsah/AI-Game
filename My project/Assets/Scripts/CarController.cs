using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarController : MonoBehaviour
{
    public Rigidbody theRB; 
    public float maxSpeed = 30f;

    public void Start()
    {
        theRB.transform.parent = null; //spherein frame basýna ekstra ilerlemesini engellemek için
    }
    public void Update() //oyuncular için visual yani görünür her saniye. bu yüzden position iþleri burada olmalý. 1 sn default
    {
        
        transform.position = theRB.position;
    }

    public void FixedUpdate() //oyuncularýn görmediði yüksek frekanslý hesaplamalar yapýlýyo. .2 sn default
    {
        theRB.AddForce(new Vector3(0f, 0f, 2f));
    }
}
