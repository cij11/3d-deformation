using UnityEngine;
using System.Collections;

/// <summary>
/// Control the mouse input
/// Left mouse button and drag to raise/lower terrain
/// </summary>
public class MouseManager : MonoBehaviour
{
    public GameObject particle;
    public GameObject level;

    private Vector3 clickPosInitial = new Vector3(0f, 0f, 0f);
    private bool editingTerrain = false;

    void Update()
    {
		//On pushing LMB
		if (Input.GetMouseButtonDown(0))
        {
			//If mouse over terrain, enter terrain editing mode. Store initial hit location
            RaycastHit hit;
            if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit))
            {
                editingTerrain = true;
                clickPosInitial = hit.point;

                Debug.Log("hit");
                LevelManager levelManager = level.GetComponent("LevelManager") as LevelManager;
                levelManager.LevelTerrainAtPosition(hit.point, clickPosInitial);
            }
        }

		//If LMB pressed and is still clicked, pass current mouse position to level manager to handle
		//terrain editing.
        if (editingTerrain == true)
        {
            if (Input.GetMouseButton(0))
            {
                RaycastHit hit;
                if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit))
                {
                    LevelManager levelManager = level.GetComponent("LevelManager") as LevelManager;
                    levelManager.LevelTerrainAtPosition(hit.point, clickPosInitial);
                }
                else editingTerrain = false;
            }
            else editingTerrain = false;
        }
    }
}