using System.Collections;
using System.Collections.Generic;
using Data.Enums;
using UnityEngine;

[System.Serializable]
public class BeeBehaviourController
{

    [Header("Runtime Settings")]
    public BeeController owner;
    [SerializeField] private BeeState beeStateBeforeIntruder = BeeState.working;

    public float navStoppingDistance
    {
        get
        {
            if(owner.beeState == BeeState.attack)
            {
                return owner.nmAgent.stoppingDistance + owner.intruderDistanceTolerance;
            }
            else
            {
                return owner.nmAgent.stoppingDistance + owner.workObjectiveDistanceTolerance;
            }
        }
        set { }
    }
       

    public BeeBehaviourController(BeeController owningBeeController)
    {
        owner = owningBeeController;
        AddDynamicBindings();
    }


    /// <summary>
    /// Add any relevant event listeners. Default listeners detect the player entering the hive
    /// </summary>
    private void AddDynamicBindings()
    {
        GameObject hM = HiveManager.instance.hiveObject;

        if (hM)
        {
            HiveController hC = hM.GetComponent<HiveController>();

            if (hC)
            {
                hC.OnIntruderEnter.AddListener(RespondToIntruderEnter);
                hC.OnIntruderExit.AddListener(RespondToIntruderExit);
            }
        }
    }

    /// <summary>
    /// Called when the player enters the hive zone
    /// </summary>
    /// <param name="intruder"></param>
    protected void RespondToIntruderEnter(Transform intruder)
    {
        owner.intruderObjective = intruder.gameObject;
        SetIntruderAction(true);
    }

    /// <summary>
    /// Called when the player leaves the hive zone
    /// </summary>
    /// <param name="intruder"></param>
    protected void RespondToIntruderExit(Transform intruder)
    {
        SetIntruderAction(false);
        owner.intruderObjective = null;
    }

    /// <summary>
    /// Apply the response to an intruder depending on the bee's assigned personality
    /// </summary>
    /// <param name="hasIntruder"></param>
    protected void SetIntruderAction(bool hasIntruder)
    {
        if(!hasIntruder && owner.personality != BeePersonality.queen)
        {
            SetBeeState(beeStateBeforeIntruder);
            EndAttackOnIntruder();
            owner.isAttacking = false;
            return;
        }

        switch (owner.personality)
        {
            case BeePersonality.attacker:
                SetBeeState(BeeState.attack);
                break;

            case BeePersonality.worker:
                SetBeeState(BeeState.defence);
                break;

        }
    }

    /// <summary>
    /// Validates the new incoming bee state with the bees current behaviour. If successful it is applied to the owning BeeController
    /// </summary>
    /// <param name="newState"></param>
    public void SetBeeState(BeeState newState)
    {
        //Safeguard against duel calls
        if (newState == owner.beeState)
            return;

        //Reset the attacking status
        owner.isAttacking = false;

        switch (newState)
        {
            case BeeState.working:
                owner.SetBeeState(newState);
                owner.AssignNewWorkObjective(owner.currentWorkObjectiveIndex);
                break;

            case BeeState.defence:
                if(owner.beeState == BeeState.returnToHive)
                {
                    beeStateBeforeIntruder = owner.beeState;
                    owner.nectarController.nectarProfile.nectarState = NectarStatus.idle;
                    owner.OnCompletedCurrentObjective.Invoke();
                }

                //Check which objective the bee is closer to for defence
                float cDist = Vector3.Distance(owner.transform.position, owner.currentWorkObjective.position);
                float lDist = Vector3.Distance(owner.transform.position, owner.workObjectiveList[owner.lastWorkObjectiveIndex].position);

                if (lDist < cDist)
                {
                    owner.AssignNewWorkObjective(owner.lastWorkObjectiveIndex);
                }
                owner.SetBeeState(newState);
                break;

            case BeeState.attack:
                beeStateBeforeIntruder = owner.beeState;
                owner.nectarController.nectarProfile.nectarState = NectarStatus.idle;
                owner.OnCompletedCurrentObjective.Invoke();
                owner.SetBeeState(newState);
                AssignObjectiveOrder(owner.intruderObjective.transform);
                break;

            case BeeState.returnToHive:
                //Give the hive location to the nav mesh agent
                owner.nmAgent.SetDestination(owner.hiveLocation.transform.position);
                owner.StartCoroutine(owner.StartMovingToObjective());
                owner.SetBeeState(newState);
                break;
        }
    }


    /// <summary>
    /// Assign a new objective order whilst during completion of a current objective
    /// </summary>
    /// <param name="objective"></param>
    public void AssignObjectiveOrder(Transform objective)
    {
        switch (owner.beeState)
        {
            case BeeState.attack:
                
                break;

            case BeeState.working:
                owner.nectarController.enableObjectiveTimeLimit = true;
                owner.nectarController.timeAtObjective = owner.timeAtObjectiveSeconds;
                owner.nectarController.ArrivedAtObjective(objective);
                break;

            case BeeState.returnToHive:
                owner.nectarController.enableObjectiveTimeLimit = false;
                owner.nectarController.ArrivedAtObjective(objective);
                break;

            default:break;
        }
    }

    /// <summary>
    /// Forces the bee to complete its current objective
    /// </summary>
    public void CancelCurrentObjectiveOrder()
    {
        owner.nectarController.CompletedObjective();
    }


    /// <summary>
    /// Handle the bees distance calculations from the nav mesh agent
    /// </summary>
    public void NavAgentValidation()
    {
        if (!owner.isAtObjective)
        {
            if (owner.nmAgent.hasPath)
            {
                if (owner.beeState == BeeState.working && owner.GetCurrentObjective().GetComponent<NectarController>().nectarProfile.numberOfSenders >= 2)
                {
                    owner.AssignNextWorkObjective();
                }
            }

            if (owner.nmAgent.remainingDistance < navStoppingDistance)
            {
                owner.isAtObjective = true;
                owner.OnArrivedAtObjective.Invoke(owner.GetCurrentObjective());
            }
        }

        if (owner.beeState == BeeState.attack)
        {
            FollowIntruder();

            owner.isAttacking = owner.nmAgent.remainingDistance < navStoppingDistance;
            owner.isAtObjective = owner.isAttacking;

            return;
        }
    }


    /// <summary>
    /// Update the nav mesh agents destination for the intruder
    /// </summary>
    public void FollowIntruder()
    {
        if (!owner.intruderObjective)
            return;

        owner.nmAgent.destination = owner.intruderObjective.transform.position;
    }


    /// <summary>
    /// Inform all listeners that the bee is attacking the intruder
    /// </summary>
    public void BeginAttackOnIntruder()
    {
        if (!owner.intruderObjective)
            return;

        owner.nectarController.ArrivedAtObjective(owner.intruderObjective.transform);
    }

    /// <summary>
    /// Inform all listeners that the bee is no longer attacking the intruder
    /// </summary>
    public void EndAttackOnIntruder()
    {
        if (!owner.isAttacking)
            return;

        if(owner.intruderObjective)
        {
            NectarController indruderNectarController = owner.intruderObjective.GetComponent<NectarController>();

            if (!indruderNectarController)
                return;
            
            indruderNectarController.nectarProfile.numberOfSenders--;
        }
    }

}

