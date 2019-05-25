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

	private float XChange = 0;
	void Start () {
		sideTerrain = terrain.GetComponent<SideTerrain>();

		aimPointMarker = GameObject.Find("Aimpoint");

		initialBulletSpeed = (power/bulletRigidBody.mass)*Time.fixedDeltaTime;
	}
	
	// Update is called once per frame
	void Update () {
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
				return;
			}

			//Vector3 aimPoint = CalculateAimPoint(currentTarget);
			//Vector3 aimPoint = CalculateAimPointIterativeNoG(currentTarget);
			Vector3 aimPoint = CalculateAimPointIterativeG(currentTarget);
			RotateTowardsAimPoint(aimPoint);
		}

		float terrainHeight = sideTerrain.GetHeight(transform.position.x); 
		gameObject.transform.SetPositionAndRotation(new Vector3(transform.position.x, sideTerrain.GetHeight(transform.position.x), 0.0f), gameObject.transform.rotation);
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

	Vector3 CalculateAimPoint(GameObject target)
	{
		Rigidbody2D targetRb = target.GetComponent<Rigidbody2D>();

		if(targetRb == null) {
			return target.transform.position;
		}

		double a = Math.Pow(targetRb.velocity.x,2) + Math.Pow(targetRb.velocity.y, 2) - Math.Pow(initialBulletSpeed, 2);

	 	double b = 2 * (targetRb.velocity.x * (target.transform.position.x - gameObject.transform.position.x) + targetRb.velocity.y * (target.transform.position.y - gameObject.transform.position.y));
		
		double c = Math.Pow(target.transform.position.x - gameObject.transform.position.x, 2) + Math.Pow(target.transform.position.y - gameObject.transform.position.y, 2);

		double discriminant = Math.Pow(b, 2) - 4 * a * c;

		double t1 = (-b + Math.Sqrt(discriminant)) / (2 * a);
		double t2 = (-b - Math.Sqrt(discriminant)) / (2 * a);
		//Debug.Log("" + t1 + "    " + t2);
		float t = (float)Math.Max(t1, t2);

		if(t < 0) {
			return target.transform.position;
		}

		float aimX = t * targetRb.velocity.x + target.transform.position.x;
		float aimY = t * targetRb.velocity.y + target.transform.position.y;

		aimPointMarker.transform.SetPositionAndRotation(new Vector3(aimX, aimY, 0), Quaternion.identity);
		return new Vector3(aimX,  aimY, 0.0f);		

	}

	Vector3 CalculateAimPointIterativeNoG(GameObject target)
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
			previousTime = Time.time;
		}

		Rigidbody2D targetRb = target.GetComponent<Rigidbody2D>();

		
		targetHitTime = Math.Abs(Vector3.Magnitude(targetPositionEstimate - gameObject.transform.position))/initialBulletSpeed; 

		targetPositionEstimate.x = previousTargetStartPosition.x + (Time.time - previousTargetStartTime) * targetRb.velocity.x;
		targetPositionEstimate.y = previousTargetStartPosition.y + (Time.time - previousTargetStartTime) * targetRb.velocity.y;
		
		
		aimPointMarker.transform.SetPositionAndRotation(new Vector3(targetPositionEstimate.x + (targetHitTime * targetRb.velocity.x), targetPositionEstimate.y + (targetHitTime*targetRb.velocity.y), 0), Quaternion.identity);
		
		previousTime = Time.time;

		return new Vector3(aimPointMarker.transform.position.x, aimPointMarker.transform.position.y, 0f);
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

		//targetPositionEstimate.x = previousTargetStartPosition.x + (Time.time - previousTargetStartTime) * targetRb.velocity.x;
		//targetPositionEstimate.y = previousTargetStartPosition.y + (Time.time - previousTargetStartTime) * targetRb.velocity.y;

		targetPositionEstimate.x = targetPositionEstimate.x + (Time.time - previousTime) * targetRb.velocity.x;
		targetPositionEstimate.y = targetPositionEstimate.y + (Time.time - previousTime) * targetRb.velocity.y;
		
		//Debug.Log(targetRb.velocity.x);
		//float xDistance = targetPositionEstimate.x - gameObject.transform.position.x;
		//float yDistance = targetPositionEstimate.y - gameObject.transform.position.y;


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
		}
		else 
		{
			if(dotProductFromGunDirection > 0)
			{
				transform.Rotate(0f, 0f, -correctionAngle);
			}
		}

		if(correctionAngle < 1f)
		{
			Fire();
		}

		lastPositionMeasurement = currentTarget.transform.position;

	//	Debug.Log(angleFromGunDirection + " " + dotProductFromGunDirection);
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

            Vector3 diff = go.transform.position - position;
            float curDistance = diff.sqrMagnitude;
            if (curDistance < distance)
            {
                closest = go;
                distance = curDistance;
            }
        }
        return closest;
	}

	void LateUpdate()
	{
		//if(rb2d != null)
		//{	
		//	Debug.Log(rb2d.velocity.magnitude);
		//}
	}
}
