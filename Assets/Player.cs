using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour {

    [SerializeField] private float moveSpeed;
    [SerializeField] private GameObject rig;
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		if (Input.GetKey(KeyCode.W))
        {
            transform.Translate(rig.transform.forward * moveSpeed * Time.deltaTime);
            //transform.Translate(0, 0, moveSpeed * Time.deltaTime);
        }
        if (Input.GetKey(KeyCode.S))
        {
            transform.Translate(-rig.transform.forward * moveSpeed * Time.deltaTime);
            //transform.Translate(0, 0, -moveSpeed * Time.deltaTime);
        }
    }
}
