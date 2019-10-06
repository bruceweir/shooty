using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Direction
{
    ClockWise,
    Anticlockwise
}


[RequireComponent(typeof(GroundVehicleProperties))]
public class ControlGroundVehicle : MonoBehaviour
{
    // Start is called before the first frame update
    public GameObject pivotPrefab;
    
    public Direction direction;
    private GeneratedTerrain terrain;

    private GroundVehicleProperties groundVehicleProperties;

    private GameObject frontWheel;
    private GameObject rearWheel;
    private float axleLength;

    private GameObject groundPivot;
    private Pivot pivot;

    void Start()
    {
        terrain = GameObject.Find("Terrain").GetComponent<GeneratedTerrain>();  
        groundVehicleProperties = gameObject.GetComponent<GroundVehicleProperties>();

    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if(pivot == null || terrain == null)
        {
            return;
        }
        UpdateMovement();    
    }

    public void InitialiseGroundVehicle(float terrainSpawnAngle, Direction directionOfTravel)
    {
        if(terrain == null)
        {
            terrain = GameObject.Find("Terrain").GetComponent<GeneratedTerrain>();  
            if(terrain == null)
            {
                Debug.Log("GroundControlVehicle terrain not found");
                return;
            }
            
        }

       
        frontWheel = Utils.GetChildWithTag(gameObject, "FrontWheel");
       
        if(frontWheel == null)
        {
            Debug.Log("Ground vehicle must have a child object with the Tag 'FrontWheel");
            return;
        }

        rearWheel = Utils.GetChildWithTag(gameObject, "RearWheel");

        
        if(rearWheel == null)
        {
            Debug.Log("Ground vehicle must have a child object with the Tag 'RearWheel");
            return;
        }

        axleLength = Mathf.Abs((frontWheel.transform.position - rearWheel.transform.position).magnitude);


        groundPivot = Instantiate(pivotPrefab, Vector3.zero, Quaternion.identity);
        groundPivot.name = "GroundPivot";
        pivot = groundPivot.GetComponent<Pivot>();

        gameObject.transform.parent = groundPivot.transform;

        Vector3 startPosition = terrain.GetPositionOnTerrainSurface(terrainSpawnAngle);

        groundPivot.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);

        float startHeight = startPosition.y;

        startPosition.y = 0;

        gameObject.transform.position = startPosition;

        //pivot.transform.Rotate(Vector3.up, Mathf.Rad2Deg * terrainSpawnAngle);

        pivot.SetHeight(startHeight);

        direction = directionOfTravel;

        
    }


    //TODO replace with behaviour
    public void UpdateMovement()
    {
        
        if(direction == Direction.ClockWise)
        {
            pivot.angularVelocity = terrain.HorizontalSpeedToAngularVelocity(-groundVehicleProperties.maxSpeed);
        }
        else
        {
            pivot.angularVelocity = terrain.HorizontalSpeedToAngularVelocity(groundVehicleProperties.maxSpeed);
        }

        float currentAngle = pivot.transform.rotation.y;

        pivot.SetHeight(terrain.GetHeightOfTerrain(gameObject));

        //get height of front and rear wheels and use that to set the local rotation angle of the vehicle

        Vector3 vehicleForward = gameObject.transform.forward;
        vehicleForward.y = 0;

        float terrainHeightAtFrontWheel = terrain.GetHeightOfTerrain(gameObject.transform.position + (vehicleForward * axleLength/2));
        float terrainHeightAtRearWheel = terrain.GetHeightOfTerrain(rearWheel.transform.position - (vehicleForward * axleLength/2));

        float angle = -Mathf.Atan((terrainHeightAtFrontWheel-terrainHeightAtRearWheel)/axleLength);

        gameObject.transform.localRotation = Quaternion.Euler(Mathf.Rad2Deg * angle, 0, 0);
        
    }
}
