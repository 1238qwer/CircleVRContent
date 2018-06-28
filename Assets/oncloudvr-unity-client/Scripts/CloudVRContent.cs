using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Assertions;
using UnityEngine.Video;

public class CloudVRContent {
    public CloudVRContent(CloudVRClient owner, string json) {
        _owner = owner;
        init(MiniJSON.Json.Deserialize(json) as Dictionary<string, object>);
    }

    public CloudVRContent(CloudVRClient owner, Dictionary<string, object> jsonParsed) {
        _owner = owner;
        init(jsonParsed);
    }

    private CloudVRClient _owner;
    private string[] _thumbnailUrls;
    private string[] _screenshotUrls;

    public string id                { get; private set; }
    public string title             { get; private set; }
    public string version           { get; private set; }
    public string author            { get; private set; }
    public string description       { get; private set; }
    public Texture2D[] thumbnails   { get; private set; }
    public Texture2D[] screenshots  { get; private set; }

    private void init(Dictionary<string, object> jsonParsed) {
        id = jsonParsed["id"] as string;
        title = jsonParsed["title"] as string;
        version = jsonParsed["version"] as string;
        author = jsonParsed["author"] as string;
        description = jsonParsed["description"] as string;

        List<object> thumbnails = jsonParsed["thumbnails"] as List<object>;
        _thumbnailUrls = thumbnails.Count > 0 ? new string[thumbnails.Count] : null;
        for (int i = 0; i < thumbnails.Count; i++) {
            _thumbnailUrls[i] = thumbnails[i] as string;
        }
        if (thumbnails.Count > 0) {
            this.thumbnails = new Texture2D[thumbnails.Count];
        }

        List<object> screenshots = jsonParsed["screenshots"] as List<object>;
        _screenshotUrls = screenshots.Count > 0 ? new string[screenshots.Count] : null;
        for (int i = 0; i < screenshots.Count; i++) {
            _screenshotUrls[i] = screenshots[i] as string;
        }
        if (screenshots.Count > 0) {
            this.screenshots = new Texture2D[screenshots.Count];
        }
    }

    public IEnumerator LoadThumbnail(MonoBehaviour caller, int index, Action<int, Texture2D> onSucceeded, Action<long, string> onFailed) {
        Assert.IsTrue(thumbnails != null && 0 <= index && index < thumbnails.Length);

        UnityWebRequest request = CloudVRAPIHelper.GetTexture(_owner, _thumbnailUrls[index]);
        yield return caller.StartCoroutine(CloudVRAPIHelper.HandleResponse(request,
            () => {
                if (CloudVRAPIHelper.CheckIfResponseIsOK(request.responseCode)) {
                    thumbnails[index] = DownloadHandlerTexture.GetContent(request);
                    onSucceeded.Invoke(index, thumbnails[index]);
                }
                else {
                    onFailed.Invoke(request.responseCode, request.downloadHandler.text);
                }
            },
            () => {
                onFailed.Invoke(CloudVRAPIHelper.ResponseCodeRequestError, request.error);
            }
        ));
    }

    public IEnumerator LoadScreenshot(MonoBehaviour caller, int index, Action<int, Texture2D> onSucceeded, Action<long, string> onFailed) {
        Assert.IsTrue(screenshots != null && 0 <= index && index < screenshots.Length);

        UnityWebRequest request = CloudVRAPIHelper.GetTexture(_owner, _screenshotUrls[index]);
        yield return caller.StartCoroutine(CloudVRAPIHelper.HandleResponse(request,
            () => {
                if (CloudVRAPIHelper.CheckIfResponseIsOK(request.responseCode)) {
                    screenshots[index] = DownloadHandlerTexture.GetContent(request);
                    onSucceeded.Invoke(index, screenshots[index]);
                }
                else {
                    onFailed.Invoke(request.responseCode, request.downloadHandler.text);
                }
            },
            () => {
                onFailed.Invoke(CloudVRAPIHelper.ResponseCodeRequestError, request.error);
            }
        ));
    }

    public IEnumerator RequestLinkage(MonoBehaviour caller, Action<CloudVRLinkage> onSucceeded, Action<long, string> onFailed) {
        UnityWebRequest request = CloudVRAPIHelper.Post(_owner, "/linkages", "{ \"content_id\": \"" + id + "\" }");
        yield return caller.StartCoroutine(CloudVRAPIHelper.HandleResponse(request,
            () => {
                if (CloudVRAPIHelper.CheckIfResponseIsOK(request.responseCode)) {
                    Dictionary<string, object> linkage = 
                        MiniJSON.Json.Deserialize(request.downloadHandler.text) as Dictionary<string, object>;
                    onSucceeded.Invoke(new CloudVRLinkage(linkage));
                }
                else {
                    onFailed.Invoke(request.responseCode, request.downloadHandler.text);
                }
            },
            () => {
                onFailed.Invoke(CloudVRAPIHelper.ResponseCodeRequestError, request.error);
            }
        ));
    }
}
