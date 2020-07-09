using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DebugManager : MonoBehaviour
{
    //PROPERTIES
    //-------------------------
    public static DebugManager instance { get; private set; }

    [Header("Customer Settings")]
    [Tooltip("Allow an output log to be displayed on stand alone builds")]
    public bool enableOnScreenLog = false;

    [Header("Required References")]
    public GameObject onScreenLogRef;
    public GameObject debugTextPrefabRef;
    public GameObject consoleContentRef;


    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(instance.transform.parent.gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

        //Enable / disable the debug log window depending on the public settings and having valid references.
        if(enableOnScreenLog)
        {
            if(!ValidateOnScreenLog())
            {
                Debug.LogError("Failed to initialise the on Screen Log");
                return;
            }

            onScreenLogRef.SetActive(true);
        }
        else
        {
            onScreenLogRef.SetActive(false);
        }
    }

    private bool ValidateOnScreenLog()
    {
        return (onScreenLogRef && debugTextPrefabRef && consoleContentRef);
    }

    /// <summary>
    /// Call this method to add a message to the debug log window
    /// </summary>
    /// <param name="header"></param>
    /// <param name="body"></param>
    public void LogStandAlone(string header, string body)
    {
        if(enableOnScreenLog)
        {
            GameObject newLogEntry = Instantiate(debugTextPrefabRef, consoleContentRef.transform);
            newLogEntry.GetComponent<Text>().text = $"{header} | {body}";
        }
    }
}
