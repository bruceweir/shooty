using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class GroundVehiclePivotControl : MonoBehaviour
{
    private GeneratedTerrain terrain;
    private GameObject attachedGroundVehicle;
    private GameObject frontWheel;
    private GameObject rearWheel;
    private float axleLength;

    private GroundVehicleProperties vehicleProperties;
 
    // Start is called before the first frame update
    void Start()
    {
        terrain = GameObject.Find("Terrain").GetComponent<GeneratedTerrain>();
        attachedGroundVehicle = gameObject.transform.GetChild(0).gameObject;
        vehicleProperties = attachedGroundVehicle.GetComponent<GroundVehicleProperties>();

        frontWheel = GetChildWithTag("FrontWheel");
        rearWheel = GetChildWithTag("RearWheel");

        axleLength = Math.Abs((frontWheel.transform.position - rearWheel.transform.position).magnitude);

        Debug.Log("vehicle axle length: " + axleLength);
    }

    GameObject GetChildWithTag(string tag)
    {
        foreach(Transform child in attachedGroundVehicle.transform)
        {
            if(child.gameObject.tag == tag)
            {
                return child.gameObject;
            }
        }

        return null;
    }

    void FixedUpdate()
    {
        gameObject.transform.Rotate(new Vector3(0, vehicleProperties.angularVelocity * Time.fixedDeltaTime, 0), Space.Self);


        //adjust pivot height
        gameObject.transform.position = new Vector3(0, terrain.GetHeightOfTerrain(Mathf.Deg2Rad * -gameObject.transform.rotation.eulerAngles.y), 0);
        
        //get height of front and rear wheels and use that to set the local rotation angle of the vehicle

        Vector3 vehicleForward = attachedGroundVehicle.transform.forward;
        vehicleForward.y = 0;

        float terrainHeightAtFrontWheel = terrain.GetHeightOfTerrain(attachedGroundVehicle.transform.position + (vehicleForward * axleLength/2));
        float terrainHeightAtRearWheel = terrain.GetHeightOfTerrain(rearWheel.transform.position - (vehicleForward * axleLength/2));

        float angle = -Mathf.Atan((terrainHeightAtFrontWheel-terrainHeightAtRearWheel)/axleLength);

        attachedGroundVehicle.transform.localRotation = Quaternion.Euler(Mathf.Rad2Deg * angle, 0, 0);
        
    }
}
