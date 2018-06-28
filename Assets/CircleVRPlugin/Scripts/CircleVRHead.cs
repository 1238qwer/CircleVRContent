using UnityEngine;

public class CircleVRHead : MonoBehaviour , IBarrierEventHandler 
{
    private readonly Vector3 POSITION = new Vector3(0, -0.015f, 0.1f);
    private readonly Vector3 EULER = new Vector3(0.0f, 0.0f, 0.0f);
    private bool connected = false;
    public bool Connected
    {
        get
        {
            return connected;
        }

        set
        {
            if (connected == value)
                return;

            connected = value;
            SetHeadColor(connected);
        }
    }

    private string trackerID;
    private const float T = 0.25f;

    public GameObject headModel;

    private static readonly Color connectedHeadColor = Color.white;
    private static readonly Color unconnectedHeadColor = Color.red;

    private Material mat;

    private void Awake()
    {
        mat = headModel.GetComponent<MeshRenderer>().material;
        gameObject.SetActive(false);
        OnDisconnect();
    }

    public void Init(string trackerID , Transform tracker)
    {
        this.trackerID = trackerID;
        CircleVRBarrier.Events.Add(this);
        transform.SetParent(tracker);
        transform.localPosition = POSITION;
        transform.localEulerAngles = EULER;
        transform.localScale = Vector3.one;
    }

    public void SetHeadColor(bool connected)
    {
        if(!connected)
        {
            OnDisconnect();
            return;
        }

        OnConnect();
    }

    private void OnConnect()
    {
        mat.color = connectedHeadColor;
        Debug.Log("[Head] Connected");
    }

    private void OnDisconnect()
    {
        mat.color = unconnectedHeadColor;
        Debug.Log("[Head] Disconnected");
    }

    public Vector3 GetPosition()
    {
        return CircleVRTrackingSystem.GetTracker(trackerID).position;
    }

    public void OnStay(bool inBarrier)
    {
        if (!inBarrier && gameObject.activeSelf)
        {
            gameObject.SetActive(false);
            Debug.Log("[CircleVR Head] Disabled");
        }

        if (inBarrier && !gameObject.activeSelf)
        {
            gameObject.SetActive(true);
            Debug.Log("[CircleVR Head] Enable");
        }
    }

    public void OnSetTrackerData(CircleVRHostNetwork.Packet.TrackerData data)
    {
        if (trackerID == data.onAirVRUserID)
        {
            if(data.connected && !connected)
                OnConnect();

            if (!data.connected && connected)
                OnDisconnect();
        }
    }
}

