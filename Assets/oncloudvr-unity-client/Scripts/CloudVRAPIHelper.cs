using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class CloudVRAPIHelper {
    public const long ResponseCodeRequestError = -1;
    public const long ResponseCodeServerBusy = 503;

    public static UnityWebRequest Get(CloudVRClient client, string path) {
        return UnityWebRequest.Get("http://" + client.host + ":" + client.port + path);
    }

    public static UnityWebRequest GetTexture(CloudVRClient client, string path) {
        return UnityWebRequestTexture.GetTexture("http://" + client.host + ":" + client.port + path);
    }

    public static UnityWebRequest Post(CloudVRClient client, string path, string body) {
        UnityWebRequest result = new UnityWebRequest("http://" + client.host + ":" + client.port + path, UnityWebRequest.kHttpVerbPOST);
        UploadHandlerRaw uh = new UploadHandlerRaw(new System.Text.UTF8Encoding().GetBytes(body));
        uh.contentType = "application/json";
        result.uploadHandler = uh;
        result.downloadHandler = new DownloadHandlerBuffer();

        return result;
    }

    public static IEnumerator HandleResponse(UnityWebRequest request, Action onSucceeded, Action onFailed) {
        yield return request.SendWebRequest();

        if (string.IsNullOrEmpty(request.error)) {
            onSucceeded.Invoke();
        }
        else {
            onFailed.Invoke();
        }

        request.Dispose();
    }

    public static bool CheckIfResponseIsOK(long responseCode) {
        return responseCode / 100 == 2;
    }
}
