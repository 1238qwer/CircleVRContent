using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ICircleVRContentClientEventHandler
{
    void OnInit(AirVRStereoCameraRig rig);
    void OnPlay();
    void OnPause();
    void OnStop();
    void OnNext();
    void OnData(int connectionId, int channelId, byte key, byte[] data, byte error);
    void OnContentServerStatusReceived(ContentServerStatus status);
}

public class CircleVRContentClient : MonoBehaviour, ICircleVRNetworkEventHandler
{
    public static ICircleVRContentClientEventHandler Event;

    private void Awake()
    {
        CircleVRNetwork.events.Add(this);
        Debug.Log("[CircleVR Content Client] Instantiated");
    }

    public void Init(AirVRStereoCameraRig rig)
    {
        if (Event != null)
            Event.OnInit(rig);
    }

    public void OnPlay()
    {
        if (Event != null)
            Event.OnPlay();
        Debug.Log("[CircleVR] Content Client Play");
    }

    public void OnPause()
    {
        if (Event != null)
            Event.OnPause();
        Debug.Log("[CircleVR] Content Client Pause");
    }

    public void OnStop()
    {
        if (Event != null)
            Event.OnStop();
        Debug.Log("[CircleVR] Content Client Stop");
    }

    public void OnNext()
    {
        if (Event != null)
            Event.OnNext();
        Debug.Log("[CircleVR] Content Client Next");
    }

    public void OnConnect(int connectionID, byte error)
    {
        CircleVRNetwork.Send(CircleVRPacketType.RequestServerContentInfo, connectionID, CircleVRNetwork.reliableChannel);
    }

    public void OnDisconnect(int connectionID, byte error)
    {
    }

    public void OnBroadcast(byte[] data, int size, byte error)
    {
    }

    public void OnData(int connectionId, int channelId, byte key, byte[] data, byte error)
    {
        CircleVRPacketType type = (CircleVRPacketType)key;

        if (type == CircleVRPacketType.ServerContentInfo)
        {
            Debug.Log("[INFO] Receive Content Server Status");

            ContentServerStatus contentServerStatus = JsonUtility.FromJson<ContentServerStatus>(CircleVRNetwork.ByteToString(data));

            if (Event != null)
                Event.OnContentServerStatusReceived(contentServerStatus);

            return;
        }

        if (type == CircleVRPacketType.Play)
        {
            OnPlay();
            return;
        }

        if (type == CircleVRPacketType.Pause)
        {
            OnPause();
            return;
        }

        if (type == CircleVRPacketType.Stop)
        {
            OnStop();
            return;
        }

        if (type == CircleVRPacketType.Next)
        {
            OnNext();
            return;
        }

        if (Event != null)
            Event.OnData(connectionId, channelId, key, data, error);
    }
}

