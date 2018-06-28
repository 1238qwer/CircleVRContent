using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CircleVRClientEmulator : MonoBehaviour, ICircleVRClientEventHandler
{
    public CircleVRHost host;
    private string userID;

    private void Awake()
    {
        CircleVRClient.Event = this;    
    }

    public void OnBounded(string userID, AirVRStereoCameraRig rig)
    {
        this.userID = userID;

        Transform tracker = CircleVRTrackingSystem.GetTracker(CircleVR.GetPair(userID).trackerID);

        rig.externalTracker = tracker.Find("CenterAnchor");

        CircleVRErrorType error;
        if (host.AddConnectedUserIDs(userID, out error))
        {
            CircleVRDisplay.GetHead(CircleVR.GetPair(userID).trackerID).Connected = true;
            return;
        }

        Debug.Log("[Client Emulator] " + error.ToString());
    }

    public void OnInit(Configuration config)
    {
    }

    public void OnUnbound()
    {
        host.RemoveConnectedUserID(userID);
        CircleVRDisplay.GetHead(CircleVR.GetPair(userID).trackerID).Connected = false;
    }
}
