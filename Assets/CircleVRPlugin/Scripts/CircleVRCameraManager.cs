using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class CircleVRCameraManager : MonoBehaviour
{
    private Dictionary<string, DisplayLayoutCamera> cams = new Dictionary<string, DisplayLayoutCamera>();

    public void Init()
    {
        DisplayLayoutCamera[] arrCam = Object.FindObjectsOfType<DisplayLayoutCamera>();

        if (DisplayLayoutCamera.config != null)
        {
            foreach (var cam in arrCam)
            {
                cams.Add(cam.id, cam);
            }

            if (DisplayLayoutCamera.config.displays.Length == 3)
                GameObject.Instantiate(Resources.Load("2POVPannel"));

            return;
        }

        if (CircleVR.IsEditorPlatform())
            return;

        foreach (var cam in arrCam)
        {
            Destroy(cam.gameObject);
        }
    }

    private void ActivateDisplay(string id, Transform target)
    {
        try
        {
            cams[id].target = target;
            DisplayLayoutFade fade = cams[id].GetComponent<DisplayLayoutFade>();
            if (fade)
                cams[id].StartCoroutine(fade.Fade(true));
        }
        catch
        {

        }
    }

    public void SetDisplay(List<string> connectedUserIDs)
    {
        Queue<string> connectedQueue = new Queue<string>();

        foreach (var connection in connectedUserIDs)
        {
            connectedQueue.Enqueue(connection);
        }

        List<DisplayLayoutCamera> list = cams.Values.ToList();

        Transform t1;
        Transform t2;
        Transform t3;
        Transform t4;

        switch (connectedQueue.Count)
        {
            case 0:
                foreach (var cam in cams.Values)
                {
                    if (cam.id == "observer")
                        continue;

                    DisplayLayoutFade fade = cam.GetComponent<DisplayLayoutFade>();
                    if (fade)
                        cam.StartCoroutine(fade.Fade(false));
                }
                break;
            case 1:
                t1 = CircleVRTrackingSystem.GetTracker(CircleVR.GetPair(connectedQueue.Dequeue()).trackerID).transform.Find("CenterAnchor");

                foreach (var key in cams.Keys)
                {
                    if (key == "observer")
                        continue;

                    ActivateDisplay(key, t1);
                }
                return;
            case 2:
                t1 = CircleVRTrackingSystem.GetTracker(CircleVR.GetPair(connectedQueue.Dequeue()).trackerID).transform.Find("CenterAnchor");
                t2 = CircleVRTrackingSystem.GetTracker(CircleVR.GetPair(connectedQueue.Dequeue()).trackerID).transform.Find("CenterAnchor");

                ActivateDisplay("display1", t1);

                switch (DisplayLayoutCamera.config.displays.Length)
                {
                    case 3:
                        ActivateDisplay("display2", t2);
                        return;
                    case 4:
                        ActivateDisplay("display2", t2);
                        ActivateDisplay("display3", t1);
                        return;
                    case 5:
                        ActivateDisplay("display2", t2);
                        ActivateDisplay("display3", t1);
                        ActivateDisplay("display4", t2);
                        return;
                }

                return;
            case 3:
                t1 = CircleVRTrackingSystem.GetTracker(CircleVR.GetPair(connectedQueue.Dequeue()).trackerID).transform.Find("CenterAnchor");
                t2 = CircleVRTrackingSystem.GetTracker(CircleVR.GetPair(connectedQueue.Dequeue()).trackerID).transform.Find("CenterAnchor");
                t3 = CircleVRTrackingSystem.GetTracker(CircleVR.GetPair(connectedQueue.Dequeue()).trackerID).transform.Find("CenterAnchor");

                ActivateDisplay("display1", t1);
                ActivateDisplay("display2", t2);
                ActivateDisplay("display3", t3);
                ActivateDisplay("display4", t2);
                return;
            case 4:
                t1 = CircleVRTrackingSystem.GetTracker(CircleVR.GetPair(connectedQueue.Dequeue()).trackerID).transform.Find("CenterAnchor");
                t2 = CircleVRTrackingSystem.GetTracker(CircleVR.GetPair(connectedQueue.Dequeue()).trackerID).transform.Find("CenterAnchor");
                t3 = CircleVRTrackingSystem.GetTracker(CircleVR.GetPair(connectedQueue.Dequeue()).trackerID).transform.Find("CenterAnchor");
                t4 = CircleVRTrackingSystem.GetTracker(CircleVR.GetPair(connectedQueue.Dequeue()).trackerID).transform.Find("CenterAnchor");

                ActivateDisplay("display1", t1);
                ActivateDisplay("display2", t2);
                ActivateDisplay("display3", t3);
                ActivateDisplay("display4", t4);
                return;
        }
    }
}
