using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour {

	// Use this for initialization
	public GameObject[] enemyTypes;

	public GameObject[] spawnPoints;
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		SpawnEnemy();
	}

	void SpawnEnemy() {
		for(int i=0; i < enemyTypes.Length; i++)
		{
			if(Random.Range(0f, 1f) < 0.02)
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
