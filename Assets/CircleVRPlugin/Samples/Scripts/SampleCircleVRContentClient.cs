using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SampleCircleVRContentClient : MonoBehaviour , ICircleVRContentClientEventHandler
{
    private void Awake()
    {
        CircleVRContentClient.Event = this;
    }

    public void OnContentServerStatusReceived(ContentServerStatus status)
    {
        Debug.Log("[SampleContentClient] OnContentServerStatusReceived");
    }

    public void OnData(int connectionId, int channelId, byte key, byte[] data, byte error)
    {
    }

    public void OnNext()
    {
        Debug.Log("[SampleContentClient] OnNext");
    }

    public void OnPause()
    {
        Debug.Log("[SampleContentClient] OnPause");
    }

    public void OnPlay()
    {
        Debug.Log("[SampleContentClient] OnPlay");
    }

    public void OnStop()
    {
        Debug.Log("[SampleContentClient] OnStop");
    }

    public void OnInit(AirVRStereoCameraRig rig)
    {
        Debug.Log("[SampleContentClient] OnInit");
    }
}
