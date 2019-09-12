using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestUtils : MonoBehaviour
{
    // Start is called before the first frame update
    public GeneratedTerrain terrain;
    public GameObject testObjectOne;
    public GameObject testObjectTwo;
    void Start()
    {
        TestHalfAngles();
        TestCreateObjectBetween();
        //TestWithinRing();
        //test the path creation in utils

//        TestPathCreation();
        

    }

    void TestHalfAngles()
    {
        Debug.Log("angle between: " + Utils.GetAngleBetweenPoints(testObjectOne.transform.position, testObjectTwo.transform.position, terrain));

    }

    void TestCreateObjectBetween()
    {
        float halfAngle = Utils.GetAngleBetweenPoints(testObjectOne.transform.position, testObjectTwo.transform.position, terrain) / 2f;

        Vector3 position = terrain.GetPosition(terrain.GetAngleRoundTerrain(testObjectOne.transform.position) + halfAngle);

        GameObject halfway = GameObject.CreatePrimitive(PrimitiveType.Cube);
        halfway.transform.position = position;
        halfway.transform.localScale = new Vector3(200, 200, 200);
        halfway.name = "halfway";

        
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
        //Vector3 testPositionOne = terrain.GetPosition(testAngleOne);
        //Vector3 testPositionTwo = terrain.GetPosition(testAngleTwo);
        

        LinkedList<Vector3> path = Utils.CreatePathWithinRing(testObjectOne.transform.position, testObjectTwo.transform.position, terrain);

        Debug.Log("ring path");
        foreach(Vector3 position in path)
        {
            Debug.Log(position);
        }

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
