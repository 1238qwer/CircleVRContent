using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CloudVRTracker {
    public CloudVRTracker(Dictionary<string, object> jsonDeserialized) {
        trackerID = jsonDeserialized["trackerID"] as string;
        userID = jsonDeserialized["userID"] as string;
    }

    public string trackerID    { get; private set; }
    public string userID       { get; private set; }
}
