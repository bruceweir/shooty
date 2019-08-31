using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//[ExecuteInEditMode]
public class GenerateTerrain : MonoBehaviour
{
    // Start is called before the first frame update

    public float terrainRadius = 100f;
    public int terrainSegments = 50;
    public float perlinScale = 1f;
    public float heightScale = 10;
    public float ringThickness = 5f;
    public GameObject[] DecorativeGameObjects;
    public float decorationProbability = .3f;
    public float minDecorationDistance = 10f;
    public float minDecorationDistanceFromCentre = 2.0f;
    public float runwayLength = 50f;
    public float runwayWidth = 5f;
    public GameObject RunwayPrefab;
    private int startOfPlayerRunway;
    private int startOfEnemyRunway;
    private int nTerrainSegmentsForRunway;
    private float xOffset;
    private float zOffset;
    private MeshFilter mf;

    private MeshCollider meshCollider;

    //private Vector3[] terrainCoordinates;
    void Start()
    {
        xOffset = Random.Range(-1000f, 1000f);
        zOffset = Random.Range(-1000f, 1000f);

        if(terrainSegments < 3)
        {
            terrainSegments = 3;
        }

        Vector3[] terrainCoordinates = GenerateTerrainRingCoordinates();

        terrainCoordinates = FlattenTerrainForRunways(terrainCoordinates);

        mf = gameObject.GetComponent<MeshFilter>();
        
        mf.sharedMesh = CreateFlatShadingMeshFromRingCoordinates(terrainCoordinates);

        meshCollider = gameObject.GetComponent<MeshCollider>();
        meshCollider.sharedMesh = null;
        meshCollider.sharedMesh = mf.sharedMesh;

        AddRunways();
        
        AddDecorations();
        
        
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

            float height = GetPerlinValue(xPos, zPos);
            
            terrainRingCoordinates[s] = new Vector3(xPos*terrainRadius, height, zPos*terrainRadius);

            angle += angleStep;
        }

