using UnityEngine;
using System.Collections;

/// <summary>
/// Cord layer.
/// Leave a trail of cord nodes behing an airborn unit
/// </summary>
public class CordLayer : MonoBehaviour {
    public float cordSpacing;
    public GameObject cordNodePrefab;

	private GameObject lastObjectPlaced;
    private float distanceCovered = 0f;
    private Vector3 previousPosition;
    private bool airborn = true;

	// Use this for initialization
	void Start () {
		Unit thisUnit = this.transform.gameObject.GetComponent ("Unit") as Unit;
		lastObjectPlaced = thisUnit.GetUnitParent ();

        previousPosition = this.transform.position;
	}
	
	// Update is called once per frame
	void FixedUpdate () {
        if (airborn == true)
        {
            Vector3 currentPosition = this.transform.position;
			Vector3 trailingVector = previousPosition - currentPosition;
			trailingVector.Normalize ();
			trailingVector = trailingVector * 10f;
            float xDist = this.transform.position.x - previousPosition.x;
            float zDist = this.transform.position.z - previousPosition.z;

			float rookDistance = Mathf.Sqrt(xDist * xDist + zDist * zDist);
            distanceCovered += rookDistance;
            if (distanceCovered > cordSpacing)
            {
				SpawnCordNode ();
            }
			previousPosition = currentPosition;
        }
	}
	private void SpawnCordNode(){
		distanceCovered = 0;

		Vector3 currentPosition = this.transform.position;
		Vector3 trailingVector = previousPosition - currentPosition;
		trailingVector.Normalize ();
		trailingVector = trailingVector * 2f;
		GameObject newNode = Instantiate(cordNodePrefab, this.transform.position + trailingVector, this.transform.rotation) as GameObject;
		CordNode cordNode = newNode.GetComponent ("CordNode") as CordNode;
		cordNode.Initialise (lastObjectPlaced, this.transform.gameObject);

		//Update the last object placed to point at this gameObject
		if (lastObjectPlaced.tag == "Cord") {
			CordNode lastPlacedNode = lastObjectPlaced.GetComponent ("CordNode") as CordNode;
			lastPlacedNode.SetDistal (newNode);
		}
		lastObjectPlaced = newNode;
	}
}