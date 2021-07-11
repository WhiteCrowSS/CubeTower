using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraShake : MonoBehaviour
{
    private Transform camTransform;
    private float shakeDur = 1f, shakeAmount = 0.04f, decreaseFactor = 1.5f;
    private Vector3 originPosition;

    private void Start()
    {
        camTransform = GetComponent<Transform>();
        originPosition = camTransform.localPosition;
    }


    private void Update()
    {
        if (shakeDur > 0)
        {
            camTransform.localPosition = originPosition + Random.insideUnitSphere * shakeAmount;
            shakeDur -= Time.deltaTime * decreaseFactor;
        }
        else
        {
            shakeDur = 0;
            camTransform.localPosition = originPosition;
        }
    }
}
