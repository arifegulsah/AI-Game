using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public CarController target;
    private Vector3 offsetDirection;

    public float minDistance = 20f, maxDistance = 40f;
    private float activeDistance;

    public Transform startTargetOffset;

    public void Start()
    {
        offsetDirection = transform.position - startTargetOffset.position;

        activeDistance = minDistance;

        offsetDirection.Normalize();
    }

    public void Update()
    {
        //h�zland�k�a kameran�n uzakla�mas�n�, yava�lad�k�a da yakla�mas�n� sa�lamak i�in
        activeDistance = minDistance + ((maxDistance - minDistance) * (target.theRB.velocity.magnitude / target.maxSpeed));

        transform.position = target.transform.position + (offsetDirection * activeDistance);
    }
}
