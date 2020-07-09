using System.Collections;
using System.Collections.Generic;
using Data.Enums;
using Data.Stucts;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class NectarController : MonoBehaviour
{
    //PROPERTIES
    //-------------------------
    [Header("Nectar Controller Settings")]
    public NectarControllerProfile nectarProfile;

    public GameObject nectarIndicator;
    public Image indicatorForeground;

    [Header("Runtime Data")]
    public BeeController owningBeeController = null;
    public SpiderController owningSpiderController = null;
    private NectarReceiver activeReceiver;
    private NectarSender activeSender;
    public NectarController objectiveNectarController;
    public NectarReceiver objectiveNectarReceiver;
    protected bool completeObjectiveOnDepletion = false;
    public bool enableObjectiveTimeLimit = false;
    public float timeAtObjective = 0.0f;

    private float startNectarValue = 0.0f;
    private float increaseRate = 0.0f;
    private float decreaseRate = 0.0f;
    private float elapsedLerpTime = 0.0f;


    //EVENTS
    //-------------------------
#pragma warning disable CS0649
    public UnityEvent OnMaxNectar;
    public UnityEvent OnDepletedNectar;
#pragma warning restore CS0649

    //METHODS
    //-------------------------
    private void Awake()
    {
        if(!nectarProfile)
        {
            Debug.LogError("Error: No nectar profile found. NectarController will be disabled");
            gameObject.SetActive(false);
            return;
        }

        //Generate an instance of the assigned NectarControllerProfile to prevent data overrite. 
        NectarControllerProfile localInstance = ScriptableObject.Instantiate<NectarControllerProfile>(nectarProfile);
        nectarProfile = localInstance;

        //Assign the attached nectar profile an owner
        nectarProfile.owner = this;
    }

    /// <summary>
    /// Public method to activate the coroutine to fill nectar indicator
    /// </summary>
    public void UpdateFillIndicator()
    {
        StartCoroutine(_UpdateInidicatorFill());
    }

    /// <summary>
    /// Coroutine to smoothly fill the nectar indicator
    /// </summary>
    /// <returns></returns>
    private IEnumerator _UpdateInidicatorFill()
    {
        float currentPct = indicatorForeground.fillAmount;
        float elapsed = 0.0f;

        while (elapsed < nectarProfile.fillSpeedSeconds)
        {
            elapsed += Time.deltaTime;
            indicatorForeground.fillAmount = Mathf.Lerp(currentPct, nectarProfile.GetCurrentNectarAsPct(), elapsed / nectarProfile.fillSpeedSeconds);
            yield return null;
        }

        indicatorForeground.fillAmount = nectarProfile.GetCurrentNectarAsPct();
        yield return null;
    }


    /// <summary>
    /// Called when notified that the owner has arrived at a specific object
    /// </summary>
    /// <param name="objective"></param>
    public void ArrivedAtObjective(Transform objective)
    {
        NectarController nc = objective.GetComponent<NectarController>();

        if (!nc) return;
        
        switch(nectarProfile.nectarControllerType)
        {
            case NectarControllerType.collector:
                SetCollectorObjective(objective);
                break;

            case NectarControllerType.theif:
                SetCollectorObjective(objective);
                break;

            default: break;
        }

    }

    /// <summary>
    /// Apply a new objective to objects with the "Collector" nectar profile
    /// </summary>
    /// <param name="objective"></param>
    private void SetCollectorObjective(Transform objective)
    {
        NectarController onc = objective.GetComponent<NectarController>();

        //Ensure the new objective has a nectar controller
        if (!onc)
            return;

        objectiveNectarController = onc;

        //Check for any receivers which match the new objectives nectar profile type
        if(ValidateReceivers(objectiveNectarController))
        {
            //Start increasing this controllers nectar if assigned to do so
            if (activeReceiver.affectOwnersSupply)
            {
                nectarProfile.numberOfReceivers++;
                increaseRate = activeReceiver.receiveRate;
                nectarProfile.SetState(NectarStatus.increasing);
            }

            //Start decreasing the objectives nectar if assigned to do so
            if (activeReceiver.affectsOthersSupply)
            {
                objectiveNectarController.nectarProfile.numberOfSenders++;
                objectiveNectarController.decreaseRate = activeReceiver.receiveRate * activeReceiver.multiplierForOthersRate;
                objectiveNectarController.nectarProfile.SetState(NectarStatus.decreasing);
            }
        }

        //Check for any senders which match the new objective nectar profile type
        if(ValidateSenders(objectiveNectarController))
        {
            //Start increasing this nectar controllers profile nectar supply
            objectiveNectarController.nectarProfile.numberOfReceivers++;
            objectiveNectarController.increaseRate = activeSender.sendRate;
            objectiveNectarController.nectarProfile.SetState(NectarStatus.increasing);


            //Start decreasing the objectives nectar supply if assigned to do so
            if (activeSender.affectsOwnersSupply)
            {
                nectarProfile.numberOfSenders++;
                decreaseRate = activeSender.sendRate;
                nectarProfile.SetState(NectarStatus.decreasing);
            }
        }

    }

    /// <summary>
    /// Checks the recieves in the nectar profile for a match with the new objective
    /// </summary>
    /// <param name="nectarController"></param>
    /// <returns></returns>
    protected bool ValidateReceivers(NectarController nectarController)
    {
        foreach(NectarReceiver receiver in nectarProfile.receivers)
        {
            if (receiver.receiveFromType == nectarController.nectarProfile.nectarControllerType)
            {
                activeReceiver = receiver;
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// Checks the senders in the nectar profile for a match with the new objective
    /// </summary>
    /// <param name="nectarController"></param>
    /// <returns></returns>
    protected bool ValidateSenders(NectarController nectarController)
    {
        foreach (NectarSender sender in nectarProfile.senders)
        {
            if (sender.sendToType == nectarController.nectarProfile.nectarControllerType)
            {
                activeSender = sender;
                return true;
            }
        }
        return false;
    }


    /// <summary>
    /// Ensure any states altered on other objects are updated once this object has completed its objective
    /// This is mainly for nectar controllers which can be influenced by a number of objects at once such as the hive or the flowers
    /// </summary>
    public void CompletedObjective()
    {
        if (!objectiveNectarController)
            return;

        if (objectiveNectarController.nectarProfile.nectarControllerType == NectarControllerType.distributer)
        {
            objectiveNectarController.nectarProfile.numberOfSenders--;
        }

        if (objectiveNectarController.nectarProfile.nectarControllerType == NectarControllerType.hub)
        {
            objectiveNectarController.nectarProfile.numberOfReceivers--;
        }
    }


    /// <summary>
    /// Reset any run time data required by the update method to handle this controllers nectar supply
    /// </summary>
    /// <returns></returns>
    public bool InitialiseActiveNectarValues()
    {
        startNectarValue = nectarProfile.currentNectar;
        elapsedLerpTime = 0.0f;

        return true;
    }

    /// <summary>
    /// Handles the increasing or decreasing in the nectar supply which is applied to the profile 
    /// Heavily safe guarded to ensure updates are only called when required
    /// </summary>
    private void Update()
    {
        //If this nectar controller is set to start increasing its nectar amount
        if (nectarProfile.nectarState == NectarStatus.increasing)
        {
            //If the owning controller belongs to the player, ensure the objective does not have a depleted supply
            if (owningSpiderController && objectiveNectarController)
            {
                if (objectiveNectarController.nectarProfile.nectarState == NectarStatus.depleted)
                {
                    return;
                }
            }

            float rate = increaseRate * Time.deltaTime;

            elapsedLerpTime += Time.deltaTime;

            nectarProfile.currentNectar += rate;

            UpdateFillIndicator();

            if (owningBeeController)
            {
                //Complete the objective if this nectar controller has reached its max level
                if (nectarProfile.currentNectar == nectarProfile.maxNectar)
                {
                    owningBeeController.OnCompletedCurrentObjective.Invoke();
                    return;
                }

                //Complete the objective if this nectar controller has reached its maximum time allowed at a specific objective
                if (enableObjectiveTimeLimit && elapsedLerpTime >= timeAtObjective)
                {
                    nectarProfile.SetState(NectarStatus.idle);
                    owningBeeController.OnCompletedCurrentObjective.Invoke();
                    return;
                }

                if (objectiveNectarController)
                {
                    //Ensure the objective is completed if the objective runs out of nectar
                    if (objectiveNectarController.nectarProfile.nectarState == NectarStatus.depleted)
                    {
                        nectarProfile.SetState(NectarStatus.idle);
                        owningBeeController.OnCompletedCurrentObjective.Invoke();
                        return;
                    }
                }
            }
        }

        //Called if this nectar controller is setto decreasing its ammount.
        if(nectarProfile.nectarState == NectarStatus.decreasing)
        {
            float rate = decreaseRate * Time.deltaTime;

            elapsedLerpTime += Time.deltaTime;

            nectarProfile.currentNectar -= rate;

            UpdateFillIndicator();

            if(owningBeeController)
            {
                //Ensure the bee's states are updated if its nectar supply has been depleted
                if (nectarProfile.nectarState == NectarStatus.depleted)
                {
                    nectarProfile.SetState(NectarStatus.idle);
                    owningBeeController.SetBeeState(BeeState.working);
                    owningBeeController.OnCompletedCurrentObjective.Invoke();
                    return;
                }
            }
        }

        //If the object is set to regenerate nectar over a constant period of time
        if(nectarProfile.canRegenerateNectar && nectarProfile.nectarState == NectarStatus.idle)
        {
            float rate = nectarProfile.regenerationRate * Time.deltaTime;

            nectarProfile.currentNectar += rate;

            nectarProfile.SetIsLowOnNectar();

            UpdateFillIndicator();
        }

        //If the object is set to decrease nectar over a constant period of time
        if(nectarProfile.canLoseNectarOverTime && nectarProfile.nectarState == NectarStatus.idle)
        {
            float rate = nectarProfile.lossRate * Time.deltaTime;

            nectarProfile.currentNectar -= rate;

            nectarProfile.SetIsLowOnNectar();

            UpdateFillIndicator();
        }
    }
}
