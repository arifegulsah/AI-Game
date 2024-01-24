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
    public LayerMask whatIsGround; //LayerMask struct� checklist �eklinde aray�zden se�ebiliriz layerler� belirliyoruz.
    public float groundRayLength = .75f;

    private float dragOnGround;
    public float gravityMod = 10f;

    public Transform leftFrontWheel, rightFrontWheel;
    public float maxWheelTurn = 25f;

    public ParticleSystem[] dustParticles;
    public float maxEmission = 20f;
    public float emissionFadeSpeed = 50f;
    private float emissionRate;

    public AudioSource engineSound, tireSound;
    public float tireFadeSpeed;

    private int nextCheckPoint;
    public int currentLap;

    public bool isAI;

    public int currentTarget; //AI arabalar i�in
    private Vector3 targetPoint;
    public float aiAccelerateSpeed = 1f, aiTurnSpeed = .8f, aiReachPointRange = 5f, aiPointVariance = 3f, aiMaxTurn = 15f;
    //           ivmeleri              , d�nerken yavasla , rangee girince next point hesapla yoksa hepsi ayn� ilerler.  
    private float aiSpeedInput;

    public void Start()
    {
        theRB.transform.parent = null; //spherein frame bas�na ekstra ilerlemesini engellemek i�in

        dragOnGround = theRB.drag;
        
        if (isAI) //ba�lang�� noktas� ilk checkpointi ele al.
        {
            targetPoint = RaceManager.instance.allCheckPoints[currentTarget].transform.position;
            RandomiseAITarget();
        }

        UIManager.instance.lapCounterText.text = currentLap + "/" + RaceManager.instance.totalLaps;
    }
    public void Update() //oyuncular i�in visual yani g�r�n�r her saniye. bu y�zden position i�leri burada olmal�. 1 sn default
    {
        if (!isAI) //Biizm araba i�in, klavyeden al�nan inputlar sadece main car i�in not for AI car
        {
            //�leri geri movement i�in
            speedInput = 0f;

            if (Input.GetAxis("Vertical") > 0)
            {
                speedInput = Input.GetAxis("Vertical") * forwardAccel;
            }
            else if (Input.GetAxis("Vertical") < 0)
            {
                speedInput = Input.GetAxis("Vertical") * reverseAccel;
            }

            //Sa�a sola  movement i�in
            turnInput = Input.GetAxis("Horizontal");

            /* if(grounded && Input.GetAxis("Vertical") != 0)
            {
                transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles + new Vector3(0f, turnInput * turnStrength * Time.deltaTime * Mathf.Sign(speedInput) * (theRB.velocity.magnitude / maxSpeed), 0f)); //mathf.sign de�erin pozitif ya da negatif oldugun us�yl�yor. sa�a sola kayd�r�rken daha smooth ve d�zg�n g�r�kmesini sa�lad�k bu sayede
            } */
        }
        else //for AI cars pointleri takip etmelerini sa�la
        {
            targetPoint.y = transform.position.y; //y eksenini hesaplamalara dahil etme.

            if(Vector3.Distance(transform.position, targetPoint) < aiReachPointRange) //e�er arabam�n pozisyonu target pozisyona olan uzakl�g� beklenilenden daha k���kse
            {
                currentTarget++;

                if (currentTarget >= RaceManager.instance.allCheckPoints.Length)
                {
                    currentTarget = 0;
                }

                targetPoint = RaceManager.instance.allCheckPoints[currentTarget].transform.position;
                RandomiseAITarget();
            }

            Vector3 targetDirection = targetPoint - transform.position;
            float angle = Vector3.Angle(targetDirection, transform.forward); //y�z oryantasyonumun gitmem gereken target ile yapt�g� a��. her updatede bu a�� kapancak ve o noktaya ulasacag�m.

            Vector3 localPos = transform.InverseTransformPoint(targetPoint);
            if(localPos.x < 0f) //solumda kal�yor. gidece�im point.
            {
                angle = -angle;
            }

            turnInput = Mathf.Clamp(angle / aiMaxTurn, -1f, 1f);

            if(Mathf.Abs(angle) < aiMaxTurn) //arabalar�n full turn atmas�n� engelle
            {
                aiSpeedInput = Mathf.MoveTowards(aiSpeedInput, 1f, aiAccelerateSpeed); //ivme kazanarak h�zlans�n
            }
            else //o noktaya daha tutarl� yaklasmas� i�in yavaslamas� laz�m. ��nk� a��s� �ok y�ksek
            {
                aiSpeedInput = Mathf.MoveTowards(aiSpeedInput, aiTurnSpeed, aiAccelerateSpeed);
            }

            // aiSpeedInput = 1f; //hep full speed gitmesin
            speedInput = aiSpeedInput * forwardAccel;

        }


        //Tekerler
        leftFrontWheel.localRotation = Quaternion.Euler(leftFrontWheel.localRotation.eulerAngles.x, (turnInput * maxWheelTurn) - 180, leftFrontWheel.localRotation.eulerAngles.z);
        rightFrontWheel.localRotation = Quaternion.Euler(rightFrontWheel.localRotation.eulerAngles.x, (turnInput * maxWheelTurn), rightFrontWheel.localRotation.eulerAngles.z);


        //transform.position = theRB.position;

        //Particle System ��in
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
            var emissionModule = dustParticles[i].emission; //var tipinde olu�turuyorum ��nk� emission dedi�i sadece bir float ya da int vs de�il whole emission sectionundan bahsediyor particle system 

            emissionModule.rateOverTime = emissionRate;
        }


        if(engineSound != null)
        {
            engineSound.pitch = 1f + ((theRB.velocity.magnitude / maxSpeed) / 2f); //h�zland�k�a engine sesi arts�n.
        }

        if(tireSound != null)
        {
            if(Mathf.Abs(turnInput) > .5f)
            {
                tireSound.volume = 1f;
            }
            else
            {
                tireSound.volume = Mathf.MoveTowards(tireSound.volume, 0f, tireFadeSpeed * Time.deltaTime); //bi anda kaybolmas�n ses azalarak bitsin.
            }
        }
    }

    public void FixedUpdate() //oyuncular�n g�rmedi�i y�ksek frekansl� hesaplamalar yap�l�yo. .2 sn default
    {
        grounded = false;

        RaycastHit hit;
        Vector3 normalTarget = Vector3.zero;

        if(Physics.Raycast(groundRayPoint.position, -transform.up, out hit, groundRayLength, whatIsGround)) //ground layer�na sahip herhangi bir �eye �arparsak
        {
            grounded = true;

            normalTarget = hit.normal; //hitledi�imiz d�zeleme inen dikme
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

        if (grounded) //bu sayede havadayken sa�a sola hareket edebilmesini engelledik
        {
            theRB.drag = dragOnGround;

            theRB.AddForce(transform.forward * speedInput * 1000f); //transofrm.forward sayesinde arac�n bakt��� y�n olan x eksenini de dahil etmi� oluyoruz. bu sayede bakt��� yy�n� baz alarak hareket ediyor araba sa�a sola gidiyor.
        }
        else
        {
            theRB.drag = .1f;
            theRB.AddForce(-Vector3.up * gravityMod * 100f); //d����� h�zland�r yoksa ucuyo gibi g�z�k�yor
        }

        if (theRB.velocity.magnitude > maxSpeed)
        {
            theRB.velocity = theRB.velocity.normalized * maxSpeed;
        }

        transform.position = theRB.position;

        if (grounded && Input.GetAxis("Vertical") != 0)
        {
            transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles + new Vector3(0f, turnInput * turnStrength * Time.deltaTime * Mathf.Sign(speedInput) * (theRB.velocity.magnitude / maxSpeed), 0f)); //mathf.sign de�erin pozitif ya da negatif oldugun us�yl�yor. sa�a sola kayd�r�rken daha smooth ve d�zg�n g�r�kmesini sa�lad�k bu sayede
        }
    }

    public void CheckPointHit(int cpNumber)
    {
        if(cpNumber == nextCheckPoint)
        {
            nextCheckPoint++;

            if(nextCheckPoint == RaceManager.instance.allCheckPoints.Length)
            {
                nextCheckPoint = 0;
                LapCompleted();
            }
        }

        if (isAI)
        {
            if (cpNumber == currentTarget)
            {
                SetNextAITarget();
            }
        }

    }

    public void SetNextAITarget()
    {
        currentTarget++;

        if (currentTarget >= RaceManager.instance.allCheckPoints.Length)
        {
            currentTarget = 0;
        }

        targetPoint = RaceManager.instance.allCheckPoints[currentTarget].transform.position;
        RandomiseAITarget();
    }

    public void LapCompleted()
    {
        currentLap++;
        if (!isAI)
        {
            UIManager.instance.lapCounterText.text = currentLap + "/" + RaceManager.instance.totalLaps;
        }
    }

    public void RandomiseAITarget() //checkpointleri random olsun 
    {
        targetPoint = targetPoint + new Vector3(Random.Range(-aiPointVariance, aiPointVariance), 0f, Random.Range(-aiPointVariance, aiPointVariance));
    }

}
