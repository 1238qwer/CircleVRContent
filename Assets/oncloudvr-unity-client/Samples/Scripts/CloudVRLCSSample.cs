using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Canvas))]

public class CloudVRLCSSample : MonoBehaviour {
	private Text _title;
	private Text _description;
	private RawImage _thumbnail;
	private RawImage[] _screenshots;

	private CloudVRClient _cloudVrClient; 

	[SerializeField] private string _hostname;
	[SerializeField] private int _port;

	// handle engine events
	private void Awake() {
		_title = transform.Find("Title").GetComponent<Text>();
		_description = transform.Find("Description").GetComponent<Text>();
		_thumbnail = transform.Find("Thumbnail").GetComponentInChildren<RawImage>();
		_screenshots = transform.Find("Screenshots").GetComponentsInChildren<RawImage>();
		
		_cloudVrClient = new CloudVRClient(_hostname, _port);
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
				
				StartCoroutine(contents[0].RequestLinkage(this, (linkage) => {
					Debug.Log(string.Format("Linkage created : {0}:{1}", linkage.host, linkage.port.ToString()));
				}, (errorCode, error) => {
					Debug.Log(string.Format("Linkage creation failed : {0} : {1}", errorCode.ToString(), error));
				}));
			}, 
			(errorCode, error) => {
				Debug.Log(string.Format("LoadContentsList failed : {0} : {1}", errorCode.ToString(), error));
			}
		));
	}
}
