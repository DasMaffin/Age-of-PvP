using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    private static UIManager _instance;

    public static UIManager Instance
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
            if(_instance != null)
            {
                Destroy(value.gameObject);
                return;
            }
            _instance = value;
        }
    }

    [SerializeField] private GameObject mainMenuButtons;
    [SerializeField] private GameObject startMenuButtons;
    [SerializeField] private GameObject lobbyMenuButtons;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        GameManager.Instance.OnStartGameButtonPressed += OnStartGameButtonPressedHandleUI;
        GameManager.Instance.OnBackFromStartGameButtonPressed += OnBackFromStartGameButtonPressedHandleUI;
        GameManager.Instance.OnLeaveGameButtonPressed += OnLeaveGameButtonPressedHandleUI;
        LobbyManager.Instance.OnCreateNewLobby += OnCreateNewLobbyHandleUI;

        DisableUIs();
        mainMenuButtons.SetActive(true);
    }

    public PlayerCardController InstantiatePlayerCard(bool isLeft = true)
    {
        GameObject instance = Instantiate(GameManager.Instance.playerCardPrefab, UIManager.Instance.transform);
        RectTransform rectTransform = instance.GetComponent<RectTransform>();

        if(isLeft)
        {
            rectTransform.anchorMin = new Vector2(0f, 0.5f);
            rectTransform.anchorMax = new Vector2(0f, 0.5f);
            rectTransform.pivot = new Vector2(0f, 0.5f);
            rectTransform.anchoredPosition = new Vector2(20f, 0f); // Optional padding
        }
        else
        {
            rectTransform.anchorMin = new Vector2(1f, 0.5f);
            rectTransform.anchorMax = new Vector2(1f, 0.5f);
            rectTransform.pivot = new Vector2(1f, 0.5f);
            rectTransform.anchoredPosition = new Vector2(-20f, 0f); // Optional padding from right edge
        }

        rectTransform.localScale = Vector3.one;

        PlayerCardController playerCard = instance.GetComponent<PlayerCardController>();

        return playerCard;
    }

    private void DisableUIs()
    {
        mainMenuButtons.SetActive(false);
        startMenuButtons.SetActive(false);
        lobbyMenuButtons.SetActive(false);
    }

    private void OnCreateNewLobbyHandleUI()
    {
        DisableUIs();
        lobbyMenuButtons.SetActive(true);
    }

    private void OnBackFromStartGameButtonPressedHandleUI()
    {
        DisableUIs();
        mainMenuButtons.SetActive(true);
    }

    private void OnStartGameButtonPressedHandleUI()
    {
        DisableUIs();
        startMenuButtons.SetActive(true);
    }

    private void OnLeaveGameButtonPressedHandleUI()
    {
        DisableUIs();
        startMenuButtons.SetActive(true);
        LobbyManager.Instance.LeaveLobby();
    }
}
