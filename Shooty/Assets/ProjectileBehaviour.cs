using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileBehaviour : MonoBehaviour
{
    // Start is called before the first frame update
    public GameObject pivotPrefab;
    public float speed;
    public float angleOfAttack;
    public float lifetime;
    public float startAngleAroundTerrain;
    public float startHeight;
    public Vector3 initialForward;
    private GeneratedTerrain terrain;
    private Pivot pivot;
    
    
    void Start()
    {
        terrain = GameObject.Find("Terrain").GetComponent<GeneratedTerrain>();
        
        Vector3 startPosition = terrain.GetPositionOnTerrainSurface(startAngleAroundTerrain);
        startPosition.y = 0;
        gameObject.transform.position = startPosition;
        gameObject.transform.LookAt(startPosition + initialForward);
        
        GameObject pp = Instantiate(pivotPrefab, Vector3.zero, Quaternion.identity);
        gameObject.transform.parent = pp.transform;
        pp.transform.position = new Vector3(0, startHeight, 0);

        pivot = pp.GetComponent<Pivot>();


        Destroy(pp, lifetime);
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        Vector2 projectileVelocity;

        projectileVelocity.x = Mathf.Cos(Mathf.Deg2Rad * angleOfAttack) * speed;
        projectileVelocity.y = -Mathf.Sin(Mathf.Deg2Rad * angleOfAttack) * speed;

        float angularVelocity = 360/((terrain.terrainRadius * 2 * Mathf.PI) / -projectileVelocity.x);

        pivot.angularVelocity = angularVelocity;

        pivot.AdjustHeight(projectileVelocity.y * Time.fixedDeltaTime);

    }
}
