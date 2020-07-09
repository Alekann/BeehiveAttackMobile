using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HiveManager : MonoBehaviour
{
    //Properties
    //---------------------------
    /// <summary>
    /// instance call to the HiveManager singleton
    /// </summary>
    public static HiveManager instance { get; private set; }

    [Header("References")]
    [Tooltip("Reference to the GameObject containing the work objectives for the bee")]
    public GameObject objectivesGroup;
    [Tooltip("Reference to the beehive within the scene")]
    public GameObject hiveObject;

    [Header("Runtime Information", order = 0)]
    [Header("-----------------", order = 1)]
    [Header("Objectives", order = 2)]
    [SerializeField] private List<Transform> workObjectiveTransforms = new List<Transform>();


    //METHODS
    //--------------------------
    private void Awake()
    {
        if(!instance)
        {
            instance = this;
        }
        else
        {
            Destroy(this);
        }

        GenerateObjectiveTransforms();
    }

    /// <summary>
    /// Returns a list of all the worker objective transform components 
    /// </summary>
    /// <returns></returns>
    public List<Transform> GetObjectiveTransforms()
    {
        return workObjectiveTransforms;
    }

    /// <summary>
    /// Stores a list of all the worker objective transform components
    /// </summary>
    private void GenerateObjectiveTransforms()
    {
        Transform[] allChildTransforms = instance.objectivesGroup.GetComponentsInChildren<Transform>();

        for(int i = 1; i < allChildTransforms.Length; i++)
        {
            //Ensure only direct children getting stored and not grandchildren etc
            if(allChildTransforms[i].parent == instance.objectivesGroup.transform)
            {
                //Store the transform
                workObjectiveTransforms.Add(allChildTransforms[i]);

                allChildTransforms[i].gameObject.tag = "WorkObjective";
            }
        }
    }
}
