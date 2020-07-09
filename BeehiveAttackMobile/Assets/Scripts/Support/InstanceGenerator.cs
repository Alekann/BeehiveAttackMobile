using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class InstanceGenerator : MonoBehaviour
{
    //PROPERTIES
    //----------------------
    private List<GameObject> instances = new List<GameObject>();
    private GameObject lastInstanceSpawned;
    private Vector3 pos = Vector3.zero;
    private Quaternion rot = Quaternion.identity;
    public int instanceCount
    {
        get { return instances.Count; }
        set { }
    }

    //EVENTS
    //----------------------
#pragma warning disable CS0649
    public UnityEvent OnInstanceSpawned;
    public UnityEvent OnAllInstancesRemoved;
#pragma warning restore CS0649

    //METHODS
    //----------------------

    /// <summary>
    /// Instantiate an object and store in the generator instance
    /// </summary>
    /// <param name="objectToSpawn"></param>
    public virtual void SpawnInstance(GameObject objectToSpawn)
    {
        _SpawnInstance(objectToSpawn, pos, rot);
    }

    /// <summary>
    /// Instantiate an object and store in the generator instance
    /// </summary>
    /// <param name="objectToSpawn"></param>
    /// <param name="position"></param>
    public virtual void SpawnInstance(GameObject objectToSpawn, Vector3 position)
    {
        _SpawnInstance(objectToSpawn, position, rot);
    }

    /// <summary>
    /// Instantiate an object and store in the generator instance
    /// </summary>
    /// <param name="objectToSpawn"></param>
    /// <param name="position"></param>
    /// <param name="rotation"></param>
    public virtual void SpawnInstance(GameObject objectToSpawn, Vector3 position, Quaternion rotation)
    {
        _SpawnInstance(objectToSpawn, position, rotation);
    }

    /// <summary>
    /// Internal method to handle spawning the instance once the public method has been called
    /// </summary>
    /// <param name="objectToSpawn"></param>
    /// <param name="position"></param>
    /// <param name="rotation"></param>
    protected virtual void _SpawnInstance(GameObject objectToSpawn, Vector3 position, Quaternion rotation)
    {
        if (!objectToSpawn)
            return;

        lastInstanceSpawned = Instantiate(objectToSpawn, position, rotation);

        instances.Add(lastInstanceSpawned);
        OnInstanceSpawned.Invoke();
    }

    /// <summary>
    /// Remove all the instances managed by this generator
    /// </summary>
    public virtual void RemoveAllInstances()
    {
        foreach(GameObject obj in instances) { Destroy(obj); }

        OnAllInstancesRemoved.Invoke();
    }

    /// <summary>
    /// Return the very last instance spawned.
    /// </summary>
    /// <returns></returns>
    public virtual GameObject GetLastInstance()
    {
        return lastInstanceSpawned;
    }
}
