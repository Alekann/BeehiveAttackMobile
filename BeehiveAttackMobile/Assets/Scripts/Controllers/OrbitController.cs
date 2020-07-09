using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OrbitController : MonoBehaviour
{

    //PROPERTIES
    //------------------------------------
    [Header("Custom Settings")]
    public float OrbitSpeed = 5.0f;
    public float pitchMin = 0.0f, pitchMax = 0.0f, headingMin = 0.0f, headingMax = 0.0f;
    public GameObject orbitObject;

    private Camera mCamera;
    private Vector2 rotateInput = Vector2.zero;
    private float pitch, heading;

    //METHODS
    //------------------------------------
    private void Awake()
    {
        pitch = transform.eulerAngles.x;
        heading = transform.eulerAngles.y;
    }


    /// <summary>
    /// Method to bind to the Joystick Controller event to receive the input data
    /// </summary>
    /// <param name="moveValue"></param>
    public void MoveHeaderPitch(Vector2 moveValue)
    {
        rotateInput = moveValue;
    }


    /// <summary>
    /// Used to calculate the new position of the camera orbit base
    /// </summary>
    private void OrbitCamera()
    {
        pitch = Mathf.Clamp(pitch + (OrbitSpeed * rotateInput.y * Time.deltaTime), pitchMin, pitchMax);
        heading = Mathf.Clamp(heading + (OrbitSpeed * rotateInput.x * Time.deltaTime), headingMin, headingMax);

        float newHeading = heading + (OrbitSpeed * rotateInput.x * Time.deltaTime);

        transform.eulerAngles = new Vector3(pitch, heading, 0.0f);
    }

    private void FixedUpdate()
    {
        OrbitCamera();
    }


}
