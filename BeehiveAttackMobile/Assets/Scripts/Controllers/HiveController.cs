using System.Collections;
using System.Collections.Generic;
using Data.Enums;
using Data.EventClasses;
using UnityEngine;

[RequireComponent(typeof(SphereCollider))]
public class HiveController : MonoBehaviour
{
    //PROPERTIES
    //-----------------------------
    [Header("Detection Settings")]
    [Tooltip("The tag which will trigger the dection zone intruder events")]
    public string detectionTag = "Player";

    [Header("References")]
    public GameObject detectionZone;


    [Header("Runtime Information", order = 0)]
    [Header("-----------------", order = 1)]
    public DetectionState alertState = DetectionState.noIntruder;
    

    //EVENTS
    //-----------------------------
    public TransformUnityEvent OnIntruderEnter;
    public TransformUnityEvent OnIntruderExit;

    //METHODS
    //-----------------------------
    private void Awake()
    {
        if(!detectionZone)
        {
            //Automatically assumes the "detection zone" object will be the first child.
            detectionZone = gameObject.GetComponentInChildren<Transform>().gameObject;
        }
    }

    /// <summary>
    /// Validate if there is an intruder inside the detection zone. Involk the OnIntruderEnter event
    /// </summary>
    /// <param name="other"></param>
    private void OnTriggerEnter(Collider other)
    {
        if(other.tag == detectionTag)
        {
            SpiderController sController = other.GetComponent<SpiderController>();

            if (!sController.isAtObjective)
            {
                OnIntruderEnter.Invoke(other.transform);

                alertState = DetectionState.intruderDetected;
            }
        }
    }


    /// <summary>
    /// Validate if the intruder has left the dection zone. Involk the OnIntruderEcit event
    /// </summary>
    /// <param name="other"></param>
    private void OnTriggerExit(Collider other)
    {
        if(other.tag == detectionTag)
        {
            SpiderController sController = other.GetComponent<SpiderController>();

            if(!sController.isAtObjective)
            {
                OnIntruderExit.Invoke(other.transform);

                alertState = DetectionState.noIntruder;
            }
        }
    }
}
