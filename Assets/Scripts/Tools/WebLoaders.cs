using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public static class WebLoaders
{
    public static IEnumerator<UnityWebRequestAsyncOperation> Load2DBufferFromWeb(string requestUrl, Vector2Int size, Action<float[,]> callback)
    {
        Debug.LogFormat("Loading data from: {0}", requestUrl);
        using (UnityWebRequest www = new UnityWebRequest(requestUrl))
        {
            DownloadHandlerBuffer bufferDl = new DownloadHandlerBuffer();
            www.downloadHandler = bufferDl;
            yield return www.SendWebRequest();
            Debug.Log(www.result);

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError(www.error);
            }
            else
            {
                Debug.Log("OK");
                var heights = new float[size[0], size[1]];
                Debug.LogFormat("Size = {0}", bufferDl.data.Length);
                Buffer.BlockCopy(bufferDl.data, 0, heights, 0, bufferDl.data.Length);
                callback(heights);
            }
        }
    }

    public static IEnumerator<UnityWebRequestAsyncOperation> LoadTextureFromWeb(string requestUrl, Action<Texture2D> callback)
    {
        Debug.LogFormat("Loading data from: {0}", requestUrl);
        using (UnityWebRequest www = new UnityWebRequest(requestUrl))
        {
            www.downloadHandler = new DownloadHandlerTexture();
            yield return www.SendWebRequest();
            Debug.Log(www.result);

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError(www.error);
            }
            else
            {
                Debug.Log("OK");
                Texture2D myTexture = ((DownloadHandlerTexture)www.downloadHandler).texture;
                Debug.Log(myTexture.dimension);
                callback(myTexture);
            }
        }
    }

}
