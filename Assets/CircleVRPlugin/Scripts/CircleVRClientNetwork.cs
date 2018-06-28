using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class CircleVRClientNetwork : MonoBehaviour, ICircleVRClientEventHandler , ICircleVRNetworkEventHandler 
{
    private string serverIP;
    private int serverPort;

    private bool connecting;
    private bool connected;

    private int connectionId;

    private const float autoRequestTime = 2.0f;
    private float autoRequestNowTime;

    private string userID = null;

    private bool initFinished;

    private bool bounded;

    private void Awake()
    {
        CircleVRNetwork.Init(1);
        CircleVRClient.Event = this;
        CircleVRNetwork.events.Add(this);
    }

    public void OnInit(Configuration config)
    {
        serverIP = config.circlevr.serverIp;
        serverPort = config.circlevr.serverPort;
        initFinished = true;
    }

    public void OnBounded(string userID , AirVRStereoCameraRig rig)
    {
        bounded = true;

        if (this.userID != userID)
            this.userID = userID;

        Connect();
    }

    public void OnUnbound()
    {
        bounded = false;

        byte error;
        if (!NetworkTransport.Disconnect(CircleVRNetwork.hostID, connectionId, out error))
        {
            Debug.LogError(((NetworkError)error).ToString());
            return;
        }

        Debug.Log("[CircleVRClientNetwork] Disconnect");
    }

    private void Connect()
    {
        if (!initFinished)
            return;

        if (connecting)
            return;

        Debug.Log("[INFO] Try Connect!");

        byte error;

        connectionId = NetworkTransport.Connect(CircleVRNetwork.hostID, serverIP, serverPort, 0, out error);

        if (error != 0)
        {
            Debug.LogError(error);
            return;
        }

        connecting = true;
    }

    private void AutoConnect()
    {
        if (connecting)
            return;

        if(autoRequestNowTime >= autoRequestTime)
        {
            autoRequestNowTime = 0.0f;
            Connect();
            return;
        }

        autoRequestNowTime += Time.deltaTime;
    }

    private void LateUpdate()
    {
        if (!initFinished)
            return;

        if (!bounded)
            return;

        if (!connected)
            AutoConnect();
    }

    public void OnDisconnect(int connectionId, byte error)
    {
        Debug.Log("[INFO] Disconnected");
        connected = false;
        connecting = false;
    }

    public void OnConnect(int connectionId, byte error)
    {
        Debug.Log("[INFO] Unity Connect Succeed!");
        SendClientData();
    }

    public void OnData(int connectionId, int channelId, byte key, byte[] data, byte error)
    {
        CircleVRPacketType type = (CircleVRPacketType)key;

        if (!connected)
        {
            if (type == CircleVRPacketType.Error)
            {
                CircleVRError cvError = JsonUtility.FromJson<CircleVRError>(CircleVRNetwork.ByteToString(data));
                Debug.Log("[INFO] Error : " + cvError.type.ToString());

                if(cvError.type == CircleVRErrorType.AlreadyHasUserID || cvError.type == CircleVRErrorType.NotFoundUserIDInPairs)
                    connecting = false;
                return;
            }

            if (type == CircleVRPacketType.HostInfo)
            {
                Debug.Log("[INFO] Circle VR Connect Succeed!");

                HostInfo hostInfo = JsonUtility.FromJson<HostInfo>(CircleVRNetwork.ByteToString(data));

                CircleVRDisplay.InitBarrier(hostInfo.safetyBarrierRadius, hostInfo.showBarrier);

                connected = true;
                connecting = false;
                CircleVRNetwork.Send(CircleVRPacketType.RequestServerContentInfo, connectionId, CircleVRNetwork.reliableChannel);
                return;
            }
        }
    }

    private void SendClientData()
    {
        ClientData data = new ClientData(userID);
        string json = JsonUtility.ToJson(data);
        CircleVRNetwork.Send(CircleVRPacketType.ClientInfo , CircleVRNetwork.StringToByte(json) , connectionId , CircleVRNetwork.reliableChannel);
    }

    public void OnBroadcast(byte[] data, int size, byte error)
    {
    }
}
