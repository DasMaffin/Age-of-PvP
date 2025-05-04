using NUnit.Framework;
using Steamworks;
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    private static GameManager _instance;

    public static GameManager Instance
    {
        get
        {
            if(_instance == null)
            {
                Debug.LogError("GameManager Instance is required but not set!");
            }
            return _instance;
        }
        set
        {
            if(_instance != null && _instance != value)
            {
                Destroy(value.gameObject);
                return;
            }
            _instance = value;
            DontDestroyOnLoad(value.gameObject);
        }
    }

    public event Action OnStartGameButtonPressed;
    public event Action OnLeaveGameButtonPressed;
    public event Action OnBackFromStartGameButtonPressed;

    public GameObject playerCardPrefab;

    [HideInInspector] public List<SOUnit> allUnits = new List<SOUnit>();
    [HideInInspector] public Transform spawnPoint;
    [HideInInspector] public Transform enemySpawnPoint;
    
    private void Awake()
    {
        Instance = this;

        allUnits = Resources.LoadAll<SOUnit>("ScriptableObjects/Units").ToList();
    }

    private void Start()
    {
        SceneManager.activeSceneChanged += SceneManager_activeSceneChanged;
    }

    private void SceneManager_activeSceneChanged(Scene arg0, Scene arg1)
    {
        if(arg1.name == "1v1")
        {
            spawnPoint = GameObject.FindGameObjectWithTag("Spawnpoint").transform;
            enemySpawnPoint = GameObject.FindGameObjectWithTag("Enemyspawnpoint").transform;
        }
    }


    public void StartGame()
    {
        OnStartGameButtonPressed?.Invoke();
    }

    public void BackFromStartGame()
    {
        InvokeOnBackFromStartGameButtonPressed();
    }

    public void LeaveGame()
    {
        OnLeaveGameButtonPressed?.Invoke();
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    public void InvokeOnBackFromStartGameButtonPressed()
    {
        OnBackFromStartGameButtonPressed?.Invoke();
    }
}