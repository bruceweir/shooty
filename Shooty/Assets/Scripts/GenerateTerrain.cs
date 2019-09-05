using System.Linq;

using System.Collections.Generic;
using UnityEngine;


//[ExecuteInEditMode]
public class GenerateTerrain : MonoBehaviour
{
    // Start is called before the first frame update
    public struct VerticesAndUVs
    {
        public Vector3[] vertices;
        public Vector2[] uvs;
    }

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
    private int startSegmentOfPlayerRunway;
    private int startSegmentOfEnemyRunway;
    private int nTerrainSegmentsForRunway;
    private float xOffset;
    private float zOffset;
    private MeshFilter meshFilter;

    private MeshCollider meshCollider;

    //private Vector3[] terrainCoordinates;
    void Start()
    {

        xOffset = UnityEngine.Random.Range(-1000f, 1000f);
        zOffset = UnityEngine.Random.Range(-1000f, 1000f);

        if(terrainSegments < 3)
        {
            terrainSegments = 3;
        }

        Vector3[] terrainCoordinates = GenerateTerrainRingCoordinates();

        terrainCoordinates = FlattenTerrainForRunways(terrainCoordinates);

        meshFilter = gameObject.GetComponent<MeshFilter>();
        
        meshFilter.mesh = CreateFlatShadingMeshFromRingCoordinates(terrainCoordinates);

        //meshFilter.mesh.RecalculateBounds();

        meshCollider = gameObject.AddComponent<MeshCollider>();
        //
        meshCollider.sharedMesh = null;
        meshCollider.sharedMesh = meshFilter.sharedMesh;

        
        AddRunways(terrainCoordinates);
        
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

        startSegmentOfPlayerRunway = Mathf.CeilToInt(0.25f * terrainSegments);
        startSegmentOfEnemyRunway = Mathf.CeilToInt(0.75f * terrainSegments);

        float heightOfPlayerRunway = terrainCoordinates[startSegmentOfPlayerRunway].y;
        float heightOfEnemyRunway = terrainCoordinates[startSegmentOfEnemyRunway].y;

        int landClearanceSegments = Mathf.CeilToInt(nTerrainSegmentsForRunway * 1.5f);

        for(int s=0; s < landClearanceSegments; s++)
        {
            terrainCoordinates[startSegmentOfPlayerRunway + s - (int)(landClearanceSegments-nTerrainSegmentsForRunway)/2].y = heightOfPlayerRunway;
            terrainCoordinates[startSegmentOfEnemyRunway + s - (int)(landClearanceSegments-nTerrainSegmentsForRunway)/2].y = heightOfEnemyRunway;
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
        
        Debug.DrawRay(new Vector3(xPos, 10000, zPos), transform.TransformDirection(Vector3.down) * 10000, Color.green);

        if (Physics.Raycast(new Vector3(xPos, 10000, zPos), transform.TransformDirection(Vector3.down), out hit, Mathf.Infinity, layerMask))
        {
            //Debug.DrawRay(new Vector3(xPos, 10000, zPos), transform.TransformDirection(Vector3.down) * 10000, Color.green);

            return 10000 - hit.distance;
        }
        else
        {
            Debug.DrawRay(new Vector3(xPos, 10000, zPos), transform.TransformDirection(Vector3.down) * 10000, Color.red);

        }

        return -1f;
    }

    VerticesAndUVs GetVertexArrayForFlatShading(Vector3[] ringCircumferenceCoordinates, float ringWidth, float lowerlevelHeight, bool relative)
    {

        VerticesAndUVs verticesAndUVs;
        verticesAndUVs.vertices = new Vector3[8 * ringCircumferenceCoordinates.Length];
        verticesAndUVs.uvs = new Vector2[8 * ringCircumferenceCoordinates.Length];

        float nCoords = (float)ringCircumferenceCoordinates.Length;
        
        for(int s=0; s < ringCircumferenceCoordinates.Length; s++)
        {
            int sOff = 8 *s;
            //near face
            //inner vertices
            
            Vector3 innerVertexNear = GetOffRingCoordinate(ringCircumferenceCoordinates[s], ringWidth/2f);

            verticesAndUVs.vertices[sOff] = new Vector3(innerVertexNear.x, innerVertexNear.y, innerVertexNear.z);
            verticesAndUVs.vertices[sOff+1] = new Vector3(innerVertexNear.x, innerVertexNear.y, innerVertexNear.z);
            
            verticesAndUVs.uvs[sOff] = new Vector2(s / nCoords, 1);
            verticesAndUVs.uvs[sOff+1] = new Vector2(s / nCoords, 1);
            
            if(relative)
            {
                verticesAndUVs.vertices[sOff+2] = new Vector3(innerVertexNear.x, innerVertexNear.y - lowerlevelHeight, innerVertexNear.z);
                verticesAndUVs.vertices[sOff+3] = new Vector3(innerVertexNear.x, innerVertexNear.y - lowerlevelHeight, innerVertexNear.z);
            }
            else
            {
                verticesAndUVs.vertices[sOff+2] = new Vector3(innerVertexNear.x, lowerlevelHeight, innerVertexNear.z);
                verticesAndUVs.vertices[sOff+3] = new Vector3(innerVertexNear.x, lowerlevelHeight, innerVertexNear.z);
            }

            verticesAndUVs.uvs[sOff] = new Vector2(s / nCoords, 1);
            verticesAndUVs.uvs[sOff+1] = new Vector2(s / nCoords, 1);
            verticesAndUVs.uvs[sOff+2] = new Vector2(s / nCoords, 1);
            verticesAndUVs.uvs[sOff+3] = new Vector2(s / nCoords, 1);
            
            //outer vertices
       
            Vector3 outerVertexNear = GetOffRingCoordinate(ringCircumferenceCoordinates[s], -ringWidth/2f);

            verticesAndUVs.vertices[sOff+4] = new Vector3(outerVertexNear.x, outerVertexNear.y, outerVertexNear.z);
            verticesAndUVs.vertices[sOff+5] = new Vector3(outerVertexNear.x, outerVertexNear.y, outerVertexNear.z);
            
            if(relative)
            {
                verticesAndUVs.vertices[sOff+6] = new Vector3(outerVertexNear.x, outerVertexNear.y - lowerlevelHeight, outerVertexNear.z);
                verticesAndUVs.vertices[sOff+7] = new Vector3(outerVertexNear.x, outerVertexNear.y - lowerlevelHeight, outerVertexNear.z);
            }
            else
            {
                verticesAndUVs.vertices[sOff+6] = new Vector3(outerVertexNear.x, lowerlevelHeight, outerVertexNear.z);
                verticesAndUVs.vertices[sOff+7] = new Vector3(outerVertexNear.x, lowerlevelHeight, outerVertexNear.z);
            }

            verticesAndUVs.uvs[sOff+4] = new Vector2(s / nCoords, 0);
            verticesAndUVs.uvs[sOff+5] = new Vector2(s / nCoords, 0);
            verticesAndUVs.uvs[sOff+6] = new Vector2(s / nCoords, 0);
            verticesAndUVs.uvs[sOff+7] = new Vector2(s / nCoords, 0);
            
        }

        return verticesAndUVs;
    }

    int[] GetTrianglesFromVerticesForFlatShading(Vector3[] vertices)
    {
        int[] triangles = new int[vertices.Length/8 * 24];
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

        return triangles;
    }

    Mesh CreateFlatShadingMeshFromRingCoordinates(Vector3[] ringCircumferenceCoordinates)
    {
        Mesh mesh = new Mesh();
        mesh.name = "TerrainRing";

        VerticesAndUVs Vuv = GetVertexArrayForFlatShading(ringCircumferenceCoordinates, ringThickness, -1f, false);
        Vector3[] vertices = Vuv.vertices;

        int[] uncappedTriangles = GetTrianglesFromVerticesForFlatShading(vertices);

        int[] triangles = new int[uncappedTriangles.Length + 24];

        uncappedTriangles.CopyTo(triangles, 0);

        int tOff = uncappedTriangles.Length;

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

    Vector3 GetOffRingCoordinate(Vector3 ringCoordinate, float distanceTowardsCentre)
    {
        Vector3 raisedVectorAtOrigin = new Vector3(0, ringCoordinate.y, 0);

        float distanceFromOrigin = (ringCoordinate-raisedVectorAtOrigin).magnitude - distanceTowardsCentre;

        Vector3 offRingCoordinate = (ringCoordinate-raisedVectorAtOrigin).normalized * distanceFromOrigin;
        
        offRingCoordinate.y = ringCoordinate.y;
        
        return offRingCoordinate;
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
            if(UnityEngine.Random.Range(0f, 1f) < decorationProbability)
            {
                //select a decoration
                int decorationIndex = UnityEngine.Random.Range(0, DecorativeGameObjects.Length);

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
        
        float distanceToEdge = UnityEngine.Random.Range(minDecorationDistanceFromCentre, edgeDistance);

        if(UnityEngine.Random.Range(0f, 1f) >= 0.5)
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

    private void AddRunways(Vector3[] terrainCoordinates)
    {
        
        Vector3[] playerRunwayRingCoordinates = new Vector3[nTerrainSegmentsForRunway];

        for(int s = 0; s < nTerrainSegmentsForRunway; s++)
        {
            Vector3 playerRunwayVector = terrainCoordinates[startSegmentOfPlayerRunway+s];

            playerRunwayRingCoordinates[s] = new Vector3(playerRunwayVector.x, playerRunwayVector.y+1, playerRunwayVector.z);
        }

        GameObject playerRunway = MakeRunwayMesh(playerRunwayRingCoordinates);
        playerRunway.name = "PlayerRunway";
        playerRunway.tag = "Runway";

        Vector3[] enemyRunwayRingCoordinates = new Vector3[nTerrainSegmentsForRunway];


        for(int s = 0; s < nTerrainSegmentsForRunway; s++)
        {
            Vector3 enemyRunwayVector = terrainCoordinates[startSegmentOfEnemyRunway+s];
            enemyRunwayRingCoordinates[s] = new Vector3(enemyRunwayVector.x, enemyRunwayVector.y+1, enemyRunwayVector.z);
        }

        GameObject enemyRunway = MakeRunwayMesh(enemyRunwayRingCoordinates);
        enemyRunway.name = "EnemyRunway";
        enemyRunway.tag = "Runway";
        
    }

    GameObject MakeRunwayMesh(Vector3[] runwayRingCoordinates)
    {
        
        VerticesAndUVs runwayVerticesAndUVs = GetVertexArrayForFlatShading(runwayRingCoordinates, runwayWidth, -1f, false);

        Vector3[] uncappedRunwayVertices = runwayVerticesAndUVs.vertices;
 
        Vector3[] cappedRunwayVertices = new Vector3[uncappedRunwayVertices.Length + 8];
        uncappedRunwayVertices.CopyTo(cappedRunwayVertices, 0);

        int cappedVoff = uncappedRunwayVertices.Length;
        
        //copy the cap end vectors and replicate them
        Vector3 vectorToCopy = Vector3.zero;

        vectorToCopy = uncappedRunwayVertices[0];
        cappedRunwayVertices[cappedVoff++] = new Vector3(vectorToCopy.x, vectorToCopy.y, vectorToCopy.z);
        vectorToCopy = uncappedRunwayVertices[2];
        cappedRunwayVertices[cappedVoff++] = new Vector3(vectorToCopy.x, vectorToCopy.y, vectorToCopy.z);
        vectorToCopy = uncappedRunwayVertices[4];
        cappedRunwayVertices[cappedVoff++] = new Vector3(vectorToCopy.x, vectorToCopy.y, vectorToCopy.z);
        vectorToCopy = uncappedRunwayVertices[6];
        cappedRunwayVertices[cappedVoff++] = new Vector3(vectorToCopy.x, vectorToCopy.y, vectorToCopy.z);
        
        vectorToCopy = uncappedRunwayVertices[uncappedRunwayVertices.Length-8];
        cappedRunwayVertices[cappedVoff++] = new Vector3(vectorToCopy.x, vectorToCopy.y, vectorToCopy.z);
        vectorToCopy = uncappedRunwayVertices[uncappedRunwayVertices.Length-6];
        cappedRunwayVertices[cappedVoff++] = new Vector3(vectorToCopy.x, vectorToCopy.y, vectorToCopy.z);
        vectorToCopy = uncappedRunwayVertices[uncappedRunwayVertices.Length-4];
        cappedRunwayVertices[cappedVoff++] = new Vector3(vectorToCopy.x, vectorToCopy.y, vectorToCopy.z);
        vectorToCopy = uncappedRunwayVertices[uncappedRunwayVertices.Length-2];
        cappedRunwayVertices[cappedVoff++] = new Vector3(vectorToCopy.x, vectorToCopy.y, vectorToCopy.z);
        

        //copy the uv coords too.
        Vector2[] uncappedRunwayUvs = runwayVerticesAndUVs.uvs;
        Vector2[] cappedRunwayUvs = new Vector2[uncappedRunwayUvs.Length + 8];

        Vector2 vector2ToCopy = Vector2.zero;

        runwayVerticesAndUVs.uvs.CopyTo(cappedRunwayUvs, 0);
        int cappedUVoff = uncappedRunwayUvs.Length;

        
        vector2ToCopy = uncappedRunwayUvs[0];
        cappedRunwayUvs[cappedUVoff++] = new Vector2(vector2ToCopy.x, vector2ToCopy.y);
        vector2ToCopy = uncappedRunwayUvs[2];
        cappedRunwayUvs[cappedUVoff++] = new Vector2(vector2ToCopy.x, vector2ToCopy.y);
        vector2ToCopy = uncappedRunwayUvs[4];
        cappedRunwayUvs[cappedUVoff++] = new Vector2(vector2ToCopy.x, vector2ToCopy.y);
        vector2ToCopy = uncappedRunwayUvs[6];
        cappedRunwayUvs[cappedUVoff++] = new Vector2(vector2ToCopy.x, vector2ToCopy.y);

        vector2ToCopy = uncappedRunwayUvs[uncappedRunwayUvs.Length-8];
        
        cappedRunwayUvs[cappedUVoff++] = new Vector2(vector2ToCopy.x, vector2ToCopy.y);
        vector2ToCopy = uncappedRunwayUvs[uncappedRunwayUvs.Length-6];
        cappedRunwayUvs[cappedUVoff++] = new Vector2(vector2ToCopy.x, vector2ToCopy.y);
        vector2ToCopy = uncappedRunwayUvs[uncappedRunwayUvs.Length-4];
        cappedRunwayUvs[cappedUVoff++] = new Vector2(vector2ToCopy.x, vector2ToCopy.y);
        vector2ToCopy = uncappedRunwayUvs[uncappedRunwayUvs.Length-2];
        cappedRunwayUvs[cappedUVoff++] = new Vector2(vector2ToCopy.x, vector2ToCopy.y);
        


        int[] uncappedRunwayTriangles = GetTrianglesFromVerticesForFlatShading(uncappedRunwayVertices);
        int[] runwayTriangles = new int[uncappedRunwayTriangles.Length + 12];

        uncappedRunwayTriangles.CopyTo(runwayTriangles, 0);

        int tOff = uncappedRunwayTriangles.Length;
        int vOff = cappedRunwayVertices.Length-8;

        runwayTriangles[tOff++] = vOff;
        runwayTriangles[tOff++] = vOff+2;
        runwayTriangles[tOff++] = vOff+1;
        
        runwayTriangles[tOff++] = vOff+1;
        runwayTriangles[tOff++] = vOff+2;
        runwayTriangles[tOff++] = vOff+3;
        
        runwayTriangles[tOff++] = vOff+4;
        runwayTriangles[tOff++] = vOff+5;
        runwayTriangles[tOff++] = vOff+6;
        
        runwayTriangles[tOff++] = vOff+5;
        runwayTriangles[tOff++] = vOff+7;
        runwayTriangles[tOff++] = vOff+6; 

        GameObject runway = Instantiate(RunwayPrefab, Vector3.zero, Quaternion.identity);

        Mesh runwayMesh = new Mesh();

        MeshFilter meshFilter = runway.GetComponent<MeshFilter>();
        
        runwayMesh.vertices = cappedRunwayVertices;
        runwayMesh.triangles = runwayTriangles;
        runwayMesh.uv = cappedRunwayUvs;

        runwayMesh.RecalculateNormals();
        runwayMesh.RecalculateTangents();
        runwayMesh.RecalculateBounds();

        meshFilter.sharedMesh = runwayMesh;

        MeshCollider runwayCollider = runway.GetComponent<MeshCollider>();
        runwayCollider.sharedMesh = runwayMesh;

        return runway;       
    }

    void OnCollision(Collision collision)
    {
        Debug.Log("Terrain OnCollision");
    }
    
}
