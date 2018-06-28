using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

public enum CircleVRPacketType : byte
{
    TrackingData = 230,
    Error,
    HostInfo,
    ClientInfo,
    ServerContentInfo,
    RequestServerContentInfo,
    Play,
    Stop,
    Pause,
    Next,
    ContentClient,
    ContentServer,
    VideoPlayer = 50,
    Name,
}

public enum CircleVRErrorType
{
    AlreadyHasUserID,
    NotFoundUserIDInPairs
}

[Serializable]
public class CircleVRError
{
    public CircleVRErrorType type;

    public CircleVRError(CircleVRErrorType type)
    {
        this.type = type;
    }
}

[Serializable]
public class HostInfo
{
    public float safetyBarrierRadius;
    public bool showBarrier;

    public HostInfo(bool showBarrier, float safetyBarrierRadius)
    {
        this.showBarrier = showBarrier;
        this.safetyBarrierRadius = safetyBarrierRadius;
    }
}

[Serializable]
public class ContentName
{
    public string name;

    public ContentName(string name)
    {
        this.name = name;
    }
}



[Serializable]
public struct ClientData
{
    public string userId;

    public ClientData(string userId)
    {
        this.userId = userId;
    }
}

[Serializable]
public class ContentServerStatus
{
    public CircleVRServerState state;
    public float elapseTime;

    public ContentServerStatus(CircleVRServerState state, float elapseTime)
    {
        this.state = state;
        this.elapseTime = elapseTime;
    }
}

public interface ICircleVRNetworkEventHandler
{
    void OnConnect(int connectionID , byte error);
    void OnDisconnect(int connectionID, byte error);
    void OnBroadcast(byte[] data , int size, byte error);
    void OnData(int connectionId, int channelId, byte key, byte[] data, byte error);
}

public class CircleVRNetwork : MonoBehaviour
{
    public static List<ICircleVRNetworkEventHandler> events = new List<ICircleVRNetworkEventHandler>();
    public static int hostID { private set; get; }
    public static int reliableChannel { private set; get; }
    public static int stateUpdateChannel { private set; get; }
    public static int unreliableChannel { private set; get; }

    public const int REC_BUFFER_SIZE = 1024;

    private static bool initialized;

    public static readonly List<int> connectionIDs = new List<int>();

    public static void Send(int hostId, byte[] data, int connectionId, int channelId)
    {
        byte error;

        Debug.Assert(sizeof(byte) * data.Length <= REC_BUFFER_SIZE);

        NetworkTransport.Send(hostId, connectionId, channelId, data, data.Length, out error);
        NetworkError e = ((NetworkError)error);
        if (e == NetworkError.Ok)
            return;

        Debug.Log("[INFO] Send Error : " + e.ToString());
    }

    public static void SendBroadcast(CircleVRPacketType type, int channelID)
    {
        foreach (var connectionID in connectionIDs)
        {
            Send(type, connectionID, channelID);
        }
    }

    public static void SendBroadcast(byte key, int channelID)
    {
        foreach (var connectionID in connectionIDs)
        {
            Send(key, connectionID, channelID);
        }
    }

    public static void Send(byte key, int connectionID, int channelID)
    {
        byte[] buffer = new byte[1];
        buffer[0] = key;
        Send(hostID, buffer, connectionID, channelID);
    }

    public static void Send(CircleVRPacketType type, int connectionID, int channelID)
    {
        Send((byte)type, connectionID, channelID);
    }

    public static void SendError(int connectionId, CircleVRErrorType cvError)
    {
        string msg = JsonUtility.ToJson(new CircleVRError(cvError));
        Send(CircleVRPacketType.Error, StringToByte(msg),connectionId , reliableChannel);
    }

    public static void SendBroadcast(CircleVRPacketType type, byte[] data, int channelID)
    {
        foreach (var connectionID in connectionIDs)
        {
            Send(type, data,  connectionID, channelID);
        }
    }

    public static void SendBroadcast(byte key, byte[] data, int channelID)
    {
        foreach (var connectionID in connectionIDs)
        {
            Send(key, data, connectionID, channelID);
        }
    }

    public static void Send(CircleVRPacketType type, byte[] data, int connectionId, int channelID)
    {
        Send((byte)type, data, connectionId, channelID);
    }

    public static void Send(byte type, byte[] data, int connectionId, int channelID)
    {
        byte[] buffer = new byte[data.Length + 1];

        buffer[0] = type;

        for (int i = 1; i < buffer.Length; i++)
        {
            buffer[i] = data[i - 1];
        }

        Send(hostID, buffer, connectionId, channelID);
    }

    public static void Init(int maxconnection)
    {
        hostID = NetworkTransport.AddHost(TransportInit(maxconnection));
    }

    public static void Init(int port , int maxconnection)
    {
        hostID = NetworkTransport.AddHost(TransportInit(maxconnection), port);
    }

    public static string ByteToString(byte[] strByte)
    {
        string str = Encoding.UTF8.GetString(strByte);

        return str;
    }

    public static byte[] IntToByte(int data)
    {
        return BitConverter.GetBytes(data);
    }

    public static byte[] StringToByte(string str)
    {
        byte[] strByte = Encoding.UTF8.GetBytes(str);

        return strByte;
    }

    private static void Decompose(IList<byte> buffer, int recBufferSize, out byte key, out byte[] data)
    {
        data = new byte[recBufferSize - 1];

        for (int i = 0; i < data.Length; i++)
        {
            data[i] = buffer[i + 1];
        }

        key = buffer[0];
    }

    private static HostTopology TransportInit(int maxConnection)
    {
        NetworkTransport.Init();
        ConnectionConfig connectionConfig = new ConnectionConfig();
        connectionConfig.FragmentSize = 1000;
        connectionConfig.PacketSize = 1470;
        reliableChannel = connectionConfig.AddChannel(QosType.ReliableSequenced);
        stateUpdateChannel = connectionConfig.AddChannel(QosType.StateUpdate);
        unreliableChannel = connectionConfig.AddChannel(QosType.Unreliable);

        initialized = true;
        return new HostTopology(connectionConfig, maxConnection);
    }

    void Update ()
    {
        if (!initialized)
            return;

        int outConnectionId;
        int outChannelId;
        int outDataSize;

        byte[] buffer = new byte[REC_BUFFER_SIZE];

        byte error;

        NetworkEventType networkEvent = NetworkEventType.DataEvent;
        do
        {
            networkEvent = NetworkTransport.ReceiveFromHost(hostID, out outConnectionId, out outChannelId, buffer, REC_BUFFER_SIZE, out outDataSize, out error);

            switch (networkEvent)
            {
                case NetworkEventType.ConnectEvent:
                    connectionIDs.Add(outConnectionId);
                    foreach (var Event in events)
                    {
                        Event.OnConnect(outConnectionId, error);
                    }
                    break;

                case NetworkEventType.DataEvent:
                    {
                        byte key;
                        byte[] data;
                        Decompose(buffer, outDataSize, out key, out data);
                        foreach (var Event in events)
                        {
                            Event.OnData(outConnectionId, outChannelId, key, data, error);
                        }
                    }
                    break;

                case NetworkEventType.DisconnectEvent:
                    connectionIDs.Remove(outConnectionId);
                    foreach (var Event in events)
                    {
                        Event.OnDisconnect(outConnectionId, error);
                    }
                    break;
            }

        } while (networkEvent != NetworkEventType.Nothing);
    }
}
