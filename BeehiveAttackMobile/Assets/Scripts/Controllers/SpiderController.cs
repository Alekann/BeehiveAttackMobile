using System.Collections;
using System.Collections.Generic;
using Data.Enums;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.AI;


[RequireComponent(typeof(Collider))]
public class SpiderController : MonoBehaviour
{

    [Header("References")]
    [Tooltip("Nav mesh agent attached to the spider")]
    public NavMeshAgent nmAgent;
    [Tooltip("A Nectar Controller object required for nectar validation")]
    public NectarController nectarController;


    [Header("Runtime Data", order = 0)]
    [Header("------------------------", order = 1)]
    [Header("Navigation", order = 2)]
    private bool _isAtObjective = false;
    public bool isAtObjective
    {
        get { return _isAtObjective; }
        set
        {
            _isAtObjective = value;
        }
    }
    public Transform hiveLocation;

    [Header("Spider Data")]
    private bool _isBeingAttacked;
    public bool isBeingAttacked
    {
        get { return _isBeingAttacked;  }
        set
        {
            _isBeingAttacked = value;

            if(value && nectarController.objectiveNectarController)
            {
                nectarController.objectiveNectarController.nectarProfile.numberOfSenders--;
            }
        }
    }

    [Header("Debug Information")]
    [SerializeField] private int initialiseCounter = 0;

    //EVENTS
    //-----------------------
#pragma warning disable CS0649
    public UnityEvent OnStartBeingAttacked, OnEndBeingAttacked;
#pragma warning restore CS0649

    //METHODS
    //---------------------------------
    private void Awake()
    {
        //Safeguard the Nav Mesh Agent
        if (!nmAgent)
        {
            nmAgent = GetComponent<NavMeshAgent>();
            if (!nmAgent)
            {
                Debug.LogError("Error: Nav Mesh Agent not attached to the Bee prefab");
                gameObject.SetActive(false);
            }
        }

        if (!nectarController)
        {
            nectarController = GetComponent<NectarController>();
        }
    }

    private void OnEnable()
    {
        //Initialise the class once the HiveManager instance is valid
        StartCoroutine(PostInitialise());
    }

    /// <summary>
    /// Called on OnEnable. Used to initialise this controller post initialisation of the game manager
    /// </summary>
    /// <returns></returns>
    private IEnumerator PostInitialise()
    {
        if (HiveManager.instance)
        {
            /*
                CODE HERE TO INITIALISE THE SPIDER CONTROLLER!
            */

            GetComponent<SpiderNectarController>().owningSpiderController = this;

            hiveLocation = HiveManager.instance.hiveObject.transform;

            EventBindings();

            yield return null;
        }
        else
        {
            /*
                THIS CLASS WILL ATTEMPT TO ACCESS THE HIVEMANAGER INSTANCE 5 TIMES BEFORE FAILING. 
            */
            initialiseCounter++;
            if (initialiseCounter == 5)
            {
                Debug.LogError("Hive manager instance not valid or failed to initialise. Bee controller is disabled");
                gameObject.SetActive(false);
                yield return null;
            }

            yield return new WaitForEndOfFrame();
            StartCoroutine(PostInitialise());
        }
    }

    /// <summary>
    /// Add listeners to the events
    /// </summary>
    private void EventBindings()
    {
        OnStartBeingAttacked.AddListener(() => isBeingAttacked = true);
        OnEndBeingAttacked.AddListener(() => isBeingAttacked = false);
    }


    /// <summary>
    /// Called once the collider on the spider has interested with a trigger
    /// </summary>
    /// <param name="other"></param>
    private void OnTriggerEnter(Collider other)
    { 
        if(other.transform.tag == "Hive" && other.GetType() == typeof(CapsuleCollider))
        {
            isAtObjective = true;
            nectarController.ArrivedAtObjective(hiveLocation);
        }
    }


    /// <summary>
    /// Called once the collider on the spider is no longer interesting with a specific trigger
    /// </summary>
    /// <param name="other"></param>
    private void OnTriggerExit(Collider other)
    {
        if (other.transform.tag == "Hive" && other.GetType() == typeof(CapsuleCollider))
        {
            isAtObjective = false;
            if(!isBeingAttacked)
            {
                nectarController.nectarProfile.SetState(NectarStatus.idle);
            }
            nectarController.objectiveNectarController.nectarProfile.numberOfSenders--;
        }
    }
}
