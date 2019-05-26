using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public class TargetTracker : MonoBehaviour {

	private GameObject currentTarget = null;
	private GameObject previousTarget = null;
	private Vector3 previousTargetStartPosition;
	private float previousTargetStartTime;
	private float previousTime = 0;
	private Vector3 targetPositionEstimate = new Vector3();
	private float projectileSpeedEstimate;
	private float initialProjectileSpeed = 0;
	private float targetHitTime;
	private Rigidbody2D rb2d;
	Health currentTargetHealth;
	private GameObject weaponBarrel;
	
	private LayerMask mask;
	// Use this for initialization
	void Start () {
		mask = LayerMask.GetMask("Ground");
	}
	
	public void SetTrackingParameters(float shotPower, float projectileMass, GameObject weaponBarrel)
	{
		initialProjectileSpeed = shotPower/projectileMass * Time.fixedDeltaTime;
		this.weaponBarrel = weaponBarrel;
	}

	public GameObject AcquireTarget()
	{
		GameObject closestEnemy = FindClosestEnemy();

		if(closestEnemy != null)
		{
			currentTargetHealth = closestEnemy.GetComponent<Health>();
		}

		return closestEnemy;
	}

	public Vector3 GetAimPoint(out bool tracking)
	{
		if(initialProjectileSpeed == 0) 
		{
			Debug.Log("TargetTracker Tracking Parameters not set");
			tracking = false;
			return new Vector3(0, 0, 0);
		}

		if(currentTargetHealth != null && currentTargetHealth.IsDestroyed())
		{
			currentTarget = AcquireTarget();
		}
		
		if(currentTarget == null)
		{
			currentTarget = AcquireTarget();
		}

		if(currentTarget == null)
		{
			tracking = false;
			return new Vector3(0, 0, 0);
		}

		tracking = true;
		return CalculateAimPointIterativeG(currentTarget);

	}
	private GameObject FindClosestEnemy() 
	{
 		GameObject[] gos;
        gos = GameObject.FindGameObjectsWithTag("Enemy");

		if(gos.Length == 0)
		{
			return null;
		}

        GameObject closest = null;
        float distance = Mathf.Infinity;
        Vector3 position = transform.position;
        foreach (GameObject go in gos)
        {
			Health health = go.GetComponent<Health>();
			if(health.IsDestroyed())
			{
				continue;
			}

            Vector3 directionToTarget = go.transform.position - position;

			//check that target is in range
			Vector3 directionNormalised = directionToTarget.normalized;

			double upComponent = directionNormalised.y;
			double verticalProjectileSpeed = initialProjectileSpeed * upComponent;
			double maxAttainableProjectileHeight = gameObject.transform.position.y + -Math.Pow(verticalProjectileSpeed, 2) / (2 *-9.81);

			if(maxAttainableProjectileHeight < go.transform.position.y)
			{
				continue;
			}

			//check that the ground (layer #12) is not in the way

			if(!targetLineOfSightOK(directionToTarget))
			{
				continue;
			}

            float curDistance = directionToTarget.sqrMagnitude;
            if (curDistance < distance)
            {
                closest = go;
                distance = curDistance;
            }
        }
        return closest;
	}

	private bool targetLineOfSightOK(GameObject target)
	{
		Vector3 directionToTarget = target.transform.position - gameObject.transform.position;

		return targetLineOfSightOK(directionToTarget);
	}

	private bool targetLineOfSightOK(Vector3 directionToTarget)
	{
		
		RaycastHit2D hit = Physics2D.Raycast(transform.position, directionToTarget, Mathf.Infinity, mask);

		if(hit.collider == null)
		{
			return true;
		}
		return false;
	}

	Vector3 CalculateAimPointIterativeG(GameObject target)
	{
		
		if(previousTarget != target)
		{
			previousTargetStartPosition = new Vector3(target.transform.position.x, target.transform.position.y, target.transform.position.z);
			previousTargetStartTime = Time.time;
			targetPositionEstimate = previousTargetStartPosition;
			previousTarget = target;
			projectileSpeedEstimate = initialProjectileSpeed;
			previousTime = Time.time;
		}

		Rigidbody2D targetRb = target.GetComponent<Rigidbody2D>();
	
		targetHitTime = Math.Abs(Vector3.Magnitude(targetPositionEstimate - gameObject.transform.position))/projectileSpeedEstimate; 


		targetPositionEstimate.x = targetPositionEstimate.x + (Time.time - previousTime) * targetRb.velocity.x;
		targetPositionEstimate.y = targetPositionEstimate.y + (Time.time - previousTime) * targetRb.velocity.y;	

		Vector3 normalisedGunUpVector = weaponBarrel.transform.up.normalized;

		projectileSpeedEstimate = initialProjectileSpeed + (normalisedGunUpVector.y  * (-9.81f * targetHitTime));
		
		previousTime = Time.time;

		return new Vector3(targetPositionEstimate.x + (targetHitTime * targetRb.velocity.x), targetPositionEstimate.y + (targetHitTime*targetRb.velocity.y), 0);
	}

	public GameObject GetCurrentlyTrackedTarget()
	{
		return currentTarget;
	}

}
