using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class CircleVRHostNetwork : MonoBehaviour ,ICircleVRNetworkEventHandler
{
    public class UserIDAndConnectionIDPair
    {
        public string userID;
        public int connectionId;
        public UserIDAndConnectionIDPair(string userID, int connectionId)
        {
            this.userID = userID;
            this.connectionId = connectionId;
        }
    }

    [Serializable]
    public class Packet
    {
        [Serializable]
        public class TrackerData
        {
            public string onAirVRUserID;
            public bool connected;
            public Vector3 position;
            public Quaternion oriented;
        }

        public Vector3 originPosition;
        public Quaternion originOriented;
        public List<TrackerData> trackerDatas;
    }

    private List<UserIDAndConnectionIDPair> connectedPairs = new List<UserIDAndConnectionIDPair>();

    private Configuration config;

    private Packet packet = null;

    private CircleVRHost host;

    private bool initFinished;

    private void Awake()
    {
        CircleVRNetwork.events.Add(this);
    }

    public void Init(Configuration config ,CircleVRHost host , int maxClientCount)
    {
        CircleVRNetwork.Init(config.circlevr.serverPort, maxClientCount);

        this.config = config;

        this.host = host;
        InitPacket();

        initFinished = true;
    }

    private void InitPacket()
    {
        packet = new Packet();
        packet.trackerDatas = new List<Packet.TrackerData>();

        foreach (Configuration.TrackerAndUserIDPair pair in config.circlevr.pairs)
        {
            Packet.TrackerData trackerData = new Packet.TrackerData();
            trackerData.onAirVRUserID = pair.userID;
            packet.trackerDatas.Add(trackerData);
        }
    }

    private UserIDAndConnectionIDPair GetConnectedPair(int connectionID)
    {
        foreach (var connectedPair in connectedPairs)
        {
            if (connectedPair.connectionId == connectionID)
                return connectedPair;
        }

        return null;
    }

    private UserIDAndConnectionIDPair GetConnectedPair(string userID)
    {
        foreach (var connectedPair in connectedPairs)
        {
            if (connectedPair.userID == userID)
                return connectedPair;
        }

        return null;
    }

    private Packet.TrackerData GetTrackerDataInPacket(string userID)
    {
        foreach (var trackerData in packet.trackerDatas)
        {
            if (trackerData.onAirVRUserID == userID)
                return trackerData;
        }

        Debug.LogError("[ERROR] Not Found CircleVR Transform By onAirVR User ID : " + userID);
        return null;
    }

    private void SetConnectedInPacket(string userID , bool connected)
    {
        GetTrackerDataInPacket(userID).connected = connected;
    }

    private void UpdatePacket(Packet.TrackerData trackerData)
    {
        if (!CircleVR.HasUserIDInPairs(trackerData.onAirVRUserID))
        {
            //TODO : must be notify in gui
            return;
        }

        Configuration.TrackerAndUserIDPair pair = CircleVR.GetPair(trackerData.onAirVRUserID);

        Transform tracker = CircleVRTrackingSystem.GetTracker(pair.trackerID);

        if (tracker == null)
        {
            //TODO : must be notify in gui
            return;
        }

        trackerData.position = tracker.localPosition;
        trackerData.oriented = tracker.localRotation;
    }

    private void LateUpdate()
    {
        if (initFinished && connectedPairs.Count > 0)
            SendTrackingData();
    }

    private void SendTrackingData()
    {
        foreach (Packet.TrackerData trackerData in packet.trackerDatas)
        {
            UpdatePacket(trackerData);
        }

        foreach (UserIDAndConnectionIDPair connection in connectedPairs)
        {
            CircleVRNetwork.Send(CircleVRPacketType.TrackingData,
                CircleVRNetwork.StringToByte(JsonUtility.ToJson(packet)),
                connection.connectionId, CircleVRNetwork.stateUpdateChannel);
        }
    }

    private void RemoveConnectedPair(int connectionID)
    {
        UserIDAndConnectionIDPair pair = GetConnectedPair(connectionID);
        host.RemoveConnectedUserID(pair.userID);
        SetConnectedInPacket(pair.userID, false);
        connectedPairs.Remove(pair);
        CircleVRDisplay.GetHead(CircleVR.GetPair(pair.userID).trackerID).Connected = false;
        Debug.Log("[CircleVR Host Network] Disconnected User ID : " + pair.userID);
    }

    private void AddConnectedPair(UserIDAndConnectionIDPair pair)
    {
        Packet.TrackerData trackerData = GetTrackerDataInPacket(pair.userID);
        trackerData.connected = true;

        CircleVRNetwork.Send(CircleVRPacketType.HostInfo,
          CircleVRNetwork.StringToByte(JsonUtility.ToJson(new HostInfo(config.circlevr.showBarrier, config.circlevr.safetyBarrierRadius))),
          pair.connectionId, CircleVRNetwork.reliableChannel);

        connectedPairs.Add(pair);
        CircleVRDisplay.GetHead(CircleVR.GetPair(pair.userID).trackerID).Connected = true;

        Debug.Log("[CircleVR Host Network] Connected User ID : " + pair.userID);
    }

    private void Disconnect(int hostId, int connectionID, byte error)
    {
        if (!NetworkTransport.Disconnect(hostId, connectionID, out error))
        {
            Debug.Log("[CircleVR Host Network] Disconnect Error : " + ((NetworkError)error).ToString());
            Debug.LogError(((NetworkError)error).ToString());
        }

        RemoveConnectedPair(connectionID);
    }

    public void OnDisconnect(int connectionID, byte error)
    {
        RemoveConnectedPair(connectionID);
    }

    public void OnData(int connectionId, int channelId, byte key, byte[] data, byte error)
    {
        CircleVRPacketType type = (CircleVRPacketType)key;

        if (type == CircleVRPacketType.VideoPlayer)
        {
            ContentName name = new ContentName(CircleVR.ContentName);
            string msg = JsonUtility.ToJson(name);

            CircleVRNetwork.Send(CircleVRPacketType.Name, CircleVRNetwork.StringToByte(msg), connectionId, CircleVRNetwork.reliableChannel);
            Debug.Log("[CircleVR Host Network] Send ContentName To Video Player : " + msg);
            return;
        }

        if (type == CircleVRPacketType.ClientInfo)
        {
            ClientData clientData = JsonUtility.FromJson<ClientData>(CircleVRNetwork.ByteToString(data));

            CircleVRErrorType cvError;

            if (host.AddConnectedUserIDs(clientData.userId, out cvError))
            {
                AddConnectedPair(new UserIDAndConnectionIDPair(clientData.userId, connectionId));
                return;
            }

            CircleVRNetwork.SendError(connectionId, cvError);
            Disconnect(CircleVRNetwork.hostID, connectionId, error);
            Debug.Log("[CircleVR Host Network] Connect Failed : " + cvError.ToString() + "\nUser ID : " + clientData.userId);
            return;
        }
    }

    public void OnConnect(int connectionID, byte error)
    {
    }

    public void OnBroadcast(byte[] data, int size, byte error)
    {
    }
}
