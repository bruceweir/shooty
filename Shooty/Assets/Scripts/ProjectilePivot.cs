using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectilePivot : MonoBehaviour
{
    // Start is called before the first frame update
    public float projectileSpeed = 600;
    public float angleOfAttack;
    public float startHeight;
    public float startAngleAroundTerrain;
    public float lifetime = 8;
    public GeneratedTerrain terrain;
    public Vector3 initialForward;
    public GameObject projectile;
    void Start()
    {
        gameObject.transform.position = new Vector3(0, 0, 0);
        gameObject.transform.rotation = Quaternion.identity;

        Vector3 startPosition = terrain.GetPositionOnTerrainSurface(startAngleAroundTerrain);
        gameObject.transform.SetPositionAndRotation(new Vector3(0, 0, 0), Quaternion.identity);
        startPosition.y = 0;
        

        GameObject p = Instantiate(projectile, startPosition, Quaternion.identity);
        p.transform.parent = gameObject.transform;
        p.transform.LookAt(p.transform.position + initialForward);
     
        gameObject.transform.position = new Vector3(0, startHeight, 0);

        Destroy(gameObject, lifetime);
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        Vector2 projectileVelocity;

        projectileVelocity.x = Mathf.Cos(Mathf.Deg2Rad * angleOfAttack) * projectileSpeed;
        projectileVelocity.y = -Mathf.Sin(Mathf.Deg2Rad * angleOfAttack) * projectileSpeed;
        
        float angularVelocity = 360/((terrain.terrainRadius * 2 * Mathf.PI) / -projectileVelocity.x);

        gameObject.transform.Rotate(0, angularVelocity * Time.fixedDeltaTime, 0, Space.World);

        Vector3 position = gameObject.transform.position;

        position.y += projectileVelocity.y * Time.fixedDeltaTime;

        gameObject.transform.position = position;

    }
}
