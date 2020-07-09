using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class JoystickController : MonoBehaviour, IDragHandler, IEndDragHandler
{
    //VARIABLES / PROPERTIES
    //-------------------------------------------------------------------------

    [Header("Joystick Settings")]
    [Tooltip("The maximum distance the handle can move from the center")]
    public float maxDistance = 10;

#pragma warning disable CS0649 
    [SerializeField] private Transform joystickHandleTransform;
#pragma warning restore CS0649
    //[Header("Runtime Data")]


    //EVENT DECLARATIONS
    //--------------------------------------------------------------------------

    //Create a class for a Unity Event providing a Vector2 parameter
    [System.Serializable]
    public class Vector2UnityEvent : UnityEvent<Vector2> { }

    //This will be what other objects / classes can listen two when our joystick is moving
    public Vector2UnityEvent JoystickOutput;

    //METHODS
    //--------------------------------------------------------------------------

    /*
     * OnDrag is predefined method declared within the IDragHandler
     * It will capture any drag data (mouse or touch) and pass this data as a parameter
     */
    public void OnDrag(PointerEventData eventData)
    {
        // Get the joystick handle position by calculating the position using the eventData
        Vector2 jsPosition = CalculatePosition( eventData.position );

        // Apply the new calculated position to the actual position of the handle
        joystickHandleTransform.position = jsPosition;

        // Calculate the vector difference between the current joystick position and its center
        Vector2 vectoralDifference = new Vector2(jsPosition.x - transform.position.x, jsPosition.y - transform.position.y);

        // Convert the position value into a scale between 0 and 1 to provide our listeners with how much our objects should move
        Vector2 inputRatio = new Vector2(vectoralDifference.x / maxDistance, vectoralDifference.y / maxDistance);

        // Send the ratio to our event which will be received by our listeners
        JoystickOutput.Invoke(inputRatio);
    }



    public void OnEndDrag(PointerEventData eventData)
    {
        //When we stop dragging / let go of the screen, set the position of the handle back to the center
        joystickHandleTransform.position = transform.position;

        //When we stop dragging ensure the event will send out a null value to make all object movements stop
        JoystickOutput.Invoke(new Vector2(0.0f, 0.0f));
    }



    private Vector3 CalculatePosition(Vector3 value)
    {
        //Check if the current "touched" screen point is outside of range our handle can move. 
        if (maxDistance * maxDistance < Vector3.SqrMagnitude(transform.position - value) )
        {
            //Calculate where the handle should position itself based on the boundry
            Vector3 newPosition = transform.position + (value - transform.position).normalized * maxDistance;

            return newPosition;
        }
        else
        {
            return value;
        }
    }
}
