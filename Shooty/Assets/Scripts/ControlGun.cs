using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[RequireComponent(typeof(TargetTracker))]
public class ControlGun : MonoBehaviour {

	// Use this for initialization
	public GameObject ammo;
	public Rigidbody2D ammoRigidBody;
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

	public GameObject explosion;
	private TargetTracker targetTracker;
	private Rigidbody2D rb2d;

	private Health currentTargetHealth = null;
	
	private float lastFiringTime = 0;
	private float initialAmmoSpeed = 0;
	private GameObject aimPointMarker;

	private LayerMask mask;

	private AudioSource audio;

	void Awake () {
		sideTerrain = GameObject.Find("Terrain").GetComponent<SideTerrain>();

		aimPointMarker = GameObject.Find("Aimpoint");

		initialAmmoSpeed = (power/ammoRigidBody.mass)*Time.fixedDeltaTime;
		
		mask = LayerMask.GetMask("Ground");

		audio = GetComponent<AudioSource>();

		targetTracker = GetComponent<TargetTracker>();

		}
	
	void Start() 
	{
		targetTracker.SetTrackingParameters(power, ammoRigidBody.mass, endOfBarrel);	
	}
	
	// Update is called once per frame
	void Update () {

		if(sideTerrain == null)
		{
			sideTerrain = GameObject.Find("Terrain").GetComponent<SideTerrain>();
		}

		float terrainHeight = sideTerrain.GetHeight(transform.position.x); 
		gameObject.transform.SetPositionAndRotation(new Vector3(transform.position.x, sideTerrain.GetHeight(transform.position.x) + transform.localScale.y/2, 0.0f), gameObject.transform.rotation);

//		if(Input.GetButtonDown("Jump"))
//		{
//			Fire();
//		}	
		if(Input.GetKey(KeyCode.Space))
		{
			Fire();
		}


		if(trackTarget)
		{
			bool trackOK;

			Vector3 aimPoint = targetTracker.GetAimPoint(out trackOK);
			
		
			if(!trackOK || !targetLineOfSightOK(aimPoint))
			{
				audio.Stop();
			}
			else
			{	
				aimPointMarker.transform.SetPositionAndRotation(aimPoint, Quaternion.identity);
				RotateTowardsAimPoint(aimPoint);
			}
			
		}

		
	}

	void FixedUpdate()
	{
		float h = Input.GetAxis("Horizontal");
		
		if(h != 0)
		{
			RotateGun(-h * rotationSpeed);
		}
		if(!trackTarget && h==0)
		{
			audio.Stop();
		}
	}

	public void Fire()
	{

		if(Time.time - lastFiringTime < 1.0/fireRate)
		{
			return;
		}

		GameObject b = Instantiate(ammo, endOfBarrel.transform.position, Quaternion.identity);
		
		rb2d = b.GetComponent<Rigidbody2D>();

		rb2d.AddForce(endOfBarrel.transform.up * power);
		
		Destroy(b, 3f);

		GameObject p = Instantiate(particles, endOfBarrel.transform.position, endOfBarrel.transform.rotation);
		p.transform.Rotate(270f, 0, 0, Space.Self);	

		lastFiringTime =  Time.time;

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
			RotateGun(correctionAngle);
		}
		else 
		{
			if(dotProductFromGunDirection > 0)
			{
				RotateGun(-correctionAngle);
			}
			else
			{
				audio.Stop();
			}
		}

	
		if(Math.Abs(correctionAngle) < minimumAccuracyToFire)
		{
			Fire();
		}

	}

	private void RotateGun(float amount)
	{
		transform.Rotate(0f, 0f, amount);

		if(!audio.isPlaying)
		{
			audio.Play();
		}
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

	public void InitiateDestruction()
	{
		Instantiate(explosion, gameObject.transform.position, Quaternion.identity);		
		Destroy(gameObject);
	}
}
