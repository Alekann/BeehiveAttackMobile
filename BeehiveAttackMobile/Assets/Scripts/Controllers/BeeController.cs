using System.Collections;
using System.Collections.Generic;
using Data.Enums;
using Data.EventClasses;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;


[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(NectarController))]
public class BeeController : MonoBehaviour
{

    //PROPERTIES
    //---------------------------------
    [Header("Bee Settings")]
    [Tooltip("Defines the behaviour of the bee")]
    [SerializeField]public BeePersonality _personality;
    public BeePersonality personality
    {
        get { return _personality;  }
        set { _personality = value; }
    }

    [Tooltip("How close the bee needs to be to complete its objective")]
    public float workObjectiveDistanceTolerance = 0.5f;
    [Tooltip("How close the bee needs to be to attack the intruder")]
    public float intruderDistanceTolerance = 1.0f;

    [Header("References")]
    [Tooltip("Nav mesh agent attached to the bee")]
    public NavMeshAgent nmAgent;
    [SerializeField] private BeeBehaviourController _behaviourController;
    [SerializeField] protected BeeBehaviourController behaviourController
    {
        get { return _behaviourController; }
        set
        {
            _behaviourController = value;
            if (_behaviourController != null)
            {
                hasBehaviourController = true;
            }
            else
            {
                hasBehaviourController = false;
            }
        }
    }
    public NectarController nectarController; 

    [Header("Runtime Information", order = 0)]
    [Header("-----------------", order = 1)]
    [Header("Objectives", order = 2)]
    public Transform currentWorkObjective;
    public List<Transform> workObjectiveList = new List<Transform>();
    public int currentWorkObjectiveIndex = 0;
    public NectarControllerProfile currentWorkObjectiveNectarProfile;
    public int lastWorkObjectiveIndex = 0;
    public float timeAtObjectiveSeconds = 2.0f;
    public bool hasWorkObjectives = false;


    [Header("Behaviour")]
    [SerializeField] private BeeState _beeState;
    public BeeState beeState
    {
        get { return _beeState; }
        private set { _beeState = value; }
    }

    public GameObject intruderObjective;
    private bool _isAttacking = false;
    public bool isAttacking
    {
        get { return _isAttacking;  }
        set
        {
            if (_isAttacking == value)
                return;

            if (behaviourController != null)
            {
                if (value)
                {
                    _isAttacking = value;
                    behaviourController.BeginAttackOnIntruder();
                    return;
                }
                else
                {
                    behaviourController.EndAttackOnIntruder();
                    _isAttacking = value;
                    return;
                }
            }
            else
            {
                _isAttacking = value;
            }
        }
    }


    [Header("Navigation")]
    public bool isMoving = false;
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

    [Header("Debug Information")]
    [SerializeField] private int initialiseCounter = 0;
    [SerializeField] private bool hasBehaviourController = false;

    //EVENTS
#pragma warning disable CS0649
    public UnityEvent OnReceivedNewObjectives;
    public TransformUnityEvent OnArrivedAtObjective;
    public UnityEvent OnCompletedCurrentObjective;
    public UnityEvent OnCompletedAllObjectives;
#pragma warning restore CS0649

    //METHODS
    //---------------------------------
    private void Awake()
    {
        //Safeguard the Nav Mesh Agent
        if(!nmAgent)
        {
            nmAgent = GetComponent<NavMeshAgent>();
            if (!nmAgent)
            {
                Debug.LogError("Error: Nav Mesh Agent not attached to the Bee prefab");
                gameObject.SetActive(false);
            }
        }

        if(!nectarController)
        {
            nectarController = GetComponent<NectarController>();
        }

        EventBindings();
    }

    private void OnEnable()
    {
        //Initialise the class once the HiveManager instance is valid
        StartCoroutine(PostInitialise());
    }

