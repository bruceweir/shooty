using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class ControlGun : MonoBehaviour {

	// Use this for initialization
	public GameObject bullet;
	public Rigidbody2D bulletRigidBody;
	public GameObject endOfBarrel;
	public GameObject particles;
	public GameObject terrain;
	private SideTerrain sideTerrain;
	private ParticleSystem gunParticles;
	public float power = 1000f;

	public float rotationSpeed = 1f;

	public float fireRate = 2f;
	public bool trackTarget = false;
	public float minimumAccuracyToFire = 1f;
	private GameObject currentTarget = null;
	private GameObject previousTarget = null;
	private Vector3 previousTargetStartPosition;
	private float previousTargetStartTime;
	private float previousTime = 0;
	private Vector3 targetPositionEstimate = new Vector3();
	private float bulletSpeedEstimate;
	private float targetHitTime;
	private Rigidbody2D rb2d;

	private Health currentTargetHealth = null;
	private Vector3 lastPositionMeasurement;

	private float lastFiringTime = 0;
	private float initialBulletSpeed = 0;
	private GameObject aimPointMarker;

	private LayerMask mask;

	private AudioSource audio;


	private float XChange = 0;
	void Awake () {
		sideTerrain = GameObject.Find("Terrain").GetComponent<SideTerrain>();

		aimPointMarker = GameObject.Find("Aimpoint");

		initialBulletSpeed = (power/bulletRigidBody.mass)*Time.fixedDeltaTime;
		
		mask = LayerMask.GetMask("Ground");

		audio = GetComponent<AudioSource>();
	}
	
	// Update is called once per frame
	void Update () {

		if(sideTerrain == null)
		{
			sideTerrain = GameObject.Find("Terrain").GetComponent<SideTerrain>();
		}

		float terrainHeight = sideTerrain.GetHeight(transform.position.x); 
		gameObject.transform.SetPositionAndRotation(new Vector3(transform.position.x, sideTerrain.GetHeight(transform.position.x) + transform.localScale.y/2, 0.0f), gameObject.transform.rotation);

		if(Input.GetButtonDown("Jump"))
		{
			Fire();
		}	


		if(trackTarget)
		{
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
				audio.Stop();
				return;
			}

			if(!targetLineOfSightOK(currentTarget))
			{
				currentTarget = null;
				audio.Stop();
				return;
			}

			Vector3 aimPoint = CalculateAimPointIterativeG(currentTarget);
			RotateTowardsAimPoint(aimPoint);
		}

		
	}

	void FixedUpdate()
	{
		float h = Input.GetAxis("Horizontal");
		transform.Rotate(0.0f, 0.0f, -h * rotationSpeed);
	}

	public void Fire()
	{

		if(Time.time - lastFiringTime < 1.0/fireRate)
		{
			return;
		}

		GameObject b = Instantiate(bullet, endOfBarrel.transform.position, Quaternion.identity);
		
		rb2d = b.GetComponent<Rigidbody2D>();

		rb2d.AddForce(endOfBarrel.transform.up * power);
		
		Destroy(b, 3f);

		GameObject p = Instantiate(particles, endOfBarrel.transform.position, endOfBarrel.transform.rotation);
		p.transform.Rotate(270f, 0, 0, Space.Self);	

		lastFiringTime =  Time.time;

	}

	
	Vector3 CalculateAimPointIterativeG(GameObject target)
	{
		if(target == null)
		{
			return new Vector3(0f, 0f, 0f);
		}

		if(previousTarget != target)
		{
			previousTargetStartPosition = new Vector3(target.transform.position.x, target.transform.position.y, target.transform.position.z);
			previousTargetStartTime = Time.time;
			targetPositionEstimate = previousTargetStartPosition;
			previousTarget = target;
			bulletSpeedEstimate = initialBulletSpeed;
			previousTime = Time.time;
		}

		Rigidbody2D targetRb = target.GetComponent<Rigidbody2D>();
	
		targetHitTime = Math.Abs(Vector3.Magnitude(targetPositionEstimate - gameObject.transform.position))/bulletSpeedEstimate; 


		targetPositionEstimate.x = targetPositionEstimate.x + (Time.time - previousTime) * targetRb.velocity.x;
		targetPositionEstimate.y = targetPositionEstimate.y + (Time.time - previousTime) * targetRb.velocity.y;
		

		Vector3 normalisedGunUpVector = endOfBarrel.transform.up.normalized;

		bulletSpeedEstimate = initialBulletSpeed + (normalisedGunUpVector.y  * (-9.81f * targetHitTime));

		aimPointMarker.transform.SetPositionAndRotation(new Vector3(targetPositionEstimate.x + (targetHitTime * targetRb.velocity.x), targetPositionEstimate.y + (targetHitTime*targetRb.velocity.y), 0), Quaternion.identity);
		
		previousTime = Time.time;

		return new Vector3(aimPointMarker.transform.position.x, aimPointMarker.transform.position.y, 0f);
	}
	void RotateTowardsAimPoint(Vector3 aimPoint)
	{
		

		Vector3 directionToTarget = aimPoint - transform.position;

		double angleFromGunDirection = Vector3.Angle(directionToTarget, endOfBarrel.transform.up);
		
		Vector3 directionToTargetNorm = Vector3.Normalize(directionToTarget);

		Vector3 gunForwardNorm = Vector3.Normalize(endOfBarrel.transform.forward);
		
		double dotProductFromGunDirection = Vector3.Dot(gunForwardNorm, directionToTargetNorm);

		
		float correctionAngle = rotationSpeed;

		if(Math.Abs(angleFromGunDirection) < rotationSpeed)
		{
			correctionAngle = (float)angleFromGunDirection;
		}

		if(dotProductFromGunDirection < 0)
		{
			
			transform.Rotate(0f, 0f, correctionAngle);

			if(!audio.isPlaying)
			{
				audio.Play();
			}
		}
		else 
		{
			if(dotProductFromGunDirection > 0)
			{
				transform.Rotate(0f, 0f, -correctionAngle);

				if(!audio.isPlaying)
				{
					audio.Play();
				}
			}
			else
			{
				audio.Stop();
			}
		}

		//Debug.Log(correctionAngle);

		if(Math.Abs(correctionAngle) < minimumAccuracyToFire)
		{
			Fire();
		}

	}

	GameObject AcquireTarget()
	{
		GameObject closestEnemy = FindClosestEnemy();

		if(closestEnemy != null)
		{
			currentTargetHealth = closestEnemy.GetComponent<Health>();
		}

		return closestEnemy;
	}

	GameObject FindClosestEnemy() 
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
			double verticalProjectileSpeed = initialBulletSpeed * upComponent;
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

	private bool targetLineOfSightOK(Vector3 directionToTarget)
	{
		
		RaycastHit2D hit = Physics2D.Raycast(transform.position, directionToTarget, Mathf.Infinity, mask);

		if(hit.collider == null)
		{
			return true;
		}
		return false;
	}

	private bool targetLineOfSightOK(GameObject target)
	{
		Vector3 directionToTarget = target.transform.position - gameObject.transform.position;

		return targetLineOfSightOK(directionToTarget);
	}



	void LateUpdate()
	{
		//if(rb2d != null)
		//{	
		//	Debug.Log(rb2d.velocity.magnitude);
		//}
	}
}
