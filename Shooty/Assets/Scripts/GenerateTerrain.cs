using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class GenerateTerrain : MonoBehaviour
{
    // Start is called before the first frame update

    public float terrainRadius = 100f;
    public int terrainSegments = 50;
    public float perlinScale = 1f;
    public float heightScale = 10;
    public float ringThickness = 5f;
    public bool showVertices = false;
    private float xOffset;
    private float zOffset;

    private float[,] perlinSamples;

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
        
        mf.mesh = CreateFlatShadingMeshFromRingCoordinates(terrainCoordinates);
    }

    

    Vector3[] GenerateTerrainRingCoordinates()
    {
        Vector3[] terrainRingCoordinates = new Vector3[terrainSegments];

        float angleStep = 2*Mathf.PI/terrainSegments;

        float angle = 0.0f;

        
        for (int s=0; s < terrainSegments; s++)
        {
            float xPos = (Mathf.Cos(angle));
            float zPos = (Mathf.Sin(angle));

            float height = GetHeight(xPos, zPos);

            terrainRingCoordinates[s] = new Vector3(xPos*terrainRadius, height, zPos*terrainRadius);

            angle += angleStep;

        }

        return terrainRingCoordinates;
    }

    public float GetHeight(float xPos, float zPos)
    {
        float height = Mathf.PerlinNoise((xPos + xOffset)*perlinScale, (zPos + zOffset)*perlinScale) * heightScale;

        return height;
    }

    Mesh CreateFlatShadingMeshFromRingCoordinates(Vector3[] ringCircumferenceCoordinates)
    {
        Mesh mesh = new Mesh();
        mesh.name = "TerrainRing";

        Vector3[] vertices = new Vector3[16 * ringCircumferenceCoordinates.Length];

        Debug.Log("vertices: " + vertices.Length);

        
        for(int s=0; s < ringCircumferenceCoordinates.Length-1; s++)
        {
            int sOff = 16 *s;
            //near face
            //inner vertices
            float distanceFromOriginInnerNear = ringCircumferenceCoordinates[s].magnitude - ringThickness/2f;
            Vector3 innerVertexNear = ringCircumferenceCoordinates[s].normalized * distanceFromOriginInnerNear;
            vertices[sOff] = new Vector3(innerVertexNear.x, ringCircumferenceCoordinates[s].y, innerVertexNear.z);
            vertices[sOff+1] = new Vector3(innerVertexNear.x, -1f, innerVertexNear.z);

            //outer vertices
            float distanceFromOriginOuterNear = ringCircumferenceCoordinates[s].magnitude + ringThickness/2f;
            Vector3 outerVertexNear = ringCircumferenceCoordinates[s].normalized * distanceFromOriginOuterNear;
            vertices[sOff+2] = new Vector3(outerVertexNear.x, ringCircumferenceCoordinates[s].y, outerVertexNear.z);
            vertices[sOff+3] = new Vector3(outerVertexNear.x, -1f, outerVertexNear.z);

            //far
            //inner vertices
            float distanceFromOriginInnerFar = ringCircumferenceCoordinates[s+1].magnitude - ringThickness/2f;
            Vector3 innerVertexFar = ringCircumferenceCoordinates[s+1].normalized * distanceFromOriginInnerFar;
            vertices[sOff+4] = new Vector3(innerVertexFar.x, ringCircumferenceCoordinates[s+1].y, innerVertexFar.z);
            vertices[sOff+5] = new Vector3(innerVertexFar.x, -1f, innerVertexFar.z);

            //outer vertices
            float distanceFromOriginOuterFar = ringCircumferenceCoordinates[s+1].magnitude + ringThickness/2f;
            Vector3 outerVertexFar = ringCircumferenceCoordinates[s+1].normalized * distanceFromOriginOuterFar;
            vertices[sOff+6] = new Vector3(outerVertexFar.x, ringCircumferenceCoordinates[s+1].y, outerVertexFar.z);
            vertices[sOff+7] = new Vector3(outerVertexFar.x, -1f, outerVertexFar.z);

            //near
            //inner face vertices

            vertices[sOff+8] = new Vector3(innerVertexNear.x, ringCircumferenceCoordinates[s].y, innerVertexNear.z);
            vertices[sOff+9] = new Vector3(innerVertexNear.x, -1f, innerVertexNear.z);
            vertices[sOff+10] = new Vector3(outerVertexNear.x, ringCircumferenceCoordinates[s].y, outerVertexNear.z);
            vertices[sOff+11] = new Vector3(outerVertexNear.x, -1f, outerVertexNear.z);
            
            //far
            //outer face vertices

            vertices[sOff+12] = new Vector3(innerVertexFar.x, ringCircumferenceCoordinates[s+1].y, innerVertexFar.z);
            vertices[sOff+13] = new Vector3(innerVertexFar.x, -1f, innerVertexFar.z);
            vertices[sOff+14] = new Vector3(outerVertexFar.x, ringCircumferenceCoordinates[s+1].y, outerVertexFar.z);
            vertices[sOff+15] = new Vector3(outerVertexFar.x, -1f, outerVertexFar.z);
            

        }

        if(showVertices)
        {
            for(int v=0; v < vertices.Length; v++)
            {
                GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                sphere.transform.position = vertices[v];    
            }
        }

        int[] triangles = new int[vertices.Length/2 * 24 + 24];

        Debug.Log("triangles: " + triangles.Length);

        int tOff = 0;

        for(int v=0; v < vertices.Length; v+=16)
        {
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

            //inner face

            triangles[tOff++] = v+8;
            triangles[tOff++] = v+9;
            triangles[tOff++] = v+12;

            triangles[tOff++] = v+9;
            triangles[tOff++] = v+13;
            triangles[tOff++] = v+12;

            //outer face
            triangles[tOff++] = v+10;
            triangles[tOff++] = v+14;
            triangles[tOff++] = v+11;
           
            triangles[tOff++] = v+11;
            triangles[tOff++] = v+14;
            triangles[tOff++] = v+15;


        }

        //close ring
    ///////////////////////////////////////s
        int vOff = vertices.Length - 16;
        //upper surface
        triangles[tOff++] = vOff+12;
        triangles[tOff++] = 0;
        triangles[tOff++] = vOff+14;
        Debug.Log("" + vertices[vOff+12] + vertices[0] + vertices[vOff+14]);
        
//        triangles[tOff++] = 2;
//        triangles[tOff++] = vOff+4;
//        triangles[tOff++] = 2;




        mesh.vertices = vertices;
        mesh.triangles = triangles;

        mesh.RecalculateNormals();
        return mesh;
    }

    Mesh CreateMeshFromRingCoordinates(Vector3[] ringCircumferenceCoordinates)
    {
        Mesh mesh = new Mesh();
        mesh.name = "TerrainRing";

        Vector3[] vertices = new Vector3[4 * ringCircumferenceCoordinates.Length];
        Vector3[] normals = new Vector3[vertices.Length];
        Color32[] colors = new Color32[vertices.Length];
        

        for (int s =0; s < ringCircumferenceCoordinates.Length; s++)
        {
            int sOff = 4 * s;

            //inner vertices
            float distanceFromOrigin = ringCircumferenceCoordinates[s].magnitude - ringThickness/2f;
            Vector3 innerVertex = ringCircumferenceCoordinates[s].normalized * distanceFromOrigin;

            //inner upper
            vertices[sOff] = new Vector3(innerVertex.x, ringCircumferenceCoordinates[s].y, innerVertex.z);
            normals[sOff] = new Vector3(0, 1, 0);
            colors[sOff] = Color.Lerp(Color.green, Color.white, ringCircumferenceCoordinates[s].y/heightScale);

            //inner lower vertex
            vertices[sOff+1] = new Vector3(innerVertex.x, -1f, innerVertex.z);
            normals[sOff+1] = innerVertex.normalized;
            
            colors[sOff+1] = Color.yellow;//Color.Lerp(Color.green, Color.white, ringCircumferenceCoordinates[s].y/heightScale);

            //outer vertices

            distanceFromOrigin = ringCircumferenceCoordinates[s].magnitude + ringThickness/2f;
            Vector3 outerVertex = ringCircumferenceCoordinates[s].normalized * distanceFromOrigin;

            
            //outer upper
            vertices[sOff+2] = new Vector3(outerVertex.x, ringCircumferenceCoordinates[s].y, outerVertex.z);
            normals[sOff+2] = new Vector3(0, 1, 0);
            colors[sOff+2] = Color.Lerp(Color.green, Color.white, ringCircumferenceCoordinates[s].y/heightScale);
            //outer lower
            vertices[sOff+3] = new Vector3(outerVertex.x, -1f, outerVertex.z);
            normals[sOff+3] = outerVertex.normalized;
            
            colors[sOff+3] = Color.yellow;//Color.Lerp(Color.green, Color.white, ringCircumferenceCoordinates[s].y/heightScale);
        }

        if(showVertices)
        {
            for(int v=0; v < vertices.Length; v++)
            {
                GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                sphere.transform.position = vertices[v];    
            }
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
        mesh.colors32 = colors;
        mesh.normals = normals;

        mesh.RecalculateNormals();
        mesh.RecalculateTangents();

        return mesh;
    }

    
    // Update is called once per frame
    void Update()
    {
        
    }
}
