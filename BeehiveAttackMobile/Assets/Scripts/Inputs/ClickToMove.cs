using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;
using UnityEngine.EventSystems;

[RequireComponent(typeof(NavMeshAgent))]
public class ClickToMove : MonoBehaviour
{
    //PROPERTIES
    //-------------------------------------
    [Header("References")]
    public NavMeshAgent navMeshAgent;
    public LayerMask clickLayerMask;
    private GameObject clickVisualiser;

    [Header("Custom Setttings")]
    public float minimumDistance = 0.2f;
    [SerializeField] private bool enableTouchDetection = false;

    [Header("Internal Runtime Data")]
    [SerializeField] private bool isMoving = false;
    private Camera mCamera;
    private short fingerIDOnUI = -1;
    private Touch touchToRaycast;
    private bool isPointerOverUIObject = false;


    //EVENTS
    //-------------------------------------
#pragma warning disable CS0649
    [SerializeField]
    private UnityEvent OnMovementStarted, OnMovementEnded;
#pragma warning restore CS0649


    //METHODS
    //-------------------------------------
    private void Awake()
    {
        if (!navMeshAgent)
        {
            //Try to find nav mesh agent in parent
            NavMeshAgent tempNMA = GetComponent<NavMeshAgent>();

            //If a nav mesh agent was found, assign it.
            if (tempNMA != null)
            {
                navMeshAgent = tempNMA;
            }
            else
            {
                Debug.LogError("Error: No Nav Mesh Agent could be found.");
                enabled = false;
            }
        }

        //Locate the ClickVisualiser prefab in the Resources folder
        UnityEngine.Object cVisualiser = Resources.Load("DynamicPrefabs/ClickVisualiser");
        if (cVisualiser)
        {
            clickVisualiser = (GameObject)GameObject.Instantiate(cVisualiser, Vector3.zero, Quaternion.identity);
            clickVisualiser.SetActive(false);
            clickVisualiser.transform.Rotate(90, 0, 0);
            clickVisualiser.transform.localScale = new Vector3(0.2f, 0.2f, 2.0f);
        }
        else
        {
            Debug.LogError("Error: Failed to find ClickVisualiser.prefab in ../Assets/Resources/DynamicPrefabs/");
        }

        if (!mCamera)
        {
            mCamera = Camera.main;
        }

        //Enable touch detection if Unity Android
#if UNITY_ANDROID
        enableTouchDetection = true;
#endif
    }


    private void OnEnable()
    {
        //Use the AddListener method on the event to assign a value to a variable when that event is involked 
        OnMovementStarted.AddListener(() => isMoving = true);
        OnMovementEnded.AddListener(() => isMoving = false);
    }


    private void OnDisable()
    {
        //Remove all listeners when the object is disabled
        OnMovementStarted.RemoveAllListeners();
        OnMovementEnded.RemoveAllListeners();
    }


    /// <summary>
    /// Return the value of isMoving
    /// </summary>
    /// <returns></returns>
    public bool GetIsMoving()
    {
        return isMoving;
    }

    private void Update()
    {
        //Monitor the distance from the moving object and target. Stop the agent once it is within range. 
        if (isMoving && navMeshAgent.hasPath && navMeshAgent.remainingDistance <= minimumDistance)
        {
            OnMovementEnded.Invoke();
        }

        //Ensure touch detection is enabled and the input system has detected at least one finger / pointer on the device. 
        if (enableTouchDetection && Input.touchCount > 0)
        {

            //Search through all registered touches
            foreach (Touch t in Input.touches)
            {
                //Ensure the selected id is not being used to interactive with UI
                if (t.fingerId != fingerIDOnUI)
                {
                    //On the begin phase, require the raycast to the world. 
                    if (t.phase == TouchPhase.Began)
                    {
                        touchToRaycast = t;
                        EnableMovement(true);
                        DebugManager.instance.LogStandAlone(this.name, "Moving in world");
                        return;
                    }
                }
                else
                {
                    //Register the release of the UI touch if finger is no longer on the UI element.
                    if (t.phase == TouchPhase.Ended)
                    {
                        fingerIDOnUI = -1;
                        return;
                    }
                }
            }
            return;
        }


        //Move the object if left mouse was pressed
        if (Input.GetMouseButtonDown(0) && !isPointerOverUIObject)
        {
            DebugManager.instance.LogStandAlone(this.name, "Activating mouse click movement");
            EnableMovement(false);
            return;
        }
    }


    /// <summary>
    /// Method to call from a UI EventSystem to register the UI touch
    /// </summary>
    public void PointerOnBeginUIInteraction()
    {
        if (!enableTouchDetection)
            return;

        fingerIDOnUI = GetLastFingerID();
        isPointerOverUIObject = true; 
        DebugManager.instance.LogStandAlone(this.name, $"Registering a UI touch at index {fingerIDOnUI}");
    }


    /// <summary>
    /// Method to call from a UI Event system to unregister the UI touch
    /// </summary>
    public void PointerOnEndUIInteraction()
    {
        if (!enableTouchDetection)
            return;
        isPointerOverUIObject = false;  
        fingerIDOnUI = -1;
    }


    private short GetLastFingerID()
    {
        if (Input.touchCount == 0)
            return -1;

        return (short)Input.GetTouch(Input.touchCount - 1).fingerId;
    }

    /// <summary>
    /// Internal method used to retrieving the target location and moving the object.
    /// </summary>
    /// <param name="isTouchInput"></param>
    private void EnableMovement(bool isTouchInput)
    {
        Vector3 targetLocation = GetTargetLocation(isTouchInput);

        if (targetLocation == transform.position)
            return;

        OnMovementStarted.Invoke();

        navMeshAgent.SetDestination(targetLocation);

        //Position the visualiser
        if (clickVisualiser != null)
        {
            Vector3 visLocation = new Vector3(targetLocation.x, targetLocation.y + 0.1f, targetLocation.z);
            clickVisualiser.transform.position = visLocation;
        }

    }


    /// <summary>
    /// Return the target location from either the mouse position or the first touch index
    /// </summary>
    /// <param name="isTouchInput"></param>
    /// <returns></returns>
    private Vector3 GetTargetLocation(bool isTouchInput)
    {
        RaycastHit rcHit = new RaycastHit();
        Ray ray;

        if (isTouchInput)
        {
            ray = mCamera.ScreenPointToRay(touchToRaycast.position);
        }
        else
        {
            ray = mCamera.ScreenPointToRay(Input.mousePosition);
        }

        if (Physics.Raycast(ray, out rcHit, clickLayerMask))
        {
            return rcHit.point;
        }
        else
        {
            return transform.position;
        }
    }


    /// <summary>
    /// Makes the target visualiser visible
    /// </summary>
    public void ShowTargetLocation()
    {
        if (clickVisualiser != null)
            clickVisualiser.SetActive(true);
    }

    /// <summary>
    /// Hides the target visualiser
    /// </summary>
    public void HideTargetLocation()
    {
        if (clickVisualiser != null)
            clickVisualiser.SetActive(false);
    }
}
