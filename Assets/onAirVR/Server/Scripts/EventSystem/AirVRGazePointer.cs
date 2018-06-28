/***********************************************************

  Copyright (c) 2017-2018 Clicked, Inc.

  Licensed under the MIT license found in the LICENSE file 
  in the Docs folder of the distributed package.

 ***********************************************************/

public class AirVRGazePointer : AirVRPointer {
    // implements AirVRPointer
    protected override AirVRInput.Device device {
        get {
            return AirVRInput.Device.HeadTracker;
        }
    }

    public override bool primaryButtonPressed {
        get {
            return AirVRInput.GetDown(cameraRig, AirVRInput.Touchpad.Button.Touch) || AirVRInput.GetDown(cameraRig, AirVRInput.Gamepad.Button.A);
        }
    }

    public override bool primaryButtonReleased {
        get {
            return AirVRInput.GetUp(cameraRig, AirVRInput.Touchpad.Button.Touch) || AirVRInput.GetUp(cameraRig, AirVRInput.Gamepad.Button.A);
        }
    }

}
