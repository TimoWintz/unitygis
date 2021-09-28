using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SharpMap.Web.Wms;
using GeoAPI;

[RequireComponent(typeof(GisTerrainSpawner))]
public class BaseMapDownloader : MonoBehaviour
{
    public string BaseMapWmsUrl = "https://wxs.ign.fr/ortho/geoportail/r/wms";
    public string BaseMapWmsVersion = "1.3.0";
    public string BaseMapLayer = "ORTHOIMAGERY.ORTHOPHOTOS.BDORTHO";
    public string BaseMapFormat = "image/jpeg";

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
        StartCoroutine(waitForTerrain(LoadBaseMap));
    }

    // Update is called once per frame
    void Update()
    {

    }

    void LoadBaseMap()
    {
        var BaseMapRequestUrl = GetBaseMapRequestUrl();
        Action<Texture2D> callback = delegate (Texture2D v)
        {
            var terrain = GetComponent<GisTerrainSpawner>();
            var terrainData = GetComponent<GisTerrainSpawner>().terrainData;
            TerrainLayer[] tex = new TerrainLayer[1];
            tex[0] = new TerrainLayer();
            tex[0].diffuseTexture = v;
            tex[0].tileSize = new Vector2(terrain.terrainWidth, -terrain.terrainLength);
            terrainData.terrainLayers = tex;
        };
        StartCoroutine(WebLoaders.LoadTextureFromWeb(BaseMapRequestUrl, callback));
    }



    private Vector2Int BaseMapSize()
    {
        var terrainData = GetComponent<GisTerrainSpawner>().terrainData;
        return new Vector2Int(terrainData.baseMapResolution, terrainData.baseMapResolution);
    }


    private string GetBaseMapRequestUrl()
    {
        var terrain = GetComponent<GisTerrainSpawner>();
        Client wmsClient = new Client(BaseMapWmsUrl);
        wmsClient.Version = BaseMapWmsVersion;
        Debug.Log(wmsClient.Layer.ChildLayers.Length);
        return Wms.GetRequestUrl(wmsClient, terrain.box, BaseMapSize(), terrain.Srs, BaseMapLayer, BaseMapFormat);
    }
}
