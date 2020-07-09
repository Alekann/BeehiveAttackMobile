using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Events;



public class LevelManager : MonoBehaviour
{
    //PROPERTIES
    //----------------------------------------------------------
    public static LevelManager instance
    {
        get;
        private set;
    }

    [Header("Custom Script Script Settings")]
    public int transitionMapLevelIndex;

    [Header("World Information")]
    [SerializeField] private string levelLoadedName;
    [SerializeField] private int levelLoadedID;

    [Header("Level Load Information")]
#pragma warning disable CS0414
    [SerializeField] private bool isLevelCurrentlyLoading = false;
#pragma warning restore CS0414
    [SerializeField] private int targetLevelIndex;
    [SerializeField] private float loadingProgress;
    [SerializeField] private string loadingProgressDisplay;

    [Header("Transition Map Properties")]
    [SerializeField] private Text percentageTxt;
    [SerializeField] private Slider percentageSlider;
    [SerializeField] private bool hasFoundTransitionProperties;

    //EVENTS
    //----------------------------------------------------------
    public UnityEvent OnTransitionMapLoaded;

    //METHODS
    //----------------------------------------------------------
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(instance.transform.parent.gameObject);

            //Bind the sceneLoaded method to OnNewLevelLoaded method
            SceneManager.sceneLoaded += OnNewLevelLoaded;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Call this method to begin transitioning to the next level
    /// </summary>
    /// <param name="levelIndex"></param>
    public void OnLoadNewLevel(int levelIndex)
    {
        //Inform the class (or anyone who wants to know) that the a new level is loading
        instance.isLevelCurrentlyLoading = true;

        //Store the target level index which comes from the initial call
        instance.targetLevelIndex = levelIndex;

        //Send the user to the transition map (specified in the custom settings)
        SceneManager.LoadScene(transitionMapLevelIndex);
    }

    /// <summary>
    /// Internal method used to handle the different logic for loading the transition map or the desired map
    /// </summary>
    /// <param name="loadedScene"></param>
    /// <param name="loadedSceneMode"></param>
    private void OnNewLevelLoaded(Scene loadedScene, LoadSceneMode loadedSceneMode)
    {
        //Update internal properties relating the current level information
        UpdateLevelInformation();

        if (levelLoadedID == transitionMapLevelIndex)
        {
            OnTransitionMapLoaded.Invoke();

            percentageTxt = GameObject.Find("PercentageTxt").GetComponent<Text>();
            percentageSlider = GameObject.Find("ProgressBar").GetComponent<Slider>();

            hasFoundTransitionProperties = (percentageTxt != null && percentageSlider != null);

            //Begin the process to load our desired level underneath our transition
            StartCoroutine(LoadAsyncOperation());
        }
        else
        {
            //User has now entered their final destination
            isLevelCurrentlyLoading = false;
        }
    }


    /// <summary>
    /// Coroutine for Async loading
    /// </summary>
    /// <returns></returns>
    private IEnumerator LoadAsyncOperation()
    {
        //Before starting any Async calculation, wait until the current frame has ended
        yield return new WaitForEndOfFrame();

        //Create an Async operation to load the desired scene in the background
        AsyncOperation newLevel = SceneManager.LoadSceneAsync(instance.targetLevelIndex, LoadSceneMode.Single);


        while (!newLevel.isDone && hasFoundTransitionProperties)
        {
            //Divide the progress returned from the Async system by 0.9f and clamp between 0 and 1
            instance.loadingProgress = Mathf.Clamp01(newLevel.progress / 0.9f);

            //Convert the return value of loadingProgress to a whole number (int) and concatinate the a '%' sign at the end 
            instance.loadingProgressDisplay = (int)(instance.loadingProgress * 100.0f) + "%";

            //Apply the display value to the transition map text
            instance.percentageTxt.text = instance.loadingProgressDisplay;

            //Apply the progress value to the transition maps slider
            instance.percentageSlider.value = instance.loadingProgress;

            yield return null;
        }

    }

    /// <summary>
    /// Support method to update public properties after level load.
    /// </summary>
    private void UpdateLevelInformation()
    {
        Scene activeScene = SceneManager.GetActiveScene();

        //Update our internal properties relating to the current level which has loaded
        levelLoadedName = activeScene.name;
        levelLoadedID = activeScene.buildIndex;
    }
}

