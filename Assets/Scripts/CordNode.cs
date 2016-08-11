using UnityEngine;
using System.Collections;

/// <summary>
/// Cord node.
/// Links turrets to give a visual representation of the network
/// </summary>
public class CordNode : MonoBehaviour {
	private GameObject proximalGO;
	private GameObject distalGO;

	private float destructTimer = 0.5f;
	private bool isForDestruction = false;

	// Use this for initialization
	void Start () {
	
	}

	public void Initialise(GameObject proximalGO, GameObject distalGO){
		this.proximalGO = proximalGO;
		this.distalGO = distalGO;
	}

	//Set the distal turret (eg, the more terminal turret in the tree.)
	public void SetDistal(GameObject distalGO){
		this.distalGO = distalGO;
	}
	// Update is called once per frame
	void Update () {
		if (isForDestruction == true) {
			destructTimer -= Time.deltaTime;

			if (destructTimer < 0f) {
				if (distalGO.tag == "Cord") {
					CordNode distalNode = distalGO.GetComponent ("CordNode") as CordNode;
					distalNode.SetForDestruction ();
					Destroy (this.gameObject);
				}
			}
		}
	}

	public void SetForDestruction(){
		isForDestruction = true;
	}
}
