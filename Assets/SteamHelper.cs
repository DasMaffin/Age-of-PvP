using Steamworks;
using System;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;

public static class SteamHelper
{
    public static void GetSteamAvatarSprite(CSteamID steamID, Action<Sprite> callback)
    {
        _ = FetchAvatarAsync(steamID, callback);
    }

    public static bool IsHost()
    {
        return IsHost(SteamUser.GetSteamID());
    }

    public static bool IsHost(CSteamID steamID)
    {
        if(LobbyManager.Instance.LobbyID == CSteamID.Nil)
            return false;
        string hostID = SteamMatchmaking.GetLobbyData(LobbyManager.Instance.LobbyID, "host");
        if(string.IsNullOrEmpty(hostID))
            return false;
        CSteamID hostCSteamID = new CSteamID(Convert.ToUInt64(hostID));
        return hostCSteamID == steamID;
    }

    private static async Task FetchAvatarAsync(CSteamID steamID, Action<Sprite> callback)
    {
        int imageID = SteamFriends.GetLargeFriendAvatar(steamID);

        int retries = 10;
        while(imageID == -1 && retries-- > 0)
        {
            await Task.Delay(100);
            imageID = SteamFriends.GetLargeFriendAvatar(steamID);
        }

        if(imageID == -1)
        {
            Debug.LogWarning("Avatar not available.");
            callback?.Invoke(null);
            return;
        }

        if(!SteamUtils.GetImageSize(imageID, out uint width, out uint height) || width == 0 || height == 0)
        {
            Debug.LogWarning("Invalid avatar size.");
            callback?.Invoke(null);
            return;
        }

        byte[] imageData = new byte[width * height * 4];
        if(!SteamUtils.GetImageRGBA(imageID, imageData, imageData.Length))
        {
            Debug.LogWarning("Failed to load avatar RGBA.");
            callback?.Invoke(null);
            return;
        }

        // Continue on the Unity main thread
        await Task.Yield();

        var tex = new Texture2D((int)width, (int)height, TextureFormat.RGBA32, false);
        tex.LoadRawTextureData(imageData);
        tex.Apply();

        Texture2D flipped = FlipTextureVertically(tex);

        Sprite avatarSprite = Sprite.Create(
            flipped,
            new Rect(0, 0, flipped.width, flipped.height),
            new Vector2(0.5f, 0.5f),
            100f
        );

        callback?.Invoke(avatarSprite);
    }

    private static Texture2D FlipTextureVertically(Texture2D original)
    {
        int width = original.width;
        int height = original.height;
        Texture2D flipped = new Texture2D(width, height, original.format, false);

        for(int y = 0; y < height; y++)
        {
            flipped.SetPixels(0, y, width, 1, original.GetPixels(0, height - y - 1, width, 1));
        }

        flipped.Apply();
        return flipped;
    }
}
