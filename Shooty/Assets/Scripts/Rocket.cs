﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rocket : MonoBehaviour {

	// Use this for initialization
	public float acceleration = 1f;
	public float maxSpeed = 10f;

	public GameObject destructionParticles;

	public GameObject explosion;
	public GameObject smokeTrail;

	public float damage = 1.0f;

	public float blastRadius = 1.0f;

	public float blowUpDelay = 1.0f;
	private Rigidbody2D rb2d;

	private Health health;

	private LayerMask destructableMask;
	private GameObject smokeReference;
	
	void Awake () 
	{
		rb2d = GetComponent<Rigidbody2D>();
		health = GetComponent<Health>();

		destructableMask = LayerMask.GetMask("Destructable");

		smokeReference = Instantiate(smokeTrail, gameObject.transform.position, Quaternion.identity);
	}
	
	void FixedUpdate() 
	{
		if(!health.IsDestroyed())
		{
    		rb2d.AddForce( Vector2.up * (rb2d.mass * Mathf.Abs(Physics2D.gravity.y) ) );   
			
			if(rb2d.velocity.magnitude < maxSpeed)
			{
				rb2d.AddForce( Vector2.right * acceleration);
			}
		}
		else
		{
			Invoke("BlowUp", blowUpDelay);
		}

		smokeReference.transform.position = gameObject.transform.position;

	}

	
	void OnCollisionEnter2D(Collision2D other)
	{
		if(other.gameObject.CompareTag("Bullet"))
		{
			Bullet b = other.gameObject.GetComponent<Bullet>();
			//TODO make common Damage script for al enemies, to work in similar fashion to Health script
			health.TakeDamage(b.damage);
			return;
		}

		if(other.gameObject.CompareTag("Enemy"))
		{
			health.TakeDamage(damage);
			return;
		}

		BlowUp();
	
	}

	void BlowUp()
	{

		GameObject p = Instantiate(destructionParticles, transform.position, Quaternion.identity);
		
		Instantiate(explosion, transform.position, transform.rotation);

		//Debug.Log("Checking blastRadius");

		Collider2D[] collisions = Physics2D.OverlapCircleAll(transform.position, blastRadius, destructableMask);

		foreach(Collider2D c in collisions)
		{		
			c.SendMessage("TakeDamage", damage);
		}

		smokeReference.GetComponent<ParticleSystem>().Stop();
		Destroy(gameObject);


	}

}
