using UnityEngine;
using System.Collections;

/// <summary>
/// Cube spawner.
/// Stress test for physics system. Spawn a large number
/// of generic physical cube objects
/// </summary>
public class CubeSpawner : MonoBehaviour {
	public int numCubes = 10;
	public GameObject GO;

	// Use this for initialization
	void Start () {
		for (int i = 0; i < numCubes; i++) {
			Instantiate (GO, new Vector3 (5f + (i * 1.1f), 10f, 20f), Quaternion.identity);
		}
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
