using UnityEngine;
using System.Collections;

/// <summary>
/// Camera rotation lock.
/// Fix the camera rotation independent of the player rotation
/// </summary>
public class CameraRotationLock : MonoBehaviour {
	Vector3 fixedOffset;

	// Use this for initialization
	void Start () {
		fixedOffset =  this.transform.position - this.transform.parent.position;
	}
	
	// Update is called once per frame
	void Update () {
		//Quaternion rot = Quaternion.identity;
	//	this.transform.rotation = Quaternion.Euler (45f, 45f, 0f);
		this.transform.position = this.transform.parent.position + fixedOffset;

	}
}
