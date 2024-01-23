using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarController : MonoBehaviour
{
    public Rigidbody theRB; 
    public float maxSpeed = 30f;

    public float forwardAccel = 8f, reverseAccel = 4f;
    private float speedInput;

    public float turnStrength = 180f;
    private float turnInput;

    private bool grounded;

    public Transform groundRayPoint, groundRayPoint2;
    public LayerMask whatIsGround; //LayerMask structý checklist þeklinde arayüzden seçebiliriz layerlerý belirliyoruz.
    public float groundRayLength = .75f;

    private float dragOnGround;
    public float gravityMod = 10f;

    public Transform leftFrontWheel, rightFrontWheel;
    public float maxWheelTurn = 25f;

    public ParticleSystem[] dustParticles;
    public float maxEmission = 20f;
    public float emissionFadeSpeed = 50f;
    private float emissionRate;


    public void Start()
    {
        theRB.transform.parent = null; //spherein frame basýna ekstra ilerlemesini engellemek için

        dragOnGround = theRB.drag;

    }
    public void Update() //oyuncular için visual yani görünür her saniye. bu yüzden position iþleri burada olmalý. 1 sn default
    {
        //Ýleri geri movement için
        speedInput = 0f;

        if (Input.GetAxis("Vertical") > 0)
        {
            speedInput = Input.GetAxis("Vertical") * forwardAccel;
        }
        else if(Input.GetAxis("Vertical") < 0)
        {
            speedInput = Input.GetAxis("Vertical") * reverseAccel;
        }

        //Saða sola  movement için
        turnInput = Input.GetAxis("Horizontal");

        if(grounded && Input.GetAxis("Vertical") != 0)
        {
            transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles + new Vector3(0f, turnInput * turnStrength * Time.deltaTime * Mathf.Sign(speedInput) * (theRB.velocity.magnitude / maxSpeed), 0f)); //mathf.sign deðerin pozitif ya da negatif oldugun usöylüyor. saða sola kaydýrýrken daha smooth ve düzgün görükmesini saðladýk bu sayede
        }

        //Tekerler
        leftFrontWheel.localRotation = Quaternion.Euler(leftFrontWheel.localRotation.eulerAngles.x, (turnInput * maxWheelTurn) - 180, leftFrontWheel.localRotation.eulerAngles.z);
        rightFrontWheel.localRotation = Quaternion.Euler(rightFrontWheel.localRotation.eulerAngles.x, (turnInput * maxWheelTurn), rightFrontWheel.localRotation.eulerAngles.z);


        transform.position = theRB.position;

        //Particle System Ýçin
        emissionRate = Mathf.MoveTowards(emissionRate, 0f, emissionFadeSpeed * Time.deltaTime);

        if (grounded && (Mathf.Abs(turnInput) > .5f || (theRB.velocity.magnitude < maxSpeed * .5f && theRB.velocity.magnitude != 0)))
        {
            emissionRate = maxEmission;
        }
        
        if(theRB.velocity.magnitude <= .5f)
        {
            emissionRate = 0;
        }

        for (int i = 0; i < dustParticles.Length; i++)
        {
            var emissionModule = dustParticles[i].emission; //var tipinde oluþturuyorum çünkü emission dediði sadece bir float ya da int vs deðil whole emission sectionundan bahsediyor particle system 

            emissionModule.rateOverTime = emissionRate;
        }

    }

    public void FixedUpdate() //oyuncularýn görmediði yüksek frekanslý hesaplamalar yapýlýyo. .2 sn default
    {
        grounded = false;

        RaycastHit hit;
        Vector3 normalTarget = Vector3.zero;

        if(Physics.Raycast(groundRayPoint.position, -transform.up, out hit, groundRayLength, whatIsGround)) //ground layerýna sahip herhangi bir þeye çarparsak
        {
            grounded = true;

            normalTarget = hit.normal; //hitlediðimiz düzeleme inen dikme
        }

        if(Physics.Raycast(groundRayPoint2.position, -transform.up, out hit, groundRayLength, whatIsGround))
        {
            grounded = true;

            normalTarget = (normalTarget + hit.normal) / 2f;
        }

        if (grounded)
        {
            transform.rotation = Quaternion.FromToRotation(transform.up, normalTarget) * transform.rotation;
        }

        if (grounded) //bu sayede havadayken saða sola hareket edebilmesini engelledik
        {
            theRB.drag = dragOnGround;

            theRB.AddForce(transform.forward * speedInput * 1000f); //transofrm.forward sayesinde aracýn baktýðý yön olan x eksenini de dahil etmiþ oluyoruz. bu sayede baktýðý yyönü baz alarak hareket ediyor araba saða sola gidiyor.
        }
        else
        {
            theRB.drag = .1f;
            theRB.AddForce(-Vector3.up * gravityMod * 100f); //düþüþü hýzlandýr yoksa ucuyo gibi gözüküyor
        }

        if (theRB.velocity.magnitude > maxSpeed)
        {
            theRB.velocity = theRB.velocity.normalized * maxSpeed;
        }
    }
}
