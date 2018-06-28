using System;
using System.Collections;
using System.Collections.Generic;
using BestHTTP.SignalR;
using UnityEngine;
using BestHTTP.WebSocket;

public class CloudVRCommandChannel {
    private const float RetryConnectPeriod = 1.0f;
    
    public CloudVRCommandChannel(CloudVRClient client, string type) {
        _client = client;
        _type = type;
    }

    private CloudVRClient _client;
    private string _type;
    private WebSocket _webSocket;
    private float _remainingTimeToConnect = -1.0f;

    private string channelUri {
        get {
            return "ws://" + _client.host + ":" + _client.port + "/wsapi/" + _type;
        }
    }

    private void openSocket() {
        _webSocket = new WebSocket(new Uri(channelUri));
        _webSocket.OnMessage += wsMesageReceived;
        _webSocket.OnError += wsErrorOccurred;
        
        _webSocket.Open();
    }

    private void closeSocket() {
        _webSocket.Close();
        _webSocket = null;
    }
    
    public delegate void CommandReceivedHandler(CloudVRCommandChannel channel, string command);
    public event CommandReceivedHandler CommandReceived;

    public bool opened {
        get {
            return _webSocket != null;
        }
    }

    public void Open() {
        _remainingTimeToConnect = 0.0f;
    }

    public void Update(float deltaTime) {
        if (_webSocket != null || _remainingTimeToConnect < 0.0f) {
            return;
        }

        _remainingTimeToConnect -= deltaTime;
        if (_remainingTimeToConnect <= 0.0f) {
            _remainingTimeToConnect = -1.0f;

            openSocket();
        }
    }

    public void Close() {
        closeSocket();
        _remainingTimeToConnect = -1.0f;
    }

    // handle WebSocket events
    private void wsMesageReceived(WebSocket socket, string message) {
        if (CommandReceived != null) {
            CommandReceived(this, message);
        }
    }

    private void wsErrorOccurred(WebSocket socket, Exception exception) {
        closeSocket();
        _remainingTimeToConnect = RetryConnectPeriod;
    }
}
