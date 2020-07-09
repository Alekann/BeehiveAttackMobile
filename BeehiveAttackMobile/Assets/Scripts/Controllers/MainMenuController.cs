using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenuController : MonoBehaviour
{
    //PROPERTIES
    //------------------------------------
    [Header("Custom Settings")]
    public int firstLevelIndex = 0;


    //METHODS
    //------------------------------------
    public void StartGame()
    {
        LevelManager.instance.OnLoadNewLevel(firstLevelIndex);
    }

    public void ExitGame()
    {
        Application.Quit();
    }
}
