using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GenerateTerrain : MonoBehaviour
{
    // Start is called before the first frame update

    public float terrainRadius = 100f;
    public int terrainSegments = 50;
    public float heightScale = 10;
    public float ringDepth = 5f;
    private float xOffset;
    private float zOffset;

    private MeshFilter mf;

    private Vector3[] terrainCoordinates;
    void Start()
    {
        xOffset = Random.Range(-1000f, 1000f);
        zOffset = Random.Range(-1000f, 1000f);

        if(terrainSegments < 3)
        {
            terrainSegments = 3;
        }
        
        terrainCoordinates = GenerateTerrainRingCoordinates();

        mf = gameObject.GetComponent<MeshFilter>();
        mf.mesh = CreateMeshFromRingCoordinates(terrainCoordinates);
    }

    Vector3[] GenerateTerrainRingCoordinates()
    {
        Vector3[] terrainRingCoordinates = new Vector3[terrainSegments];

        float angleStep = 2*Mathf.PI/terrainSegments;

        float angle = 0.0f;
        
        for (int s=0; s < terrainSegments; s++)
        {
            float xPos = (Mathf.Cos(angle) * terrainRadius);
            float zPos = (Mathf.Sin(angle) * terrainRadius);

            float height = Mathf.PerlinNoise(xPos + xOffset, zPos + zOffset) * heightScale;

            terrainRingCoordinates[s] = new Vector3(xPos, height, zPos);

            angle += angleStep;

        }

        return terrainRingCoordinates;
    }

    Mesh CreateMeshFromRingCoordinates(Vector3[] ringCircumferenceCoordinates)
    {
        Mesh mesh = new Mesh();
        mesh.name = "TerrainRing";

        Vector3[] vertices = new Vector3[4 * (ringCircumferenceCoordinates.Length)];

        for (int s =0; s < ringCircumferenceCoordinates.Length; s++)
        {
            int sOff = 4 * s;

            //inner vertices
            float distanceFromOrigin = ringCircumferenceCoordinates[s].magnitude - ringDepth/2f;
            Vector3 innerVertex = ringCircumferenceCoordinates[s].normalized * distanceFromOrigin;

            //inner upper
            vertices[sOff] = new Vector3(innerVertex.x, ringCircumferenceCoordinates[s].y, innerVertex.z);

            //inner lower vertex
            vertices[sOff+1] = new Vector3(innerVertex.x, -1f, innerVertex.z);

            //outer vertices

            distanceFromOrigin = ringCircumferenceCoordinates[s].magnitude + ringDepth/2f;
            Vector3 outerVertex = ringCircumferenceCoordinates[s].normalized * distanceFromOrigin;

            //outer upper
            vertices[sOff+2] = new Vector3(outerVertex.x, ringCircumferenceCoordinates[s].y, outerVertex.z);

            //outer lower
            vertices[sOff+3] = new Vector3(outerVertex.x, -1f, outerVertex.z);

            
        }

        for(int v=0; v < vertices.Length; v++)
        {
            GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            sphere.transform.position = vertices[v];
            
        
        }

        Debug.Log("vertices array: " + vertices.Length);

        int[] triangles = new int[(3 * 8 * vertices.Length/4) + 24]; //8 triangles per 4 vertices and another 8 triangles to close the ring

        Debug.Log("triangle array: " + triangles.Length);

        int tOff=0;

        for (int v=0; v < vertices.Length-4; v+=4)
        {
            Debug.Log("v:" + v + " tOff: " + tOff);

            //upper surface
            triangles[tOff++] = v;
            triangles[tOff++] = v+4;
            triangles[tOff++] = v+2;

            triangles[tOff++] = v+2;
            triangles[tOff++] = v+4;
            triangles[tOff++] = v+6; 

            //lower surface
            triangles[tOff++] = v+1;
            triangles[tOff++] = v+3;
            triangles[tOff++] = v+5;

            triangles[tOff++] = v+3;
            triangles[tOff++] = v+7;
            triangles[tOff++] = v+5;

            //inner ring surface

            triangles[tOff++] = v;
            triangles[tOff++] = v+1;
            triangles[tOff++] = v+4;

            triangles[tOff++] = v+1;
            triangles[tOff++] = v+5;
            triangles[tOff++] = v+4;

            //outer ring surface
            triangles[tOff++] = v+2;
            triangles[tOff++] = v+6;
            triangles[tOff++] = v+3;

            triangles[tOff++] = v+3;
            triangles[tOff++] = v+6;
            triangles[tOff++] = v+7;
        }

        //close the ring
        int vrt = vertices.Length - 4;

        //upper
        triangles[tOff++] = vrt;
        triangles[tOff++] = 0;
        triangles[tOff++] = vrt+2;

        triangles[tOff++] = vrt+2;
        triangles[tOff++] = 0;
        triangles[tOff++] = 2;

        //lower
        triangles[tOff++] = vrt+1;
        triangles[tOff++] = vrt+3;
        triangles[tOff++] = 1;

        triangles[tOff++] = vrt+3;
        triangles[tOff++] = 3;
        triangles[tOff++] = 1;

        //inner 
        triangles[tOff++] = vrt;
        triangles[tOff++] = vrt+1;
        triangles[tOff++] = 0;

        triangles[tOff++] = vrt+1;
        triangles[tOff++] = 1;
        triangles[tOff++] = 0;

        //outer

        triangles[tOff++] = vrt+2;
        triangles[tOff++] = 2;
        triangles[tOff++] = vrt+3;

        triangles[tOff++] = vrt+3;
        triangles[tOff++] = 2;
        triangles[tOff++] = 3;

        
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        return mesh;
    }

    
    // Update is called once per frame
    void Update()
    {
        
    }
}
