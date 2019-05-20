using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class ControlGun : MonoBehaviour {

	// Use this for initialization
	public GameObject bullet;
	public GameObject endOfBarrel;
	public GameObject particles;
	public GameObject terrain;
	private SideTerrain sideTerrain;
	private ParticleSystem gunParticles;
	public float power = 1000f;

	public float rotationSpeed = 1f;

	public bool trackTarget = false;
	private GameObject currentTarget = null;
	private Rigidbody2D rb2d;

	private Vector3 lastPositionMeasurement;

	void Start () {
		sideTerrain = terrain.GetComponent<SideTerrain>();
	}
	
	// Update is called once per frame
	void Update () {
		if(Input.GetButtonDown("Jump"))
		{
			Fire();
		}	

		if(trackTarget)
		{
			RotateTowardsCurrentTarget();
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
		GameObject b = Instantiate(bullet, endOfBarrel.transform.position, Quaternion.identity);
		
		rb2d = b.GetComponent<Rigidbody2D>();

		rb2d.AddForce(endOfBarrel.transform.up * power);
		
		Destroy(b, 3f);

		GameObject p = Instantiate(particles, endOfBarrel.transform.position, endOfBarrel.transform.rotation);
		p.transform.Rotate(270f, 0, 0, Space.Self);	
	}

	void RotateTowardsCurrentTarget()
	{
		if(currentTarget == null)
		{

			currentTarget = AcquireTarget();
		}

		if(currentTarget == null)
		{
			return;
		}

		Vector3 currentTargetPosition = currentTarget.transform.position;

		Vector3 directionToTarget = currentTarget.transform.position - transform.position;

		float distanceToTarget = directionToTarget.magnitude;

		double angleFromGunDirection = Vector3.Angle(directionToTarget, endOfBarrel.transform.up);

		float verticalComponent = endOfBarrel.transform.up.y;

		double verticalDistance = currentTarget.transform.position.y - transform.position.y;

		double initialVerticalSpeed = power * verticalComponent;

		double bulletSpeedAtTargetHeight = Math.Sqrt(Math.Pow(initialVerticalSpeed, 2.0) + (2 * -9.81 * verticalDistance));

		Debug.Log(verticalDistance);
		
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

		lastPositionMeasurement = currentTarget.transform.position;

	//	Debug.Log(angleFromGunDirection + " " + dotProductFromGunDirection);
	}

	GameObject AcquireTarget()
	{
		Debug.Log("AcquireTarget");

		GameObject closestEnemy = FindClosestEnemy();

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
}
