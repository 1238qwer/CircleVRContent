using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CloudVRGCSSample : MonoBehaviour {
	private Text _title;
	private Text _description;
	private RawImage _thumbnail;
	private RawImage[] _screenshots;

	private CloudVRClient _cloudVrClient;
	private CloudVRCommandChannel _commandChannel;

	[SerializeField] private string _hostname;
	[SerializeField] private int _port;

	// handle engine events
	private void Awake() {
		_title = transform.Find("Title").GetComponent<Text>();
		_description = transform.Find("Description").GetComponent<Text>();
		_thumbnail = transform.Find("Thumbnail").GetComponentInChildren<RawImage>();
		_screenshots = transform.Find("Screenshots").GetComponentsInChildren<RawImage>();
		
		_cloudVrClient = new CloudVRClient(_hostname, _port);
		_commandChannel = new CloudVRCommandChannel(_cloudVrClient, "content");
		_commandChannel.CommandReceived += onCloudVRCommandReceived;
	}

	private void Start() {
		StartCoroutine(_cloudVrClient.LoadContentsList(this, 
			(contents) => {
				if (contents == null) {
					Debug.Log("No content exists.");
					return;
				}
	
				_title.text = contents[0].title;
				_description.text = contents[0].description;
				StartCoroutine(contents[0].LoadThumbnail(this, 0, (index, thumbnail) => {
					_thumbnail.texture = thumbnail;
				}, (errorCode, error) => {
					Debug.Log(string.Format("LoadThumbnail failed : {0} : {1}", errorCode.ToString(), error));
				}));
	
				for (int i = 0; i < Mathf.Min(_screenshots.Length, contents[0].screenshots.Length); i++) {
					StartCoroutine(contents[0].LoadScreenshot(this, i, (index, screenshot) => {
						_screenshots[index].texture = screenshot;
					}, (errorCode, error) => {
						Debug.Log(string.Format("LoadScreenshot {3} failed : {0} : {1}", errorCode.ToString(), error, i));
					}));
				}
			}, 
			(errorCode, error) => {
				Debug.Log(string.Format("LoadContentsList failed : {0} : {1}", errorCode.ToString(), error));
			}
		));
		
		StartCoroutine(_cloudVrClient.GetTrackers(this, 
			(trackers) => {
				foreach (var tracker in trackers) {
					Debug.Log(string.Format("Tracker : {0} : {1}", tracker.userID, tracker.trackerID));
				}
			},
			(errorCode, error) => {
				Debug.Log(string.Format("GetTrackers failed : {0} : {1}", errorCode.ToString(), error));
			}
		));
	}

	private void Update() {
		if (Input.GetKeyDown(KeyCode.L)) {
			StartCoroutine(_cloudVrClient.RequestGroupLinkage(this,
				(linkage) => {
					Debug.Log(string.Format("Group Linkage : {0}:{1}", linkage.host, linkage.port));
				},
				(errorCode, error) => {
					Debug.Log(string.Format("RequestGroupLinkage failed : {0} : {1}", errorCode.ToString(), error));
				}
			));
		}
		else if (Input.GetKeyDown(KeyCode.C)) {
			if (_commandChannel.opened == false) {
				_commandChannel.Open();
				Debug.Log("CloudVR CommandChannel opened");
			}
			else {
				_commandChannel.Close();
				Debug.Log("CloudVR CommandChannel closed");
			}
		}
		
		_commandChannel.Update(Time.deltaTime);
	}
	
	// handle CloudVRCommandChannel commands
	private void onCloudVRCommandReceived(CloudVRCommandChannel channel, string command) {
		Debug.Log("CloudVR Command received : " + command);
	}
}
