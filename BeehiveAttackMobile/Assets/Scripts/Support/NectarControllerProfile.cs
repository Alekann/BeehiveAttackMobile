using System.Collections;
using System.Collections.Generic;
using Data.Enums;
using Data.Stucts;
using UnityEngine;



[CreateAssetMenu(fileName = "Nectar Profile", menuName = "Beehive Attack Objects/Nectar Profile")]
public class NectarControllerProfile : ScriptableObject
{

    //PROPERTIES
    //-------------------------------
    [Header("Profile Settings")]
    public NectarControllerType nectarControllerType;
    public float maxNectar = 1000.0f;
    public float startingNectar = 500.0f;
    public float lowIndicatorPercent = 30.0f;
    public bool canRegenerateNectar = false;
    public float regenerationRate = 30f;
    public bool canLoseNectarOverTime = false;
    public float lossRate = 30.0f;
    private NectarController _owner;
    public NectarController owner
    {
        get { return _owner; }
        set
        {
            _owner = value;
            if(_owner)
            {
                initialise();
            }
        }
    }

    [Header("Receive Settings")]
    public List<NectarReceiver> receivers = new List<NectarReceiver>();

    [Header("Sender Settings")]
    public List<NectarSender> senders = new List<NectarSender>();

    [Header("Nectar Indicator Display")]
    public bool enableIndicator = true;
    public bool alwaysShowIndicator = false;
    public float fillSpeedSeconds = 0.5f;
    public Color defaultColor = Color.blue;
    public Color defaultColorLowNectar = Color.cyan;
    public Color increaseColor = Color.green;
    public Color increaseColorLowNectar = Color.green;
    public Color decreaseColor = Color.red;
    public Color decreaseColorLowNectar = Color.red;


    [Header("Runtime Data", order = 0)]
    [Header("------------------------", order = 1)]
    private NectarStatus _nectarState = NectarStatus.idle;
    public NectarStatus nectarState
    {
        get { return _nectarState; }
        set
        {
            _nectarState = value;
            UpdateInidicatorColour();
            if (owner.name.Contains("Bee")) { }
                //Debug.LogWarning($"Bee has new state {nectarState}");
        }
    }
    private bool _isLowNectar = false;
    public bool isLowNectar
    {
        get { return _isLowNectar; }
        set
        {
            _isLowNectar = value;
            UpdateInidicatorColour();
        }
    }
    protected int _numberofReceivers = 0;
    virtual public int numberOfReceivers
    {
        get { return _numberofReceivers; }
        set
        {
            _numberofReceivers = (int)Mathf.Clamp(value, 0, Mathf.Infinity);
            if (value == 0) SetState(NectarStatus.idle);
        }
    }
    protected int _numberofSenders = 0;
    virtual public int numberOfSenders
    {
        get { return _numberofSenders; }
        set
        {
            _numberofSenders = (int)Mathf.Clamp(value, 0, Mathf.Infinity); 
            
            if (value == 0) SetState(NectarStatus.idle);
        }
    }
    private float _currentNectar = 0.0f;
    public float currentNectar
    {
        get { return _currentNectar; }
        set
        {
            _currentNectar = value;
            if (value <= 0)
            {
                _currentNectar = 0;
                SetState(NectarStatus.depleted);
            }

            if(value >= maxNectar)
            {
                _currentNectar = maxNectar;
                SetState(NectarStatus.full);
            }
        }
    }




    //METHODS
    //--------------------------------
    public void initialise()
    {
        if(enableIndicator && !owner.nectarIndicator)
        {
            Debug.LogError($"Error: {owner.name} - Cannot locator Nectar Indicator");
            return;
        }

        SetState(NectarStatus.idle);
        currentNectar = startingNectar;
        SetIsLowOnNectar();
        owner.nectarIndicator.SetActive(enableIndicator && alwaysShowIndicator);
        owner.UpdateFillIndicator();
        numberOfReceivers = 0;
       
    }

    public float GetCurrentNectarAsPct()
    {
        return (currentNectar / maxNectar);

    }


    public void SetState(NectarStatus newState)
    {
        nectarState = newState;
        owner.InitialiseActiveNectarValues();

        switch (newState)
        {
            case NectarStatus.idle:
                break;

            case NectarStatus.increasing:
                break;

            case NectarStatus.decreasing:
                break;

            case NectarStatus.full:
                owner.OnMaxNectar.Invoke();
                break;

            case NectarStatus.depleted:
                owner.OnDepletedNectar.Invoke();
                break;
        }
    }

    public void SetIsLowOnNectar()
    {
        isLowNectar = GetCurrentNectarAsPct() < (lowIndicatorPercent / 100);
    }

    public void UpdateInidicatorColour()
    {
        if (!owner.indicatorForeground)
            return;

        switch(nectarState)
        {
            case NectarStatus.idle:
            case NectarStatus.full:
                owner.indicatorForeground.color = isLowNectar? defaultColorLowNectar : defaultColor;
                break;

            case NectarStatus.increasing:
                owner.indicatorForeground.color = isLowNectar ? increaseColorLowNectar : increaseColor;
                break;

            case NectarStatus.decreasing:
                owner.indicatorForeground.color = isLowNectar ? decreaseColorLowNectar : decreaseColor;
                break;

        }
    }
}
