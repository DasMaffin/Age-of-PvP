using Steamworks;
using System;
using Unity.VisualScripting;
using UnityEngine;

public class InviteFriendsManager : MonoBehaviour
{
    protected Callback<LobbyEnter_t> lobbyEntered;
    protected Callback<LobbyChatUpdate_t> lobbyChatUpdate;

    void Start()
    {
        lobbyEntered = Callback<LobbyEnter_t>.Create(OnLobbyEntered);
        lobbyChatUpdate = Callback<LobbyChatUpdate_t>.Create(OnLobbyChatUpdate);
    }

    void OnLobbyEntered(LobbyEnter_t callback)
    {
        CSteamID lobbyID = new CSteamID(callback.m_ulSteamIDLobby);
        Debug.Log("Joined lobby: " + lobbyID);

        // Find the other player
        int memberCount = SteamMatchmaking.GetNumLobbyMembers(lobbyID);
        for(int i = 0; i < memberCount; i++)
        {
            CSteamID member = SteamMatchmaking.GetLobbyMemberByIndex(lobbyID, i);
            if(member != SteamUser.GetSteamID())
            {
                Debug.Log("Found other player: " + member);
                // Save this ID and begin sending/receiving messages
            }
        }
    }

    void OnLobbyChatUpdate(LobbyChatUpdate_t callback)
    {
        CSteamID lobbyId = new CSteamID(callback.m_ulSteamIDLobby);
        CSteamID changedUser = new CSteamID(callback.m_ulSteamIDUserChanged);

        EChatMemberStateChange stateChange = (EChatMemberStateChange)callback.m_rgfChatMemberStateChange;

        if(stateChange.HasFlag(EChatMemberStateChange.k_EChatMemberStateChangeEntered))
        {
            Debug.Log("Player joined lobby: " + changedUser);
            // You can now begin sending them messages, etc.
        }

        if(stateChange.HasFlag(EChatMemberStateChange.k_EChatMemberStateChangeLeft) ||
            stateChange.HasFlag(EChatMemberStateChange.k_EChatMemberStateChangeDisconnected))
        {
            Debug.Log("Player left lobby: " + changedUser);
        }
    }
}
