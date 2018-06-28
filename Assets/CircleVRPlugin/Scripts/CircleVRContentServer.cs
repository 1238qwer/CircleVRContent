using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class CircleVRServerCommand {
    public enum Type {
        Unknown,
        Play,
        Pause,
        Stop,
        Next
    }

    [SerializeField] private string Command;

    public Type type {
        get {
            if (Command.Equals("play")) {
                return Type.Play;
            }
            else if (Command.Equals("pause")) {
                return Type.Pause;
            }
            else if (Command.Equals("stop")) {
                return Type.Stop;
            }
            else if (Command.Equals("next")) {
                return Type.Next;
            }
            return Type.Unknown;
        }
    }
}

public enum CircleVRServerState
{
    Ready,
    Playing,
    Pause
}

public interface ICircleVRContentServerEventHandler
{
    void OnInit();
    void OnPlay();
    void OnPause();
    void OnStop();
    void OnNext();
    void OnData(int connectionId, int channelId, byte key, byte[] data, byte error);
}

public class CircleVRContentServer : MonoBehaviour, ICircleVRNetworkEventHandler
{
    public static ICircleVRContentServerEventHandler Event;

    private CloudVRCommandChannel commandChannel;

    private CircleVRServerState state = CircleVRServerState.Ready;

    protected float elapseTime;

    // handle CloudVR command events
    private void onCloudVRCommandReceived(CloudVRCommandChannel channel, string command)
    {
        CircleVRServerCommand cmd = JsonUtility.FromJson<CircleVRServerCommand>(command);
        Debug.Log("[CircleVR] Command received : " + cmd.type);

        OnCommand(cmd.type);
    }

    public void OnCommand(CircleVRServerCommand.Type command)
    {
        switch (command)
        {
            case CircleVRServerCommand.Type.Play:
                OnPlay();
                break;
            case CircleVRServerCommand.Type.Pause:
                OnPause();
                break;
            case CircleVRServerCommand.Type.Stop:
                OnStop();
                break;
            case CircleVRServerCommand.Type.Next:
                OnNext();
                break;
        }
    }

    private void Awake()
    {
        CircleVRNetwork.events.Add(this);
    }

    public void Init(Configuration config)
    {
        if (config.cloudVRClient != null)
        {
            commandChannel = new CloudVRCommandChannel(config.cloudVRClient, "content");
            commandChannel.CommandReceived += onCloudVRCommandReceived;
            commandChannel.Open();
        }

        if(CircleVR.IsEditorPlatform())
            CircleVRControlPannel.onCommand += controlPannelCommandReceived;

        if (Event != null)
            Event.OnInit();

        Debug.Log("[CircleVR Content Server] Initialized");
    }

    private void controlPannelCommandReceived(string command)
    {
        OnCommand((CircleVRServerCommand.Type)Enum.Parse(typeof(CircleVRServerCommand.Type),command));
    }

    private void Update()
    {
        if (commandChannel != null)
            commandChannel.Update(Time.deltaTime);


        if (state != CircleVRServerState.Playing)
            return;

        elapseTime += Time.deltaTime;
    }

    public ContentServerStatus GetContentServerStatus()
    {
        Debug.Log("[INFO] Content Server Status\nPlaying / " + state.ToString() + " , ElapseTime / " + elapseTime.ToString());
        return new ContentServerStatus(state, elapseTime);
    }

    private void OnPlay()
    {
        if (state == CircleVRServerState.Playing)
            return;

        state = CircleVRServerState.Playing;
        CircleVRNetwork.SendBroadcast(CircleVRPacketType.Play, CircleVRNetwork.reliableChannel);
        if (Event != null)
            Event.OnPlay();
        Debug.Log("[INFO] Content Server Send Play");
    }

    private void OnPause()
    {
        if (state == CircleVRServerState.Pause)
            return;

        state = CircleVRServerState.Pause;
        CircleVRNetwork.SendBroadcast(CircleVRPacketType.Pause, CircleVRNetwork.reliableChannel);
        if (Event != null)
            Event.OnPause();
        Debug.Log("[INFO] Content Server Send Pause");
    }

    private void OnStop()
    {
        state = CircleVRServerState.Ready;
        elapseTime = 0.0f;

        CircleVRNetwork.SendBroadcast(CircleVRPacketType.Stop, CircleVRNetwork.reliableChannel);
        if (Event != null)
            Event.OnStop();
        Debug.Log("[INFO] Content Server Send Stop");
    }

    private void OnNext()
    {
        CircleVRNetwork.SendBroadcast(CircleVRPacketType.Next, CircleVRNetwork.reliableChannel);
        if (Event != null)
            Event.OnNext();
        Debug.Log("[INFO] Content Server Send Next");
    }

    public void OnConnect(int connectionID, byte error)
    {
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

        if (type == CircleVRPacketType.RequestServerContentInfo)
        {
            string contentServerStatus = JsonUtility.ToJson(GetContentServerStatus());
            CircleVRNetwork.Send(CircleVRPacketType.ServerContentInfo, CircleVRNetwork.StringToByte(contentServerStatus), connectionId, CircleVRNetwork.reliableChannel);
            Debug.Log("[INFO] Send Content Server Status");
            return;
        }

        if (Event != null)
            Event.OnData(connectionId, channelId, key, data, error);
    }
}