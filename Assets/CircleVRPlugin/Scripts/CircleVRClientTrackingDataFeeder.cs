using UnityEngine;
using System.Collections.Generic;

public class CircleVRClientTrackingDataFeeder : MonoBehaviour , ICircleVRNetworkEventHandler
{
    private void Awake()
    {
        CircleVRNetwork.events.Add(this);
    }

    private void SetTrackerTransform(CircleVRHostNetwork.Packet.TrackerData trackerData)
    {
        Transform trackerTransform = CircleVRTrackingSystem.GetTracker(trackerData.onAirVRUserID);

        if (!CircleVRTrackingSystem.HasTracker(trackerData.onAirVRUserID))
            trackerTransform = CircleVRTrackingSystem.CreateTracker(trackerData.onAirVRUserID);

        trackerTransform.localPosition = Vector3.Lerp(trackerTransform.localPosition, trackerData.position, 0.25f);
        trackerTransform.localRotation = Quaternion.Lerp(trackerTransform.localRotation, trackerData.oriented, 0.25f);
    }

    public void OnConnect(int connectionID, byte error) { }
    public void OnDisconnect(int connectionID, byte error) { }
    public void OnBroadcast(byte[] data, int size, byte error) { }

    public void OnData(int connectionId, int channelId, byte key, byte[] data, byte error)
    {
        CircleVRPacketType type = (CircleVRPacketType)key;

        if (type == CircleVRPacketType.TrackingData)
        {
            CircleVRHostNetwork.Packet packet = JsonUtility.FromJson<CircleVRHostNetwork.Packet>(CircleVRNetwork.ByteToString(data));

            foreach (CircleVRHostNetwork.Packet.TrackerData trackerData in packet.trackerDatas)
            {
                Debug.Assert(trackerData != null); 
                SetTrackerTransform(trackerData);
                CircleVRDisplay.GetHead(trackerData.onAirVRUserID).Connected = trackerData.connected;
            }
            return;
        }
    }
}
