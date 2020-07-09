using System.Collections;
using System.Collections.Generic;
using Data.Enums;
using UnityEngine;

public class BeeGenerator : InstanceGenerator
{
    //PROPERTIES
    //----------------------
    public BeePersonality typeOfBee;

    //METHODS
    //----------------------

    /// <summary>
    /// Call to spawn a new bee of a specific type (Requires Bee prefrab reference)
    /// </summary>
    /// <param name="objectToSpawn"></param>
    public override void SpawnInstance(GameObject objectToSpawn)
    {
        base.SpawnInstance(objectToSpawn);

        BeeController bC = GetLastInstance().GetComponent<BeeController>();

        if (bC)
            bC.personality = typeOfBee;
    }
}
