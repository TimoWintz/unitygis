using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;

using GeoAPI.Geometries;
using SharpMap;
using SharpMap.Web.Wms;
using GeoAPI;

[RequireComponent(typeof(GisTerrainSpawner))]
public class HeightMapDownloader : MonoBehaviour
{
    public string HeightmapWmsUrl = "https://wxs.ign.fr/altimetrie/geoportail/r/wms";
    public string HeightmapWmsVersion = "1.3.0";
    public string HeightmapLayer = "ELEVATION.ELEVATIONGRIDCOVERAGE.HIGHRES";
    public string HeightmapFormat = "image/x-bil;bits=32";
    public int HeightMapBlurLevel = 2;

    IEnumerator waitForTerrain(Action callback)
    {
        Debug.Log("Waiting for terrain...");
        yield return new WaitUntil(() => GetComponent<GisTerrainSpawner>().terrain != null);
        yield return new WaitUntil(() => GetComponent<GisTerrainSpawner>().terrainData != null);
        Debug.Log("OK");
        callback();
    }

    // Start is called before the first frame update
    void Start()
    {
        NetTopologySuiteBootstrapper.Bootstrap();
        StartCoroutine(waitForTerrain(LoadHeightMap));
    }

    // Update is called once per frame
    void Update()
    {

    }

    void LoadHeightMap()
    {
        var HeightmapRequestUrl = GetHeightmapRequestUrl();
        Action<float[,]> callback = delegate (float[,] v)
        {
            var terrainHeight = GetComponent<GisTerrainSpawner>().terrainHeight;
            ArrayTools.Clip(v, 0.0f);
            ArrayTools.MulAdd(v, 1.0f / terrainHeight, 0.0f);
            if (HeightMapBlurLevel > 0)
            {
                var gb = new GaussianBlur(v);
                v = gb.Process(HeightMapBlurLevel);
            }

            var enumerable = from float item in v select item;
            Debug.LogFormat("max = {0}, min = {1}", enumerable.Max(), enumerable.Min());
            GetComponent<GisTerrainSpawner>().terrainData.SetHeights(0, 0, v);
        };
        StartCoroutine(WebLoaders.Load2DBufferFromWeb(HeightmapRequestUrl, HeightMapSize(), callback));
    }

    private string GetHeightmapRequestUrl()
    {
        Client wmsClient = new Client(HeightmapWmsUrl);
        wmsClient.Version = HeightmapWmsVersion;
        Debug.Log(wmsClient.Layer.ChildLayers.Length);
        var c = GetComponent<GisTerrainSpawner>();
        return Wms.GetRequestUrl(wmsClient, c.box, HeightMapSize(), c.Srs, HeightmapLayer, HeightmapFormat);
    }

    private int HeightMapResolution()
    {
        return GetComponent<GisTerrainSpawner>().heightmapResolution;
    }

    private Vector2Int HeightMapSize()
    {
        return new Vector2Int(HeightMapResolution(), HeightMapResolution());
    }
}
