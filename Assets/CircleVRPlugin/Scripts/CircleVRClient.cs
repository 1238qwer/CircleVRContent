using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ICircleVRClientEventHandler
{
    void OnInit(Configuration config);
    void OnBounded(string userID , AirVRStereoCameraRig rig);
    void OnUnbound();
}

public class CircleVRClient : MonoBehaviour, AirVRCameraRigManager.EventHandler
{
    public static ICircleVRClientEventHandler Event;

    private static AirVRStereoCameraRig rig;

    private string userID;

    public void Init(Configuration config, AirVRStereoCameraRig rig)
    {
        rig = CreateAirVRCameraRig(rig);

        CircleVRTrackingSystem.onCreateTracker += onCreateTracker;

        QualitySettings.vSyncCount = 0;

        if (Event != null)
            Event.OnInit(config);

        Debug.Log("[CircleVR Client] Initialized");

        SetupContentClient();
    }

    private void SetupContentClient()
    {
        CircleVR.DontDestroyInstantiate("ContentClient", Vector3.zero, Quaternion.identity).AddComponent<CircleVRContentClient>();
    }

    private void onCreateTracker(string ID, Transform trackerTransform)
    {
        if (ID == userID)
        {
            rig.externalTrackingOrigin = CircleVRTrackingSystem.Origin;
            rig.externalTracker = trackerTransform.Find("CenterAnchor");
        }
    }

    private AirVRStereoCameraRig CreateAirVRCameraRig(AirVRStereoCameraRig customizedRigPrefab)
    {
        int onAirVRPort = 9090;
        AirVRServerInitParams initParam = GameObject.FindObjectOfType<AirVRServerInitParams>();
        if (initParam)
            onAirVRPort = initParam.port;

        if (!customizedRigPrefab)
        {
            rig = FindObjectOfType<AirVRStereoCameraRig>();

            if(!rig)
                rig = CircleVR.DontDestroyInstantiate("AirVRCameraRig" , Vector3.zero , Quaternion.identity).AddComponent<AirVRStereoCameraRig>();
        }
        else
            rig = CircleVR.DontDestroyInstantiate(customizedRigPrefab.gameObject , Vector3.zero , Quaternion.identity).GetComponent<AirVRStereoCameraRig>();

        rig.trackingModel = AirVRStereoCameraRig.TrackingModel.ExternalTracker;

        if (!customizedRigPrefab)
        {
            rig.centerEyeAnchor.gameObject.AddComponent<AudioListener>();
            rig.centerEyeAnchor.gameObject.AddComponent<AirVRServerAudioOutputRouter>();
        }

        Debug.Log("[CircleVR Client] onAirVR Port : " + onAirVRPort.ToString());

        AirVRCameraRigManager.managerOnCurrentScene.Delegate = this;

        return rig;
    }

    public void AirVRCameraRigActivated(AirVRCameraRig cameraRig)
    {
    }

    public void AirVRCameraRigDeactivated(AirVRCameraRig cameraRig)
    {
    }

    public void AirVRCameraRigHasBeenUnbound(AirVRCameraRig cameraRig)
    {
        if (Event != null)
            Event.OnUnbound();

        Debug.Log("[CircleVRClient] Unbounded");
    }

    public void AirVRCameraRigWillBeBound(int clientHandle, AirVRClientConfig config, List<AirVRCameraRig> availables, out AirVRCameraRig selected)
    {
        selected = rig;

        userID = config.userID;

        if (Event != null)
            Event.OnBounded(config.userID , rig);

        Debug.Log("[CircleVR Client] Bounded User ID : " + config.userID);
    }
}
