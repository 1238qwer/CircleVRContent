/***********************************************************

  Copyright (c) 2017-2018 Clicked, Inc.

  Licensed under the MIT license found in the LICENSE file 
  in the Docs folder of the distributed package.

 ***********************************************************/

public class AirVRTrackedControllerPointer : AirVRPointer {
    // implements AirVRPointer
    protected override AirVRInput.Device device {
        get {
            return AirVRInput.Device.TrackedController;
        }
    }

    public override bool primaryButtonPressed {
        get {
            return AirVRInput.GetDown(cameraRig, AirVRInput.TrackedController.Button.TouchpadClick) || AirVRInput.GetDown(cameraRig, AirVRInput.TrackedController.Button.IndexTrigger);
        }
    }

    public override bool primaryButtonReleased {
        get {
            return AirVRInput.GetUp(cameraRig, AirVRInput.TrackedController.Button.TouchpadClick) || AirVRInput.GetUp(cameraRig, AirVRInput.TrackedController.Button.IndexTrigger);
        }
    }
}