        return terrainRingCoordinates;
    }

    Vector3[] FlattenTerrainForRunways(Vector3[] terrainCoordinates)
    {

        float totalTerrainLength = terrainRadius * 2 * Mathf.PI;
        float proportion = runwayLength / totalTerrainLength;

        nTerrainSegmentsForRunway = Mathf.CeilToInt(Mathf.Max(1.0f, terrainSegments * proportion));

        startOfPlayerRunway = Mathf.CeilToInt(0.25f * terrainSegments);
        startOfEnemyRunway = Mathf.CeilToInt(0.75f * terrainSegments);

        float heightOfPlayerRunway = terrainCoordinates[startOfPlayerRunway].y;
        float heightOfEnemyRunway = terrainCoordinates[startOfEnemyRunway].y;

        for(int s=0; s < nTerrainSegmentsForRunway; s++)
        {
            terrainCoordinates[startOfPlayerRunway + s].y = heightOfPlayerRunway;
            terrainCoordinates[startOfEnemyRunway + s].y = heightOfEnemyRunway;
        }

        //apply filter to avoid large discontinuities due to runways
        float filterCoeff = 0.5f;

        ApplyFilter(0, terrainSegments, 1);

        return terrainCoordinates;


        //////////Local method//////////////////
        void ApplyFilter(int filterStartIndex, int filterSteps, int step)
        {
            float previousSample = terrainCoordinates[filterStartIndex].y;

            for(int s = 0; s < filterSteps; s+=step)
            {
                int offset = (filterStartIndex + s + 1) % terrainSegments;
                float currentSample = terrainCoordinates[offset].y;

                terrainCoordinates[offset].y = (filterCoeff * previousSample) + ((1-filterCoeff)*currentSample);

                previousSample = terrainCoordinates[offset].y;
            }

            return;
        }

    }

    public Vector3 GetPosition(float angleAroundTerrain)
    {
        float xPos = Mathf.Cos(angleAroundTerrain) * terrainRadius;
        float zPos = Mathf.Sin(angleAroundTerrain) * terrainRadius;
        float height = GetHeightOfTerrain(xPos, zPos);

        return new Vector3(xPos, height, zPos);
    }

    public float GetAngleRoundTerrain(Vector3 position)
    {
        return Mathf.Atan2(position.z, position.x);
    }

    public float GetPerlinValue(float xPos, float zPos)
    {
        float height = Mathf.PerlinNoise((xPos + xOffset)*perlinScale, (zPos + zOffset)*perlinScale) * heightScale;

        height = Mathf.Max(0, height);
        return height;
    }

    public float GetHeightOfTerrain(Vector3 position)
    {
        return GetHeightOfTerrain(position.x, position.z);
    }

    public float GetHeightOfTerrain(float angle)
    {
        Vector3 position = GetPosition(angle);

        return position.y;
    }
    public float GetHeightOfTerrain(float xPos, float zPos)
    {
        int layerMask = 1 << 12;
        RaycastHit hit;
        // Does the ray intersect the ground
        
        if (Physics.Raycast(new Vector3(xPos, 10000, zPos), transform.TransformDirection(Vector3.down), out hit, Mathf.Infinity, layerMask))
        {
            Debug.DrawRay(new Vector3(xPos, 10000, zPos), transform.TransformDirection(Vector3.down) * 10000, Color.green);

            return 10000 - hit.distance;
        }
        else
        {
            Debug.DrawRay(new Vector3(xPos, 10000, zPos), transform.TransformDirection(Vector3.down) * 10000, Color.red);

        }

        return -1f;
    }

    Mesh CreateFlatShadingMeshFromRingCoordinates(Vector3[] ringCircumferenceCoordinates)
    {
        Mesh mesh = new Mesh();
        mesh.name = "TerrainRing";

        Vector3[] vertices = new Vector3[8 * ringCircumferenceCoordinates.Length];

        Debug.Log("vertices: " + vertices.Length);

        
        for(int s=0; s < ringCircumferenceCoordinates.Length; s++)
        {
            int sOff = 8 *s;
            //near face
            //inner vertices
            float distanceFromOriginInnerNear = ringCircumferenceCoordinates[s].magnitude - ringThickness/2f;
            Vector3 innerVertexNear = ringCircumferenceCoordinates[s].normalized * distanceFromOriginInnerNear;
            vertices[sOff] = new Vector3(innerVertexNear.x, ringCircumferenceCoordinates[s].y, innerVertexNear.z);
            vertices[sOff+1] = new Vector3(innerVertexNear.x, ringCircumferenceCoordinates[s].y, innerVertexNear.z);
            
            vertices[sOff+2] = new Vector3(innerVertexNear.x, -1f, innerVertexNear.z);
            vertices[sOff+3] = new Vector3(innerVertexNear.x, -1f, innerVertexNear.z);
            
            //outer vertices
            float distanceFromOriginOuterNear = ringCircumferenceCoordinates[s].magnitude + ringThickness/2f;
            Vector3 outerVertexNear = ringCircumferenceCoordinates[s].normalized * distanceFromOriginOuterNear;
            vertices[sOff+4] = new Vector3(outerVertexNear.x, ringCircumferenceCoordinates[s].y, outerVertexNear.z);
            vertices[sOff+5] = new Vector3(outerVertexNear.x, ringCircumferenceCoordinates[s].y, outerVertexNear.z);
            
            vertices[sOff+6] = new Vector3(outerVertexNear.x, -1f, outerVertexNear.z);
            vertices[sOff+7] = new Vector3(outerVertexNear.x, -1f, outerVertexNear.z);

        }

        int[] triangles = new int[vertices.Length/2 * 24 + 24];

        Debug.Log("triangles: " + triangles.Length);

        int tOff = 0;

        for(int v=0; v < vertices.Length-8; v+=8)
        {
            //upper surface
            triangles[tOff++] = v;
            triangles[tOff++] = v+8;
            triangles[tOff++] = v+4;
            
            triangles[tOff++] = v+4;
            triangles[tOff++] = v+8;
            triangles[tOff++] = v+12;

            //lower surface
            triangles[tOff++] = v+2;
            triangles[tOff++] = v+6;
            triangles[tOff++] = v+10;

            triangles[tOff++] = v+6;
            triangles[tOff++] = v+14;
            triangles[tOff++] = v+10;

            //inner face

            triangles[tOff++] = v+1;
            triangles[tOff++] = v+3;
            triangles[tOff++] = v+11;

            triangles[tOff++] = v+1;
            triangles[tOff++] = v+11;
            triangles[tOff++] = v+9;
 
            //outer face
            triangles[tOff++] = v+5;
            triangles[tOff++] = v+13;
            triangles[tOff++] = v+7;
           
            triangles[tOff++] = v+7;
            triangles[tOff++] = v+13;
            triangles[tOff++] = v+15;


        }

            //close ring
        ///////////////////////////////////////s
        int vOff = vertices.Length - 16;
            
        //upper
        triangles[tOff++] = vOff+8;
        triangles[tOff++] = 0;
        triangles[tOff++] = vOff+12;

        triangles[tOff++] = 0;
        triangles[tOff++] = 4;
        triangles[tOff++] = vOff+12;

        //lower
        triangles[tOff++] = vOff+14;
        triangles[tOff++] = 2;
        triangles[tOff++] = vOff+10;

        triangles[tOff++] = 6;
        triangles[tOff++] = 2;
        triangles[tOff++] = vOff+14;

        //inner
        triangles[tOff++] = vOff+11;
        triangles[tOff++] = 1;
        triangles[tOff++] = vOff+9;

        triangles[tOff++] = vOff+11;
        triangles[tOff++] = 3;
        triangles[tOff++] = 1;

        //outer
        triangles[tOff++] = vOff+13;
        triangles[tOff++] = 5;
        triangles[tOff++] = vOff+15;

        triangles[tOff++] = vOff+15;
        triangles[tOff++] = 5;
        triangles[tOff++] = 7;
        
        mesh.vertices = vertices;
        mesh.triangles = triangles;

        mesh.RecalculateNormals();
        mesh.RecalculateTangents();
        mesh.RecalculateBounds();

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

        int[] triangles = new int[(3 * 8 * vertices.Length/4) + 24]; //8 triangles per 4 vertices and another 8 triangles to close the ring


        int tOff=0;

        for (int v=0; v < vertices.Length-4; v+=4)
        {
  
            //upper surface
            triangles[tOff++] = v;
            triangles[tOff++] = v+2;
            triangles[tOff++] = v+4;

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

    void AddDecorations()
    {
        float terrainCircumference = 2 * Mathf.PI * terrainRadius;

        float maxNumberOfDecorations = terrainCircumference / minDecorationDistance;

        float angleStep = 2 * Mathf.PI / maxNumberOfDecorations;

        float angle = 0.0f;

        while(angle < 2*Mathf.PI)
        {
            if(Random.Range(0f, 1f) < decorationProbability)
            {
                //select a decoration
                int decorationIndex = Random.Range(0, DecorativeGameObjects.Length);

                Vector3 decorationPosition = GetSuitableDecorationPosition(angle);

                Instantiate(DecorativeGameObjects[decorationIndex], decorationPosition, Quaternion.identity);
            }

            angle += angleStep;
        }

    }

    Vector3 GetSuitableDecorationPosition(float angle)
    {
        Vector3 ringPosition = GetPosition(angle);

        Vector3 directionToCentre = (ringPosition - new Vector3(0, ringPosition.y, 0)).normalized;

        float edgeDistance = ringThickness/2;
        
        float distanceToEdge = Random.Range(minDecorationDistanceFromCentre, edgeDistance);

        if(Random.Range(0f, 1f) >= 0.5)
        {
            //inner
            ringPosition -= directionToCentre * distanceToEdge;

        }
        else
        {
            //outer
            ringPosition += directionToCentre * distanceToEdge;

        }

        return ringPosition;
    }

    private void AddRunways()
    {
        GameObject playerRunway = Instantiate(RunwayPrefab, Vector3.zero, Quaternion.identity);

        Mesh playerRunwayMesh = new Mesh();

        Vector3[] playerRunwayVertices = new Vector3[nTerrainSegmentsForRunway * 4];
        int[] playerRunwayTriangles = new int[nTerrainSegmentsForRunway * 8];
        MeshFilter playerMF = playerRunway.GetComponent<MeshFilter>();

        playerMF.sharedMesh = playerRunwayMesh;
    }
}
