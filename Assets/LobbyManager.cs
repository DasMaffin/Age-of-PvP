using Steamworks;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.U2D;
using UnityEngine.UI;

public class LobbyManager : MonoBehaviour
{
    private static LobbyManager _instance;

    public static LobbyManager Instance
    {
        get
        {
            if(_instance == null)
            {
                Debug.LogError("LobbyManager Instance is required but not set!");
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
            DontDestroyOnLoad(value.gameObject);
        }
    }

    public CSteamID LobbyID { get; private set; }
    public CSteamID hostID;
    public CSteamID enemyID;
    public event Action OnCreateNewLobby;

    public PlayerCardController myPlayerCard;
    public PlayerCardController opponentPlayerCard;

    private bool selfReady = false;
    [HideInInspector]
    public bool SelfReady
    {
        get => selfReady;
        set
        {
            selfReady = value;
            LobbyManager.Instance.myPlayerCard.readyStatus.text = value ? "Ready" : "Not Ready";
            SceneManager.LoadScene("1v1");
            if(selfReady && enemyReady) SceneManager.LoadScene("1v1");
        }
    }
    private bool enemyReady = false;
    [HideInInspector]
    public bool EnemyReady
    {
        get => enemyReady;
        set
        {
            enemyReady = value;
            LobbyManager.Instance.opponentPlayerCard.readyStatus.text = value ? "Ready" : "Not Ready";
            if(selfReady && enemyReady) SceneManager.LoadScene("1v1");
        }
    }


    private void Awake()
    {
        Instance = this;
    }

    public void newFriendsOnlyLobby()
    {
        SteamMatchmaking.CreateLobby(ELobbyType.k_ELobbyTypeFriendsOnly, 2);
    }

    public void LeaveLobby()
    {
        Destroy(myPlayerCard.gameObject);
        Destroy(opponentPlayerCard.gameObject);
        GameManager.Instance.InvokeOnBackFromStartGameButtonPressed();

        SteamMatchmaking.LeaveLobby(LobbyID);
        LobbyID = CSteamID.Nil;
    }

    public void ChangeReady()
    {
        byte[] data;
        bool ready = !LobbyManager.Instance.SelfReady;
        ReadyStateData rsData = new ReadyStateData { IsReady = ready };
        NetworkData packet = NetworkData.Create(DataType.ReadyState, rsData);
        data = packet.ToByteArray();

        for(int i = 0; i < SteamMatchmaking.GetNumLobbyMembers(LobbyManager.Instance.LobbyID); i++)
        {
            CSteamID memberID = SteamMatchmaking.GetLobbyMemberByIndex(LobbyManager.Instance.LobbyID, i);

            // Don't send to yourself
            //if(memberID == SteamUser.GetSteamID())
            //    continue;

            SteamNetworking.SendP2PPacket(
                memberID,
                data,
                (uint)data.Length,
                EP2PSend.k_EP2PSendUnreliable
            );
        }
    }

    #region Steam Callbacks

    private Callback<LobbyCreated_t> lobbyCreated;
    private Callback<LobbyEnter_t> lobbyEntered;
    private Callback<GameLobbyJoinRequested_t> gameLobbyJoinRequested;
    private Callback<LobbyChatUpdate_t> lobbyChatUpdate;

    private void Start()
    {
        lobbyCreated = Callback<LobbyCreated_t>.Create(OnLobbyCreated);
        lobbyEntered = Callback<LobbyEnter_t>.Create(OnLobbyEntered);
        gameLobbyJoinRequested = Callback<GameLobbyJoinRequested_t>.Create(OnGameLobbyJoinRequested);
        lobbyChatUpdate = Callback<LobbyChatUpdate_t>.Create(OnLobbyChatUpdate);
    }

    private void OnLobbyCreated(LobbyCreated_t callback)
    {
        if(callback.m_eResult == EResult.k_EResultOK)
        {
            LobbyID = new CSteamID(callback.m_ulSteamIDLobby);
            SteamMatchmaking.SetLobbyData(LobbyID, "host", SteamUser.GetSteamID().ToString());
        }
    }

    void OnGameLobbyJoinRequested(GameLobbyJoinRequested_t callback)
    {
        if(LobbyID != CSteamID.Nil)
        {
            LeaveLobby();
        }
        Debug.Log("Received join request for lobby: " + callback.m_steamIDLobby);
        SteamMatchmaking.JoinLobby(callback.m_steamIDLobby);
    }

    void OnLobbyEntered(LobbyEnter_t callback)
    {
        OnCreateNewLobby?.Invoke();
        LobbyID = new CSteamID(callback.m_ulSteamIDLobby);

        // Find the other player
        myPlayerCard = UIManager.Instance.InstantiatePlayerCard(true);
        myPlayerCard.playerName.text = SteamFriends.GetPersonaName();

        SteamHelper.GetSteamAvatarSprite(SteamUser.GetSteamID(), (sprite) =>
        {
            if(sprite != null)
                myPlayerCard.playerImage.sprite = sprite;
        });

        CSteamID opponent = CSteamID.Nil;
        int memberCount = SteamMatchmaking.GetNumLobbyMembers(LobbyID);
        for(int i = 0; i < memberCount; i++)
        {
            CSteamID member = SteamMatchmaking.GetLobbyMemberByIndex(LobbyID, i);
            if(member != SteamUser.GetSteamID())
            {
                opponent = member;
                break;
            }
        }
        opponentPlayerCard = UIManager.Instance.InstantiatePlayerCard(false);
        if(opponent == CSteamID.Nil)
        {
            return;
        }
        opponentPlayerCard.playerName.text = SteamFriends.GetFriendPersonaName(opponent);

        SteamHelper.GetSteamAvatarSprite(opponent, (sprite) =>
        {
            if(sprite != null)
                opponentPlayerCard.playerImage.sprite = sprite;
        });        
    }

    void OnLobbyChatUpdate(LobbyChatUpdate_t callback)
    {
        CSteamID lobbyId = new CSteamID(callback.m_ulSteamIDLobby);
        CSteamID changedUser = new CSteamID(callback.m_ulSteamIDUserChanged);

        EChatMemberStateChange stateChange = (EChatMemberStateChange)callback.m_rgfChatMemberStateChange;

        if(stateChange.HasFlag(EChatMemberStateChange.k_EChatMemberStateChangeEntered))
        {
            Debug.Log("Player joined lobby: " + changedUser);

            opponentPlayerCard.playerName.text = SteamFriends.GetFriendPersonaName(changedUser);

            SteamHelper.GetSteamAvatarSprite(changedUser, (sprite) =>
            {
                if(sprite != null)
                    opponentPlayerCard.playerImage.sprite = sprite;
            });
            // You can now begin sending them messages, etc.
        }

        if(stateChange.HasFlag(EChatMemberStateChange.k_EChatMemberStateChangeLeft) ||
            stateChange.HasFlag(EChatMemberStateChange.k_EChatMemberStateChangeDisconnected))
        {
            SteamMatchmaking.LeaveLobby(LobbyID);
            Debug.Log("Player left lobby: " + changedUser);
            if(SteamHelper.IsHost())
            {
                opponentPlayerCard.playerName.text = "Waiting on friend...";
                opponentPlayerCard.playerImage.sprite = null;
            }
            else
            {
                LeaveLobby();
            }
        }
    }

    #endregion
}
