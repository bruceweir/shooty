using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour {

	// Use this for initialization
	public GameObject[] enemyTypes;

	public GameObject[] spawnPoints;
	public double spawnPropability = 0.02;
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		SpawnEnemy();
	}

	void SpawnEnemy() {
		for(int i=0; i < enemyTypes.Length; i++)
		{
			if(Random.Range(0f, 1f) < spawnPropability)
			{
				int spawnPointIndex = Random.Range(0, spawnPoints.Length);
				SpawnChecker sC = spawnPoints[spawnPointIndex].GetComponent<SpawnChecker>();

				if(sC.SpawnClear())
				{
					Instantiate(enemyTypes[i], sC.transform.position, Quaternion.identity);
				}
			}
		}
	}
}
