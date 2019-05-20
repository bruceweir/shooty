using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rocket : MonoBehaviour {

	// Use this for initialization
	public float acceleration = 1f;
	public float maxSpeed = 10f;

	public GameObject destructionParticles;

	public GameObject explosion;

	public float damage = 1.0f;

	public float blastRadius = 1.0f;
	private Rigidbody2D rb2d;
	
	private bool damaged = false;

	
	void Awake () 
	{
		rb2d = GetComponent<Rigidbody2D>();
	}
	
	// Update is called once per frame
	void Update () {
	}

	void FixedUpdate() 
	{
		if(!damaged)
		{
    		rb2d.AddForce( Vector2.up * (rb2d.mass * Mathf.Abs(Physics2D.gravity.y) ) );   
			
			if(rb2d.velocity.magnitude < maxSpeed)
			{
				rb2d.AddForce( Vector2.right * acceleration);
			}
		}
	}

	public void Damaged()
	{
		damaged = true;
	}

	void OnCollisionEnter2D(Collision2D other)
	{
		if(other.gameObject.CompareTag("Bullet") || other.gameObject.CompareTag("Enemy"))
		{
			Damaged();
		}

		if(other.gameObject.CompareTag("ground"))
		{
			GameObject p = Instantiate(destructionParticles, transform.position, Quaternion.identity);
			Destroy(gameObject);

			Instantiate(explosion, transform.position, transform.rotation);
		}
	}
}
