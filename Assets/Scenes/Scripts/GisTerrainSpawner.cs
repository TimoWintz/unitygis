
using UnityEngine;
using UnityEngine.Networking;

using SharpMap;
using SharpMap.Web.Wms;
using GeoAPI;
using GeoAPI.Geometries;
using System.Drawing;
using NetTopologySuite;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;


public class GisTerrainSpawner : MonoBehaviour
{

    [HideInInspector]
    public GameObject terrain;

    [HideInInspector]
    public TerrainData terrainData;

    public int terrainHeight = 8849; // max height
    public int terrainWidth = 2000;
    public int terrainLength = 2000;
    public int heightmapResolution = 2049;
    public int baseMapResolution = 1024;
    public int detailResolution = 1024;
    public int detailResolutionPerPatch = 32;
    public int alphaMapResolution = 512;

    public Envelope box = new Envelope(912500, 914500, 6463500, 6465500);
    public Vector2Int Size;


    public string Srs = "IGNF:LAMB93";





    // Start is called before the first frame update
    void Start()
    {
        NetTopologySuiteBootstrapper.Bootstrap();
        CreateTerrain();
    }
    // Update is called once per frame
    void Update()
    {

    }

    void CreateTerrain()
    {
        terrainData = new TerrainData();

        terrainData.baseMapResolution = baseMapResolution;
        terrainData.heightmapResolution = heightmapResolution;
        terrainData.alphamapResolution = alphaMapResolution;
        terrainData.SetDetailResolution(detailResolution, detailResolutionPerPatch);

        terrainData.size = new Vector3(terrainWidth, terrainHeight, terrainLength);

        terrainData.name = name;
        terrain = (GameObject)UnityEngine.Terrain.CreateTerrainGameObject(terrainData);

        terrain.transform.SetParent(gameObject.transform);
    }






}