using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VehicleSpawner : MonoBehaviour
{
    // Start is called before the first frame update

    public GameObject GroundPivotPrefab;

    public GameObject[] enemyVehicles;

    
    private GeneratedTerrain terrain;

    
    void Start()
    {
        terrain = GameObject.Find("Terrain").GetComponent<GeneratedTerrain>();

        SpawnEnemy();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void SpawnEnemy()
    {
        if(enemyVehicles.Length == 0)
        {
            return;
        }

        Debug.Log("Spawn enemy TODO Add spawn locations to terrain");

        float angleOfSpawnPoint = 0;// terrain.GetAngleRoundTerrain(new Vector3(100, 0, 0));

        Vector3 spawnPosition = terrain.GetPositionOnTerrainSurface(0);

        GameObject enemyVehicle = Instantiate(enemyVehicles[0], new Vector3(spawnPosition.x, 0, spawnPosition.z), Quaternion.identity);
        GameObject pivot = Instantiate(GroundPivotPrefab, new Vector3(0, 0, 0), Quaternion.identity);

        enemyVehicle.transform.parent = pivot.transform;

        //pivot.transform.rotation = Quaternion.Euler(0, angleOfSpawnPoint, 0);

    }
}
