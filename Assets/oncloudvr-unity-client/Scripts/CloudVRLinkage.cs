using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.Analytics;
using UnityEngine.Assertions;

public class CloudVRLinkage {
    public CloudVRLinkage(Dictionary<string, object> jsonDeserialized) {
        List<object> linkages = jsonDeserialized["linkages"] as List<object>;
        Debug.Assert(linkages.Count > 0);

        Dictionary<string, object> linkage = linkages[0] as Dictionary<string, object>; 
        id = linkage["id"] as string;
        string[] url = (linkage["host"] as string).Split(new char[] { ':' }, System.StringSplitOptions.RemoveEmptyEntries);
        Assert.IsTrue(url.Length == 2);

        int port;
        host = url[0];
        this.port = int.TryParse(url[1], out port) ? port : 0;
    }

    public string id    { get; private set; }
    public string host  { get; private set; }
    public int port     { get; private set; }
}

public class CloudVRGroupLinkage {
    public CloudVRGroupLinkage(Dictionary<string, object> jsonDeserialized) {
        string[] url = (jsonDeserialized["address"] as string).Split(new char[] { ':' }, System.StringSplitOptions.RemoveEmptyEntries);
        Debug.Assert(url.Length == 2);

        int port;
        host = url[0];
        this.port = int.TryParse(url[1], out port) ? port : 0;
    }
    
    public string host  { get; private set; }
    public int port     { get; private set; }
}