    private IEnumerator PostInitialise()
    {
        if (HiveManager.instance)
        {
            /*
                CODE HERE TO INITIALISE THE BEE CONTROLLER!
            */
            nmAgent.stoppingDistance = 0.5f;
            hiveLocation = HiveManager.instance.hiveObject.transform;

            currentWorkObjectiveIndex = 0;
            lastWorkObjectiveIndex = 0;

            AquireWorkObjectives();

            behaviourController = new BeeBehaviourController(this);

            GetComponent<NectarController>().owningBeeController = this;

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
    /// Add any required listeners to the events
    /// </summary>
    public void EventBindings()
    {
        OnArrivedAtObjective.AddListener(AssignObjectiveOrder);
        OnCompletedCurrentObjective.AddListener(nectarController.CompletedObjective);
    }


    /// <summary>
    /// Public version of the beeState used in the bee behaviour controller
    /// </summary>
    /// <param name="newState"></param>
    public void SetBeeState(BeeState newState)
    {
        beeState = newState;
    }


    /// <summary>
    /// Returns the transform for the current objective the bee is tending to 
    /// </summary>
    /// <returns></returns>
    public Transform GetCurrentObjective()
    {
        switch(beeState)
        {
            case BeeState.working:
            case BeeState.defence:
                return currentWorkObjective;

            case BeeState.attack:
                if(intruderObjective)
                {
                    return intruderObjective.transform;
                }
                return null;

            case BeeState.returnToHive:
                return hiveLocation;

            default: return null;
        }
    }


    /// <summary>
    /// Public method used to specifically give the bee an objective. 
    /// Note, the order maybe rejected depending on the bees current states.
    /// </summary>
    /// <param name="objective"></param>
    public void AssignObjectiveOrder(Transform objective)
    {
        behaviourController.AssignObjectiveOrder(objective);
    }


    /// <summary>
    /// Asks the game manager for a list of all the cached objective transforms
    /// </summary>
    public void AquireWorkObjectives()
    {
        //Clear and new objectives before storing new objective list.
        workObjectiveList.Clear();

        //Access the HiveManager and pull the latest objectives
        workObjectiveList.AddRange(HiveManager.instance.GetObjectiveTransforms());

        //Inform any listeners of the update
        OnReceivedNewObjectives.Invoke();

        hasWorkObjectives = true;
    }


    /// <summary>
    /// Assign a work objective by index
    /// </summary>
    /// <param name="newWorkObjectiveIndex"></param>
    public void AssignNewWorkObjective(int newWorkObjectiveIndex)
    {
        //Ensure the bee is working before it can receive a new objective
        if (beeState != BeeState.working)
            return;


        //Ensure the new index exists in our current work objectives list
        if (newWorkObjectiveIndex >= 0 && newWorkObjectiveIndex < workObjectiveList.Count)
        {
            //Update the last last work objective index with the current.
            if (currentWorkObjectiveIndex != -1)
            {
                lastWorkObjectiveIndex = currentWorkObjectiveIndex;
            }

            //Update the current work index to the assigned index
            currentWorkObjectiveIndex = newWorkObjectiveIndex;
            currentWorkObjective = workObjectiveList[currentWorkObjectiveIndex];
            currentWorkObjectiveNectarProfile = currentWorkObjective.GetComponent<NectarController>().nectarProfile;

            //Give the new objective to the nav mesh agent
            nmAgent.SetDestination(currentWorkObjective.position);

            StartCoroutine(StartMovingToObjective());
        }
    }

    /// <summary>
    /// Coroutine to delay the activation of the new destination as the NavMesh will not update instantaniously
    /// </summary>
    /// <returns></returns>
    public IEnumerator StartMovingToObjective()
    {
        yield return new WaitForSeconds(0.1f);
        isAtObjective = false;
    }
  
    
    /// <summary>
    /// Method to assign the next work objective in the bees work order list
    /// </summary>
    public void AssignNextWorkObjective()
    {
        //Store the proposed index for validation
        int tIndex = currentWorkObjectiveIndex + 1;

        //Ensure the index is valid
        if(tIndex < 0 && tIndex >= workObjectiveList.Count)
        {
            return;
        }

        //Assign the new objective to the bee
        AssignNewWorkObjective(tIndex);
    }


    /// <summary>
    /// Method to assign the bee to the first work objective in the work order list
    /// </summary>
    public void AssignFirstObjective()
    {
        AssignNewWorkObjective(0);
    }


    /// <summary>
    /// Invokes the OnCompletedAllObjectives event if the work order list has been completed
    /// </summary>
    public void CheckForCompletedObjectives()
    {
        if (currentWorkObjectiveIndex == workObjectiveList.Count - 1)
            OnCompletedAllObjectives.Invoke();
    }


    /// <summary>
    /// Public method to assign the bee back to its hive 
    /// </summary>
    public void ReturnToHive()
    {
        behaviourController.SetBeeState(BeeState.returnToHive);
    }


    /// <summary>
    /// Call the movement functionality / validation inside the behaviour controller
    /// </summary>
    private void Update()
    {
        if (!hasBehaviourController)
            return;

        behaviourController.NavAgentValidation();
    }
}
