using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    // Start is called before the first frame update
    public GameObject projectilePrefab;
    public GameObject muzzleFlash;
    public GameObject projectileOrigin;
    public float rateOfFire;
    private float nextShotTime;

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
        
        //GameObject projectile = Instantiate(projectilePrefab, projectileOrigin.transform.position, projectileOrigin.transform.rotation);
        GameObject projectile = Instantiate(projectilePrefab, Vector3.zero, Quaternion.identity);
        
        ProjectileBehaviour pb = projectile.GetComponent<ProjectileBehaviour>();

        pb.angleOfAttack = -Mathf.Rad2Deg * CalcAngleOfAttack();
        pb.startAngleAroundTerrain = terrain.GetAngleRoundTerrain(projectileOrigin.transform.position);
        pb.initialForward = projectileOrigin.transform.forward;
        pb.startHeight = projectileOrigin.transform.position.y;

        GameObject flash = Instantiate(muzzleFlash, projectileOrigin.transform.position, projectileOrigin.transform.rotation);
        flash.transform.parent = gameObject.transform;
        
        shotNoise.Play();

        nextShotTime = Time.realtimeSinceStartup + 1/rateOfFire;

    }
}
