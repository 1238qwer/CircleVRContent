using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Valve.VR;

public class CircleVRSteamVRTrackingDataFeeder : MonoBehaviour
{
    public void Update()
    {
        List<string> currentIDs = new List<string>();
        var error = ETrackedPropertyError.TrackedProp_Success;
        for (uint i = 0; i < 16; i++)
        {
            var result = new StringBuilder(64);
            OpenVR.System.GetStringTrackedDeviceProperty(i, ETrackedDeviceProperty.Prop_RenderModelName_String, result, 64, ref error);

            if (result.ToString().Contains("tracker"))
            {
                StringBuilder id = new StringBuilder(64);

                OpenVR.System.GetStringTrackedDeviceProperty(i, ETrackedDeviceProperty.Prop_SerialNumber_String, id, 64, ref error);

                currentIDs.Add(id.ToString());

                if (CircleVRTrackingSystem.HasTracker(id.ToString()))
                    continue;

                SteamVR_TrackedObject trackedObj = CircleVRTrackingSystem.CreateTracker(id.ToString()).gameObject.AddComponent<SteamVR_TrackedObject>();
                trackedObj.SetDeviceIndex((int)i);
            }
        }
    }
}
