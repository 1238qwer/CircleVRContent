using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SampleCircleVRContentServer : MonoBehaviour , ICircleVRContentServerEventHandler
{
    private void Awake()
    {
        CircleVRContentServer.Event = this;
    }

    public void OnData(int connectionId, int channelId, byte key, byte[] data, byte error)
    {
    }

    public void OnNext()
    {
        Debug.Log("[SampleContentServer] OnNext");
    }

    public void OnPause()
    {
        Debug.Log("[SampleContentServer] OnPause");
    }

    public void OnPlay()
    {
        Debug.Log("[SampleContentServer] OnPlay");
    }

    public void OnStop()
    {
        Debug.Log("[SampleContentServer] OnStop");
    }

    public void OnInit()
    {
        Debug.Log("[SampleContentServer] OnInit");
    }
}
