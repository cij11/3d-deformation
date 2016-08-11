using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Unit.
/// Units are turrets. Units function like golf players, but instead
/// of launching golf balls, they launch other units.
/// 
/// A/S keys rotate the unit
/// W/S move to the next/previous unit
/// Hold 'space' to launch a unit
/// </summary>
public class Unit : MonoBehaviour {

	public GameObject unitPrefab;

	private bool activeUnit = true;
	private float aimingAngle;
	private GameObject parentUnit = null;
	private Unit parentUnitScript = null;
    private List<GameObject> childUnits;
    private float shotMaxPower = 1000f;
    private float shotMinPower = 100f;
    private float shotPower = 500f;

	// Use this for initialization
	void Start () {
        childUnits = new List<GameObject>();
	}
	
	// Update is called once per frame
	void Update () {
		this.transform.GetChild (0).transform.localRotation = Quaternion.Euler (0.0f, aimingAngle, 0.0f);

		

		LineRenderer lineRenderer = this.transform.GetChild (1).gameObject.GetComponent<LineRenderer> () as LineRenderer;
		lineRenderer.SetPosition (0, this.transform.position);
		if (parentUnit == null) {
			lineRenderer.SetPosition (1, this.transform.position);
		} else {
			lineRenderer.SetPosition (1, this.parentUnit.transform.position);
		}
	}

    public void ChargeUnit(float chargeRate)
    {
        shotPower += Time.deltaTime * chargeRate;
        if (shotPower > shotMaxPower) shotPower = shotMaxPower;
    }
	public void FireUnit(){
		Quaternion launchQuat = Quaternion.AngleAxis (aimingAngle, Vector3.up);
		Vector3 launchUnitVector = launchQuat * Vector3.forward;
		launchUnitVector.y = 1.5f;
		GameObject launchedUnit = Instantiate (unitPrefab, this.transform.position + launchUnitVector*3f, this.transform.rotation) as GameObject;
		Unit childUnit = launchedUnit.GetComponent ("Unit") as Unit;
		childUnit.SetParentUnit (this.gameObject);
		childUnit.SetFiringAngle (this.aimingAngle);
		Rigidbody rigidBody = launchedUnit.GetComponent<Rigidbody>() as Rigidbody;
		rigidBody.AddForce(launchUnitVector * shotPower);

        childUnits.Add(launchedUnit);
        shotPower = shotMinPower;
	}

	public void ChangeAimingAngle(float delta){
		aimingAngle += delta;
		if (delta > 360) delta = delta-360;
		if (delta < 0) delta = 360+ delta;
	}

	public void SetParentUnit(GameObject parentUnit){
		this.parentUnit = parentUnit;
		parentUnitScript = parentUnit.GetComponent("Unit") as Unit;
	}
	public void SetFiringAngle(float aimingAngle){
		this.aimingAngle = aimingAngle;
	}

	public void ActivateParent(){
		this.activeUnit = false;
		parentUnitScript.Activate ();
	}

    public GameObject GetUnitParent()
    {
        return parentUnit;
    }
    public Unit GetUnitParentScript()
    {
        return parentUnitScript;
    }

    public GameObject GetChildUnit(int childIndex)
    {
        if (childIndex < childUnits.Count)
        {
            return childUnits[childIndex];
        }
        else return null;
    }
    public Unit getChildUnitScript(int childIndex)
    {
        if (childIndex < childUnits.Count)
        {
            GameObject childAtIndex =  childUnits[childIndex];
            Unit childScript = childAtIndex.GetComponent("Unit") as Unit;
            return childScript;
        }
        else return null;
    }

    public GameObject GetUnitNearestCurrentAngle()
    {
        List<GameObject> destinationUnits = new List<GameObject>();
        if (parentUnit != null) destinationUnits.Add(parentUnit);
        if (childUnits.Count != 0) destinationUnits.AddRange(childUnits);

        if (destinationUnits.Count == 0) return null;

        //Make a vector pointing in the current direction of aim
        Quaternion launchQuat = Quaternion.AngleAxis(aimingAngle, Vector3.up);
        Vector3 aimingVector = launchQuat * Vector3.forward;

        float smallestAngle = 360f;
        GameObject nearestUnit = childUnits[0];

        foreach (GameObject unit in destinationUnits)
        {
            Vector3 vectorToChild = unit.transform.position - this.transform.position;
            float angleToChild = Vector3.AngleBetween(aimingVector, vectorToChild);
            if (angleToChild < smallestAngle)
            {
                smallestAngle = angleToChild;
                nearestUnit = unit;
            }

        }
        return nearestUnit;
    }

	public void Activate(){
		activeUnit = true;
	}
		
}
