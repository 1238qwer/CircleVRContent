using UnityEngine;
using UnityEngine.Networking;
using System.Collections.Generic;
using Valve.VR;
using System.Text;
using System.Linq;
using System;

public interface ICircleVRHostEventHander
{
    void OnInit(Configuration conifg);
    void OnAddConnectedUserID(string userID);
    void OnRemoveConnectedUserID(string userID);
}

public class CircleVRHost : MonoBehaviour
{
    public static readonly Vector3 HOST_CAMERA_ANCHOR_EULER = new Vector3(90, 0, 0);
    public static List<ICircleVRHostEventHander> events = new List<ICircleVRHostEventHander>();

    private List<string> connectedUserIDs = new List<string>();

    private Configuration config;

    public void Init(Configuration config)
    {
        this.config = config;

        CircleVRDisplay.Camera.SetDisplay(connectedUserIDs);

        foreach (var Event in events)
        {
            Event.OnInit(config);
        }

        Debug.Log("[CircleVR Host] Initialized");

        SetupContentServer(config);
    }

    private void SetupContentServer(Configuration config)
    {
        CircleVR.DontDestroyInstantiate("ContentServer", Vector3.zero, Quaternion.identity).AddComponent<CircleVRContentServer>().Init(config);
    }

    public bool AddConnectedUserIDs(string userID, out CircleVRErrorType error)
    {
        if(connectedUserIDs.Contains(userID))
        {
            Debug.Log("[CircleVR Host] Already Host Has User ID : " + userID.ToString());
            error = CircleVRErrorType.AlreadyHasUserID;
            return false;
        }

        if(CircleVR.GetPair(userID) == null)
        {
            Debug.LogError("[CircleVR Host] Not Found User ID In Pairs : " + userID);
            error = CircleVRErrorType.NotFoundUserIDInPairs;
            return false;
        }

        connectedUserIDs.Add(userID);

        CircleVRDisplay.Camera.SetDisplay(connectedUserIDs);

        foreach (var Event in events)
        {
            Event.OnAddConnectedUserID(userID);
        }

        Debug.Log("[CircleVR Host] Add UserID Succeed! : " + userID.ToString());

        error = 0;
        return true;
    }

    public void RemoveConnectedUserID(string userID)
    {
        if (!connectedUserIDs.Contains(userID))
        {
            Debug.LogError("[CircleVRHost] Not Found Connected User ID : " + userID);
            return;
        }

        Debug.Log("[CircleVRHost] UserID [ " + userID + " ] Disconnected!");

        connectedUserIDs.Remove(userID);
        CircleVRDisplay.Camera.SetDisplay(connectedUserIDs);

        foreach (var Event in events)
        {
            Event.OnRemoveConnectedUserID(userID);
        }
    }
}