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

    public AudioSource engineSound, tireSound;
    public float tireFadeSpeed;

    private int nextCheckPoint;
    public int currentLap;

    public bool isAI;

    public int currentTarget; //AI arabalar için
    private Vector3 targetPoint;
    public float aiAccelerateSpeed = 1f, aiTurnSpeed = .8f, aiReachPointRange = 5f, aiPointVariance = 3f, aiMaxTurn = 15f;
    //           ivmeleri              , dönerken yavasla , rangee girince next point hesapla yoksa hepsi ayný ilerler.  
    private float aiSpeedInput;

    public void Start()
    {
        theRB.transform.parent = null; //spherein frame basýna ekstra ilerlemesini engellemek için

        dragOnGround = theRB.drag;
        
        if (isAI) //baþlangýç noktasý ilk checkpointi ele al.
        {
            targetPoint = RaceManager.instance.allCheckPoints[currentTarget].transform.position;
            RandomiseAITarget();
        }

        UIManager.instance.lapCounterText.text = currentLap + "/" + RaceManager.instance.totalLaps;
    }
    public void Update() //oyuncular için visual yani görünür her saniye. bu yüzden position iþleri burada olmalý. 1 sn default
    {
        if (!isAI) //Biizm araba için, klavyeden alýnan inputlar sadece main car için not for AI car
        {
            //Ýleri geri movement için
            speedInput = 0f;

            if (Input.GetAxis("Vertical") > 0)
            {
                speedInput = Input.GetAxis("Vertical") * forwardAccel;
            }
            else if (Input.GetAxis("Vertical") < 0)
            {
                speedInput = Input.GetAxis("Vertical") * reverseAccel;
            }

            //Saða sola  movement için
            turnInput = Input.GetAxis("Horizontal");

            /* if(grounded && Input.GetAxis("Vertical") != 0)
            {
                transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles + new Vector3(0f, turnInput * turnStrength * Time.deltaTime * Mathf.Sign(speedInput) * (theRB.velocity.magnitude / maxSpeed), 0f)); //mathf.sign deðerin pozitif ya da negatif oldugun usöylüyor. saða sola kaydýrýrken daha smooth ve düzgün görükmesini saðladýk bu sayede
            } */
        }
        else //for AI cars pointleri takip etmelerini saðla
        {
            targetPoint.y = transform.position.y; //y eksenini hesaplamalara dahil etme.

            if(Vector3.Distance(transform.position, targetPoint) < aiReachPointRange) //eðer arabamýn pozisyonu target pozisyona olan uzaklýgý beklenilenden daha küçükse
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
            float angle = Vector3.Angle(targetDirection, transform.forward); //yüz oryantasyonumun gitmem gereken target ile yaptýgý açý. her updatede bu açý kapancak ve o noktaya ulasacagým.

            Vector3 localPos = transform.InverseTransformPoint(targetPoint);
            if(localPos.x < 0f) //solumda kalýyor. gideceðim point.
            {
                angle = -angle;
            }

            turnInput = Mathf.Clamp(angle / aiMaxTurn, -1f, 1f);

            if(Mathf.Abs(angle) < aiMaxTurn) //arabalarýn full turn atmasýný engelle
            {
                aiSpeedInput = Mathf.MoveTowards(aiSpeedInput, 1f, aiAccelerateSpeed); //ivme kazanarak hýzlansýn
            }
            else //o noktaya daha tutarlý yaklasmasý için yavaslamasý lazým. çünkü açýsý çok yüksek
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


        if(engineSound != null)
        {
            engineSound.pitch = 1f + ((theRB.velocity.magnitude / maxSpeed) / 2f); //hýzlandýkça engine sesi artsýn.
        }

        if(tireSound != null)
        {
            if(Mathf.Abs(turnInput) > .5f)
            {
                tireSound.volume = 1f;
            }
            else
            {
                tireSound.volume = Mathf.MoveTowards(tireSound.volume, 0f, tireFadeSpeed * Time.deltaTime); //bi anda kaybolmasýn ses azalarak bitsin.
            }
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

        transform.position = theRB.position;

        if (grounded && Input.GetAxis("Vertical") != 0)
        {
            transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles + new Vector3(0f, turnInput * turnStrength * Time.deltaTime * Mathf.Sign(speedInput) * (theRB.velocity.magnitude / maxSpeed), 0f)); //mathf.sign deðerin pozitif ya da negatif oldugun usöylüyor. saða sola kaydýrýrken daha smooth ve düzgün görükmesini saðladýk bu sayede
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
