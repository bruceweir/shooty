using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestUtils : MonoBehaviour
{
    // Start is called before the first frame update
    public GeneratedTerrain terrain;
    public GameObject testObjectOne;
    public GameObject testObjectTwo;
    private GameObject marker;
    void Start()
    {
        marker = GameObject.CreatePrimitive(PrimitiveType.Cube);
        marker.transform.localScale = new Vector3(200, 200, 200);

//        TestHalfAngles();
//        TestCreateObjectBetween();
        //TestWithinRing();
        //test the path creation in utils

        //TestPathCreation();
        

    }

    // Update is called once per frame
    void Update()
    {
        TestHasLineOfSight();
    }

    void TestHasLineOfSight()
    {
        Debug.Log(Utils.HasLineOfSight(testObjectOne, testObjectTwo, terrain));
    }


    void TestHalfAngles()
    {
        Debug.Log("angle between: " + Utils.GetAngleBetweenPoints(testObjectOne.transform.position, testObjectTwo.transform.position, terrain));

    }

    void TestCreateObjectBetween()
    {
        float halfAngle = Utils.GetAngleBetweenPoints(testObjectOne.transform.position, testObjectTwo.transform.position, terrain) / 2f;

        Vector3 position = terrain.GetPositionOnTerrainSurface(terrain.GetAngleRoundTerrain(testObjectOne.transform.position) + halfAngle);

        marker.transform.position = position;
        
        
    }

    void TestWithinRing()
    {
        //Vector3 pointOne = terrain.GetPosition(testAngleOne);
        //Vector3 pointTwo = terrain.GetPosition(testAngleTwo);
        
        if(Utils.LineBetweenPointsWithinRing(testObjectOne.transform.position, testObjectTwo.transform.position, terrain))
        {
            Debug.Log("line between within ring");
        }
        else
        {
            Debug.Log("line between NOT within ring");
        }

        Debug.DrawLine(testObjectOne.transform.position, testObjectTwo.transform.position, Color.red, 30);
    }
    void TestPathCreation()
    {
        
        LinkedList<Vector3> path = Utils.CreatePathWithinRing(testObjectOne.transform.position, testObjectTwo.transform.position, terrain);

        Debug.Log("ring path");
        int count = 0;
        foreach(Vector3 position in path)
        {
            Debug.Log(position);
            GameObject pathMarker = GameObject.CreatePrimitive(PrimitiveType.Cube);
            pathMarker.transform.position = position;
            pathMarker.transform.localScale = new Vector3(10, 10, 10);
            pathMarker.name = "pathMarker " + count.ToString();
            count++;
        }

    }

    
}
