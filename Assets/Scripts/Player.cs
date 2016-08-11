using UnityEngine;
using System.Collections;

/// <summary>
/// Player.
/// Controller class.
/// Controls the current Unit using WASD and space.
/// Focal point for the camera.
/// </summary>
public class Player : MonoBehaviour {

    public enum Mode { idle, firing, transit};

    public GameObject startingUnit;

    private Mode mode = Mode.idle;
    private GameObject currentUnit = null;
    private Unit currentUnitScript;
    private Unit destinationUnitScript = null;
    private Vector3 startPosition;
    private Vector3 endPosition; 
    float lerpTime = 1f;
    float currentLerpTime = 0f;
    // Use this for initialization
    void Start () {
        currentUnit = startingUnit;
        currentUnitScript = currentUnit.GetComponent("Unit") as Unit;
	}
	
	// Update is called once per frame
	void Update () {
        this.transform.position = currentUnit.transform.position;
	    switch (mode)
        {
            case Mode.idle:
                {
                    if (Input.GetKey("a")) currentUnitScript.ChangeAimingAngle(-4f);
                    if (Input.GetKey("d")) currentUnitScript.ChangeAimingAngle(4f);
                    if (Input.GetKey("w"))
                    {
                        //    if (currentUnitScript.GetChildUnit(0) != null) MountUnit(currentUnitScript.GetChildUnit(0));
                            if (currentUnitScript.GetChildUnit(0) != null) MountUnit(currentUnitScript.GetUnitNearestCurrentAngle());
                            else if (currentUnitScript.GetUnitParent()!=null) MountUnit(currentUnitScript.GetUnitParent());
                        // MountUnit(currentUnitScript.GetChildUnitNearestCurrentAngle());
                    }
                    if (Input.GetKey("s"))
                    {
                        if (currentUnitScript.GetUnitParent() != null) MountUnit(currentUnitScript.GetUnitParent());
                    }
                    if (Input.GetKeyDown("space"))
                    {
                        mode = Mode.firing;
                    }
                    break;
                }
            case Mode.transit:
                {
                    currentLerpTime += Time.deltaTime;
                    if (currentLerpTime > lerpTime)
                    {
                        mode = Mode.idle;
                    }
                    endPosition = currentUnit.transform.position;
                    this.transform.position = Vector3.Lerp(startPosition, endPosition, currentLerpTime);

                    if (Input.GetKey("a")) currentUnitScript.ChangeAimingAngle(-4f);
                    if (Input.GetKey("d")) currentUnitScript.ChangeAimingAngle(4f);
                    break;
                }
            case Mode.firing:
                {
                    if (Input.GetKey("a")) currentUnitScript.ChangeAimingAngle(-4f);
                    if (Input.GetKey("d")) currentUnitScript.ChangeAimingAngle(4f);
                    if (Input.GetKey("space")) currentUnitScript.ChargeUnit(1000f);
                    if (Input.GetKeyUp("space"))
                    {
                        currentUnitScript.FireUnit();
                        mode = Mode.idle;
                    }
                    break;
                }
        }
    }
    
    private void MountUnit(GameObject unitToMount)
    {
        mode = Mode.transit;
        startPosition = this.transform.position;
        endPosition = unitToMount.transform.position;
        currentLerpTime = 0f;
        currentUnit = unitToMount;
        currentUnitScript = currentUnit.GetComponent("Unit") as Unit;
    }
}
