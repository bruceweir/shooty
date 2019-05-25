using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public class SideTerrain : MonoBehaviour {

	// Use this for initialization
	public MeshFilter meshFilter;
	public Mesh mesh;

	public int smoothingPasses = 1;
	public int minHeight = 1;
	public int maxHeight = 30;
	public int terrainLength = 100;
	public int terrainSmoothingRadius = 2;

	public int verticesPerMeter = 1;
	public bool destructableTerrain = false;

	float[] heightmap;

	private	PolygonCollider2D polygonCollider;

	

	void Start() {
		
	}
	void Awake () {
		mesh = new Mesh();
		mesh.name = "Terrain mesh";
		meshFilter.mesh = mesh;

		terrainLength *= verticesPerMeter;

		heightmap = new float[terrainLength];

		for(int i=0; i < terrainLength; i++)
		{
			heightmap[i] = UnityEngine.Random.Range(minHeight, maxHeight);
		}

		for(int i=0; i < smoothingPasses; i++) {
			Smooth();
		}
		
		polygonCollider = gameObject.AddComponent<PolygonCollider2D>();
		
		BuildMesh();

	}
	
	// Update is called once per frame
	void Smooth()
	{
		for(int i=0; i < terrainLength; i++)
		{
			float height = heightmap[i];
			float heightSum = 0;
			float heightCount = 0;

			for(int n=i-terrainSmoothingRadius; n < i+terrainSmoothingRadius+1; n++)
			{
				if(n >= 0 && n < heightmap.Length)
				{
					float heightOfNeighbour = heightmap[n];
					heightSum += heightOfNeighbour;
					heightCount++;
				}
			}

			float averageHeight = heightSum/heightCount;

			heightmap[i] = averageHeight;
		}
	}

	void BuildMesh()
	{
		mesh.Clear();
		List<Vector3> positions = new List<Vector3>();
		List<Vector2> positionsForPolygonCollider = new List<Vector2>();
		
		List<int> triangles = new List<int>();

		int offset = 0;

		positionsForPolygonCollider.Add(new Vector2(0, 0));
		
		float step = 1f/verticesPerMeter;

		for(int i=0; i < terrainLength-1; i++)
		{
			offset = i*4;

			float h = heightmap[i];
			float hn = heightmap[i+1];

			float xPos = (float)i * step;

			positions.Add(new Vector3(xPos+0, 0, 0));
			positions.Add(new Vector3(xPos+step, 0, 0));
			positions.Add(new Vector3(xPos+0, h, 0));
			positions.Add(new Vector3(xPos+step, hn, 0));
			
			triangles.Add(offset + 0);
			triangles.Add(offset + 2);
			triangles.Add(offset + 1);
			
			triangles.Add(offset + 1);
			triangles.Add(offset + 2);
			triangles.Add(offset + 3);
			
			if(i % 4 == 0)
			{
				positionsForPolygonCollider.Add(new Vector2(xPos+0, h));
			} 
			else 
			{ 
				if( i==terrainLength-2 )
				{
					positionsForPolygonCollider.Add(new Vector2(xPos+step, hn));
				}
			}
			
		}

		positionsForPolygonCollider.Add(new Vector2(terrainLength-1, 0));
		

		mesh.vertices = positions.ToArray();
		mesh.triangles = triangles.ToArray();
		mesh.RecalculateNormals();

		polygonCollider.points = positionsForPolygonCollider.ToArray();
		
	}

	void OnCollisionEnter2D(Collision2D other)
	{
		if(!destructableTerrain)
		{
			return;
		}
		
		if(other.gameObject.CompareTag("Enemy"))
		{
			Rocket rocket = other.gameObject.GetComponent<Rocket>();
			DamageGround(other.gameObject.transform.position.x, rocket.blastRadius);
		}
	}

	void DamageGround(float positionX, float radius)
	{

		int damageRadius = (int)radius;
		
		int centreIndexOfDamage = (int)(positionX - transform.position.x) * verticesPerMeter;
		int leftIndexOfDamage = centreIndexOfDamage-(damageRadius * verticesPerMeter);
		int rightIndexOfDamage = centreIndexOfDamage+(damageRadius * verticesPerMeter);

		for(int i = leftIndexOfDamage; i < rightIndexOfDamage; i++)
		{
			if (i < 0 || i > terrainLength-1) {
				continue;
			}

			double distanceFromCentre = (double)Math.Abs(centreIndexOfDamage - i) / (double)verticesPerMeter;
			
			double angle = Math.Acos(distanceFromCentre/radius);
			
			double damageAtPoint = radius * Math.Sin(angle);
			//Debug.Log("" + i + " " + distanceFromCentre + " " + angle + " " + damageAtPoint);

			heightmap[i] -= (float)damageAtPoint;

			if(heightmap[i] < 1)
			{
				heightmap[i] = 1;
			}
		}

		BuildMesh();
	}

	public float GetHeight(float x)
	{
		
		float worldPosX = x - transform.position.x;
		int index = (int)worldPosX * (int)verticesPerMeter;

		if(index < 0 || index >= terrainLength)
		{
			return 0f;
		}

		return heightmap[index] + transform.position.y;
	}
}
