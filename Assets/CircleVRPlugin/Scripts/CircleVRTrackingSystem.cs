using UnityEngine;
using System.Collections.Generic;
using Valve.VR;

public class CircleVRTrackingSystem
{
    private static readonly Vector3 ANCHOR_POSITION = new Vector3(0.0f, 0.015f, -0.1f);
    private static readonly Vector3 ANCHOR_EULER = new Vector3(90.0f, 0.0f, 0.0f);

    public static event TrackerEventHandler onCreateTracker;

    public delegate void TrackerEventHandler(string ID, Transform trackerTransform);

    public delegate void TrackerOriginEventHandler(Transform origin);

    private static Dictionary<string, Transform> trackers = new Dictionary<string, Transform>();

    private static Transform origin;

    public static Transform Origin
    {
        set
        {
            origin = value;

            foreach (var trackersValue in trackers.Values)
            {
                trackersValue.transform.SetParent(origin, false);
            }
        }
        get
        {
            return origin;
        }
    }

    public static void Init(Configuration config, Transform trackerOrigin)
    {
        origin = trackerOrigin;

        if(config.isHost || CircleVR.IsEditorPlatform())
        {
            HostSetup(config);
            return;
        }

        ClientSetup();
    }

    private static void ClientSetup()
    {
        CircleVR.DontDestroyInstantiate("ClientTrackingDataFeeder", Vector3.zero, Quaternion.identity).AddComponent<CircleVRClientTrackingDataFeeder>();
        Debug.Log("[TrackingSystem] Client Setup");
    }

    private static void HostSetup(Configuration config)
    {
        Debug.Log("[TrackingSystem] Host Setup");

        if (OpenVR.System != null)
        {
            CircleVR.DontDestroyInstantiate("SteamVRTrackingDataFeeder", Vector3.zero, Quaternion.identity).AddComponent<CircleVRSteamVRTrackingDataFeeder>();
            return;
        }

        CircleVR.DontDestroyInstantiate("TrackingSimulator", Vector3.zero, Quaternion.identity).AddComponent<CircleVRTrackingSimulator>().Init(config);
    }

    public static bool HasTracker(string id)
    {
        return trackers.ContainsKey(id);
    }

    public static Transform GetTracker(string ID)
    {
        Transform tmp;
        if (trackers.TryGetValue(ID, out tmp))
            return tmp;

        //Debug.Log("[CircleVR] Not Found Tracker User ID : " + ID.ToString());
        return null;
    }

    public static Transform CreateTracker(string ID)
    {
        Transform trackerTransform = CircleVR.DontDestroyInstantiate(ID, Vector3.zero, Quaternion.identity).transform;

        trackers.Add(ID, trackerTransform);

        CreateCenterAnchor(trackerTransform);

        Origin = Origin;

        if (onCreateTracker != null)
            onCreateTracker(ID, trackerTransform);


        Debug.Log("[CircleVR Tracking System] Create Tracker : " + ID);
        Debug.Log("[CircleVR Tracking System] Tracker Count : " + trackers.Count.ToString());

        return trackerTransform;
    }

    private static void CreateCenterAnchor(Transform tracker)
    {
        GameObject centerAnchor = new GameObject("CenterAnchor");
        centerAnchor.transform.SetParent(tracker);
        centerAnchor.transform.localPosition = ANCHOR_POSITION;
        centerAnchor.transform.localEulerAngles = ANCHOR_EULER;
    }
}
