using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    // Start is called before the first frame update
    public GameObject projectilePivotPrefab;
    public float projectileSpeed;
    public float rateOfFire;
    private float nextShotTime;

    private float angleOfAttack;
    private GeneratedTerrain terrain;

    private AudioSource shotNoise;

    void Start()
    {
        terrain = GameObject.Find("Terrain").GetComponent<GeneratedTerrain>();
        shotNoise = GetComponent<AudioSource>();
    }


    void Update()
    {
        
    }

    private float CalcAngleOfAttack()
    {
        Vector3 toPivot = gameObject.transform.position - gameObject.transform.root.transform.position;

        float X = Vector3.Cross(gameObject.transform.forward, toPivot).normalized.y;
        float Y = Vector3.Dot(gameObject.transform.forward, Vector3.up);

        return Mathf.Atan2(Y, X);

    }
    public void Fire()
    {
        if(Time.realtimeSinceStartup < nextShotTime)
        {
            return;
        }
        

        GameObject ppp = Instantiate(projectilePivotPrefab, Vector3.zero, Quaternion.identity);

        ProjectilePivot p = ppp.GetComponent<ProjectilePivot>();

        p.angleOfAttack = -Mathf.Rad2Deg * CalcAngleOfAttack();
        p.startHeight = gameObject.transform.position.y;
        p.startAngleAroundTerrain = terrain.GetAngleRoundTerrain(gameObject.transform.position);
        p.initialForward = gameObject.transform.forward;
        p.projectileSpeed = projectileSpeed;

        shotNoise.Play();

        nextShotTime = Time.realtimeSinceStartup + 1/rateOfFire;

    }
}
