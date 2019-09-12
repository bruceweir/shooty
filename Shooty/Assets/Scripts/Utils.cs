using System.Collections;
using System.Collections.Generic;
//using System.Collections.Generic.LinkedList;
using UnityEngine;

public static class Utils
{
    // Start is called before the first frame update
    public static bool HasLineOfSight(GameObject one, GameObject two, GeneratedTerrain terrain)
    {
        LinkedList<Vector3> path = CreatePathWithinRing(one.transform.position, two.transform.position, terrain);


        return true;
    }

    public static LinkedList<Vector3> CreatePathWithinRing(Vector3 start, Vector3 end, GeneratedTerrain terrain)
    {
        LinkedList<Vector3> path = new LinkedList<Vector3>();

        path.AddFirst(start);
        path.AddLast(end);

        //check if point is visible to its next neighbour, if not insert a new point
        LinkedListNode<Vector3> step = path.First;

        while(step.Next != null)
        {
            if(LineBetweenPointsWithinRing(step.Value, step.Next.Value, terrain))
            {
                step = step.Next;
            }
            else
            {
                float halfAngle = GetAngleBetweenPoints(step.Value, step.Next.Value, terrain) / 2;
                Vector3 inbetweenPoint = terrain.GetPosition(halfAngle);
                path.AddAfter(step, inbetweenPoint);
            }

            if(path.Count > 100)
            {
                Debug.Log("path too long");
                return path;
            }

        }

        return path;
    }

    public static bool LineBetweenPointsWithinRing(Vector3 pointOne, Vector3 pointTwo, GeneratedTerrain terrain)
    {
        //cos(t) = a/h
        //a = h * cos(t)

        //hyp = ring radius

        float halfAngle = GetAngleBetweenPoints(pointOne, pointTwo, terrain) / 2f;

        float adj = terrain.terrainRadius * Mathf.Cos(halfAngle);

        if(adj > terrain.terrainRadius - (terrain.ringThickness/2f))
        {
            return true;
        }

        return false;
    }

    public static float GetAngleBetweenPoints(Vector3 one, Vector3 two, GeneratedTerrain terrain)
    {
        float angleOne = terrain.GetAngleRoundTerrain(one);
        float angleTwo = terrain.GetAngleRoundTerrain(two);

        Debug.Log("angleOne: " + angleOne + " angleTwo: " + angleTwo);
        return angleTwo - angleOne;
    }
}
