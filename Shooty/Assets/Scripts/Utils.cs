using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public static class Utils
{
    // Start is called before the first frame update
    public static bool HasLineOfSight(GameObject one, GameObject two, GeneratedTerrain terrain)
    {
        LinkedList<Vector3> path = CreatePathWithinRing(one.transform.position, two.transform.position, terrain);

        if(path.Count == 0)
        {
            return false;
        }

        LinkedListNode<Vector3> step = path.First;
        
        int layerMask = (1 << 12) | (1 << 15); //terrain and cloud
        RaycastHit hit;
            
        while(step.Next != null)
        {
            //check if the terrain is in between these steps on the path
            Vector3 vectorToNext = step.Next.Value - step.Value;
            if (Physics.Raycast(step.Value, step.Next.Value - step.Value, out hit, vectorToNext.magnitude, layerMask))
            {
                return false;
            }
            step = step.Next;
        }

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
                Vector3 inbetweenPoint = terrain.GetPositionOnTerrainSurface(terrain.GetAngleRoundTerrain(step.Value) + halfAngle);
                
                //set height of new point to be average of neighbours (since they will be evenly spaced)
                inbetweenPoint.y = (step.Value.y + step.Next.Value.y)/2f;

                path.AddAfter(step, inbetweenPoint);
            }

            if(path.Count > 100)
            {
                Debug.Log("path too long");
                path.Clear();
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

        float angleBetween = angleTwo - angleOne;

        if(Math.Abs(angleBetween) > Mathf.PI)
        {
            angleBetween = (angleBetween - Mathf.PI) - Mathf.PI;
        }

        return angleBetween;
    }

    /* Returns the first child of an object with the specified tag, null otherwise */
    public static GameObject GetChildWithTag(GameObject go, string tag)
    {
        foreach(Transform child in go.transform)
        {
            GameObject recurse = GetChildWithTag(child.gameObject, tag);
            if(recurse != null)
            {
                return recurse;
            }
            
            if(child.gameObject.tag == tag)
            {
                return child.gameObject;
            }
        }

        return null;
    }

    /* Returns the first child of an object with the specified name, null otherwise */
    
    public static GameObject GetChildWithName(GameObject go, string name)
    {
        foreach(Transform child in go.transform)
        {
            GameObject recurse = GetChildWithName(child.gameObject, name);
            
            if(recurse != null)
            {
                return recurse;
            }

            if(child.gameObject.name == name)
            {
                return child.gameObject;
            }
        }

        return null;
    }
}
