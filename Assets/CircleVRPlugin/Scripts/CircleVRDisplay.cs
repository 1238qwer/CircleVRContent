using System.Collections.Generic;
using UnityEngine;

public class CircleVRDisplay : MonoBehaviour
{
    public static CircleVRCameraManager Camera { private set; get; }

    private static CircleVRBarrier barrier;

    public static Dictionary<string, CircleVRHead> heads = new Dictionary<string, CircleVRHead>();

    public void Init()
    {
        Camera = CircleVR.DontDestroyInstantiate("CameraManager", Vector3.zero, Quaternion.identity).AddComponent<CircleVRCameraManager>();
        Camera.Init();

        CircleVRTrackingSystem.onCreateTracker += onCreateTracker;

        Debug.Log("[CircleVR Display] Initialized");
    }

    public static void InitBarrier(float barrierRadius, bool showBarrier)
    {
        barrier = CircleVR.DontDestroyInstantiate("Barrier", Vector3.zero, Quaternion.identity).AddComponent<CircleVRBarrier>();
        barrier.Init(barrierRadius, Object.FindObjectOfType<CircleVR>().barrierMaterial, showBarrier);
    }

    private void onCreateTracker(string ID, Transform transform)
    {
        Debug.Assert(!heads.ContainsKey(ID));

        heads.Add(ID, CreateHead(transform , ID));
        Debug.Log("[CircleVR Display] Created Head : " + ID);
    }

    private CircleVRHead CreateHead(Transform origin , string trackerID)
    {
        GameObject prefab = Resources.Load<GameObject>("head");

        CircleVRHead head = CircleVR.DontDestroyInstantiate(prefab, prefab.transform.localPosition, prefab.transform.localRotation).GetComponent<CircleVRHead>();
        head.Init(trackerID, origin);

        return head;
    }

    public static CircleVRHead GetHead(string trackerID)
    {
        Debug.Assert(heads.ContainsKey(trackerID));

        return heads[trackerID];
    }
}
