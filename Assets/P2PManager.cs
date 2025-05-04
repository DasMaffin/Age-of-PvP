using UnityEngine;
using Steamworks;
using System;

public class P2PManager : MonoBehaviour
{
    // Update is called once per frame
    void Update()
    {
        while(SteamNetworking.IsP2PPacketAvailable(out uint size))
        {
            byte[] buffer = new byte[size];

            if(SteamNetworking.ReadP2PPacket(buffer, size, out uint bytesRead, out CSteamID sender))
            {
                using(var stream = new System.IO.MemoryStream(buffer))
                using(var reader = new System.IO.BinaryReader(stream))
                {
                    DataType type = (DataType)reader.ReadByte();
                    switch(type)
                    {
                        case DataType.ReadyState:
                            bool ready = reader.ReadBoolean();
                            if(sender == SteamUser.GetSteamID())
                            {
                                LobbyManager.Instance.SelfReady = ready;
                            }
                            else
                            {
                                LobbyManager.Instance.EnemyReady = ready;
                            }
                            break;
                        case DataType.UnitData:
                            int unitID = reader.ReadInt32();
                            print("Unit ID: " + unitID);
                            throw new NotImplementedException("Unit data not implemented yet");
                            break;
                        default:
                            Debug.LogError("Unknown data type received: " + type);
                            break;
                    }
                }
            }
        }
    }
}

[Serializable]
public enum DataType
{
    ReadyState,
    UnitData
}

public interface INetworkPayload { }

[Serializable]
public struct ReadyStateData : INetworkPayload
{
    public bool IsReady;
}

[Serializable]
public struct UnitData : INetworkPayload
{
    public int UnitID;
}

[Serializable]
public struct NetworkData
{
    public DataType Type;
    public byte[] Data;

    public static NetworkData Create<T>(DataType type, T data) where T : INetworkPayload
    {
        return new NetworkData
        {
            Type = type,
            Data = Serialize(data)
        };
    }

    private static byte[] Serialize<T>(T data)
    {
        using(var ms = new System.IO.MemoryStream())
        {
            var formatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
            formatter.Serialize(ms, data);
            return ms.ToArray();
        }
    }

    public T GetData<T>() where T : INetworkPayload
    {
        using(var ms = new System.IO.MemoryStream(Data))
        {
            var formatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
            return (T)formatter.Deserialize(ms);
        }
    }

    internal byte[] ToByteArray()
    {
        using(var stream = new System.IO.MemoryStream())
        using(var writer = new System.IO.BinaryWriter(stream))
        {
            writer.Write((int)Type);
            writer.Write(Data);

            return stream.ToArray();
        }
    }
}