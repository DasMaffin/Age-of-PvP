using Steamworks;
using UnityEngine;
using UnityEngine.EventSystems;

public class BuyUnit : MonoBehaviour, IPointerDownHandler
{
    public SOUnit unit;

    public void OnPointerDown(PointerEventData eventData)
    {
        byte[] data;
        bool ready = !LobbyManager.Instance.SelfReady;
        UnitData uData = new UnitData { UnitID = 0 };
        NetworkData packet = NetworkData.Create(DataType.UnitData, uData);
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


        //UnitController u = Instantiate(unit.unitPrefab, GameManager.Instance.spawnPoint.position, Quaternion.identity).GetComponent<UnitController>();
        //u.unit = unit.unit;
        //u.gameObject.tag = "MyUnit";
    }
}
