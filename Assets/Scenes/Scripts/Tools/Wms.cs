using System.Collections;
using System.Text;
using System.Collections.Generic;
using System.Globalization;


using UnityEngine;

using GeoAPI.Geometries;
using SharpMap;
using SharpMap.Web.Wms;


static class Wms
{
    public static string GetRequestUrl(Client wmsClient, Envelope box, Vector2Int size, string srs, string layer, string format)
    {
        Client.WmsOnlineResource resource = wmsClient.GetMapRequests[0];

        //We prefer posting. Seek for supported post method
        for (int i = 0; i < wmsClient.GetMapRequests.Length; i++)
            if (wmsClient.GetMapRequests[i].Type.ToLower() == "get")
                resource = wmsClient.GetMapRequests[i];

        if (resource.Type.ToLower() != "get")
        {
            throw new System.Exception("Could not get get request");
        }


        System.Text.StringBuilder strReq = new StringBuilder(resource.OnlineResource);
        if (!resource.OnlineResource.Contains("?"))
            strReq.Append("?");
        if (!strReq.ToString().EndsWith("&") && !strReq.ToString().EndsWith("?"))
            strReq.Append("&");

        CultureInfo fmt = new CultureInfo("en-US");

        strReq.AppendFormat(fmt, "REQUEST=GetMap&BBOX={0},{1},{2},{3}",
            box.MinX, box.MinY, box.MaxX, box.MaxY);
        strReq.AppendFormat("&WIDTH={0}&Height={1}", size[0], size[1]);
        strReq.AppendFormat("&Layers={0}", layer);
        strReq.AppendFormat("&FORMAT={0}", format);
        strReq.Append("&STYLES=");
        if (srs == string.Empty)
            throw new System.Exception("Spatial reference system not set");
        if (wmsClient.Version == "1.3.0")
            strReq.AppendFormat("&CRS={0}", srs);
        else
            strReq.AppendFormat("&SRS={0}", srs);
        strReq.AppendFormat("&VERSION={0}", wmsClient.Version);
        return strReq.ToString();
    }
}
