using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIBillboard : MonoBehaviour
{
    private Quaternion startingRotation;
    private Camera mCamera;
    private Vector3 forwardDir;
    

    void Start()
    {
        startingRotation = transform.rotation;
        mCamera = Camera.main;
    }


    void Update()
    {
        if (!mCamera)
            return;

        //Rotate the object to face the camera
        transform.rotation = mCamera.transform.rotation * startingRotation;

        transform.eulerAngles = new Vector3(0.0f, transform.eulerAngles.y, 0.0f); 

    }
}
