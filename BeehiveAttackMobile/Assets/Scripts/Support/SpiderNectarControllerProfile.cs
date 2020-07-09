using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "Spider Nectar Profile", menuName = "Beehive Attack Objects/Spider Nectar Profile")]
public class SpiderNectarControllerProfile : NectarControllerProfile
{

    [Header("Runtime Data", order = 0)]
    [Header("-------------------------", order = 1)]

    public SpiderController owningSpiderController = null;
    public bool canReceiveFromHive = false;

    public override int numberOfReceivers
    {
        get { return base.numberOfReceivers; }
        set { base.numberOfReceivers = value;  }
    }

    public override int numberOfSenders
    {
        get{ return base.numberOfSenders; }
        set
        {
            int prevNumberOfSenders = _numberofSenders;
            base.numberOfSenders = value;

            if (prevNumberOfSenders == 0)
            {
                owner.owningSpiderController.OnStartBeingAttacked.Invoke();
            }
            else if(_numberofSenders == 0)
            {
                owner.owningSpiderController.OnEndBeingAttacked.Invoke();
            }
        }
    }
}
