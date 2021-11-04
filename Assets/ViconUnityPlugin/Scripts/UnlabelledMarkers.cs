using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

using ViconDataStreamSDK.CSharp;


namespace UnityVicon
{
    /* this instantiates and tracks unlabelled markers using the vicon datastream. Good for single markers on fingertips etc.
     * Create a prefab in your assets - I like a brightly coloured sphere scaled at 0.005f. Unlabelled markers have no rotations, so spheres are good.
     * ViconDataStreamClient unity prefab is missing the UnlabeledMarkerCount as shown in the vicon datastream sdk manual - so add to it:
     * 
       public Output_GetUnlabeledMarkerCount GetUnlabeledMarkerCount()
    {
        return m_Client.GetUnlabeledMarkerCount();
    }

    You will also need to add a public bool enableUnlabeledMarkers and 
      if (enableUnlabeledMarkers)
        {
            m_Client.EnableUnlabeledMarkerData();
        }
    in the ConnectClient() method around line 257 to force it, I did before the SetAxisMapping call, needs to be after the if( UseLightweightData ) stuff, or lightweightData will turn it off.
     */
    public class UnlabelledMarkers : MonoBehaviour
    {
       
        public ViconDataStreamClient Client;
        public GameObject unlabeledMarkerPrefab;
        public uint maximumMarkers = 50;
        private uint MarkerCount;
        List<GameObject> MarkerPrefabs = new List<GameObject>();

     
        public UnlabelledMarkers()
        {
        }
        void DestroyMissingMarkers()
        {
            if (MarkerPrefabs.Count > MarkerCount)
            {
                for (int i = (int)MarkerCount; i < MarkerPrefabs.Count; i++)
                {
                    Destroy(MarkerPrefabs[i]);
                    MarkerPrefabs.Remove(MarkerPrefabs[i]);
                }
            }
        }
        void InstantiateNewMarker(Vector3 pos)
        {
            GameObject marker = Instantiate(unlabeledMarkerPrefab, pos, Quaternion.identity, transform);
           
            MarkerPrefabs.Add(marker);
        }
        void Update()
        {
            // MarkerCount = Client.UnlabeledMarkerCount();
            Output_GetUnlabeledMarkerCount GetMarkerCount= Client.GetUnlabeledMarkerCount();
            DestroyMissingMarkers();
               
            if (GetMarkerCount.MarkerCount>0)  // Test for success with GetMarkerCount.Result == Result.Success but don't think needed
            {
                MarkerCount = GetMarkerCount.MarkerCount;
               
                for (int i = 0; i < (int)MarkerCount&& i< maximumMarkers; i++)
                {
                    Output_GetUnlabeledMarkerGlobalTranslation Output = Client.GetUnlabeledMarkerGlobalTranslation((uint)i);
                    Vector3 pos = new Vector3(-(float)Output.Translation[1] * 0.001f, (float)Output.Translation[2] * 0.001f, (float)Output.Translation[0] * 0.001f);
                    if (MarkerPrefabs.Count<= i)
                    {
                        InstantiateNewMarker(pos);
                    }
                    else
                    {
                        MarkerPrefabs[i].transform.position = pos;
                    }

                }
            }
             
        }
    }
}